using UnityEngine;
using System;

public class WorldState : BaseState
{
    public override void BackToPreviousState(GameStateSnapshot lastSnapshot)
    {
        CurrentColor.SetState(lastSnapshot.worldColor);
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
        Debug.Log($"WorldState.Initialize开始, buttonManager: {buttonManager}");
        _buttonManager = buttonManager;
        buttonManager.OnObeliskPressed += HandleButtonPressed;
        buttonManager.OnObeliskReleased += HandleButtonReleased;
        Debug.Log("WorldState.Initialize完成");
    }

    protected override void Awake()
    {
        base.Awake();
    }

     protected override void OnDestroy()
     {
        _buttonManager.OnObeliskPressed -= HandleButtonPressed;
        _buttonManager.OnObeliskReleased -= HandleButtonReleased;
     }
}
