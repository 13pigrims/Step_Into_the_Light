using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraSwitch : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera tpsCamera;
    [SerializeField] private CinemachineVirtualCamera topDownCamera;

    private bool isTopDown = false;
    bool IsChangeCamera;

    public void OnChangeCamera(InputAction.CallbackContext ctx)
    {
        IsChangeCamera = ctx.ReadValueAsButton();
    }

    // 在 Update 里监听按键
    void Update()
    {
        if (IsChangeCamera)
        {
            ToggleCamera();
        }
    }

    public void ToggleCamera()
    {
        isTopDown = !isTopDown;
        SetCamera(isTopDown);
    }

    // 也可以从 UI 按钮或其他脚本直接调用这两个方法
    public void SwitchToTPS() => SetCamera(false);
    public void SwitchToTopDown() => SetCamera(true);

    private void SetCamera(bool topDown)
    {
        // Brain 会自动 blend 到 Priority 更高的那个
        tpsCamera.Priority = topDown ? 10 : 11;
        topDownCamera.Priority = topDown ? 11 : 10;
    }
}