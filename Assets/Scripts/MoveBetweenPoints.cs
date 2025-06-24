using UnityEngine;

public class MoveBetweenPoints : MonoBehaviour {
    public Transform waypointA;  // ָ�����
    public Transform waypointB;  // ָ���յ�
    public float speed = 2f;     // �ٶ�

    private Vector3 globalWaypointA; // �洢������������
    private Vector3 globalWaypointB; // �洢�յ����������
    private bool movingToB = true;   // ����

    void Start() {
        if (waypointA == null || waypointB == null) {
            //Debug.LogError("���� Inspector ������ waypointA �� waypointB��");
            return;
        }

        // **��¼��������**
        globalWaypointA = waypointA.position;
        globalWaypointB = waypointB.position;
    }

    void Update() {
        // Ŀ��λ�ã�ʹ�ü�¼���������꣩
        Vector3 targetPosition = movingToB ? globalWaypointB : globalWaypointA;

        // �� Fish �ƶ���Ŀ��λ��
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        // ���ӽ�Ŀ���ʱ���л�����
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f) {
            movingToB = !movingToB;
        }
    }
}
