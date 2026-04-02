using UnityEngine;

public class PlayerSensor : MonoBehaviour
{
    // 指定可以被搬运的障碍物层级
    public LayerMask pushableLayer;
    // 检测到的障碍物的法线/与玩家正前方的夹角
    private float angleThreshold = 45f;
    private Vector3 HitNormal;
    [Header("可被推动物体的最低高度")]
    public float pushableObjectHeight= 0.8f;
    private float checkDistance = 1f;
    // 定义一个方法用于检测玩家前方是否有可被推动的物体,输入包含玩家的Transform组件和玩家的前进方向
    public MovingObject MovingObjectCheck(Transform playerTransform, Vector3 inputDirection)
    {
        // 从玩家位置出发，沿着输入方向发出一条射线，长度为checkDistance
        if(Physics.Raycast(playerTransform.position + Vector3.up * pushableObjectHeight, playerTransform.forward, out RaycastHit hit, checkDistance, pushableLayer))
        {
            HitNormal = hit.normal;
            // 计算物体法线与玩家位置和前进方向的夹角
            if (Vector3.Angle(- HitNormal, playerTransform.forward) > angleThreshold || Vector3.Angle(- HitNormal, inputDirection) > angleThreshold)
            {
                // 如果夹角大于设定的阈值，则认为玩家不在物体的正前方，返回null
                return null;
            }
            // 如果夹角小于等于设定的阈值，则认为玩家在物体的正前方，返回该物体的MovingObject组件
            MovingObject movableObject;
            if (hit.collider.TryGetComponent<MovingObject>(out movableObject))
            { 
                return movableObject;
            }
        }
        return null;
    }
}
