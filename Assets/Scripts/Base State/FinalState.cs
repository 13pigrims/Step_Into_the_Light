using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 通关物体：角色碰到后消失、播放胜利音效、弹出通关面板
/// 层级结构：挂载 FinalState 的空物体 → 子物体 Prefab（视觉模型）
/// 受 ObeliskButton 影响变色
/// </summary>
public class FinalState : BaseState
{
    [Header("旋转动画")]
    [Tooltip("子物体倾斜角度")]
    [SerializeField] private float tiltAngle = 45f;

    [Tooltip("旋转一圈的时间（秒）")]
    [SerializeField] private float rotationPeriod = 5f;

    // 子物体（视觉模型）
    private Transform _childModel;

    // 是否已被收集
    private bool _isCollected = false;

    protected override void Awake()
    {
        base.Awake();
        // 让子物体的 Trigger 事件传递到这里
        var rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void Start()
    {
        // 获取子物体（第一个子物体作为视觉模型）
        if (transform.childCount > 0)
        {
            // 这里可以这样获取子物体吗？
            _childModel = transform.GetChild(0);
            // 倾斜子物体
            _childModel.localRotation = Quaternion.Euler(tiltAngle, 0f, 0f);
        }
        else
        {
            Debug.LogWarning($"[FinalState] {name} 没有子物体，旋转动画将不生效。");
        }
    }

    private void Update()
    {
        if (_isCollected || _childModel == null) return;

        // 顺时针旋转（绕 Y 轴，在倾斜的基础上叠加）
        float degreesPerSecond = 360f / rotationPeriod;
        _childModel.Rotate(Vector3.up, -degreesPerSecond * Time.deltaTime, Space.World);
    }

    // ========== ObeliskButton 变色 ==========

    public override void Initialize(ButtonManager buttonManager)
    {
        _buttonManager = buttonManager;
        buttonManager.OnObeliskPressed += HandleButtonPressed;
        buttonManager.OnObeliskReleased += HandleButtonReleased;
    }

    public override void HandleButtonPressed()
    {
        ExchangeColor();
    }

    public override void HandleButtonReleased()
    {
        ExchangeColor();
    }

    // ========== 通关触发 ==========

    /// <summary>
    /// 角色进入 Trigger 区域时触发
    /// 需要角色有 Collider + Rigidbody(Kinematic)，且 Tag 为 "Player"
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (_isCollected) return;

        if (other.CompareTag("Player"))
        {
            // Monochrome 状态下无法收集
            if (CurrentColor.GetState() == ColorType.State.Monochrome)
                return;
            _isCollected = true;
            Debug.Log($"[FinalState] 角色收集了 {name}，通关！");

            // 播放胜利音效
            var audio = GameRoot.GetInstance().AudioManager_Root;
            var winClip = GameRoot.GetInstance().WinClip;
            if (winClip != null && audio != null)
                audio.PlaySFX(winClip);

            // 隐藏子物体（表示已获取）
            if (_childModel != null)
                _childModel.gameObject.SetActive(false);

            // 弹出通关面板
            GameRoot.GetInstance().UIManager_Root.PushPanel(new WinPanel());
        }
    }

    // ========== 撤销支持 ==========

    public override void BackToPreviousState(GameStateSnapshot lastSnapshot)
    {
        // FinalState 不参与撤销（收集后不能撤销回来）
        // 如果需要支持撤销，可以在这里加逻辑
    }

    public override bool IsInteractive()
    {
        return false; // 不可被选中操作
    }

    protected override void OnDestroy()
    {
        if (_buttonManager != null)
        {
            _buttonManager.OnObeliskPressed -= HandleButtonPressed;
            _buttonManager.OnObeliskReleased -= HandleButtonReleased;
        }
    }
}