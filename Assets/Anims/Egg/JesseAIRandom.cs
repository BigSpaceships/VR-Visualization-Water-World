using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;

public class JesseAIRandom : MonoBehaviour {
    public float radius = 10f;
    private NavMeshAgent agent; // NavMesh����
    private Animator animator; // ���Animator����

    void Start() {
        agent = GetComponent<NavMeshAgent>(); // ��ȡNavMesh Agent���
        animator = GetComponent<Animator>(); // ��ȡAnimator���
        StartCoroutine(MoveRandomly()); // ��ʼ����ƶ���Э��
    }

    IEnumerator MoveRandomly() {
        while (true) {
            int action = GenerateRandomNumber(new float[] { 600, 1, 40, 20, 20 });
            //Debug.Log(action);
            //��ʼ������״̬
            animator.SetBool("Running", false);
            animator.SetInteger("IdleState", 0);
            switch (action) {
                case 0: //����
                    animator.SetBool("Running", true); // ��ʼ���Ŷ���
                    //MoveToRandomPosition(); // �ƶ������λ��
                    // �ȴ�ֱ����ɫ����Ŀ�ĵ�
                    //while (!HasReachedDestination()) {
                    //    yield return null; // �ȴ���һ֡
                    //}
                    yield return StartCoroutine(MoveToRandom()); // ִ���ƶ�Э�̲��ȴ������
                    break;
                case 1: //����
                    // ����ȴ�ʱ��
                    float waitTime = UnityEngine.Random.Range(0.5f, 3f);
                    yield return new WaitForSeconds(waitTime);
                    break;
                case 2: //��һ��
                    animator.SetInteger("IdleState", 1);
                    yield return new WaitForSeconds(1f);
                    animator.SetInteger("IdleState", 0);
                    yield return new WaitForSeconds(5f);
                    break;
                case 3: //������1
                    animator.SetInteger("IdleState", 2);
                    yield return new WaitForSeconds(1f);
                    animator.SetInteger("IdleState", 0);
                    yield return new WaitForSeconds(5f);
                    break;
                case 4: //������2
                    animator.SetInteger("IdleState", 3);
                    yield return new WaitForSeconds(1f);
                    animator.SetInteger("IdleState", 0);
                    yield return new WaitForSeconds(5f);
                    break;
                default: break;
            }
        }
    }

    void MoveToRandomPosition() {
        agent.SetDestination(GetBalancedRandomNavPoint(transform.position, radius)); 
    }

    Vector3 lastPosition;
    float stuckTimer = 0f;
    float stuckThreshold = 3f; // �����ڼ���û������Ϊ��ס
    float moveCheckInterval = 0.2f;

    IEnumerator MoveToRandom() {
        Vector3 target = GetBalancedRandomNavPoint(transform.position, radius);
        if (!agent.isOnNavMesh) {
            Debug.LogWarning($"{gameObject.name} ���� NavMesh �ϣ���������");
            yield break;
        }
        agent.SetDestination(target);
        lastPosition = transform.position;
        stuckTimer = 0f;

        if (!agent.isOnNavMesh) {
            yield break;
        }

        // ��·�����
        yield return new WaitUntil(() => !agent.pathPending);

        while (agent.remainingDistance > 0.02f) {
            if (Vector2.Distance(new Vector2(transform.position.x, transform.position.z),
                                 new Vector2(lastPosition.x, lastPosition.z)) < 0.05f) {
                stuckTimer += Time.deltaTime;
                if (stuckTimer > stuckThreshold) {
                    Debug.LogWarning("��ס�ˣ����µ�����");
                    agent.ResetPath();
                    yield break;
                }
            } else {
                stuckTimer = 0f;
                lastPosition = transform.position;
            }

            yield return new WaitForSeconds(moveCheckInterval);
        }

        // ������ɵ���
        yield break;
    }

    Vector3 GetBalancedRandomNavPoint(Vector3 origin, float radius, int sampleCount = 60) {
        List<Vector3> validPoints = new List<Vector3>();
        List<Vector3> validDirs = new List<Vector3>();
        NavMeshPath path = new NavMeshPath();

        for (int i = 0; i < sampleCount; i++) {
            Vector2 random2D = UnityEngine.Random.insideUnitCircle * radius;
            Vector3 candidate = origin + new Vector3(random2D.x, 0, random2D.y);

            if (NavMesh.CalculatePath(origin, candidate, NavMesh.AllAreas, path) &&
                path.status == NavMeshPathStatus.PathComplete) {
                validPoints.Add(candidate);
                validDirs.Add((candidate - origin).normalized);
            }
        }

        if (validPoints.Count == 0)
            return origin; // ���е㶼�޷�����������ԭ��

        // ����ƽ������
        Vector3 averageDir = Vector3.zero;
        foreach (var dir in validDirs)
            averageDir += dir;
        averageDir.Normalize();

        // ����ӽ�ƽ��������Ǹ���
        float maxDot = -1f;
        int bestIndex = 0;

        for (int i = 0; i < validDirs.Count; i++) {
            float dot = Vector3.Dot(validDirs[i], averageDir);
            if (dot > maxDot) {
                maxDot = dot;
                bestIndex = i;
            }
        }

        return validPoints[bestIndex];
    }


    void Update() {
    }

    bool HasReachedDestination() {
        // ����ȷ��û�д������·������·������Ч��
        if (!agent.pathPending && agent.pathStatus == NavMeshPathStatus.PathComplete) {
            // ���·����Ч�����ʣ������Ƿ�С��ֹͣ����
            return agent.remainingDistance <= agent.stoppingDistance;
        }

        // ���·�����ڼ����У�����·������������Ч����ô������δ����Ŀ�ĵ�
        return false;

        // ����Ƿ���·���Լ��Ƿ��Ѿ����ʣ������Ƿ�С��ĳ����ֵ������ʹ��agent.stoppingDistance��
        //return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
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