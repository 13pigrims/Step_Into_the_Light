using UnityEngine;

/// <summary>
/// 方向提示 UI：选中物体后在其四周显示可移动方向的箭头
/// 箭头在 3D 世界空间中跟随物体，只显示能走的方向
/// 挂在 LevelRoot 同一个 GameObject 上
/// </summary>
public class DirectionHintUI : MonoBehaviour
{
    [Header("箭头设置")]
    [Tooltip("箭头 Prefab（朝 +Z 方向的箭头模型）")]
    public GameObject arrowPrefab;

    [Tooltip("箭头距离物体中心的偏移距离")]
    public float arrowOffset = 0.7f;

    [Tooltip("箭头悬浮高度（Y 方向）")]
    public float arrowHeight = 0.05f;

    [Tooltip("箭头缩放")]
    public float arrowScale = 0.3f;

    [Header("动画")]
    [Tooltip("箭头上下浮动幅度")]
    public float bobAmplitude = 0.05f;

    [Tooltip("箭头上下浮动速度")]
    public float bobSpeed = 3f;

    // 四个方向的箭头
    private GameObject _arrowUp;    // +Z
    private GameObject _arrowDown;  // -Z
    private GameObject _arrowLeft;  // -X
    private GameObject _arrowRight; // +X

    // 箭头容器
    private Transform _arrowContainer;

    // 当前跟随的物体
    private BaseState _target;

    // 缓存 gridCellSize
    private float _gridCellSize = 1f;

    private void Start()
    {
        // 创建箭头容器
        _arrowContainer = new GameObject("DirectionHints").transform;
        _arrowContainer.SetParent(null);

        // 创建四个方向的箭头（Y 旋转角度让箭头尖指向远离物体的方向）
        _arrowUp = CreateArrow("Arrow_Up", Vector3.forward, 0f);
        _arrowDown = CreateArrow("Arrow_Down", Vector3.back, 180f);
        _arrowLeft = CreateArrow("Arrow_Left", Vector3.left, -90f);
        _arrowRight = CreateArrow("Arrow_Right", Vector3.right, 90f);

        // 初始隐藏
        HideAll();

        // 监听选中事件
        if (InputManager.Instance != null)
            InputManager.Instance.OnObjectSelected += OnObjectSelected;
    }

    /// <summary>
    /// 创建一个方向箭头
    /// </summary>
    private GameObject CreateArrow(string name, Vector3 direction, float yRotation)
    {
        GameObject arrow;

        if (arrowPrefab != null)
        {
            arrow = Instantiate(arrowPrefab, _arrowContainer);
        }
        else
        {
            // 没有 Prefab 时用简易几何体代替
            arrow = CreateDefaultArrow();
            arrow.transform.SetParent(_arrowContainer);
        }

        arrow.name = name;
        arrow.transform.localRotation = Quaternion.Euler(0f, yRotation, 0f);
        arrow.transform.localScale = Vector3.one * arrowScale;

        // 确保箭头不会被射线检测到
        SetLayerRecursive(arrow, LayerMask.NameToLayer("Ignore Raycast"));

        return arrow;
    }

    /// <summary>
    /// 没有 Prefab 时创建默认箭头（一个拉长的 Cube）
    /// </summary>
    private GameObject CreateDefaultArrow()
    {
        var arrow = new GameObject("DefaultArrow");

        // 箭头主体（拉长的方块）
        var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.transform.SetParent(arrow.transform);
        body.transform.localPosition = new Vector3(0f, 0f, 0.3f);
        body.transform.localScale = new Vector3(0.3f, 0.15f, 0.8f);
        Destroy(body.GetComponent<Collider>());

        // 箭头头部（三角形用旋转的方块模拟）
        var head = GameObject.CreatePrimitive(PrimitiveType.Cube);
        head.transform.SetParent(arrow.transform);
        head.transform.localPosition = new Vector3(0f, 0f, 0.85f);
        head.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
        head.transform.localScale = new Vector3(0.5f, 0.15f, 0.5f);
        Destroy(head.GetComponent<Collider>());

        // 设置半透明绿色材质
        var mat = new Material(Shader.Find("Standard"));
        mat.SetFloat("_Mode", 3); // Transparent
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
        mat.color = new Color(0.3f, 1f, 0.5f, 0.6f);

        body.GetComponent<Renderer>().sharedMaterial = mat;
        head.GetComponent<Renderer>().sharedMaterial = mat;

        return arrow;
    }

    private void OnObjectSelected(BaseState state)
    {
        _target = state;

        if (_target != null)
        {
            _gridCellSize = 1f;
            // 通过反射或直接访问获取 gridCellSize
            // BaseState 的 gridCellSize 是 protected，这里用默认值
            // 如果需要精确值，可以在 BaseState 里加一个 public getter
        }
    }

    private void LateUpdate()
    {
        if (_target == null || _target.IsMoving)
        {
            HideAll();
            return;
        }

        // 更新箭头位置到目标物体
        Vector3 basePos = _target.transform.position;
        float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
        float y = arrowHeight + bob;

        MoveAxis axis = _target.GetMoveAxis();

        // 纵向箭头（W/S → Z 方向）：Horizontal 模式下隐藏
        bool showVertical = (axis == MoveAxis.Vertical || axis == MoveAxis.All);
        UpdateArrow(_arrowUp, basePos, Vector3.forward, new Vector3(0, 0, -1), y, showVertical);
        UpdateArrow(_arrowDown, basePos, Vector3.back, new Vector3(0, 0, 1), y, showVertical);

        // 横向箭头（A/D → X 方向）：Vertical 模式下隐藏，方向修正
        bool showHorizontal = (axis == MoveAxis.Horizontal || axis == MoveAxis.All);
        UpdateArrow(_arrowLeft, basePos, Vector3.left, new Vector3(-1, 0, 0), y, showHorizontal);
        UpdateArrow(_arrowRight, basePos, Vector3.right, new Vector3(1, 0, 0), y, showHorizontal);
    }

    /// <summary>
    /// 更新单个箭头的位置和可见性
    /// </summary>
    private void UpdateArrow(GameObject arrow, Vector3 basePos, Vector3 visualDir, Vector3 moveDir, float y, bool axisAllowed)
    {
        if (!axisAllowed)
        {
            arrow.SetActive(false);
            return;
        }

        // 检测该方向是否可移动
        bool canMove = _target.canMoveOn(moveDir);

        arrow.SetActive(canMove);

        if (canMove)
        {
            arrow.transform.position = basePos + visualDir * arrowOffset + Vector3.up * y;
        }
    }

    private void HideAll()
    {
        if (_arrowUp != null) _arrowUp.SetActive(false);
        if (_arrowDown != null) _arrowDown.SetActive(false);
        if (_arrowLeft != null) _arrowLeft.SetActive(false);
        if (_arrowRight != null) _arrowRight.SetActive(false);
    }

    private void SetLayerRecursive(GameObject obj, int layer)
    {
        if (layer < 0) return;
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursive(child.gameObject, layer);
        }
    }

    private void OnDestroy()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.OnObjectSelected -= OnObjectSelected;

        if (_arrowContainer != null)
            Destroy(_arrowContainer.gameObject);
    }
}