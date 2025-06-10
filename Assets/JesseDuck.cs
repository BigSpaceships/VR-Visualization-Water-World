using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class JesseDuck : MonoBehaviour {
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
            int action = GenerateRandomNumber(new float[] { 2, 20, 20, 20, 20, 20, 20 });
            //Debug.Log(action);
            //初始化动画状态
            animator.SetInteger("IdleState", 0);
            switch (action) {
                case 0: //发呆
                    // 随机等待时间
                    float waitTime = Random.Range(0.5f, 3f);
                    yield return new WaitForSeconds(waitTime);
                    break;
                case 1: //动画1
                    animator.SetInteger("IdleState", 1);
                    yield return new WaitForSeconds(0.2f);
                    animator.SetInteger("IdleState", 0);
                    //yield return new WaitForSeconds(2f);
                    break;
                case 2: //动画2
                    animator.SetInteger("IdleState", 2);
                    yield return new WaitForSeconds(0.2f);
                    animator.SetInteger("IdleState", 0);
                    //yield return new WaitForSeconds(2f);
                    break;
                case 3: //动画3
                    animator.SetInteger("IdleState", 3);
                    yield return new WaitForSeconds(0.2f);
                    animator.SetInteger("IdleState", 0);
                    //yield return new WaitForSeconds(2f);
                    break;
                case 4: //动画4
                    animator.SetInteger("IdleState", 4);
                    yield return new WaitForSeconds(0.2f);
                    animator.SetInteger("IdleState", 0);
                    //yield return new WaitForSeconds(2f);
                    break;
                case 5: //动画5
                    animator.SetInteger("IdleState", 5);
                    yield return new WaitForSeconds(0.2f);
                    animator.SetInteger("IdleState", 0);
                    //yield return new WaitForSeconds(2f);
                    break;
                case 6: //动画6
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
        Vector3 randomDirection = Random.insideUnitSphere * radius; // 获取随机方向和长度
        randomDirection += transform.position; // 确保它是相对于角色当前位置的

        NavMeshHit hit; // 用于存储NavMesh采样结果的变量
        Vector3 finalPosition = Vector3.zero; // 初始化最终位置

        //找到最近的NavMesh位置
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1)) {
            finalPosition = hit.position;
        }

        agent.SetDestination(finalPosition); // 命令NavMeshAgent移动到最终位置
    }

    void Update() {
    }

    bool HasReachedDestination() {
        // 检查是否还有路径以及是否已经到达（剩余距离是否小于某个阈值，这里使用agent.stoppingDistance）
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