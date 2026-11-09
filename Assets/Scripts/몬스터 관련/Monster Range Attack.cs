using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterRangeAttack : MonoBehaviour
{
    public MonsterStatus monsterStatus;
    public MonsterSetting monsterSetting;
    public Animator monsterAnim;
    public NavMeshAgent agent;
    public GameObject projectile;
    public float attackDelay; // 공격 딜레이
    public float attackSpeed; // 공격 속도
    public float attackAnimSpeed = 1f; // 공격 애니메이션 재생 속도
    private float attackTimer = 0f; // 공격 타이머
    
    void Start()
    {
        monsterStatus = GetComponent<MonsterStatus>();
        monsterSetting = GetComponent<MonsterSetting>();
        agent = GetComponent<NavMeshAgent>();
        monsterAnim = GetComponent<Animator>();
        attackSpeed = monsterStatus.attackSpeed;
    }

    void Update()
    {
        // 플레이어가 있고, 플레이어와의 거리가 stoppingDistance 이내일 때
        if (monsterSetting.nearPlayer != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, monsterSetting.nearPlayer.transform.position);
            if (distanceToPlayer <= agent.stoppingDistance)
            {
                RangeAttack();
            }
            else
            {
                attackTimer = 0f;
            }
        }
    }

    public void RangeAttack()
    {
        attackTimer += Time.deltaTime;

        // 공격 중이 아닐 때만 회전 가능
        if (monsterSetting.nearPlayer != null)
        {
            // 플레이어 방향으로 회전
            Vector3 directionToPlayer = (monsterSetting.nearPlayer.transform.position - transform.position).normalized;
            directionToPlayer.y = 0; // y축 회전 방지
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * monsterSetting.agent.angularSpeed);
        }

        if(attackTimer >= attackSpeed && !monsterStatus.isAttack)
        {
            //Debug.Log("범위 공격 시작");
            StartCoroutine(FireProjectile());
            attackTimer = 0f;
        }
    }

    private IEnumerator FireProjectile()
    {
        monsterStatus.anim.SetTrigger("IsAttack");
        monsterStatus.anim.SetFloat("AttackSpeed", attackAnimSpeed);  // 공격 애니메이션 속도만 설정
        // 애니메이션이 실제로 "IsAttack" 상태로 전환될 때까지 기다림
        yield return new WaitUntil(() => monsterStatus.anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"));
        monsterStatus.isAttack = true;
        yield return new WaitForSeconds(attackDelay);

        // 투사체 오브젝트 풀링
        GameObject attack = GetOrCreateProjectile();
        if (attack != null)
        {
            attack.transform.position = transform.position + transform.forward * 1f + Vector3.up * 1f;
            attack.transform.rotation = transform.rotation;

            // Projectile 컴포넌트 설정
            Projectile projectileScript = attack.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                projectileScript.Setup(transform.forward, monsterStatus.attackPower);
            }

            attack.SetActive(true);
        }

        // 애니메이션이 종료될 때까지 기다림
        yield return new WaitUntil(() => !monsterStatus.anim.GetCurrentAnimatorStateInfo(0).IsName("IsAttack"));
        monsterStatus.isAttack = false;
    }

    private GameObject GetOrCreateProjectile()
    {
        Transform parent = EnemyManager.Instance.transform.GetChild(1);
        GameObject pooledProjectile = EnemyManager.Instance.GetDfProjectileFromPool(projectile, parent);
        Projectile projectileComponent = pooledProjectile != null ? pooledProjectile.GetComponent<Projectile>() : null;
        if (projectileComponent != null)
        {
            projectileComponent.onRelease = EnemyManager.Instance.ReturnDfProjectile;
        }
        return pooledProjectile;
    }
}
