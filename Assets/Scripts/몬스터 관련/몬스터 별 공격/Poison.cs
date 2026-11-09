using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Posion : MonoBehaviour
{
    public MonsterStatus monsterStatus;
    public MonsterSetting monsterSetting;
    public NavMeshAgent agent;
    public Animator monsterAnim;

    public GameObject attackPrefab;

    public float attackDelay = 0.25f; // 공격 딜레이
    public float attackSpeed;
    public float attackAnimSpeed = 1f; // 공격 애니메이션 재생 속도
    public float explosionDelay = 1f; // 폭발 딜레이
    private float attackTimer = 0f;

    private float timer;

    void Start()
    {
        monsterStatus = GetComponent<MonsterStatus>();
        monsterSetting = GetComponent<MonsterSetting>();
        agent = GetComponent<NavMeshAgent>();
        //monsterAnim = GetComponent<Animator>();
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
                DropPoisonAtPlayer();
            }
            else
            {
                attackTimer = 0f;
            }
        }
    }

    void DropPoisonAtPlayer()
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
            StartCoroutine(DropAttack());
            attackTimer = 0f;
        }
    }

    private GameObject GetOrCreatePoison()
    {
        Transform parent = EnemyManager.Instance.transform.GetChild(1);
        GameObject poison = EnemyManager.Instance.GetRangedProjectileFromPool(attackPrefab, parent);
        if (poison != null)
        {
            ExplosionZone zone = poison.GetComponent<ExplosionZone>();
            if (zone != null)
            {
                zone.onRelease = EnemyManager.Instance.ReturnRangedProjectile;
            }
        }
        return poison;
    }

    private IEnumerator DropAttack()
    {
        monsterStatus.anim.SetTrigger("IsAttack");
        monsterStatus.anim.SetFloat("AttackSpeed", attackAnimSpeed);  // 공격 애니메이션 속도만 설정
        // 애니메이션이 실제로 "IsAttack" 상태로 전환될 때까지 기다림
        yield return new WaitUntil(() => monsterStatus.anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"));
        monsterStatus.isAttack = true;
        yield return new WaitForSeconds(attackDelay);

        Vector3 dropPosition = monsterSetting.nearPlayer.transform.position;
        dropPosition.y = 0f;

        GameObject attack = GetOrCreatePoison();
        if (attack != null)
        {
            attack.transform.position = dropPosition;
            attack.transform.rotation = Quaternion.identity;
            
            ExplosionZone explosionZone = attack.GetComponent<ExplosionZone>();
            if (explosionZone != null)
            {
                explosionZone.damage = monsterStatus.attackPower;
                explosionZone.damageDelay = explosionDelay;
            }
            
            attack.SetActive(true);
        }

        // 애니메이션이 종료될 때까지 기다림
        yield return new WaitUntil(() => !monsterStatus.anim.GetCurrentAnimatorStateInfo(0).IsName("IsAttack"));
        monsterStatus.isAttack = false;
    }
}
