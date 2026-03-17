using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.PlayerSettings;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private float movementX;
    private float movementY;
    private int count;
    public float speed;

    public Camera sensorCam;
    public RenderTexture shadowRT;
    public float deathThreshold = 0.1f;

    private Texture2D tex2D;
    private Vector3 lastSafePosition;

    public TextMeshProUGUI countText;
    public GameObject winText;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        count = 0;
        SetCountText();
        winText.SetActive(false);
        // 建议使用 RGB24，不需要 Alpha 通道
        tex2D = new Texture2D(1, 1, TextureFormat.RGB24, false);
        lastSafePosition = transform.position;
    }

    void SetCountText()
    {
        countText.text = "Count: " + count.ToString();
        if (count >= 12) winText.SetActive(true);
    }

    void OnMove(InputValue movementValue)
    {
        Vector2 movementVector = movementValue.Get<Vector2>();
        movementX = movementVector.x;
        movementY = movementVector.y;
    }

    private void FixedUpdate()
    {
        // 创建一个输入方向上的向量
        Vector3 movement = new Vector3(movementX, 0.0f, movementY);
        // 确定玩家所代表的球的半径
        float r = 0.5f;
        Vector3[] checkPoints = GetCheckPoints(transform.position, r);
        // 对于数组中的所有位置进行判定
        foreach (Vector3 posBox in checkPoints)
        { // 如果判断点位中有点位处于阴影，返回
            if (IsPositionInShadow(posBox) == true)
            {
                // 将玩家的线速度与角速度同时归零
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                // 并将玩家位置返回上一帧安全位置
                transform.position = lastSafePosition;
                return;
            }
        }
        lastSafePosition = transform.position;
        // 当前帧存储的安全位置，在对于进行下一帧物体是否可以运动提供了参考
        // 只有下一帧检测箱点位同时通过时才可以运动 
        // 如果运动方向上存在移动,有
        if (movement.magnitude > 0.01f)
        {  
            // 根据速度大小进行下一帧位置的动态判断
            float dynamicBuffer = speed * Time.fixedDeltaTime + 0.15f;
            Vector3 nextPos = transform.position + movement.normalized * dynamicBuffer;
            Vector3[] nextCheckPoints = GetCheckPoints(nextPos, r);
            foreach (Vector3 posBox in nextCheckPoints)
            { 
                // 如果判断点位中有点位处于阴影，返回
                if (IsPositionInShadow(posBox) == true)
                {
                    // 将玩家的线速度与角速度同时归零
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    return;
                }
                // 下一帧判断点位全部处于光照区，可施加运动
            }
            rb.AddForce(movement * speed);
        }
    }

    // 创建一个用于点位判断的向量数组
    private Vector3[] GetCheckPoints(Vector3 center, float r)
    {
        return new Vector3[]{
   // 涵盖中心正左右上下及右上右下左上左下，总计九个测试点位
      center,
      center + new Vector3(r, 0, 0), center + new Vector3(-r, 0, 0),
      center + new Vector3(0, 0, r), center + new Vector3(0, 0, -r),
      center + new Vector3(r, 0, r), center + new Vector3(r, 0, -r),
      center + new Vector3(-r, 0, r), center + new Vector3(-r, 0, -r),
   };
}

    private bool IsPositionInShadow(Vector3 worldpos)
    {
        Vector3 viewportpos = sensorCam.WorldToViewportPoint(worldpos);
        // 如果玩家处在视野范围内，有：
        if (viewportpos.x >= 0 && viewportpos.x <= 1 && viewportpos.y >= 0 && viewportpos.y <= 1)
        {
            // 设置一个临时值，用于在每次调用完RenderTexture后刷新上一帧的RenderTexture上的Pixels
            RenderTexture old_rt = RenderTexture.active;
            // 使用RenderTexture.active设置当前活跃的RenderTexture, 使用的是挂载在相机上的RenderTexture
            RenderTexture.active = shadowRT;
            // 写好ReadPixels所需要的参数，使用ReadPixels()从当前的渲染目标读取像素值，并写入到tex中
            // 定义一个Rect用来表示需要从原RenderTexture中读取的区域
            // 即玩家坐标的那块像素，Rect(x, y, extended width, extended height)
            Rect regionToReadFrom = new Rect((int)(viewportpos.x * shadowRT.width), (int)(viewportpos.y * shadowRT.height), 1, 1);
            // 定义需要被复制像素的texture2d坐标的起始点
            int xPosToWriteTo = 0;
            int yPosToWriteTo = 0;
            // 写入像素
            tex2D.ReadPixels(regionToReadFrom, xPosToWriteTo, yPosToWriteTo);
            tex2D.Apply();
            // 销毁上一帧中所使用的RenderTexture, 将当前活跃的texture设为old_rt
            RenderTexture.active = old_rt;
            // 使用GetPixels获取该像素上的颜色，已知red(1.0f, 0.0f, 0.0f), 而black(0.0f, 0.0f, 0.0f)
            // 因此这里只需要判断(r, g, b)中的r是否为0
            // 如果为零，则说明是黑色，玩家处在阴影中，返回该值
            Color color = tex2D.GetPixel(0, 0);
            return color.r < deathThreshold;
        }
        // 不在阴影内和不在相机视野内的情况则返回false
        return false;
    }
    /* private void OnTriggerEnter(Collider other)
     {
         if (other.gameObject.CompareTag("PickUp"))
         {
             other.gameObject.SetActive(false);
             count++;
             SetCountText();
         }
     }*/
}