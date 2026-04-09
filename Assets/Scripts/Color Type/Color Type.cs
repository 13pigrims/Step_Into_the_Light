using UnityEngine;
using UnityEngine.UIElements;

public class ColorType
{
    public enum State { Colored, Monochrome };
    [SerializeField] private State _currentState;
    private Renderer _renderer;
    private Material _coloredMat;
    private Material _monochromeMat;
    /// <summary>
    /// 用于获取当前状态的函数和设置当前状态的函数
    /// </summary>
    /// <returns></returns>
    public State GetState() { return _currentState; }
    public void SetState(State state)
    {
        _currentState = state;
        ApplyShader();
    }
    /// <summary>
    /// 构造函数，用于初始化物体颜色
    /// </summary>
    /// <param name="renderer"></param>
    /// <param name="initialState"></param>
    public ColorType(Renderer renderer, State initialState, Material monochromeMat)
    {
        _renderer = renderer;
        _currentState = initialState;
        _coloredMat = renderer.sharedMaterial; // 保存原始材
        _monochromeMat = monochromeMat;
        ApplyShader();
    }
    /// <summary>
    /// 实际上应用着色器的函数，根据当前状态设置材质属性来切换颜色模式
    /// </summary>
    private void ApplyShader()
    {
        if (_renderer == null)
        {
            Debug.LogWarning("Renderer is null!");
            return;
        }
        _renderer.material = _currentState == State.Monochrome
            ? _monochromeMat
            : _coloredMat;

    }
    /// <summary>
    /// 根据状态切换颜色模式的函数
    /// </summary>
    public void Exchange()
    {
        // 当前状态为彩色则切换为黑白
        _currentState = (_currentState == State.Colored)
             ? State.Monochrome
             : State.Colored;
        Debug.Log($"Switched to {_currentState} mode.");
        //  应用当前状态
        ApplyShader();
    }
}
