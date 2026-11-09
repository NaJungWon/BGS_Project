using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TRbullet : MonoBehaviour
{
    public MonsterStatus monsterStatus;
    public MonsterSetting monsterSetting;
    public Animator monsterAnim;
    public NavMeshAgent agent;

    public GameObject bulletPrefab;

    public float attackAnimSpeed = 1.5f; // 공격 애니메이션 재생 속도
    public float attackDelay; // 공격 딜레이
    public float attackSpeed; // 공격 속도
    //public float bulletSpeed = 10f; // 총알 속도

    private float attackTimer = 0f; // 공격 타이머

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        monsterStatus = GetComponent<MonsterStatus>();
        monsterSetting = GetComponent<MonsterSetting>();
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
                ShootInThreeDirections();
            }
            else
            {
                attackTimer = 0f;
            }
        }
    }

    void ShootInThreeDirections()
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
            StartCoroutine(FireProjectile());
            attackTimer = 0f;
        }
    }

    private GameObject GetOrCreateBullet()
    {
        // 비활성화된 투사체 찾기
        foreach (GameObject bullet in EnemyManager.Instance.enemySpecialProjectiles)
        {
            if (!bullet.activeSelf && bullet.name == bulletPrefab.name + "(Clone)")
            {
                return bullet;
            }
        }

        // 비활성화된 투사체가 없으면 새로 생성
        GameObject newBullet = Instantiate(bulletPrefab);
        newBullet.SetActive(false);
        // EnemyManager의 첫 번째 자식의 자식으로 설정
        newBullet.transform.SetParent(EnemyManager.Instance.transform.GetChild(1));
        EnemyManager.Instance.RegisterSpecialProjectile(newBullet);
        return newBullet;
    }

    private IEnumerator FireProjectile()
    {
        monsterStatus.anim.SetTrigger("IsAttack");
        monsterStatus.anim.SetFloat("AttackSpeed", attackAnimSpeed);  // 공격 애니메이션 속도만 설정

        // 애니메이션이 실제로 "IsAttack" 상태로 전환될 때까지 기다림
        yield return new WaitUntil(() => monsterStatus.anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"));
        monsterStatus.isAttack = true;
        yield return new WaitForSeconds(attackDelay);

        // 3방향 발사
        Vector3 forward = transform.forward;
        Vector3 left = Quaternion.Euler(0, -30f, 0) * forward;
        Vector3 right = Quaternion.Euler(0, 30f, 0) * forward;

        // 각 방향으로 발사
        FireBullet(forward);
        FireBullet(left);
        FireBullet(right);

        // 애니메이션이 종료될 때까지 기다림
        yield return new WaitUntil(() => !monsterStatus.anim.GetCurrentAnimatorStateInfo(0).IsName("IsAttack"));
        monsterStatus.isAttack = false;
    }

    private void FireBullet(Vector3 direction)
    {
        GameObject bullet = GetOrCreateBullet();
        if (bullet != null)
        {
            bullet.transform.position = transform.position + transform.forward * 1f + Vector3.up * 1f;
            bullet.transform.rotation = Quaternion.LookRotation(direction);
            
            Projectile projectileScript = bullet.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                projectileScript.Setup(direction, monsterStatus.attackPower);
            }
            
            bullet.SetActive(true);
        }
    }
}
