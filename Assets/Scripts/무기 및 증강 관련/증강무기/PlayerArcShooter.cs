using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerArcShooter : MonoBehaviour
{
    [Header("장착 플레이어")]
    public GameObject player;
    public PlayerStatus playerStatus;
    public UpgradeInGame upgradeInGame;

    [Header("발사체")]
    public GameObject weaponPrefab; //기본 무기

    [Header("이펙트")]
    public GameObject hitEffect; // 몬스터 피격 이펙트
    private Queue<GameObject> hitEffects = new Queue<GameObject>(); // 몬스터 피격 이펙트 큐
    private HashSet<Vector3> hitPositions = new HashSet<Vector3>(); // 이미 이펙트가 터진 위치 추적

    [Header("무기 설정")]
    public float weaponDamage;
    public float weaponCooldown = 0.3f; // 발사 쿨타임(초)
    public float weaponProjecTile = 1; // 한 번에 발사할 총알 개수
    public float projectileSpeed = 15f; // 투사체 속도
    
    private float cooldownTimer = 0f; // 쿨타임 타이머
    private float currentCooldown; // 현재 쿨타임
    private GameObject playerAttackRange; //플레이어 공격 범위

    [Header("진화 여부")]
    public bool isArousal; //능력 진화 여부

    public List<GameObject> activeProjectiles = new List<GameObject>(); // 활성화된 투사체
    public Queue<GameObject> inactiveProjectiles = new Queue<GameObject>(); // 비활성화된 투사체

    // 진화 시 궤도 발사 관련 변수
    private float currentOrbitAngle = 0f; // 현재 궤도 각도
    private float ORBIT_ANGLE_INCREMENT = 10f; // 궤도 각도 증가량
    private float ORBIT_RADIUS = 2f; // 궤도 반지름
    private int ORBIT_PROJECTILE_COUNT = 4; // 궤도 발사체 개수

    void Start()
    {
        player = transform.parent.parent.gameObject;
        playerStatus = player.GetComponent<PlayerStatus>();
        upgradeInGame = player.GetComponent<UpgradeInGame>();
        playerAttackRange = player.GetComponent<PlayerSystem>().playerAttackRange;
    }

    void Update()
    {
        cooldownTimer += Time.deltaTime;
        if (isArousal)
        {
            currentCooldown = 0.1f;
        }
        else
        {
            currentCooldown = weaponCooldown;
        }
        
        if (cooldownTimer >= (currentCooldown / (1 + (upgradeInGame.abilityCardValues.attackSpeed + upgradeInGame.upgradeAbilityValues.attackSpeed)/100)))
        {
            hitPositions.Clear(); // 새로운 발사 시 이펙트 위치 초기화
            if (isArousal)
            {
                FireOrbit();
            }
            else
            {
                FireArc();
            }
            cooldownTimer = 0f;
        }
    }

    private void HandleProjectileDestroy(GameObject projectile)
    {
        if (activeProjectiles.Contains(projectile))
        {
            // 이벤트 구독 해제
            StormShooting shooting = projectile.GetComponent<StormShooting>();
            if (shooting != null)
            {
                shooting.OnProjectileDestroy -= HandleProjectileDestroy;
                shooting.OnMonsterHitEffect -= HandleMonsterHitEffect;
            }

            // 모든 Trail Renderer 비활성화
            TrailRenderer[] trails = projectile.GetComponentsInChildren<TrailRenderer>();
            foreach (TrailRenderer trail in trails)
            {
                trail.Clear();
                trail.emitting = false;
            }

            activeProjectiles.Remove(projectile);
            projectile.SetActive(false);
            inactiveProjectiles.Enqueue(projectile);
        }
    }

    // 피격 이펙트 풀링 관리
    private GameObject GetOrCreateHitEffect()
    {
        GameObject effectInstance = null;

        while (hitEffects.Count > 0 && effectInstance == null)
        {
            effectInstance = hitEffects.Dequeue();
        }

        if (effectInstance == null)
        {
            effectInstance = Instantiate(hitEffect);
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

        // 파티클 재생이 끝나면 이펙트 비활성화 및 큐에 반환
        effect.SetActive(false);
        hitEffects.Enqueue(effect);
    }

    // 몬스터 피격 시 이펙트 처리
    public void HandleMonsterHitEffect(Vector3 position)
    {
        // 이미 이펙트가 터진 위치인지 확인
        if (hitPositions.Contains(position))
        {
            return;
        }

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

            // 이펙트가 터진 위치 기록
            hitPositions.Add(position);
        }
    }

    // 진화 시 궤도형 발사 함수
    void FireOrbit()
    {
        SphereCollider sphereCollider = playerAttackRange.GetComponent<SphereCollider>();
        float attackRange = sphereCollider.radius * playerStatus.attackRange;

        // 궤도상의 발사체 위치 계산 (상,하,좌,우)
        for (int i = 0; i < ORBIT_PROJECTILE_COUNT; i++)
        {
            float angle = currentOrbitAngle + (i * 90f); // 90도 간격으로 배치 (상,하,좌,우)
            Vector3 orbitPosition = CalculateOrbitPosition(angle);
            // 발사 방향을 수평으로 설정
            Vector3 direction = (orbitPosition - player.transform.position).normalized;
            direction.y = 0f;
            direction = direction.normalized;

            // 비활성화된 투사체가 있는지 확인
            GameObject projectile = null;
            if (inactiveProjectiles.Count == 0)
            {
                // 새로운 투사체 생성
                projectile = Instantiate(weaponPrefab, orbitPosition, Quaternion.LookRotation(direction));
                // EnemyManager의 첫 번째 자식 오브젝트의 자식으로 설정
                projectile.transform.SetParent(EnemyManager.Instance.transform.GetChild(0));
                projectile.SetActive(false);
                inactiveProjectiles.Enqueue(projectile);
            }

            // 비활성화된 투사체 중 하나를 가져와서 활성화
            projectile = inactiveProjectiles.Dequeue();

            projectile.transform.position = orbitPosition;
            projectile.transform.rotation = Quaternion.LookRotation(direction);

            // 모든 Trail Renderer 활성화
            TrailRenderer[] trails = projectile.GetComponentsInChildren<TrailRenderer>();
            foreach (TrailRenderer trail in trails)
            {
                trail.Clear();
                trail.emitting = true;
            }

            StormShooting shooting = projectile.GetComponent<StormShooting>();
            if (shooting != null)
            {
                shooting.Initialize(player, weaponDamage, attackRange, projectileSpeed);
                shooting.OnProjectileDestroy += HandleProjectileDestroy;
                shooting.OnMonsterHitEffect += HandleMonsterHitEffect;
            }

            projectile.SetActive(true);
            activeProjectiles.Add(projectile);
        }

        // 다음 발사를 위해 궤도 각도 업데이트
        currentOrbitAngle = (currentOrbitAngle + ORBIT_ANGLE_INCREMENT) % 360f;
    }

    // 궤도상의 위치 계산
    private Vector3 CalculateOrbitPosition(float angle)
    {
        float radian = angle * Mathf.Deg2Rad;
        float x = Mathf.Cos(radian) * ORBIT_RADIUS;
        float z = Mathf.Sin(radian) * ORBIT_RADIUS;
        return player.transform.position + new Vector3(x, 1f, z);
    }

    // 360도 랜덤 방향으로 1개의 투사체를 발사하는 함수 (추후 bulletsPerShot 증가 가능)
    void FireArc()
    {
        SphereCollider sphereCollider = playerAttackRange.GetComponent<SphereCollider>();
        float attackRange = sphereCollider.radius * playerStatus.attackRange;

        for (int i = 0; i < (int)weaponProjecTile + playerStatus.projecTile; i++)
        {
            float randomAngle = Random.Range(0f, 360f); // 0~360도 랜덤
            Vector3 randomDir = Quaternion.Euler(0, randomAngle, 0) * Vector3.forward;
            Vector3 spawnPosition = new Vector3(player.transform.position.x, player.transform.position.y + 1f, player.transform.position.z);

            // 비활성화된 투사체가 있는지 확인
            GameObject projectile = null;
            if (inactiveProjectiles.Count == 0)
            {
                // 새로운 투사체 생성
                projectile = Instantiate(weaponPrefab, spawnPosition, Quaternion.LookRotation(randomDir));
                // EnemyManager의 첫 번째 자식 오브젝트의 자식으로 설정
                projectile.transform.SetParent(EnemyManager.Instance.transform.GetChild(0));
                projectile.SetActive(false);
                inactiveProjectiles.Enqueue(projectile);
            }

            // 비활성화된 투사체 중 하나를 가져와서 활성화
            projectile = inactiveProjectiles.Dequeue();

            projectile.transform.position = spawnPosition;
            projectile.transform.rotation = Quaternion.LookRotation(randomDir);

            // 모든 Trail Renderer 활성화
            TrailRenderer[] trails = projectile.GetComponentsInChildren<TrailRenderer>();
            foreach (TrailRenderer trail in trails)
            {
                trail.Clear();
                trail.emitting = true;
            }

            StormShooting shooting = projectile.GetComponent<StormShooting>();
            if (shooting != null)
            {
                // 중복 구독 방지: 기존 이벤트 구독 해제
                shooting.OnProjectileDestroy -= HandleProjectileDestroy;
                shooting.OnMonsterHitEffect -= HandleMonsterHitEffect;

                shooting.Initialize(player, weaponDamage, attackRange, projectileSpeed);
                shooting.OnProjectileDestroy += HandleProjectileDestroy;
                shooting.OnMonsterHitEffect += HandleMonsterHitEffect;
            }

            projectile.SetActive(true);
            activeProjectiles.Add(projectile);
        }
    }
}
