using UnityEngine;
using UnityEngine.Rendering; // 必须引用

public class ShadowLogicPro : MonoBehaviour
{
    public Camera sensorCamera;
    public RenderTexture shadowRT;

    [Header("判定参数")]
    public float shadowThreshold = 0.1f; // 低于这个值认为是影子
    public bool isPlayerInShadow { get; private set; }

    void Update()
    {
        // 1. 获取玩家在相机视口中的比例坐标 (0到1之间)
        Vector3 viewportPos = sensorCamera.WorldToViewportPoint(transform.position);

        // 2. 检查玩家是否在相机画面内
        if (viewportPos.x >= 0 && viewportPos.x <= 1 && viewportPos.y >= 0 && viewportPos.y <= 1)
        {
            RequestAsyncReadback(viewportPos.x, viewportPos.y);
        }
    }

    void RequestAsyncReadback(float u, float v)
    {
        // 计算目标像素坐标
        int x = Mathf.Clamp((int)(u * shadowRT.width), 0, shadowRT.width - 1);
        int y = Mathf.Clamp((int)(v * shadowRT.height), 0, shadowRT.height - 1);

        // 发起异步读取请求，只读取玩家所在位置的 1x1 像素
        AsyncGPUReadback.Request(shadowRT, 0, x, 1, y, 1, 0, 1, (AsyncGPUReadbackRequest request) => {
            if (request.hasError) return;

            // 获取读取到的数据
            var data = request.GetData<Color32>();
            if (data.Length > 0)
            {
                // 如果红色分量很低，说明采样到了黑影
                isPlayerInShadow = data[0].r < (shadowThreshold * 255);

                if (isPlayerInShadow)
                {
                    OnEnterShadow();
                }
            }
        });
    }

    void OnEnterShadow()
    {
        Debug.Log("踩到影子了！触发逻辑...");
        // 执行死亡重置或物理反馈
    }
}