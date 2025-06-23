using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class JesseDuck : MonoBehaviour {
    public float radius = 10f;
    private NavMeshAgent agent; // NavMesh����
    private Animator animator; // ����Animator����

    void Start() {
        agent = GetComponent<NavMeshAgent>(); // ��ȡNavMesh Agent���
        animator = GetComponent<Animator>(); // ��ȡAnimator���
        StartCoroutine(MoveRandomly()); // ��ʼ����ƶ���Э��
    }

    IEnumerator MoveRandomly() {
        while (true) {
            int action = GenerateRandomNumber(new float[] { 2, 20, 20, 20, 20, 20, 20 });
            //Debug.Log(action);
            //��ʼ������״̬
            animator.SetInteger("IdleState", 0);
            switch (action) {
                case 0: //����
                    // ����ȴ�ʱ��
                    float waitTime = Random.Range(0.5f, 3f);
                    yield return new WaitForSeconds(waitTime);
                    break;
                case 1: //����1
                    animator.SetInteger("IdleState", 1);
                    yield return new WaitForSeconds(0.2f);
                    animator.SetInteger("IdleState", 0);
                    //yield return new WaitForSeconds(2f);
                    break;
                case 2: //����2
                    animator.SetInteger("IdleState", 2);
                    yield return new WaitForSeconds(0.2f);
                    animator.SetInteger("IdleState", 0);
                    //yield return new WaitForSeconds(2f);
                    break;
                case 3: //����3
                    animator.SetInteger("IdleState", 3);
                    yield return new WaitForSeconds(0.2f);
                    animator.SetInteger("IdleState", 0);
                    //yield return new WaitForSeconds(2f);
                    break;
                case 4: //����4
                    animator.SetInteger("IdleState", 4);
                    yield return new WaitForSeconds(0.2f);
                    animator.SetInteger("IdleState", 0);
                    //yield return new WaitForSeconds(2f);
                    break;
                case 5: //����5
                    animator.SetInteger("IdleState", 5);
                    yield return new WaitForSeconds(0.2f);
                    animator.SetInteger("IdleState", 0);
                    //yield return new WaitForSeconds(2f);
                    break;
                case 6: //����6
                    animator.SetInteger("IdleState", 6);
                    yield return new WaitForSeconds(0.2f);
                    animator.SetInteger("IdleState", 0);
                    //yield return new WaitForSeconds(2f);
                    break;
                default: break;
            }
        }
    }

    void MoveToRandomPosition() {
        Vector3 randomDirection = Random.insideUnitSphere * radius; // ��ȡ�������ͳ���
        randomDirection += transform.position; // ȷ����������ڽ�ɫ��ǰλ�õ�

        NavMeshHit hit; // ���ڴ洢NavMesh��������ı���
        Vector3 finalPosition = Vector3.zero; // ��ʼ������λ��

        //�ҵ������NavMeshλ��
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1)) {
            finalPosition = hit.position;
        }

        agent.SetDestination(finalPosition); // ����NavMeshAgent�ƶ�������λ��
    }

    bool HasReachedDestination() {
        // ����Ƿ���·���Լ��Ƿ��Ѿ����ʣ������Ƿ�С��ĳ����ֵ������ʹ��agent.stoppingDistance��
        return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
    }

    public int GenerateRandomNumber(float[] probabilities) {
        float totalProbability = 0;
        foreach (var probability in probabilities) {
            totalProbability += probability;
        }

        float randomPoint = UnityEngine.Random.value * totalProbability;

        float cumulativeProbability = 0;
        for (int i = 0; i < probabilities.Length; i++) {
            cumulativeProbability += probabilities[i];
            if (randomPoint < cumulativeProbability) {
                return i;
            }
        }
        return 0;
    }
}