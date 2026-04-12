using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 栅格影子管理器（单例）
/// 追踪所有被影子占据的格子，提供查询接口
/// 同时负责 Scene 视图中的网格可视化
/// </summary>
public class GridShadowManager : MonoBehaviour
{
    public static GridShadowManager Instance { get; private set; }

    [Header("网格设置")]
    [Tooltip("必须与 BaseState 中的 gridCellSize 一致")]
    public float cellSize = 1f;

    [Tooltip("网格原点（世界坐标），通常为 (0,0,0)")]
    public Vector3 gridOrigin = Vector3.zero;

    [Header("网格可视化（仅 Scene 视图）")]
    [Tooltip("是否在 Scene 中显示网格")]
    public bool showGrid = true;

    [Tooltip("网格绘制范围：X 方向格数（从原点向两侧各延伸）")]
    public int gridExtentX = 10;

    [Tooltip("网格绘制范围：Z 方向格数（从原点向两侧各延伸）")]
    public int gridExtentZ = 10;

    [Tooltip("网格线颜色")]
    public Color gridColor = new Color(1f, 1f, 1f, 0.15f);

    [Tooltip("原点十字线颜色")]
    public Color originColor = new Color(1f, 0.5f, 0f, 0.5f);

    [Tooltip("影子占用格高亮颜色")]
    public Color shadowHighlight = new Color(0.2f, 0.2f, 0.2f, 0.35f);

    [Tooltip("是否显示格子坐标标签（格子多时会影响性能）")]
    public bool showCoordinates = false;

    [Header("光源设置")]
    [Tooltip("场景中的平行光，用于计算影子方向")]
    public Light mainLight;

    [Tooltip("手动指定光方向（优先级高于 mainLight）。为 (0,0,0) 时使用 mainLight")]
    public Vector3 manualLightDir = Vector3.zero;

    [Header("影子规则")]
    [Tooltip("1格物体投射的影子长度（格数）")]
    public int shadowLength = 2;

    /// <summary>
    /// 影子方向（网格偏移），由光源方向计算得出
    /// 例如光从左上来 → 影子向右下延伸 → shadowGridDir = (1, 1)
    /// </summary>
    public Vector2Int ShadowGridDir { get; private set; }

    // 影子占用表：gridPos → 占据该格子的投影器列表
    // 用 List 是因为多个物体的影子可能重叠在同一格
    private Dictionary<Vector2Int, List<GridShadowCaster>> _shadowMap = new();

    // 所有注册的投影器
    private List<GridShadowCaster> _allCasters = new();

