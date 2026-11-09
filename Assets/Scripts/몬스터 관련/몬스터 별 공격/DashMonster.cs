using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class DashMonster : MonoBehaviour
{
    public NavMeshAgent agent;
    public MonsterSetting monsterSetting;
    public MonsterStatus monsterStatus;
    public float damage = 20f; // 돌진 공격 데미지량
    public float attackSpeed = 5f; // 공격 속도
    public float dashDistance = 20f; // 돌진 거리
    public float dashSpeed = 20f; // 돌진 속도
    public float dashDuration = 1.5f; // 돌진 시간
    public float stopAfterDash = 2f; // 돌진 후 정지 시간

    public bool isDash = false;
    public float attackTimer = 0f;

    void Start()
    {
        monsterSetting = GetComponent<MonsterSetting>();
        agent = GetComponent<NavMeshAgent>();
        monsterStatus = GetComponent<MonsterStatus>();
        damage = monsterStatus.attackPower;
        attackSpeed = monsterStatus.attackSpeed;
    }

    void Update()
    {
        if(isDash)
        {
            monsterStatus.attackPower = damage * 2;
        }
        else
        {
            monsterStatus.attackPower = damage;
        }

        // 플레이어가 있고, 플레이어와의 거리가 stoppingDistance 이내일 때
        if (monsterSetting.nearPlayer != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, monsterSetting.nearPlayer.transform.position);
            if (distanceToPlayer <= agent.stoppingDistance)
            {
                DashAttack();
            }
            else
            {
                attackTimer = 0f;
            }
        }
    }

    public void DashAttack()
    {
        attackTimer += Time.deltaTime;

        // 공격 중이 아닐 때만 회전 가능
        if (monsterSetting.nearPlayer != null && !isDash)
        {
            // 플레이어 방향으로 회전
            Vector3 directionToPlayer = (monsterSetting.nearPlayer.transform.position - transform.position).normalized;
            directionToPlayer.y = 0; // y축 회전 방지
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * monsterSetting.agent.angularSpeed);
        }

        if(attackTimer >= attackSpeed && !isDash)
        {
            //Debug.Log("범위 공격 시작");
            StartCoroutine(Dash());
            //attackTimer = 0f;
        }
    }

    private IEnumerator Dash()
    {
        monsterStatus.anim.SetBool("IsDash", true);
        isDash = true;

        // 돌진 방향 설정
        Vector3 dashDirection = (monsterSetting.nearPlayer.transform.position - transform.position).normalized;
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + (dashDirection * dashDistance);
        
        float dashTime = 0f;
        while (dashTime < dashDuration)
        {
            // 현재까지 이동한 거리 계산
            float distanceTraveled = Vector3.Distance(startPosition, transform.position);
            
            // 목표 거리에 도달했는지 확인
            if (distanceTraveled >= dashDistance)
            {
                break;
            }

            // 지정된 속도로 이동
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, dashSpeed * Time.deltaTime);
            
            dashTime += Time.deltaTime;
            yield return null;
        }

        monsterStatus.anim.SetBool("IsDash", false);
        yield return new WaitForSeconds(stopAfterDash);
        // 이동이 완전히 끝난 후 isDash를 false로 설정
        attackTimer = 0f;
        isDash = false;
    }

    void OnDisable()
    {
        
    }
}