using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraSwitch : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera isometricCamera;
    [SerializeField] private CinemachineVirtualCamera topDownCamera;

    private bool _isTopDown = false;
    // 创建一个InputAction用于绑定键位输入
    private InputAction _switchCameraAction;

    private void Awake()
    {
        // 初始化 InputAction，绑定到 "C" 键
        _switchCameraAction = new InputAction("SwitchCamera", binding: "<Keyboard>/tab");
        _switchCameraAction.performed += ctx => ToggleCamera();
    }

    public void ToggleCamera()
    {
        Debug.Log("切换相机");
        _isTopDown = !_isTopDown;
        SetCamera(_isTopDown);
    }

    // 也可以从 UI 按钮或其他脚本直接调用这两个方法
    public void SwitchToIsometric() => SetCamera(false);
    public void SwitchToTopDown() => SetCamera(true);

    private void SetCamera(bool topDown)
    {
        // Brain 会自动 blend 到 Priority 更高的那个
        isometricCamera.Priority = topDown ? 10 : 11;
        topDownCamera.Priority = topDown ? 11 : 10;
    }

    private void OnEnable()
    {
        _switchCameraAction.Enable();
    }

    private void OnDisable()
    {
        _switchCameraAction.Disable();
    }

    private void OnDestroy()
    {
        _switchCameraAction.Disable();
    }
}