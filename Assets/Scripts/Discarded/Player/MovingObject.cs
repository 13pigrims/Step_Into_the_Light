using UnityEngine;

public class MovingObject : MonoBehaviour
{
    public Transform[] interactivePoints;
    // 声明一个返回值为Transform的方法，用以返回距离玩家坐标最近的交互点
    public Transform GetInteractPoint(Transform playerTransform)
    { 
        Transform interactPoint = null;
        // 声明一个变量用于存储当前最近的距离，由于还没有遍历过交互点，所以初始值设为无穷大
        float closestDistance = Mathf.Infinity;
        // 遍历所有交互点，计算每个交互点与玩家坐标之间的距离
        foreach (var point in interactivePoints)
        {
            float dis = Vector3.Distance(point.position, transform.position);
            // 如果有当前点小于最小距离，更新最小距离和最近交互点
            if (dis < closestDistance)
            { 
                closestDistance = dis;
                interactPoint = point;
            }
        }
        return interactPoint;
    }
}
