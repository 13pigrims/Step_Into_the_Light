using UnityEngine;
using System;
public class ButtonManager
{
    // 单例
    public static ButtonManager Instance;

    // 事件
    public event Action OnObeliskPressed;
    public event Action OnObeliskReleased;
    public event Action OnPedestalPressed;
    public event Action OnPedestalReleased;

    // 构造函数，由LevelRoot调用
    public ButtonManager()
    {
        Instance = this;
    }

    public static ButtonManager GetInstance()
    {
        // 初始化访问到的Instance
        if (Instance == null)
        {
            Debug.LogError("ButtonManager实体不存在！");
            return null;
        }
        else
        {
            return Instance;
        }
    }

    public void NotifyButtonPressed(BaseButton button)
    {
        if (button is ObeliskButton)
            OnObeliskPressed?.Invoke();
        else if (button is PedestalButton)
            OnPedestalPressed?.Invoke();
    }

    public void NotifyButtonReleased(BaseButton button)
    {
        if (button is ObeliskButton)
            OnObeliskReleased?.Invoke();
        else if (button is PedestalButton)
            OnPedestalReleased?.Invoke();
    }
}
