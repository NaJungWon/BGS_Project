using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class RushMonster : MonoBehaviour
{
    public MonsterStatus monsterStatus;
    public MonsterSetting monsterSetting;
    public NavMeshAgent agent;
    public Animator monsterAnim;
    public float attackAnimSpeed = 2f; // 공격 애니메이션 재생 속도
    public float minSpeed = 1f; // 최소 속도
    public float maxSpeed = 10f; // 최대 속도
    public float acceleration = 1f; // 속도 증가 속도
    public float speedIncrease = 0.1f; // 속도 증가량
    public float afterHitRushTime = 1f; // 충돌 후 추가 이동 시간
    public float hitDelay = 2f; // 충돌 딜레이
    private bool hasHitPlayer = false;

    private bool isCharging = false;
    private bool canHit = true;

    void Start()
    {
        // 필요한 컴포넌트들을 가져옴
        agent = GetComponent<NavMeshAgent>();
        monsterSetting = GetComponent<MonsterSetting>();
        monsterStatus = GetComponent<MonsterStatus>();
        monsterAnim = GetComponent<Animator>();
        minSpeed = monsterStatus.moveSpeed;
        // 초기 상태 설정
        isCharging = true;
        canHit = true;
    }

    void FixedUpdate()
    {
        if (canHit && !hasHitPlayer)
        {
            // 점진적으로 속도 증가
            agent.speed = Mathf.Min(agent.speed + speedIncrease * Time.deltaTime, maxSpeed);
        }
        else
        {
            agent.speed = minSpeed;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!canHit || hasHitPlayer) return;

        if (other.CompareTag("Player"))
        {   
            StartCoroutine(RushToHitPlayer());
            // 플레이어에게 데미지와 효과 적용
            var rb = other.GetComponent<Rigidbody>();
            var ctrl = other.GetComponent<PlayerController>();
            
            if (rb != null && ctrl != null)
            {
                // 플레이어를 위로 튕겨올리고 앞으로 밀어냄
                Vector3 launchForce = Vector3.up * 10f + transform.forward * 2f;
                ctrl.LaunchPlayer(launchForce, 1.5f);
                // 슬로우 효과 적용
                ctrl.ApplySlow(0.5f, 2f);
            }
        }
    }

    IEnumerator RushToHitPlayer()
    {
        monsterStatus.anim.SetTrigger("IsAttack");
        monsterStatus.anim.SetFloat("AttackSpeed", attackAnimSpeed);  // 공격 애니메이션 속도만 설정
        // 애니메이션이 실제로 "IsAttack" 상태로 전환될 때까지 기다림
        yield return new WaitUntil(() => monsterStatus.anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"));
        monsterStatus.isAttack = true;
        canHit = false;
        hasHitPlayer = true;

        // agent 움직임 멈춤
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        yield return new WaitForSeconds(hitDelay);

        // 애니메이션이 종료될 때까지 기다림
        yield return new WaitUntil(() => !monsterStatus.anim.GetCurrentAnimatorStateInfo(0).IsName("IsAttack"));
        monsterStatus.isAttack = false;
        canHit = true;
        hasHitPlayer = false;
        // agent 움직임 재개
        agent.isStopped = false;
    }
}
