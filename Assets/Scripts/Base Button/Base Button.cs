using UnityEngine;
using System;

public abstract class BaseButton : MonoBehaviour
{
    // 状态成员
    private bool _isPressed;
    public bool IsPressed { get { return _isPressed; } private set { _isPressed = value; } }

    // 事件
    public static event Action OnShadowPressed;
    public static event Action OnShadowWithdraw;

    // 依赖
    private ButtonManager _buttonManager;

    /// <summary>
    /// 注入ButtonManager依赖
    /// </summary>
    public void Initialize(ButtonManager buttonManager)
    {
        _buttonManager = buttonManager;
        Debug.Log($"ButtonManager注入成功: {name}");
    }

    /// <summary>
    /// 影子进入时触发按压事件
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"OnTriggerEnter: {name} 检测到 {other.name}, Tag: {other.tag}");
        if (other.CompareTag("Shadow"))
        {
            _isPressed = true;
            Debug.Log($"{name} 被影子覆盖，触发Pressed");
            OnShadowPressed?.Invoke();
            NotifyStateChanged(true);
        }
    }

    /// <summary>
    /// 影子离开时触发释放事件
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"OnTriggerExit: {name} 检测到 {other.name} 离开, Tag: {other.tag}");
        if (other.CompareTag("Shadow"))
        {
            _isPressed = false;
            Debug.Log($"{name} 影子撤出，触发Released");
            OnShadowWithdraw?.Invoke();
            NotifyStateChanged(false);
        }
    }

    /// <summary>
    /// 通知ButtonManager当前按钮状态变化
    /// </summary>
    protected virtual void NotifyStateChanged(bool isPressed)
    {
        if (_buttonManager == null)
        {
            Debug.LogError($"ButtonManager未注入: {name}");
            return;
        }
        if (isPressed)
        {
            Debug.Log($"{name} 通知ButtonManager: Pressed");
            _buttonManager.NotifyButtonPressed(this);
        }
        else
        {
            Debug.Log($"{name} 通知ButtonManager: Released");
            _buttonManager.NotifyButtonReleased(this);
        }
    }

    /// <summary>
    /// 子类如果有额外订阅时在这里取消
    /// </summary>
    protected virtual void OnDestroy() { }
}