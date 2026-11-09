using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LionRain : MonoBehaviour
{
    [Header("장착 플레이어")]
    public GameObject player;
    public UpgradeInGame upgradeInGame;
    public Transform playerTransform;  // 플레이어의 Transform (위치, 회전, 크기 정보)
    [Header("발사체")]
    public GameObject weaponPrefab; //기본 무기
    [Header("이펙트")]
    public GameObject hitEffect; // 몬스터 피격 이펙트
    private Queue<GameObject> hitEffects = new Queue<GameObject>(); // 몬스터 피격 이펙트 풀
    private HashSet<Vector3> hitPositions = new HashSet<Vector3>(); // 이미 이펙트가 터진 위치 추적
    [Header("무기 데미지/ 발사체 속도/ 생존 시간")]
    public float weaponDamage;
    public float projectileSpeed = 10f; // 발사체의 초기 이동 속도
    public float convergeSpeed = 5f; // 발사체가 수렴 지점으로 이동하는 속도
    public float fireRate = 0.2f; // 발사체 연사 속도 (몇 초마다)
    public float convergeDelay = 0.5f; // 발사체가 수렴하기 전 대기 시간
    [Header("발사 속도")]
    public float weaponCooldown = 5f; //스킬 쿨타임
    public float waitCool = 1f; //스킬이 준비되는 동안 쿨타임을 기다리는 대기시간
    
    // 타이머 변수 추가
    public float currentWaitCoolTimer = 0f;
    public float currentWeaponCooldownTimer = 0f;

    public bool isFiring = false; // 현재 발사체 연사가 진행 중인지 여부
    private bool fireLeft = true; // 발사체가 왼쪽에서 나갈지 오른쪽에서 나갈지 여부

    [Header("진화 여부")]
    public bool isArousal; //능력 진화 여부

    public List<GameObject> activeProjectiles = new List<GameObject>(); // 활성화된 투사체
    public Queue<GameObject> inactiveProjectiles = new Queue<GameObject>(); // 비활성화된 투사체

    private float currentAngle = 0f; // 현재 발사 각도
    private float angleIncrement = 45f; // 각도 증가량

    private void Start()
    {
        player = transform.parent.parent.gameObject;
        playerTransform = player.transform;
        upgradeInGame = player.GetComponent<UpgradeInGame>();
        // 타이머 초기화
        currentWaitCoolTimer = 0f;
        currentWeaponCooldownTimer = 0f;
    }

    private void Update()
    {
        // 대기 시간 타이머 처리
        if (currentWaitCoolTimer < waitCool)
        {
            currentWaitCoolTimer += Time.deltaTime;
        }
        // 대기 시간이 끝나면 쿨타임 타이머 처리
        else
        {
            currentWeaponCooldownTimer += Time.deltaTime;
            
            // 쿨타임 계산 (isArousal이 true면 1초, false면 weaponCooldown 사용)
            float targetCooldown = isArousal 
                ? 1f / (1 + (upgradeInGame.abilityCardValues.attackSpeed + upgradeInGame.upgradeAbilityValues.attackSpeed) / 100)
                : weaponCooldown / (1 + (upgradeInGame.abilityCardValues.attackSpeed + upgradeInGame.upgradeAbilityValues.attackSpeed) / 100);
            
            // 쿨타임이 다 찼을 때
            if (currentWeaponCooldownTimer >= targetCooldown)
            {
                ActivateSkill();
                // 타이머 초기화
                currentWaitCoolTimer = 0f;
                currentWeaponCooldownTimer = 0f;
            }
        }
    }

    // 스킬 발동 메서드
    public void ActivateSkill()
    {
        if (isArousal)
        {
            StartCoroutine(FireProjectilesEvolution()); // 진화 시 4방향 발사
        }
        else if (!isFiring)
        {
            StartCoroutine(RepeatFireProjectiles(3)); // 기존 좌우 발사
        }
    }

    // 기존 발사체 연사를 반복 실행하는 코루틴
    private IEnumerator RepeatFireProjectiles(int repeatCount)
    {
        isFiring = true;

        for (int i = 0; i < repeatCount; i++)
        {
            yield return StartCoroutine(FireProjectiles());
            yield return new WaitForSeconds(0.2f);
        }

        isFiring = false;
    }

    // 기존 발사체를 발사하는 코루틴
    private IEnumerator FireProjectiles()
    {
        // 왼쪽 또는 오른쪽 방향 결정 (한 번씩 번갈아가며)
        Vector3 sideOffset = fireLeft ? Vector3.left : Vector3.right;
        Vector3 convergePoint = playerTransform.position + sideOffset * 8f;
        convergePoint.y = 1f;
        convergePoint.z = playerTransform.position.z;

        float arrivalTime = 0.2f;

        // 3개의 발사체를 발사
        for (int i = 0; i < 3; i++)
        {
            float spreadOffset = -1.5f + (i * 1.5f);
            float spawnZ = convergePoint.z + spreadOffset;
            Vector3 spawnPosition = new Vector3(
                playerTransform.position.x + sideOffset.x,
                1f,
                spawnZ
            );

            GameObject projectile = GetOrCreateProjectile();
            projectile.transform.position = spawnPosition;
            projectile.transform.forward = (convergePoint - spawnPosition).normalized;

            SetupTrailRenderer(projectile);
            SetupProjectileProperties(projectile);

            Vector3 controlPoint = (spawnPosition + convergePoint) / 2f;
            controlPoint.z += (i == 0 ? -2f : (i == 2 ? 2f : 0f));
            controlPoint.y = 1f;

            StartCoroutine(MoveProjectile(projectile, convergePoint, arrivalTime, controlPoint, i));
        }

        fireLeft = !fireLeft;
        yield return new WaitForSeconds(fireRate);
    }

    // 기존 발사체 이동 코루틴
    private IEnumerator MoveProjectile(GameObject projectile, Vector3 convergePoint, float arrivalTime, Vector3 controlPoint, int index)
    {
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        Vector3 startPosition = projectile.transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < arrivalTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / arrivalTime;

            Vector3 newPosition = Mathf.Pow(1 - t, 2) * startPosition +
                                2 * (1 - t) * t * controlPoint +
                                Mathf.Pow(t, 2) * convergePoint;

            newPosition.y = 1f;

            if (rb != null)
            {
                rb.MovePosition(newPosition);
                
                Vector3 nextPosition = Mathf.Pow(1 - ((elapsedTime + Time.deltaTime) / arrivalTime), 2) * startPosition +
                                     2 * (1 - ((elapsedTime + Time.deltaTime) / arrivalTime)) * ((elapsedTime + Time.deltaTime) / arrivalTime) * controlPoint +
                                     Mathf.Pow(((elapsedTime + Time.deltaTime) / arrivalTime), 2) * convergePoint;
                
                Vector3 moveDirection = (nextPosition - newPosition).normalized;
                if (moveDirection != Vector3.zero)
                {
                    projectile.transform.forward = moveDirection;
                }
            }

            yield return null;
        }

        HandleProjectileDestroy(projectile);
    }

    // 진화 시 4방향 발사체를 발사하는 코루틴
    private IEnumerator FireProjectilesEvolution()
    {
        isFiring = true;

        // 기본 방향 (상, 하, 좌, 우)
        Vector3[] baseDirections = new Vector3[] 
        { 
            Vector3.forward,
            Vector3.back,
            Vector3.left,
            Vector3.right
        };

        // 현재 각도에 따라 방향 회전
        Vector3[] currentDirections = new Vector3[4];
        for (int i = 0; i < 4; i++)
        {
            currentDirections[i] = Quaternion.Euler(0, currentAngle, 0) * baseDirections[i];
        }

        foreach (Vector3 direction in currentDirections)
        {
            // 발사 방향을 수평으로 설정
            Vector3 shootDirection = direction;
            shootDirection.y = 0f;
            shootDirection = shootDirection.normalized;

            // 수렴 지점 설정
            Vector3 convergePoint = playerTransform.position + (shootDirection * 8f);
            convergePoint.y = 1f;

            // 발사 위치의 기준점 설정
            Vector3 baseSpawnPosition = playerTransform.position + (shootDirection * 2f);
            baseSpawnPosition.y = 1f;

            float arrivalTime = 0.2f;

            // 3개의 발사체를 발사
            for (int i = 0; i < 3; i++)
            {
                // 발사체가 퍼지는 방향 계산 (발사 방향의 수직 방향)
                Vector3 spreadDirection = Vector3.Cross(shootDirection, Vector3.up).normalized;
                float spreadOffset = -1.5f + (i * 1.5f);
                
                // 최종 발사 위치 계산
                Vector3 spawnPosition = baseSpawnPosition + (spreadDirection * spreadOffset);

                GameObject projectile = GetOrCreateProjectile();
                projectile.transform.position = spawnPosition;
                projectile.transform.forward = shootDirection;

                SetupTrailRenderer(projectile);
                SetupProjectileProperties(projectile);

                // 제어점 계산 (발사 위치와 수렴 지점의 중간)
                Vector3 controlPoint = (spawnPosition + convergePoint) / 2f;
                // 제어점을 발사 방향의 수직 방향으로 약간 이동
                controlPoint += spreadDirection * (i == 0 ? -2f : (i == 2 ? 2f : 0f));
                controlPoint.y = 1f;

                StartCoroutine(MoveProjectile(projectile, convergePoint, arrivalTime, controlPoint, i));
            }
        }

        // 다음 발사를 위해 각도 업데이트 (0 -> 45 -> 0)
        currentAngle = (currentAngle == 0f) ? 45f : 0f;

        isFiring = false;
        yield return null;
    }

    // 진화 시 발사체 이동 코루틴
    private IEnumerator MoveProjectileEvolution(GameObject projectile, Vector3 targetPosition, float arrivalTime)
    {
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        Vector3 startPosition = projectile.transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < arrivalTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / arrivalTime;

            // 일직선 이동
            Vector3 newPosition = Vector3.Lerp(startPosition, targetPosition, t);
            newPosition.y = 1f;

            if (rb != null)
            {
                rb.MovePosition(newPosition);
            }

            yield return null;
        }

        HandleProjectileDestroy(projectile);
    }

    // 발사체 생성 및 초기화 헬퍼 메서드
    private GameObject GetOrCreateProjectile()
    {
        if (inactiveProjectiles.Count == 0)
        {
            GameObject newProjectile = Instantiate(weaponPrefab);
            newProjectile.transform.SetParent(EnemyManager.Instance.transform.GetChild(0));
            newProjectile.SetActive(false);
            inactiveProjectiles.Enqueue(newProjectile);
        }

        GameObject projectile = inactiveProjectiles.Dequeue();
        projectile.SetActive(true);
        activeProjectiles.Add(projectile);
        return projectile;
    }

    // Trail Renderer 설정 헬퍼 메서드
    private void SetupTrailRenderer(GameObject projectile)
    {
        TrailRenderer trail = projectile.transform.GetChild(2).GetComponent<TrailRenderer>();
        if (trail != null)
        {
            trail.Clear();
            trail.emitting = true;
        }
    }

    // 발사체 속성 설정 헬퍼 메서드
    private void SetupProjectileProperties(GameObject projectile)
    {
        LionBullet lionBullet = projectile.GetComponent<LionBullet>();
        if (lionBullet == null) return;

        // 중복 구독 방지: 기존 이벤트 구독 해제
        lionBullet.OnProjectileDestroy -= HandleProjectileDestroy;
        lionBullet.OnMonsterHitEffect -= HandleMonsterHitEffect;

        lionBullet.player = player;
        lionBullet.weaponDamage = weaponDamage;
        lionBullet.OnProjectileDestroy += HandleProjectileDestroy;
        lionBullet.OnMonsterHitEffect += HandleMonsterHitEffect;
    }

    private void HandleProjectileDestroy(GameObject projectile)
    {
        if (activeProjectiles.Contains(projectile))
        {
            // 이벤트 구독 해제
            LionBullet lionBullet = projectile.GetComponent<LionBullet>();
            if (lionBullet != null)
            {
                lionBullet.OnProjectileDestroy -= HandleProjectileDestroy;
                lionBullet.OnMonsterHitEffect -= HandleMonsterHitEffect;
            }

            // Trail Renderer 비활성화 (자식 오브젝트에서 찾기)
            TrailRenderer trail = projectile.transform.GetChild(2).GetComponent<TrailRenderer>();
            if (trail != null)
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
}
