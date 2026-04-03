using UnityEngine;
using System;

public abstract class BaseButton : MonoBehaviour
{
    // 状态成员
    private bool _isNowPressed;
    private bool _isBeforePressed;
    private bool _isPressed;
    public bool IsPressed { get { return _isPressed; } private set { _isPressed = value; } }
    // 事件
    public static event Action OnShadowPressed;
    public static event Action OnShadowWithdraw;
    // 依赖
    private ButtonManager _buttonManager;
    /// <summary>
    /// 初始化方法，注入ButtonManager依赖
    /// </summary>
    /// <param name="buttonManager"></param>
    public void Initialize(ButtonManager buttonManager)
    {
        _buttonManager = buttonManager;
    }
    /// <summary>
    /// 更新方法，检测按压状态变化并触发事件
    /// </summary>
    protected virtual void Update()
    {
        _isBeforePressed = _isNowPressed;
        _isNowPressed = IsChangeStatePressed();
    // 从上一帧未按压到当前帧被按压
    if (!_isBeforePressed && _isNowPressed)
        {
            IsPressed = true;
            OnShadowPressed?.Invoke();
            NotifyStateChanged(true);
        }
        // 从上一帧按压状态到当前帧被释放
        else if (_isBeforePressed && !_isNowPressed)
        {
            IsPressed = false;
            OnShadowWithdraw?.Invoke();
            NotifyStateChanged(false);
        }
    }
    /// <summary>
    /// 子类中实现具体的按压状态检测逻辑
    /// </summary>
    /// <returns></returns>
    public virtual bool IsChangeStatePressed() { return false; }
    /// <summary>
    /// 通知ButtonManager当前按钮状态变化
    /// </summary>
    /// <param name="isPressed"></param>
    protected virtual void NotifyStateChanged(bool isPressed)
    {
        if (isPressed)
            _buttonManager.NotifyButtonPressed(this);
        else
            _buttonManager.NotifyButtonReleased(this);
    }
    /// <summary>
    /// 子类如果有额外订阅时在这里取消，避免内存泄漏
    /// </summary>
    protected virtual void OnDestroy() { }
}
