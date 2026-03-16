using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TopDownPlayer : MonoBehaviour
{
    public Light mainLight;

    // 核心组件
    private Rigidbody rb;
    private Animator animator;
    private Transform PlayerTransform;
    private PlayerSensor playerSensor;

    // 阴影与移动
    public float speed;
    private Vector3 lastSafePosition;
    private Vector2 PlayerInputVec;
    public float RotationSpeed = 360f;
    Vector3 PlayerMovement = Vector3.zero;

    float IdleSpeed = 1.5f;
    float WalkSpeed = 2f;
    float currentSpeed;
    float targetSpeed;

    // 状态参数
    bool IsWalk;
    bool IsPushPressed;
    bool pushStateChange;

    // UI 组件
    public TextMeshProUGUI countText;
    public GameObject winText;
    private int count = 0;

    // 交互对象
    Transform interactPoint;
    MovingObject movableObject;

    // 状态枚举
    public enum PlayerPosture { Stand, Push }
    public enum LocomotionState { Idle, Walk }

    PlayerPosture playerPosture = PlayerPosture.Stand;
    LocomotionState locomotionState = LocomotionState.Idle;

    void Start()
    {
        animator = GetComponent<Animator>();
        PlayerTransform = transform;
        rb = GetComponent<Rigidbody>();
        playerSensor = GetComponent<PlayerSensor>();

        SetCountText();
        winText.SetActive(false);
        lastSafePosition = transform.position;
    }

    void Update()
    {
        RotatePlayer();
        Push();
        SwitchPlayerState();
        SetupAnimator();
    }

    #region 输入处理
    public void OnPlayerMove(InputAction.CallbackContext ctx)
    {
        PlayerInputVec = ctx.ReadValue<Vector2>();
    }

    public void OnPlayeRun(InputAction.CallbackContext ctx)
    {
        IsWalk = ctx.ReadValue<float>() > 0;
    }

    public void GetPushInput(InputAction.CallbackContext ctx)
    {
        IsPushPressed = ctx.ReadValueAsButton();
    }
    #endregion

    void SwitchPlayerState()
    {
        switch (playerPosture)
        {
            case PlayerPosture.Stand:
                if (pushStateChange)
                {
                    playerPosture = PlayerPosture.Push;
                }
                break;
            case PlayerPosture.Push:
                if (pushStateChange)
                {
                    playerPosture = PlayerPosture.Stand;
                }
                break;
        }
        pushStateChange = false;
        locomotionState = PlayerInputVec.magnitude == 0 ? LocomotionState.Idle : LocomotionState.Walk;
    }

    void SetupAnimator()
    {
        animator.SetBool("推", playerPosture == PlayerPosture.Push);
    }

    void Push()
    {
        if (IsPushPressed && playerPosture == PlayerPosture.Stand)
        {
            float checkDistance = 5.0f;
            Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;

            if (Physics.Raycast(rayOrigin, transform.forward, out RaycastHit hit, checkDistance))
            {
                movableObject = hit.collider.GetComponent<MovingObject>();

                if (movableObject != null)
                {
                    pushStateChange = true;
                    transform.forward = -hit.normal;
                }
            }
        }
        else if (!IsPushPressed && playerPosture == PlayerPosture.Push)
        {
            pushStateChange = true;
            movableObject = null;
        }
    }

    void RotatePlayer()
    {
        if (playerPosture == PlayerPosture.Push) return;
        if (PlayerInputVec.Equals(Vector2.zero)) return;

        PlayerMovement.x = PlayerInputVec.x;
        PlayerMovement.z = PlayerInputVec.y;

        Quaternion TargetRotation = Quaternion.LookRotation(PlayerMovement, Vector3.up);
        PlayerTransform.rotation = Quaternion.RotateTowards(PlayerTransform.rotation, TargetRotation, RotationSpeed * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        Vector3 inputDir = new Vector3(PlayerInputVec.x, 0, PlayerInputVec.y);

        if (IsPositionInShadow(transform.position))
        {
            transform.position = lastSafePosition + Vector3.up * 0.05f;
            currentSpeed = 0;
            return;
        }

        targetSpeed = IsWalk ? WalkSpeed : IdleSpeed;

        if (playerPosture == PlayerPosture.Push)
        {
            MoveWhilePushing();
            return;
        }

        float finalTarget = (inputDir.magnitude > 0.01f) ? targetSpeed : 0f;
        currentSpeed = Mathf.Lerp(currentSpeed, finalTarget, 0.2f);

        if (currentSpeed > 0.01f)
        {
            Vector3 moveDir = transform.forward * currentSpeed * Time.fixedDeltaTime;
            Vector3 nextPos = transform.position + moveDir;

            bool canMove = true;
            // ... 阴影预测检测逻辑 ...

            if (canMove)
            {
                rb.MovePosition(nextPos);
                lastSafePosition = transform.position;
            }
            else
            {
                currentSpeed = 0;
            }
        }

        animator.SetFloat("Vertical Speed", currentSpeed);
    }

    private Vector3[] GetCheckPoints(Vector3 center, float r)
    {
        return new Vector3[] {
            center,
            center + new Vector3(r, 0, 0), center + new Vector3(-r, 0, 0),
            center + new Vector3(0, 0, r), center + new Vector3(0, 0, -r),
            center + new Vector3(r, 0, r), center + new Vector3(r, 0, -r),
            center + new Vector3(-r, 0, r), center + new Vector3(-r, 0, -r),
        };
    }

    private bool IsPositionInShadow(Vector3 worldpos)
    {
        Vector3 LightDir = mainLight.transform.forward;
        Vector3 PlayerRayDir = -LightDir;
        float rayLength = 50f;

        if (Physics.Raycast(worldpos + Vector3.up * 0.2f, PlayerRayDir, out RaycastHit hit, rayLength))
        {
            return true;
        }
        return false;
    }

    void MoveWhilePushing()
    {
        if (movableObject == null) return;

        float moveInput = PlayerInputVec.y;

        if (Mathf.Abs(moveInput) > 0.01f)
        {
            float pushSpeed = 1.2f;
            Vector3 moveDelta = transform.forward * moveInput * pushSpeed * Time.fixedDeltaTime;

            rb.MovePosition(rb.position + moveDelta);

            Rigidbody boxRb = movableObject.GetComponent<Rigidbody>();

            if (boxRb != null)
            {
                boxRb.MovePosition(boxRb.position + moveDelta);
            }
            else
            {
                movableObject.transform.position += moveDelta;
            }
        }

        animator.SetFloat("Vertical Speed", Mathf.Abs(moveInput));
    }

    void SetCountText()
    {
        countText.text = "Count: " + count.ToString();
        if (count >= 12) winText.SetActive(true);
    }
}