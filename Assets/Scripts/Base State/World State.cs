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
        _buttonManager = buttonManager;
        buttonManager.OnObeliskPressed += HandleButtonPressed;
        buttonManager.OnObeliskReleased += HandleButtonReleased;
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
