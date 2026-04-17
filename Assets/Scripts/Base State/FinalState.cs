using System;
using System.Collections;
using UnityEngine;

public class FinalState : BaseState
{
    [Header("旋转动画")]
    [Tooltip("子物体倾斜角度")]
    [SerializeField] private float tiltAngle = 45f;

    [Tooltip("旋转一圈的时间（秒）")]
    [SerializeField] private float rotationPeriod = 5f;

    private Transform _childModel;
    private bool _isCollected = false;

    // 记录当前在 Trigger 内的角色
    private Collider _playerInside = null;

    protected override void Awake()
    {
        base.Awake();
        var rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void Start()
    {
        if (transform.childCount > 0)
        {
            _childModel = transform.GetChild(0);
            _childModel.localRotation = Quaternion.Euler(tiltAngle, 0f, 0f);
        }
    }

    private void Update()
    {
        if (_isCollected) return;

        // 旋转动画
        if (_childModel != null)
        {
            float degreesPerSecond = 360f / rotationPeriod;
            _childModel.Rotate(Vector3.up, -degreesPerSecond * Time.deltaTime, Space.World);
        }

        // 持续检测：玩家已经在 Trigger 内 + 颜色变为 Colored → 立即通关
        if (_playerInside != null && CurrentColor.GetState() == ColorType.State.Colored)
        {
            TriggerWin();
        }
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

    private void OnTriggerEnter(Collider other)
    {
        if (_isCollected || !other.CompareTag("Player")) return;
        _playerInside = other;

        if (CurrentColor.GetState() == ColorType.State.Colored)
            TriggerWin();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            _playerInside = null;
    }

    private void OnTriggerStay(Collider other)
    {
        if (_isCollected || !other.CompareTag("Player")) return;
        _playerInside = other;

        if (CurrentColor.GetState() == ColorType.State.Colored)
            TriggerWin();
    }

    private void TriggerWin()
    {
        if (_isCollected) return;
        _isCollected = true;

        Debug.Log($"[FinalState] 角色收集了 {name}，通关！");

        var audio = GameRoot.GetInstance().AudioManager_Root;
        var winClip = GameRoot.GetInstance().WinClip;
        if (winClip != null && audio != null)
            audio.PlaySFX(winClip);

        if (_childModel != null)
            _childModel.gameObject.SetActive(false);

        GameRoot.GetInstance().UIManager_Root.PushPanel(new WinPanel());
    }

    public override void BackToPreviousState(GameStateSnapshot lastSnapshot) { }
    public override bool IsInteractive() { return false; }

    protected override void OnDestroy()
    {
        if (_buttonManager != null)
        {
            _buttonManager.OnObeliskPressed -= HandleButtonPressed;
            _buttonManager.OnObeliskReleased -= HandleButtonReleased;
        }
    }
}