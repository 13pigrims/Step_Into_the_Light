using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 栅格影子投射器（替代原 ShadowProjector）
/// 挂载在需要产生影子的物体上，根据光源方向在网格上生成影子格子
/// 每个影子格子有 BoxCollider（Tag="Shadow"）+ 视觉 Quad
/// </summary>
public class GridShadowCaster : MonoBehaviour
{
    [Header("影子视觉")]
    [Tooltip("影子使用的材质（半透明黑色）")]
    public Material shadowMaterial;

    [Tooltip("影子离地高度，防止 Z-fighting")]
    public float shadowYOffset = 0.01f;

    [Tooltip("影子物体所在的 Layer（建议设为 Shadow 层，避免干扰移动射线）")]
    public int shadowLayer = 0;

    [Header("调试")]
    [Tooltip("在 Scene 视图中显示影子格子")]
    public bool showGizmos = true;

    // 当前影子占据的格子
    private List<Vector2Int> _currentShadowCells = new();

    // 影子 GameObject 池（每个格子一个）
    private List<GameObject> _shadowObjects = new();

    // 上一次的物体网格坐标，用于检测是否需要更新
    private Vector2Int _lastGridPos;

    // 缓存引用
    private GridShadowManager _manager;

    private void Start()
    {
        _manager = GridShadowManager.Instance;
        if (_manager == null)
        {
            Debug.LogError($"[GridShadowCaster] {name}: GridShadowManager 不存在！请在场景中添加。");
            enabled = false;
            return;
        }

        // 注册到管理器
        _manager.RegisterCaster(this);

        // 初始计算
        _lastGridPos = _manager.WorldToGrid(transform.position);
        RecalculateShadow();
    }

    private void LateUpdate()
    {
        // 检测物体是否移动到了新的格子
        Vector2Int currentGridPos = _manager.WorldToGrid(transform.position);
        if (currentGridPos != _lastGridPos)
        {
            _lastGridPos = currentGridPos;
            RecalculateShadow();
        }
    }

    /// <summary>
    /// 重新计算影子格子并更新视觉/碰撞体
    /// </summary>
    public void RecalculateShadow()
    {
        if (_manager == null) return;

        // 1. 清除旧的影子占用
        _manager.ClearCasterShadow(this);

        // 2. 计算新的影子格子
        Vector2Int gridPos = _manager.WorldToGrid(transform.position);
        _currentShadowCells = _manager.CalcShadowCells(gridPos);

        // 3. 在管理器中标记占用
        foreach (var cell in _currentShadowCells)
        {
            _manager.MarkShadowCell(cell, this);
        }

        // 4. 更新视觉和碰撞体
        UpdateShadowObjects();
    }

    /// <summary>
    /// 创建/更新/回收影子 GameObject
    /// </summary>
    private void UpdateShadowObjects()
    {
        float cellSize = _manager.cellSize;

        // 确保有足够的影子对象
        while (_shadowObjects.Count < _currentShadowCells.Count)
        {
            _shadowObjects.Add(CreateShadowCell(cellSize));
        }

        // 启用需要的，定位到正确位置
        for (int i = 0; i < _currentShadowCells.Count; i++)
        {
            var obj = _shadowObjects[i];
            obj.SetActive(true);

            Vector3 worldPos = _manager.GridToWorld(_currentShadowCells[i], shadowYOffset);
            obj.transform.position = worldPos;
            obj.transform.localScale = new Vector3(cellSize, cellSize, cellSize);
        }

        // 禁用多余的（物体从有影子变为无影子时，如正顶光）
        for (int i = _currentShadowCells.Count; i < _shadowObjects.Count; i++)
        {
            _shadowObjects[i].SetActive(false);
        }
    }

    /// <summary>
    /// 创建一个影子格子的 GameObject（Quad + BoxCollider）
    /// </summary>
    private GameObject CreateShadowCell(float cellSize)
    {
        var obj = new GameObject($"{name}_Shadow_{_shadowObjects.Count}");
        obj.transform.SetParent(transform); // 跟随父物体层级，方便管理
        obj.tag = "Shadow";
        obj.layer = shadowLayer;

        // 设置到 Button 不检测的层？不需要，Button 的 OverlapBox 检测的是 Tag
        // 但如果你有专门的 Shadow Layer，可以在这里设置

        // 视觉：一个朝上的 Quad
        var meshFilter = obj.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = CreateQuadMesh();

        var meshRenderer = obj.AddComponent<MeshRenderer>();
        if (shadowMaterial != null)
        {
            meshRenderer.sharedMaterial = shadowMaterial;
        }
        else
        {
            // 没有指定材质时，创建一个默认的半透明黑色材质
            var mat = new Material(Shader.Find("Unlit/Color"));
            mat.color = new Color(0, 0, 0, 0.5f);
            meshRenderer.sharedMaterial = mat;
        }

        // 碰撞体：BoxCollider，用于按钮的 OverlapBox 检测
        var collider = obj.AddComponent<BoxCollider>();
        collider.size = new Vector3(1f, 0.1f, 1f); // 薄片状
        collider.center = Vector3.zero;
        // 不设为 Trigger，保持实体碰撞（与原 ShadowProjector 一致）

        return obj;
    }

    /// <summary>
    /// 创建一个朝上的单位 Quad 网格（铺在地面上）
    /// </summary>
    private Mesh CreateQuadMesh()
    {
        var mesh = new Mesh { name = "ShadowQuad" };
        mesh.vertices = new Vector3[]
        {
            new(-0.5f, 0, -0.5f),
            new(-0.5f, 0,  0.5f),
            new( 0.5f, 0,  0.5f),
            new( 0.5f, 0, -0.5f)
        };
        mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
        mesh.normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
        mesh.uv = new Vector2[]
        {
            new(0, 0), new(0, 1), new(1, 1), new(1, 0)
        };
        mesh.RecalculateBounds();
        return mesh;
    }

    private void OnDestroy()
    {
        if (_manager != null)
            _manager.UnregisterCaster(this);

        // 销毁影子对象
        foreach (var obj in _shadowObjects)
        {
            if (obj != null)
                Destroy(obj);
        }
        _shadowObjects.Clear();
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos || _manager == null) return;

        Gizmos.color = new Color(0.2f, 0.2f, 0.2f, 0.4f);
        float cellSize = _manager.cellSize;
        foreach (var cell in _currentShadowCells)
        {
            Vector3 worldPos = _manager.GridToWorld(cell, 0.05f);
            Gizmos.DrawCube(worldPos, new Vector3(cellSize, 0.02f, cellSize));
        }
    }
}