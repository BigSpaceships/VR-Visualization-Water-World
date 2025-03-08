using UnityEngine;

public class MoveBetweenPoints : MonoBehaviour {
    public Transform waypointA;  // 指定起点
    public Transform waypointB;  // 指定终点
    public float speed = 2f;     // 速度

    private Vector3 globalWaypointA; // 存储起点的世界坐标
    private Vector3 globalWaypointB; // 存储终点的世界坐标
    private bool movingToB = true;   // 方向

    void Start() {
        if (waypointA == null || waypointB == null) {
            Debug.LogError("请在 Inspector 中设置 waypointA 和 waypointB！");
            return;
        }

        // **记录世界坐标**
        globalWaypointA = waypointA.position;
        globalWaypointB = waypointB.position;
    }

    void Update() {
        // 目标位置（使用记录的世界坐标）
        Vector3 targetPosition = movingToB ? globalWaypointB : globalWaypointA;

        // 让 Fish 移动到目标位置
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        // 当接近目标点时，切换方向
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f) {
            movingToB = !movingToB;
        }
    }
}
