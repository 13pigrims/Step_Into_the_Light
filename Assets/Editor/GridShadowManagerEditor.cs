using UnityEngine;
using UnityEditor;

/// <summary>
/// GridShadowManager 自定义 Inspector
/// 提供网格可视化控制和便捷工具按钮
/// 放在 Assets/Editor 文件夹下
/// </summary>
[CustomEditor(typeof(GridShadowManager))]
public class GridShadowManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GridShadowManager mgr = (GridShadowManager)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("工具", EditorStyles.boldLabel);

        // 对齐所有可移动物体到网格
        if (GUILayout.Button("将所有 BaseState 物体对齐到网格"))
        {
            SnapAllToGrid(mgr);
        }

        // 刷新影子方向
        if (GUILayout.Button("刷新影子方向"))
        {
            mgr.UpdateShadowDirection();
            SceneView.RepaintAll();
        }

        // 显示当前影子方向信息
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("运行时信息", EditorStyles.boldLabel);

        if (Application.isPlaying)
        {
            EditorGUILayout.LabelField($"影子网格方向: ({mgr.ShadowGridDir.x}, {mgr.ShadowGridDir.y})");
        }
        else
        {
            EditorGUILayout.HelpBox(
                "运行游戏后可在此查看影子方向等运行时信息。\n" +
                "Scene 视图中可看到：\n" +
                "· 白色网格线 = 格子边界\n" +
                "· 橙色十字 = 网格原点\n" +
                "· 橙色箭头 = 影子延伸方向\n" +
                "· 深色方块 = 被影子占据的格子",
                MessageType.Info);
        }
    }

    /// <summary>
    /// 将场景中所有 BaseState 物体的位置对齐到最近的格子中心
    /// </summary>
    private void SnapAllToGrid(GridShadowManager mgr)
    {
        var allStates = FindObjectsByType<BaseState>(FindObjectsSortMode.None);
        int count = 0;

        Undo.RecordObjects(allStates, "Snap All to Grid");

        foreach (var state in allStates)
        {
            Vector3 pos = state.transform.position;
            Vector3 origin = mgr.gridOrigin;
            float cell = mgr.cellSize;

            pos.x = (Mathf.Floor((pos.x - origin.x) / cell) + 0.5f) * cell + origin.x;
            pos.z = (Mathf.Floor((pos.z - origin.z) / cell) + 0.5f) * cell + origin.z;

            if (state.transform.position != pos)
            {
                state.transform.position = pos;
                count++;
            }
        }

        Debug.Log($"[GridShadowManager] 已将 {count} 个物体对齐到网格（共 {allStates.Length} 个 BaseState）");
    }
}

/// <summary>
/// 编辑器菜单工具：网格对齐选中物体
/// </summary>
public static class GridSnapTools
{
    /// <summary>
    /// 将当前选中的物体对齐到网格（快捷键 Ctrl+Shift+G）
    /// </summary>
    [MenuItem("Tools/Grid Snap/对齐选中物体到网格 %#g")]
    public static void SnapSelectedToGrid()
    {
        var mgr = Object.FindFirstObjectByType<GridShadowManager>();
        if (mgr == null)
        {
            Debug.LogError("场景中没有 GridShadowManager，无法对齐。");
            return;
        }

        var selected = Selection.gameObjects;
        if (selected.Length == 0)
        {
            Debug.LogWarning("未选中任何物体。");
            return;
        }

        Undo.RecordObjects(selected, "Snap to Grid");

        int count = 0;
        foreach (var go in selected)
        {
            Vector3 pos = go.transform.position;
            Vector3 origin = mgr.gridOrigin;
            float cell = mgr.cellSize;

            pos.x = (Mathf.Floor((pos.x - origin.x) / cell) + 0.5f) * cell + origin.x;
            pos.z = (Mathf.Floor((pos.z - origin.z) / cell) + 0.5f) * cell + origin.z;

            if (go.transform.position != pos)
            {
                go.transform.position = pos;
                count++;
            }
        }

        Debug.Log($"[Grid Snap] 已将 {count} 个物体对齐到网格");
    }

    /// <summary>
    /// 切换 Scene 视图中的网格显示
    /// </summary>
    [MenuItem("Tools/Grid Snap/切换网格显示")]
    public static void ToggleGridDisplay()
    {
        var mgr = Object.FindFirstObjectByType<GridShadowManager>();
        if (mgr != null)
        {
            Undo.RecordObject(mgr, "Toggle Grid");
            mgr.showGrid = !mgr.showGrid;
            SceneView.RepaintAll();
            Debug.Log($"[Grid Snap] 网格显示: {(mgr.showGrid ? "开" : "关")}");
        }
    }
}