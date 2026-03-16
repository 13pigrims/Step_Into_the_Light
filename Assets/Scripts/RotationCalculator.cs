using UnityEngine;

public class RotationCalculator : MonoBehaviour
{
    [Tooltip("拖入你的Directional Light")]
    public Light SunLight;
private Vector3 LastPosition;
    public float speed = 5.0f;
    void Start()
    {
        LastPosition = transform.position;
    }

    private void LateUpdate()
    {
        // 判断是否产生位移
        if (transform.position != LastPosition)
        {
            // 以光源为中心，当前帧的target.position为终点，计算局部空间正交基并转换为四元数
            Vector3 direction = transform.position - SunLight.transform.position;
            Quaternion targetRot = Quaternion.LookRotation(direction);
            // 光源的transform不会实时变换，所以此时的SunLight.transform.rotation依旧是上一帧的
            SunLight.transform.rotation = Quaternion.Slerp(SunLight.transform.rotation,
            targetRot, Time.deltaTime * speed);
        }
        // 更新位置
        LastPosition = transform.position;
    }
}
