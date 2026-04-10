using UnityEngine;
using System;

public class CharacterState : BaseState
{
    // 游戏结束事件，供外部订阅
    public static event Action OnGameOver;
    private bool _isGameOver;

    public override void BackToPreviousState(GameStateSnapshot lastSnapshot)
    {
        CurrentColor.SetState(lastSnapshot.characterColor);
        _currentTransform.position = lastSnapshot.characterPosition;
    }

    public override bool canMoveOn(Vector3 movement)
    {
        // 脚部的射线检测
        Vector3 feetPos = _currentTransform.position + Vector3.up * 0.1f;
        if (Physics.Raycast(feetPos, Vector3.down, out RaycastHit shadowHit, 0.5f))
        {
            if (shadowHit.collider.CompareTag("Shadow"))
            {
                if (!_isGameOver)
                {
                    _isGameOver = true;
                    GameRoot.GetInstance().UIManager_Root.PushPanel(new GameOverPanel());
                }
                return false;
            }
            if (shadowHit.collider.CompareTag("Wall") || shadowHit.collider.CompareTag("Object"))
                return false;
        }
        return true;
    }

    public override void Move(Vector3 movement)
    {
        _currentTransform.position += movement * moveSpeed * Time.deltaTime;
        _currentTransform = transform;
        // 播放移动音效，只有当角色实际移动时才播放
        if (movement.sqrMagnitude > 0.01f)
        {
            GameRoot.GetInstance().AudioManager_Root.PlaySFX(GameRoot.GetInstance().MoveClip);
        }
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

    public void ResetGameOver()
    {
        _isGameOver = false;
    }

    protected override void OnDestroy()
    {
        _buttonManager.OnPedestalPressed -= HandleButtonPressed;
        _buttonManager.OnPedestalReleased -= HandleButtonReleased;
    }
}
