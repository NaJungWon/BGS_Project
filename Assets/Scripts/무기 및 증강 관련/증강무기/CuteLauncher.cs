using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CuteLauncher : MonoBehaviour
{
    [Header("장착 플레이어")]
    public GameObject player;
    public PlayerStatus playerStatus;
    public UpgradeInGame upgradeInGame;
    private GameObject playerBody;

    [Header("발사체")]
    public GameObject weaponPrefab; //기본 무기

    [Header("무기 데미지/ 투사체 개수/ 발사체 속도")]
    public float weaponDamage;
    public float weaponCooldown = 0.1f; // 발사 쿨타임
    public float projectileSpeed; // 총알 속도
    private float timer;
    private GameObject playerAttackRange; //플레이어 공격 범위

    [Header("진화 여부")]
    public bool isArousal; //능력 진화 여부

    [Header("이펙트")]
    public GameObject hitEffect; // 몬스터 피격 이펙트
    private Queue<GameObject> hitEffects = new Queue<GameObject>(); // 몬스터 피격 이펙트 풀

    public List<GameObject> activeProjectiles = new List<GameObject>(); // 활성화된 투사체
    public Queue<GameObject> inactiveProjectiles = new Queue<GameObject>(); // 비활성화된 투사체

    void Start()
    {
        // player를 이 스크립트가 컴포넌트된 부모 오브젝트로 설정
        player = transform.parent.parent.gameObject; // 부모 오브젝트에서 플레이어를 찾는 방법
        playerBody = player.transform.GetChild(0).gameObject;
        playerStatus = player.GetComponent<PlayerStatus>();
        upgradeInGame = player.GetComponent<UpgradeInGame>();
        playerAttackRange = player.GetComponent<PlayerSystem>().playerAttackRange;
    }

    void Update()
    {
        timer += Time.deltaTime; // 쿨타임 감소
        if (!isArousal)
        {
            // 기본: 기존 코드 유지
            if (timer >= (weaponCooldown / (1 + (upgradeInGame.abilityCardValues.attackSpeed + upgradeInGame.upgradeAbilityValues.attackSpeed)/100)))
            {
                timer = 0;
                FireProjectile();
            }
        }
        else
        {
            // 진화 시: 0.1초에 공속 계산 적용
            if (timer >= (0.1f / (1 + (upgradeInGame.abilityCardValues.attackSpeed + upgradeInGame.upgradeAbilityValues.attackSpeed)/100)))
            {
                timer = 0;
                FireProjectile();
            }
        }
    }

    void FireProjectile()
    {
        List<Transform> targets = GetTargetsInRange();
        
        // SphereCollider의 radius에 attackRange를 곱해서 직선 거리 계산
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
        GameObject projectile = inactiveProjectiles.Dequeue();

        // TrailRenderer 초기화
        TrailRenderer[] trails = projectile.GetComponentsInChildren<TrailRenderer>();
        foreach (TrailRenderer trail in trails)
        {
            trail.Clear();
        }

        Vector3 spawnPosition = new Vector3(player.transform.position.x, 1f, player.transform.position.z);
        projectile.transform.position = spawnPosition;

        // 타겟이 없으면 플레이어 정면으로 발사
        if (targets.Count == 0)
        {
            // 플레이어 정면 방향으로 발사
            Vector3 shootDirection = playerBody.transform.forward;
            shootDirection.y = 0f; // y값은 0으로 유지하여 수평으로 발사
            shootDirection = shootDirection.normalized;

            Launcher_Bullet bullet = projectile.GetComponent<Launcher_Bullet>();
            bullet.Initialize(transform, shootDirection, player, weaponDamage, attackRange);
            bullet.OnProjectileDestroy += HandleProjectileDestroy;
            bullet.OnMonsterHitEffect += HandleMonsterHitEffect; // 이벤트 구독
        }
        else
        {
            // 타겟이 있으면 가장 가까운 타겟을 향해 발사
            Vector3 targetPosition = targets[0].position;
            targetPosition.y = 1f; // 타겟의 y좌표를 1로 고정
            Vector3 targetShootDirection = (targetPosition - spawnPosition).normalized;
            targetShootDirection.y = 0f; // 발사 방향의 y값도 0으로 유지하여 수평으로 발사

            Launcher_Bullet bullet = projectile.GetComponent<Launcher_Bullet>();
            bullet.Initialize(transform, targetShootDirection, player, weaponDamage, attackRange);
            bullet.OnProjectileDestroy += HandleProjectileDestroy;
            bullet.OnMonsterHitEffect += HandleMonsterHitEffect; // 이벤트 구독
        }

        projectile.SetActive(true);
        activeProjectiles.Add(projectile);
    }

    private void HandleProjectileDestroy(GameObject projectile)
    {
        if (activeProjectiles.Contains(projectile))
        {
            // 이벤트 구독 해제
            Launcher_Bullet bullet = projectile.GetComponent<Launcher_Bullet>();
            if (bullet != null)
            {
                bullet.OnProjectileDestroy -= HandleProjectileDestroy;
                bullet.OnMonsterHitEffect -= HandleMonsterHitEffect;
            }

            activeProjectiles.Remove(projectile);
            projectile.SetActive(false);
            inactiveProjectiles.Enqueue(projectile);
        }
    }

    List<Transform> GetTargetsInRange()
    {
        List<Transform> targets = new List<Transform>();
        SphereCollider sphereCollider = playerAttackRange.GetComponent<SphereCollider>();
        float attackRange = sphereCollider.radius * playerStatus.attackRange;

        // 범위 내의 모든 적을 찾아서 거리와 함께 저장
        List<(Transform target, float distance)> targetDistances = new List<(Transform, float)>();
        
        foreach (GameObject enemy in EnemyManager.Instance.enemies)
        {
            if (enemy == null) continue;

            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist <= attackRange)
            {
                targetDistances.Add((enemy.transform, dist));
            }
        }

        // 거리순으로 정렬 (가까운 순)
        targetDistances.Sort((a, b) => a.distance.CompareTo(b.distance));

        // 정렬된 순서대로 targets 리스트에 추가
        foreach (var targetDistance in targetDistances)
        {
            targets.Add(targetDistance.target);
        }

        return targets;
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
            // EnemyManager의 첫 번째 자식 오브젝트의 자식으로 설정
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
    public void HandleMonsterHitEffect(Vector3 position)
    {
        // 피격 이펙트 재생
        GameObject hitEffectInstance = GetOrCreateHitEffect();
        if (hitEffectInstance != null)
        {
            // 이펙트 위치 설정 (y좌표 1로 설정)
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
