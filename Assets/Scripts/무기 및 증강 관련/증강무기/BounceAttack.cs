using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class BounceAttack : MonoBehaviour
{
    [Header("장착 플레이어")]
    public GameObject player;
    public PlayerStatus playerStatus;
    public UpgradeInGame upgradeInGame;

    [Header("발사체")]
    public GameObject weaponPrefab; //기본 무기

    [Header("무기 설정")]
    public float weaponDamage;              //무기데미지
    public float weaponCooldown = 1f;             // 발사 주기(초)
    private float timer = 0f;

    private GameObject playerAttackRange; //플레이어 공격 범위

    [Header("진화 여부")]
    public bool isArousal; //능력 진화 여부

    public List<GameObject> activeProjectiles = new List<GameObject>(); //활성화된 투사체
    public Queue<GameObject> inactiveProjectiles = new Queue<GameObject>(); //비활성화된 투사체

    [Header("이펙트")]
    public GameObject hitEffect; // 몬스터 피격 이펙트
    public Queue<GameObject> hitEffects = new Queue<GameObject>(); // 몬스터 피격 이펙트 풀

    void Start()
    {
        player = transform.parent.parent.gameObject;
        playerStatus = player.GetComponent<PlayerStatus>();
        playerAttackRange = player.GetComponent<PlayerSystem>().playerAttackRange;
        upgradeInGame = player.GetComponent<UpgradeInGame>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= (weaponCooldown / (1 + (upgradeInGame.abilityCardValues.attackSpeed + upgradeInGame.upgradeAbilityValues.attackSpeed)/100)))
        {
            Transform target = GetClosestEnemy(); // 가장 가까운 적을 찾음
            if (target != null)
            {
                SphereCollider sphereCollider = playerAttackRange.GetComponent<SphereCollider>();
                float attackRange = sphereCollider.radius * playerStatus.attackRange;

                // 비활성화된 발사체가 있는지 확인
                if (inactiveProjectiles.Count == 0)
                {
                    // 새로운 발사체 생성
                    GameObject newProjectile = Instantiate(weaponPrefab, transform.position, Quaternion.identity);
                    // EnemyManager의 첫 번째 자식 오브젝트의 자식으로 설정
                    newProjectile.transform.SetParent(EnemyManager.Instance.transform.GetChild(0));
                    newProjectile.SetActive(false);
                    inactiveProjectiles.Enqueue(newProjectile);
                }

                // 비활성화된 발사체 중 하나를 가져와서 활성화
                GameObject proj = inactiveProjectiles.Dequeue();

                proj.transform.position = transform.position;
                proj.transform.localScale = Vector3.one * playerStatus.attackRange;
                ProjectileBounce projectileBounce = proj.GetComponent<ProjectileBounce>();
                projectileBounce.DfSetting(target, player, weaponDamage, playerStatus.projecTile, attackRange, isArousal);
                projectileBounce.OnProjectileDestroy += HandleProjectileDestroy;
                projectileBounce.OnMonsterHitEffect += HandleMonsterHitEffect; // 이펙트 이벤트 구독

                proj.SetActive(true);
                activeProjectiles.Add(proj);
            }
            timer = 0f;
        }
    }

    private void HandleProjectileDestroy(GameObject projectile)
    {
        if (activeProjectiles.Contains(projectile))
        {
            ProjectileBounce projectileBounce = projectile.GetComponent<ProjectileBounce>();
            projectileBounce.OnProjectileDestroy -= HandleProjectileDestroy;
            projectileBounce.OnMonsterHitEffect -= HandleMonsterHitEffect; // 이벤트 구독 해제

            activeProjectiles.Remove(projectile);
            projectile.SetActive(false);
            inactiveProjectiles.Enqueue(projectile);
        }
    }

    // EnemyManager에서 등록된 적들 중 가장 가까운 적을 찾는 함수
    Transform GetClosestEnemy()
    {
        SphereCollider sphereCollider = playerAttackRange.GetComponent<SphereCollider>();
        float attackRange = sphereCollider.radius * playerStatus.attackRange;

        Transform closest = null;
        float minDist = Mathf.Infinity;

        foreach (GameObject enemy in EnemyManager.Instance.enemies)
        {
            if (enemy == null) continue;

            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < minDist && dist <= attackRange)
            {
                minDist = dist;
                closest = enemy.transform;
            }
        }

        return closest;
    }

    // 피격 이펙트 풀링 관리
    private GameObject GetOrCreateHitEffect()
    {
        GameObject effectInstance = null;

        while (hitEffects.Count > 0 && effectInstance == null)
        {
            effectInstance = hitEffects.Dequeue();
        }

        // 비활성화된 이펙트가 없으면 새로 생성
        if (effectInstance == null)
        {
            effectInstance = Instantiate(hitEffect);
            // EnemyManager의 네 번째 자식 오브젝트의 자식으로 설정
            effectInstance.transform.SetParent(EnemyManager.Instance.transform.GetChild(3));
        }

        effectInstance.SetActive(false);
        return effectInstance;
    }

    // 파티클 재생이 끝나면 이펙트를 비활성화하는 코루틴
    private IEnumerator DisableEffectAfterParticle(GameObject effect, ParticleSystem particleSystem)
    {
        // 파티클 재생
        particleSystem.Play();

        // 파티클이 재생 중인 동안 대기
        while (particleSystem.isPlaying)
        {
            yield return null;
        }

        // 파티클 재생이 끝나면 이펙트 비활성화
        effect.SetActive(false);
        hitEffects.Enqueue(effect);
    }

    // 몬스터 피격 시 이펙트 처리
    private void HandleMonsterHitEffect(Vector3 position)
    {
        // 피격 이펙트 재생
        GameObject hitEffectInstance = GetOrCreateHitEffect();
        if (hitEffectInstance != null)
        {
            // 이펙트 위치 설정 (y좌표 1 올림)
            Vector3 effectPosition = position;
            effectPosition.y = 1f;
            hitEffectInstance.transform.position = effectPosition;
            
            hitEffectInstance.SetActive(true);
            
            // 파티클 시스템 가져오기
            ParticleSystem particleSystem = hitEffectInstance.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                // 파티클 재생이 끝나면 자동으로 비활성화하는 코루틴 시작
                StartCoroutine(DisableEffectAfterParticle(hitEffectInstance, particleSystem));
            }
        }
    }
}
