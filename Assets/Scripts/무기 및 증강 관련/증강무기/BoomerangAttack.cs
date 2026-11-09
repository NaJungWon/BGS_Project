using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class BoomerangAttack : MonoBehaviour
{
    [Header("장착 플레이어")]
    public GameObject player;
    public PlayerStatus playerStatus;
    public UpgradeInGame upgradeInGame;
    [Header("발사체")]
    public GameObject weaponPrefab; //기본 무기

    [Header("무기 설정")]
    public float weaponDamage;//무기데미지
    public float weaponProjecTile = 1; //투사체 개수
    public float weaponCooldown = 3f; //쿨타임
    private float timer = 0f;
    private GameObject playerAttackRange; //플레이어 공격 범위

    [Header("진화 여부")]
    public bool isArousal; //능력 진화 여부

    public List<GameObject> activeBoomerangs = new List<GameObject>(); //활성화된 투사체
    public Queue<GameObject> inactiveBoomerangs = new Queue<GameObject>(); //비활성화된 투사체
    public List<GameObject> activeSplitBoomerangs = new List<GameObject>(); //활성화된 분열 투사체
    public Queue<GameObject> inactiveSplitBoomerangs = new Queue<GameObject>(); //비활성화된 분열 투사체
    private Dictionary<Vector3, bool> spawnedPositions = new Dictionary<Vector3, bool>(); // 좌표별 생성 여부 저장

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
            FireBoomerang();
            timer = 0f;
        }
    }

    void FireBoomerang()
    {
        List<Transform> targets = GetTargetsInRange();
        int requiredBoomerangs = (int)weaponProjecTile + playerStatus.projecTile;
        
        // 필요한 부메랑 개수만큼 비활성화된 부메랑이 있는지 확인
        if (inactiveBoomerangs.Count < requiredBoomerangs)
        {
            // 부족한 만큼 새로운 부메랑 생성
            int needToCreate = requiredBoomerangs - inactiveBoomerangs.Count;
            for (int i = 0; i < needToCreate; i++)
            {
                GameObject newBoomerang = Instantiate(weaponPrefab);
                // EnemyManager의 첫 번째 자식 오브젝트의 자식으로 설정
                newBoomerang.transform.SetParent(EnemyManager.Instance.transform.GetChild(0));
                newBoomerang.SetActive(false);
                inactiveBoomerangs.Enqueue(newBoomerang);
            }
        }

        // projectile 개수만큼 부메랑 발사
        for (int i = 0; i < requiredBoomerangs; i++)
        {
            Vector3 direction;
            if (i < targets.Count)
            {
                // 타겟이 있는 경우 해당 방향으로 발사
                direction = (targets[i].position - transform.position).normalized;
            }
            else
            {
                // 타겟이 부족한 경우 랜덤 방향으로 발사
                float randomAngle = UnityEngine.Random.Range(0f, 360f);
                direction = new Vector3(Mathf.Cos(randomAngle * Mathf.Deg2Rad), 0, Mathf.Sin(randomAngle * Mathf.Deg2Rad));
            }
            ActivateBoomerang(direction);
        }
    }

    void ActivateBoomerang(Vector3 direction)
    {
        // 비활성화된 부메랑이 있는지 확인
        if (inactiveBoomerangs.Count > 0)
        {
            GameObject boom = inactiveBoomerangs.Dequeue();
            
            // 부메랑의 생성 위치 설정 (플레이어 위치)
            Vector3 spawnPos = transform.position;
            spawnPos.y = transform.position.y;
            boom.transform.position = spawnPos;
            
            // 공격 범위 계산
            SphereCollider sphereCollider = playerAttackRange.GetComponent<SphereCollider>();
            float attackRange = sphereCollider.radius * playerStatus.attackRange;
            
            // 부메랑 컴포넌트 가져와서 초기화
            Boomerang boomerangScript = boom.GetComponent<Boomerang>();
            
            // 이벤트 구독 해제 후 다시 구독
            boomerangScript.OnBoomerangReturn -= HandleBoomerangReturn;
            boomerangScript.OnBoomerangReturn += HandleBoomerangReturn;
            
            // 진화된 부메랑일 때만 피격 이벤트 구독
            if (isArousal)
            {
                boomerangScript.OnMonsterHit -= HandleMonsterHit;
                boomerangScript.OnMonsterHit += HandleMonsterHit;
            }

            // 피격 이펙트 이벤트 구독
            boomerangScript.OnMonsterHitEffect -= HandleMonsterHitEffect;
            boomerangScript.OnMonsterHitEffect += HandleMonsterHitEffect;
            
            // 최신 값으로 초기화
            boomerangScript.Initialize(transform, direction, player, weaponDamage, attackRange, isArousal, false);
            
            // 부메랑 활성화 및 활성화 리스트에 추가
            boom.SetActive(true);
            activeBoomerangs.Add(boom);
        }
    }

    void HandleBoomerangReturn(GameObject boomerang)
    {
        // 활성화된 부메랑 리스트에 있다면
        if (activeBoomerangs.Contains(boomerang))
        {
            activeBoomerangs.Remove(boomerang); // 활성화 리스트에서 제거
            boomerang.SetActive(false); // 비활성화
            inactiveBoomerangs.Enqueue(boomerang); // 비활성화 큐에 추가
        }
    }

    void HandleMonsterHit(Vector3 hitPosition)
    {
        // 이미 생성된 위치인지 확인
        if (spawnedPositions.ContainsKey(hitPosition))
        {
            return; // 이미 생성된 위치면 무시
        }

        // 새로운 위치에 대해 생성 시작
        spawnedPositions[hitPosition] = true;

        // 필요한 분열 부메랑 개수 확인
        int requiredSplitBoomerangs = 4;
        
        // 필요한 만큼 비활성화된 분열 부메랑이 있는지 확인
        if (inactiveSplitBoomerangs.Count < requiredSplitBoomerangs)
        {
            // 부족한 만큼 새로운 분열 부메랑 생성
            int needToCreate = requiredSplitBoomerangs - inactiveSplitBoomerangs.Count;
            for (int i = 0; i < needToCreate; i++)
            {
                GameObject newSplitBoomerang = Instantiate(weaponPrefab);
                newSplitBoomerang.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f); // 크기를 1/2로 설정
                // EnemyManager의 첫 번째 자식 오브젝트의 자식으로 설정
                newSplitBoomerang.transform.SetParent(EnemyManager.Instance.transform.GetChild(0));
                newSplitBoomerang.SetActive(false);
                inactiveSplitBoomerangs.Enqueue(newSplitBoomerang);
            }
        }

        // 범위 내 적 목록 가져오기
        List<Transform> targets = GetTargetsInRange();
        int targetIndex = 0;

        // 4개의 분열 부메랑 생성
        for (int i = 0; i < requiredSplitBoomerangs; i++)
        {
            Vector3 direction;
            if (targetIndex < targets.Count)
            {
                // 타겟이 있는 경우 해당 방향으로 발사
                direction = (targets[targetIndex].position - hitPosition).normalized;
                targetIndex++;
            }
            else
            {
                // 타겟이 부족한 경우 랜덤 방향으로 발사
                float randomAngle = UnityEngine.Random.Range(0f, 360f);
                direction = new Vector3(
                    Mathf.Cos(randomAngle * Mathf.Deg2Rad),
                    0,
                    Mathf.Sin(randomAngle * Mathf.Deg2Rad)
                );
            }

            // 비활성화된 분열 부메랑이 있는지 확인
            if (inactiveSplitBoomerangs.Count > 0)
            {
                GameObject splitBoomerang = inactiveSplitBoomerangs.Dequeue();

                // 분열 부메랑 위치 설정
                splitBoomerang.transform.position = hitPosition;

                // 공격 범위 계산
                SphereCollider sphereCollider = playerAttackRange.GetComponent<SphereCollider>();
                float attackRange = sphereCollider.radius * playerStatus.attackRange;

                // 분열 부메랑 초기화
                Boomerang boomerangScript = splitBoomerang.GetComponent<Boomerang>();
                
                // 이벤트 구독 해제 후 다시 구독
                boomerangScript.OnBoomerangReturn -= HandleSplitBoomerangReturn;
                boomerangScript.OnBoomerangReturn += HandleSplitBoomerangReturn;
                
                // 피격 이펙트 이벤트 구독
                boomerangScript.OnMonsterHitEffect -= HandleMonsterHitEffect;
                boomerangScript.OnMonsterHitEffect += HandleMonsterHitEffect;
                
                // 분열 부메랑의 데미지와 공격 범위를 1/2로 설정
                float splitWeaponDamage = weaponDamage * 0.5f;
                float splitAttackRange = attackRange * 0.5f;
                
                // 최신 값으로 초기화
                boomerangScript.Initialize(transform, direction, player, splitWeaponDamage, splitAttackRange, isArousal, true);

                // 분열 부메랑 활성화
                splitBoomerang.SetActive(true);
                activeSplitBoomerangs.Add(splitBoomerang);
            }
        }

        // 처리된 위치가 너무 많아지면 오래된 것부터 제거
        if (spawnedPositions.Count > 100)
        {
            spawnedPositions.Clear();
        }
    }

    void HandleSplitBoomerangReturn(GameObject boomerang)
    {
        // 활성화된 분열 부메랑 리스트에 있다면
        if (activeSplitBoomerangs.Contains(boomerang))
        {
            activeSplitBoomerangs.Remove(boomerang); // 활성화 리스트에서 제거
            boomerang.SetActive(false); // 비활성화
            inactiveSplitBoomerangs.Enqueue(boomerang); // 비활성화 큐에 추가
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
        hitEffects.Enqueue(effect); // 이펙트 풀에 추가
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

