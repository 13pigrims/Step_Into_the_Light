using UnityEngine;

/// <summary>
/// 作用：根据物体网格（MeshFilter）生成一个“投影到地面”的影子网格，并把它设置成子物体 ShadowCollider 的 MeshCollider。
/// 用途：让玩家踩到/碰到影子产生效果（失败、扣血等）。
/// 
/// 核心思路：
/// 1) 取源Mesh所有顶点（本地坐标）
/// 2) 转成世界坐标
/// 3) 沿光照方向投影到地面（Raycast命中地面，或回退到指定Y平面）
/// 4) 再把投影点转成 ShadowCollider 的本地坐标，写入 MeshCollider 使用的 Mesh 顶点
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(MeshFilter))]
public class ShadowProjector : MonoBehaviour
{
    [Header("光源设置")]
    [Tooltip("场景里的 Directional Light。为空时默认向下投影。")]
    public Light mainLight; // Directional Light

    [Tooltip("手动指定光方向。为(0,0,0)时使用 mainLight 的方向。")]
    public Vector3 manualLightDir = Vector3.zero; // (0,0,0) 时使用 mainLight

    [Header("地面设置")]
    [Tooltip("是否使用 Raycast 投影到真实地面（支持斜坡/起伏）。")]
    public bool useRaycast = true;

    [Tooltip("地面所在的Layer。Raycast 只会命中这些层。")]
    public LayerMask groundMask = ~0;

    [Tooltip("Raycast最大距离（从顶点发射出去能打多远）。")]
    public float raycastMaxDistance = 200f;

    [Tooltip("Raycast起点沿“反光方向”挪一点，避免起点刚好在地面Collider内部导致打不到。")]
    public float rayStartOffset = 0.05f;

    [Tooltip("投影到地面后沿法线/向上抬一点点，避免网格与地面重叠（穿地或Z-fighting）。")]
    public float surfaceOffset = 0.01f;

    [Tooltip("当 Raycast 没命中时，回退投影到该 Y 平面。")]
    public float groundY = 0f;

    [Header("碰撞体设置")]
    [Tooltip("是否自动配置 ShadowCollider 的 MeshCollider 参数以及 Tag。")]
    public bool autoSetupCollider = true;

    [Tooltip("更精准：关闭 Convex（注意：凹 MeshCollider 不支持 Trigger；会自动变为非Trigger实体碰撞体）。")]
    public bool useNonConvexMeshCollider = true;

    // 本体的网格来源（要投影的那个Mesh）
    private MeshFilter _meshFilter;

    // 动态生成/更新的影子网格（赋给 MeshCollider.sharedMesh）
    private Mesh _shadowMesh;

    // ShadowCollider 上的 MeshCollider
    private MeshCollider _shadowCollider;

    // ShadowCollider 的 Transform（用于世界<->本地坐标转换）
    private Transform _shadowColliderTransform;

    // RaycastNonAlloc 的缓存，避免每帧GC
    private RaycastHit[] _hits;
    private const int DefaultHitBufferSize = 16;

    private void Awake()
    {
        // 1) 获取本体MeshFilter（作为源网格）
        _meshFilter = GetComponent<MeshFilter>();
        Debug.Log($"已获取本体物体的Mesh Collider");


        // 2) 找到子物体 ShadowCollider（你需要在层级里手工建立这个子物体）
        var child = transform.Find("ShadowCollider");
        if (child == null)
        {
            Debug.LogError("[ShadowProjector] Missing child Transform named 'ShadowCollider'.");
            enabled = false;
            return;
        }

        // 3) 获取 ShadowCollider 的 MeshCollider
        _shadowColliderTransform = child;
        _shadowCollider = child.GetComponent<MeshCollider>();
        if (_shadowCollider == null)
        {
            Debug.LogError("[ShadowProjector] 'ShadowCollider' must have a MeshCollider.");
            enabled = false;
            return;
        }

        // 4) 创建用于承载“影子”的 Mesh（会每次更新顶点/三角形）
        _shadowMesh = new Mesh { name = "ShadowMesh" };

        // 5) 初始化 Raycast 命中缓存
        _hits = new RaycastHit[DefaultHitBufferSize];

        // 6) 自动配置碰撞体参数
        if (autoSetupCollider)
        {
            _shadowCollider.sharedMesh = null;

            if (useNonConvexMeshCollider)
            {
                // 关键点：
                // - convex = false 才能保留凹形轮廓（更精准）
                // - 但凹 MeshCollider 不支持 Trigger，所以必须 isTrigger = false（实体碰撞）
                _shadowCollider.convex = false;
                _shadowCollider.isTrigger = false;
            }
            else
            {
                // convex = true 时可以 isTrigger = true（可用于 OnTriggerEnter）
                // 但会被凸包化 => 轮廓会变“胖”，不够精准
                _shadowCollider.convex = true;
                _shadowCollider.isTrigger = true;
            }

            // 用于玩家脚本里 CompareTag("Shadow") 判断
            if (!child.CompareTag("Shadow"))
                child.tag = "Shadow";
        }
        UpdateShadowColliderMesh();
    }

    private void OnDrawGizmos()
    {
        if (_shadowMesh == null) return;
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawMesh(_shadowMesh, _shadowColliderTransform.position);
    }

    private void LateUpdate()
    {
        // 为什么用 LateUpdate：更容易拿到物体最终姿态（尤其有动画/其他脚本改transform）
        bool lightChanged = mainLight != null && mainLight.transform.hasChanged;

        // transform.hasChanged：本体移动/旋转/缩放变化时才更新，提高性能
        if (transform.hasChanged || lightChanged)
        {
            UpdateShadowColliderMesh();

            // 重置标记，避免下一帧重复更新
            transform.hasChanged = false;
            if (mainLight != null) mainLight.transform.hasChanged = false;
        }
    }

