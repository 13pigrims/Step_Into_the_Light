using UnityEngine;
using System;

public class CharacterState : BaseState
{
    // 游戏结束事件，供外部订阅
    public static event Action OnGameOver;
    public override void BackToPreviousState(GameStateSnapshot lastSnapshot)
    {
        CurrentColor.SetState(lastSnapshot.characterColor);
        _currentTransform.position = lastSnapshot.characterPosition;
    }

    public override bool canMoveOn(Vector3 movement)
    {
        if (Physics.Raycast(_currentTransform.position, movement.normalized, out RaycastHit hit, checkDistance))
        {
            if (hit.collider.CompareTag("Shadow"))
            {
                // TODO: 接入Game Over Panel
                return false;
            }
            if (hit.collider.CompareTag("Wall") || hit.collider.CompareTag("Object"))
                return false;
        }
        return true;
    }

    public override void Move(Vector3 movement)
    {
        _currentTransform.position += movement * moveSpeed * Time.deltaTime;
        _currentTransform = transform;
    }

    public override void Initialize(ButtonManager buttonManager)
    {
        _buttonManager = buttonManager;
        buttonManager.OnPedestalPressed += HandleButtonPressed;
        buttonManager.OnPedestalReleased += HandleButtonReleased;
    }

    public override void InitializeInput(InputManager inputManager, CharacterState characterState)
    {
        _inputManager = inputManager;
    }

    public override bool IsInteractive()
    {
        return true; // 角色永远可交互
    }

    protected override void Awake()
    {
        base.Awake();
    }

    public override void HandleButtonPressed()
    {
        ExchangeColor();
    }

    public override void HandleButtonReleased()
    {
        ExchangeColor();
    }
    /// <summary>
    /// 用于触发游戏结束面板
    /// </summary>
    private void TriggerGameOver()
    {
        OnGameOver?.Invoke();
    }

    protected override void OnDestroy()
    {
        _buttonManager.OnPedestalPressed -= HandleButtonPressed;
        _buttonManager.OnPedestalReleased -= HandleButtonReleased;
    }
}
