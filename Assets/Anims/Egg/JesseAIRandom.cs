using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;

public class JesseAIRandom : MonoBehaviour {
    public float radius = 10f;
    private NavMeshAgent agent; // NavMesh代理
    private Animator animator; // 添加Animator引用

    void Start() {
        agent = GetComponent<NavMeshAgent>(); // 获取NavMesh Agent组件
        animator = GetComponent<Animator>(); // 获取Animator组件
        StartCoroutine(MoveRandomly()); // 开始随机移动的协程
    }

    IEnumerator MoveRandomly() {
        while (true) {
            int action = GenerateRandomNumber(new float[] { 600, 1, 40, 20, 20 });
            //Debug.Log(action);
            //初始化动画状态
            animator.SetBool("Running", false);
            animator.SetInteger("IdleState", 0);
            switch (action) {
                case 0: //行走
                    animator.SetBool("Running", true); // 开始播放动画
                    //MoveToRandomPosition(); // 移动到随机位置
                    // 等待直到角色到达目的地
                    //while (!HasReachedDestination()) {
                    //    yield return null; // 等待下一帧
                    //}
                    yield return StartCoroutine(MoveToRandom()); // 执行移动协程并等待它完成
                    break;
                case 1: //发呆
                    // 随机等待时间
                    float waitTime = UnityEngine.Random.Range(0.5f, 3f);
                    yield return new WaitForSeconds(waitTime);
                    break;
                case 2: //跳一跳
                    animator.SetInteger("IdleState", 1);
                    yield return new WaitForSeconds(1f);
                    animator.SetInteger("IdleState", 0);
                    yield return new WaitForSeconds(5f);
                    break;
                case 3: //动耳朵1
                    animator.SetInteger("IdleState", 2);
                    yield return new WaitForSeconds(1f);
                    animator.SetInteger("IdleState", 0);
                    yield return new WaitForSeconds(5f);
                    break;
                case 4: //动耳朵2
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
    float stuckThreshold = 3f; // 几秒内几乎没动就认为卡住
    float moveCheckInterval = 0.2f;

    IEnumerator MoveToRandom() {
        Vector3 target = GetBalancedRandomNavPoint(transform.position, radius);
        if (!agent.isOnNavMesh) {
            //DebugText3D.Show(transform, "❌ Not on NavMesh", 2f); 
            yield break;
        }
        agent.SetDestination(target);
        lastPosition = transform.position;
        stuckTimer = 0f;

        if (!agent.isOnNavMesh) {
            yield break;
        }

        // 等路径完成
        yield return new WaitUntil(() => !agent.pathPending);

        while (agent.remainingDistance > 0.02f) {
            if (Vector2.Distance(new Vector2(transform.position.x, transform.position.z),
                                 new Vector2(lastPosition.x, lastPosition.z)) < 0.05f) {
                stuckTimer += Time.deltaTime;
                if (stuckTimer > stuckThreshold) {
                    Debug.LogWarning("卡住了，重新导航！");
                    agent.ResetPath();
                    yield break;
                }
            } else {
                stuckTimer = 0f;
                lastPosition = transform.position;
            }

            yield return new WaitForSeconds(moveCheckInterval);
        }

        // 正常完成导航
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
            return origin; // 所有点都无法导航，返回原地

        // 计算平均方向
        Vector3 averageDir = Vector3.zero;
        foreach (var dir in validDirs)
            averageDir += dir;
        averageDir.Normalize();

        // 找最接近平均方向的那个点
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
        // 首先确保没有待处理的路径并且路径是有效的
        if (!agent.pathPending && agent.pathStatus == NavMeshPathStatus.PathComplete) {
            // 如果路径有效，检查剩余距离是否小于停止距离
            return agent.remainingDistance <= agent.stoppingDistance;
        }

        // 如果路径还在计算中，或者路径不完整或无效，那么我们尚未到达目的地
        return false;

        // 检查是否还有路径以及是否已经到达（剩余距离是否小于某个阈值，这里使用agent.stoppingDistance）
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