    private void Awake()
    {
        // 不 Destroy gameObject，因为和 LevelRoot 共享同一个 GameObject
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("GridShadowManager 已存在，禁用重复实例。");
            enabled = false;
            return;
        }
        Instance = this;
        UpdateShadowDirection();
    }

    private void OnDestroy()
    {
        // 关卡卸载时清除单例引用
        if (Instance == this)
            Instance = null;
    }

    // ========== 光源方向 → 网格影子方向 ==========

    /// <summary>
    /// 重新计算影子的网格方向。光源方向变化时调用。
    /// </summary>
    public void UpdateShadowDirection()
    {
        Vector3 lightDir = GetLightDirWorld();
        ShadowGridDir = QuantizeLightToGridDir(lightDir);
        Debug.Log($"[GridShadowManager] 光方向: {lightDir}, 影子网格方向: {ShadowGridDir}");
    }

    /// <summary>
    /// 设置新的手动光方向并刷新所有影子（第三章换方向时调用）
    /// </summary>
    public void SetLightDirection(Vector3 newDir)
    {
        manualLightDir = newDir;
        UpdateShadowDirection();
        RecalculateAllShadows();
    }

    /// <summary>
    /// 获取光照方向（世界空间）
    /// </summary>
    private Vector3 GetLightDirWorld()
    {
        if (manualLightDir != Vector3.zero)
            return manualLightDir.normalized;
        if (mainLight != null)
            return mainLight.transform.forward.normalized;
        return Vector3.down; // 默认正上方光 → 无延伸影子
    }

    /// <summary>
    /// 将3D光方向量化为2D网格偏移方向
    /// 光的XZ分量取反（影子朝光的反方向延伸），量化为 -1/0/1
    /// </summary>
    private Vector2Int QuantizeLightToGridDir(Vector3 lightDir)
    {
        // 取XZ平面分量，影子朝光的反方向
        float sx = -lightDir.x;
        float sz = -lightDir.z;

        // 如果XZ分量都很小（正顶光），影子不延伸
        if (Mathf.Abs(sx) < 0.01f && Mathf.Abs(sz) < 0.01f)
            return Vector2Int.zero;

        // 量化为 -1, 0, 1
        int gx = (Mathf.Abs(sx) > 0.01f) ? (int)Mathf.Sign(sx) : 0;
        int gz = (Mathf.Abs(sz) > 0.01f) ? (int)Mathf.Sign(sz) : 0;

        return new Vector2Int(gx, gz);
    }

    // ========== 影子占用管理 ==========

    /// <summary>注册一个影子投射器</summary>
    public void RegisterCaster(GridShadowCaster caster)
    {
        if (!_allCasters.Contains(caster))
            _allCasters.Add(caster);
    }

    /// <summary>注销一个影子投射器</summary>
    public void UnregisterCaster(GridShadowCaster caster)
    {
        _allCasters.Remove(caster);
        // 清除该投射器的所有影子占用
        ClearCasterShadow(caster);
    }

    /// <summary>清除指定投射器在占用表中的所有记录</summary>
    public void ClearCasterShadow(GridShadowCaster caster)
    {
        // 遍历所有格子，移除该 caster
        var keysToRemove = new List<Vector2Int>();
        foreach (var kvp in _shadowMap)
        {
            kvp.Value.Remove(caster);
            if (kvp.Value.Count == 0)
                keysToRemove.Add(kvp.Key);
        }
        foreach (var key in keysToRemove)
            _shadowMap.Remove(key);
    }

    /// <summary>标记一个格子被某投射器的影子占据</summary>
    public void MarkShadowCell(Vector2Int gridPos, GridShadowCaster caster)
    {
        if (!_shadowMap.ContainsKey(gridPos))
            _shadowMap[gridPos] = new List<GridShadowCaster>();
        if (!_shadowMap[gridPos].Contains(caster))
            _shadowMap[gridPos].Add(caster);
    }

    /// <summary>查询某格是否有影子</summary>
    public bool HasShadow(Vector2Int gridPos)
    {
        return _shadowMap.ContainsKey(gridPos) && _shadowMap[gridPos].Count > 0;
    }

    /// <summary>重算所有影子（光源方向变化时调用）</summary>
    public void RecalculateAllShadows()
    {
        foreach (var caster in _allCasters)
        {
            caster.RecalculateShadow();
        }
    }

    // ========== 坐标工具 ==========

    /// <summary>世界坐标 → 网格坐标</summary>
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector3 local = worldPos - gridOrigin;
        return new Vector2Int(
            Mathf.RoundToInt(local.x / cellSize),
            Mathf.RoundToInt(local.z / cellSize)
        );
    }

    /// <summary>网格坐标 → 世界坐标（地面中心）</summary>
    public Vector3 GridToWorld(Vector2Int gridPos, float groundY = 0f)
    {
        return gridOrigin + new Vector3(
            gridPos.x * cellSize,
            groundY,
            gridPos.y * cellSize
        );
    }

    /// <summary>
    /// 计算一个物体的影子应占据的格子列表
    /// </summary>
    public List<Vector2Int> CalcShadowCells(Vector2Int objectGridPos)
    {
        var cells = new List<Vector2Int>();

        // 正顶光 → 无延伸影子
        if (ShadowGridDir == Vector2Int.zero)
            return cells;

        for (int i = 1; i <= shadowLength; i++)
        {
            cells.Add(objectGridPos + ShadowGridDir * i);
        }

        return cells;
    }

    // ========== Scene 视图网格绘制 ==========

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showGrid) return;
        DrawGrid();
        DrawShadowCells();
        DrawLightDirection();
    }

    /// <summary>绘制网格线</summary>
    private void DrawGrid()
    {
        float y = gridOrigin.y + 0.005f; // 略高于地面

        // 网格线
        Gizmos.color = gridColor;
        for (int x = -gridExtentX; x <= gridExtentX; x++)
        {
            Vector3 startX = gridOrigin + new Vector3(x * cellSize, y, -gridExtentZ * cellSize);
            Vector3 endX = gridOrigin + new Vector3(x * cellSize, y, gridExtentZ * cellSize);
            Gizmos.DrawLine(startX, endX);
        }
        for (int z = -gridExtentZ; z <= gridExtentZ; z++)
        {
            Vector3 startZ = gridOrigin + new Vector3(-gridExtentX * cellSize, y, z * cellSize);
            Vector3 endZ = gridOrigin + new Vector3(gridExtentX * cellSize, y, z * cellSize);
            Gizmos.DrawLine(startZ, endZ);
        }

        // 原点十字线（加粗效果）
        Gizmos.color = originColor;
        float halfCell = cellSize * 0.5f;
        Vector3 origin = gridOrigin + Vector3.up * y;
        Gizmos.DrawLine(origin + Vector3.left * halfCell, origin + Vector3.right * halfCell);
        Gizmos.DrawLine(origin + Vector3.back * halfCell, origin + Vector3.forward * halfCell);
        // 原点小方块
        Gizmos.DrawWireCube(origin, new Vector3(cellSize * 0.15f, 0.01f, cellSize * 0.15f));

        // 坐标标签
        if (showCoordinates)
        {
            var style = new GUIStyle();
            style.normal.textColor = new Color(1f, 1f, 1f, 0.5f);
            style.fontSize = 10;
            style.alignment = TextAnchor.MiddleCenter;

            for (int x = -gridExtentX; x <= gridExtentX; x++)
            {
                for (int z = -gridExtentZ; z <= gridExtentZ; z++)
                {
                    Vector3 pos = gridOrigin + new Vector3(x * cellSize, y + 0.02f, z * cellSize);
                    UnityEditor.Handles.Label(pos, $"{x},{z}", style);
                }
            }
        }
    }

    /// <summary>高亮被影子占据的格子</summary>
    private void DrawShadowCells()
    {
        if (_shadowMap == null) return;

        Gizmos.color = shadowHighlight;
        foreach (var kvp in _shadowMap)
        {
            if (kvp.Value.Count > 0)
            {
                Vector3 worldPos = GridToWorld(kvp.Key, gridOrigin.y + 0.01f);
                Gizmos.DrawCube(worldPos, new Vector3(cellSize * 0.95f, 0.02f, cellSize * 0.95f));
            }
        }
    }

    /// <summary>在原点画一个箭头表示光方向和影子延伸方向</summary>
    private void DrawLightDirection()
    {
        if (ShadowGridDir == Vector2Int.zero) return;

        float y = gridOrigin.y + 0.03f;
        Vector3 center = gridOrigin + Vector3.up * y;

        // 影子延伸方向（橙色箭头）
        Vector3 shadowDir3D = new Vector3(ShadowGridDir.x, 0, ShadowGridDir.y).normalized;
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
        Vector3 arrowEnd = center + shadowDir3D * cellSize * 1.5f;
        Gizmos.DrawLine(center, arrowEnd);
        // 箭头头部
        Vector3 right = Vector3.Cross(Vector3.up, shadowDir3D) * cellSize * 0.2f;
        Gizmos.DrawLine(arrowEnd, arrowEnd - shadowDir3D * cellSize * 0.3f + right);
        Gizmos.DrawLine(arrowEnd, arrowEnd - shadowDir3D * cellSize * 0.3f - right);

        // 标签
        var labelStyle = new GUIStyle();
        labelStyle.normal.textColor = new Color(1f, 0.6f, 0.1f, 1f);
        labelStyle.fontSize = 12;
        labelStyle.fontStyle = FontStyle.Bold;
        UnityEditor.Handles.Label(arrowEnd + Vector3.up * 0.1f, $"影子方向 ({ShadowGridDir.x},{ShadowGridDir.y})", labelStyle);
    }
#endif
}