    /// <summary>
    /// 生成/更新 ShadowCollider 的 MeshCollider 所使用的网格
    /// </summary>
    private void UpdateShadowColliderMesh()
    {
        // 取源网格（注意用 sharedMesh，避免实例化 mesh 导致额外内存）
        var srcMesh = _meshFilter.sharedMesh;
        if (srcMesh == null || srcMesh.vertexCount == 0)
            return;

        // 获取光照方向（世界空间）
        Vector3 lightDirWorld = GetLightDirWorld();
        if (!IsValidLightDir(lightDirWorld))
            return;

        // 1) 源顶点从“本体本地坐标”转换到“世界坐标”
        var srcVertices = srcMesh.vertices;
        var worldVertices = new Vector3[srcVertices.Length];
        for (int i = 0; i < srcVertices.Length; i++)
            worldVertices[i] = transform.TransformPoint(srcVertices[i]);

        // 2) 每个世界顶点投影到地面，并写入 ShadowCollider 的“本地坐标”数组
        // 注意：MeshCollider.sharedMesh 的 vertices 必须是 Collider 物体自身的本地坐标
        var shadowLocalVertices = new Vector3[worldVertices.Length];
        for (int i = 0; i < worldVertices.Length; i++)
        {
            // 世界坐标投影点
            Vector3 projectedWorld = ProjectWorldPoint(worldVertices[i], lightDirWorld);

            // 转回 ShadowCollider 的本地坐标，写入 mesh.vertices
            shadowLocalVertices[i] = _shadowColliderTransform.InverseTransformPoint(projectedWorld);
        }

        // 3) 用投影后的顶点 + 源mesh的三角形索引（triangles）生成影子mesh
        _shadowMesh.Clear();
        _shadowMesh.vertices = shadowLocalVertices;

        // 复用源模型的三角面连接关系（顶点连法不变，只是顶点位置变成投影位置）
        _shadowMesh.triangles = srcMesh.triangles;

        // 限定包围盒（MeshCollider 更新通常需要）
        _shadowMesh.RecalculateBounds();

        // 4) 刷新 MeshCollider（sharedMesh 直接替换有时不会立刻更新，先置空更稳）
        _shadowCollider.sharedMesh = null;
        _shadowCollider.sharedMesh = _shadowMesh;
        // 同步mesh到MeshFilter用于可视化
        MeshFilter mf = _shadowColliderTransform.GetComponent<MeshFilter>();
        if (mf != null) mf.sharedMesh = _shadowMesh;
    }

    /// <summary>
    /// 获取光照方向（世界向量）。
    /// 约定：lightDirWorld 表示“从顶点朝地面投射的方向”。
    /// </summary>
    private Vector3 GetLightDirWorld()
    {
        // 优先手动方向
        if (manualLightDir != Vector3.zero)
            return manualLightDir.normalized;

        // 使用主光：Directional Light 的 forward 指向“光照射方向的反方向”
        // 所以取 -forward 才是光线传播方向（常见约定）
        if (mainLight != null)
            return mainLight.transform.forward.normalized;

        // 没有光时默认向下投影
        return Vector3.down;
    }

    /// <summary>
    /// 检查光方向是否合法。
    /// 如果 dir.y 接近 0（光几乎水平），投影到 groundY 平面会发生除0或巨大数值。
    /// </summary>
    private static bool IsValidLightDir(Vector3 dir)
        => dir.sqrMagnitude > 0.0001f && Mathf.Abs(dir.y) > 0.0001f;

    /// <summary>
    /// 把世界空间一点沿光方向投影到地面。
    /// 优先使用 Raycast 命中 groundMask 上的 Collider；失败则投影到固定 Y 平面。
    /// </summary>
    private Vector3 ProjectWorldPoint(Vector3 pointWorld, Vector3 lightDirWorld)
    {
        if (useRaycast)
        {
            // 把起点往“反方向”挪一点，避免起点落在地面碰撞内部/边界导致 Raycast 不稳定
            Vector3 origin = pointWorld - lightDirWorld * rayStartOffset;

            // 沿光方向发射射线，寻找地面命中点
            var ray = new Ray(origin, lightDirWorld);

            // RaycastNonAlloc：把结果写进 _hits，避免GC
            int count = Physics.RaycastNonAlloc(
                ray,
                _hits,
                raycastMaxDistance,
                groundMask,
                QueryTriggerInteraction.Ignore);

            if (count > 0)
            {
                // 从多个命中里选“最近的那个”，避免穿过薄地面命中更远的Collider
                int best = 0;
                float bestDist = _hits[0].distance;
                for (int i = 1; i < count; i++)
                {
                    if (_hits[i].distance < bestDist)
                    {
                        bestDist = _hits[i].distance;
                        best = i;
                    }
                }

                var hit = _hits[best];

                // 命中点沿法线抬一点，避免穿地
                return hit.point + hit.normal * surfaceOffset;
            }
        }

        // Raycast失败：回退投影到 groundY 的平面（y = groundY）
        // 解方程：pointWorld + lightDirWorld * t 的 y 分量等于 groundY
        float t = (groundY - pointWorld.y) / lightDirWorld.y;

        // 回退时用 Vector3.up 抬一点即可（没有法线信息）
        return pointWorld + lightDirWorld * t + Vector3.up * surfaceOffset;
    }
}