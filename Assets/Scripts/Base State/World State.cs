using UnityEngine;
using System;

public class WorldState : BaseState
{
    [SerializeField] public int worldID;

    protected override void Awake()
    {
        base.Awake();

#if UNITY_EDITOR
        // 检测重复 ID
        var allWorlds = FindObjectsByType<WorldState>(FindObjectsSortMode.None);
        foreach (var w in allWorlds)
        {
            if (w != this && w.worldID == worldID)
            {
                Debug.LogError($"WorldState 重复ID: {worldID}, 对象: {w.name}");
            }
        }
#endif
    }

    public override void BackToPreviousState(GameStateSnapshot lastSnapshot)
    {
        // 根据 ID 找到对应的快照
        foreach (var ws in lastSnapshot.worldSnapshots)
        {
            if (ws.worldID == worldID)
            {
                CurrentColor.SetState(ws.color);
                break;
            }
        }
    }

    public override void ExchangeColor()
    {
        base.ExchangeColor();
    }

    public override void HandleButtonPressed()
    {
        ExchangeColor();
    }

    public override void HandleButtonReleased()
    {
        ExchangeColor();
    }

    public override void Initialize(ButtonManager buttonManager)
    {
        _buttonManager = buttonManager;
        buttonManager.OnObeliskPressed += HandleButtonPressed;
        buttonManager.OnObeliskReleased += HandleButtonReleased;
    }

    protected override void OnDestroy()
    {
        _buttonManager.OnObeliskPressed -= HandleButtonPressed;
        _buttonManager.OnObeliskReleased -= HandleButtonReleased;
    }
}