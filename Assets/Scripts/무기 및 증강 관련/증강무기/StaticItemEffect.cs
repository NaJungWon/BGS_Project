using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;

public class StaticItemEffect : MonoBehaviour
{
    [Header("장착 플레이어")]
    public GameObject player; // 이 효과를 가진 무기를 장착한 플레이어 오브젝트
    public PlayerStatus playerStatus;
    public UpgradeInGame upgradeInGame;
    [Header("이펙트")]
    public GameObject effect; // 기본 이펙트
    public GameObject effectArousal; // 진화 이펙트
    
    private Queue<GameObject> hitEffects = new Queue<GameObject>(); // 몬스터 피격 이펙트 풀
    private Queue<GameObject> hitEffectsArousal = new Queue<GameObject>(); // 진화 이펙트 풀

    [Header("무기 데미지/ 쿨타임/ 튕기는 거리/ 튕기는 횟수")]
    public float weaponDamage; // 무기의 기본 데미지
    public float weaponCooldown = 2f; // 체인 라이트닝이 발동되는 쿨타임(초)
    public float weaponProjecTile = 3; // 번개가 튈 수 있는 최대 횟수

    private GameObject playerAttackRange; //플레이어 공격 범위

    private float timer = 0f; // 쿨타임 측정용 타이머
    public bool canSplash; //스태틱 효과 발동 가능

    [Header("진화 여부")]
    public bool isArousal; //능력 진화 여부

    private Dictionary<GameObject, float> prevHealths = new Dictionary<GameObject, float>(); // 이전 체력값 저장용

    void Start()
    {
        // 이 스크립트가 붙은 오브젝트의 부모의 부모를 player로 설정
        player = transform.parent.parent.gameObject;
        playerStatus = player.GetComponent<PlayerStatus>();
        upgradeInGame = player.GetComponent<UpgradeInGame>();
        playerAttackRange = player.GetComponent<PlayerSystem>().playerAttackRange;
        canSplash = true; // 시작 시 true로 설정
    }

    void Update()
    {
        // 1. 체력 감소 감지 부분
        // EnemyManager의 모든 적을 순회하면서 체력 변화를 감지
        foreach (GameObject enemy in EnemyManager.Instance.enemies.ToArray())
        {
            if (enemy == null) continue;
            MonsterStatus status = enemy.GetComponent<MonsterStatus>();
            if (status == null) continue;

            float prev;
            prevHealths.TryGetValue(enemy, out prev);
            // 체력이 이전보다 감소했을 때 체인 라이트닝 발동
            if (status.currentHealth < prev)
            {
                if (canSplash)
                {
                    canSplash = false;
                    timer = 0f;
                    ChainLightning(enemy, false); // false: 데미지를 주는 체인 라이트닝
                }
            }
            // 현재 체력값을 이전 체력값으로 저장
            prevHealths[enemy] = status.currentHealth;
        }

        // 2. 몬스터 비활성화 감지 부분
        // 이전 프레임에 있던 몬스터가 현재 비활성화되었는지 확인
        foreach (GameObject enemy in prevHealths.Keys.ToArray())
        {
            if (!enemy.activeInHierarchy)
            {
                if (canSplash)
                {
                    canSplash = false;
                    timer = 0f;
                    ChainLightning(enemy, true); // true: 데미지를 주지 않는 체인 라이트닝
                }
                prevHealths.Remove(enemy); // 비활성화된 몬스터는 리스트에서 제거
            }
        }

        // 3. 쿨타임 관리
        // 체인 라이트닝이 발동된 후 쿨타임 적용
        if (!canSplash)
        {
            timer += Time.deltaTime;
            if (timer >= (weaponCooldown / (1 + (upgradeInGame.abilityCardValues.attackSpeed + upgradeInGame.upgradeAbilityValues.attackSpeed)/100)))
            {
                canSplash = true;
                timer = 0f;
            }
        }

        // 4. 진화 상태에 따른 이펙트 정리
        if (isArousal)
        {
            CleanupNormalEffects();
        }
    }

    // 일반 이펙트 정리
    private void CleanupNormalEffects()
    {
        int count = hitEffects.Count;
        for (int i = 0; i < count; i++)
        {
            GameObject effect = hitEffects.Dequeue();
            if (effect == null) continue;

            if (!effect.activeSelf)
            {
                Destroy(effect);
            }
            else
            {
                hitEffects.Enqueue(effect);
            }
        }
    }

    // EnemyManager에 등록된 적들 중에서 가장 가까운 적을 찾는 함수
    GameObject FindClosestEnemy()
    {
        GameObject closest = null;
        float minDist = Mathf.Infinity;

        foreach (GameObject enemy in EnemyManager.Instance.enemies)
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = enemy;
            }
        }

        return closest;
    }

    // 피격 이펙트 풀링 관리
    private GameObject GetOrCreateHitEffect(out Queue<GameObject> targetQueue)
    {
        // 진화 상태에 따라 사용할 이펙트와 큐 선택
        GameObject targetEffect = isArousal ? effectArousal : effect;
        targetQueue = isArousal ? hitEffectsArousal : hitEffects;

        GameObject effectInstance = null;

        while (targetQueue.Count > 0 && effectInstance == null)
        {
            effectInstance = targetQueue.Dequeue();
        }

        if (effectInstance == null)
        {
            effectInstance = Instantiate(targetEffect);
            // EnemyManager의 첫 번째 자식 오브젝트의 자식으로 설정
            effectInstance.transform.SetParent(EnemyManager.Instance.transform.GetChild(3));
        }

        effectInstance.SetActive(false);
        return effectInstance;
    }

    // 파티클 재생이 끝나면 이펙트를 비활성화하는 코루틴
    private IEnumerator DisableEffectAfterParticle(GameObject effect, ParticleSystem particleSystem, Queue<GameObject> pool)
    {
        // 파티클 재생
        particleSystem.Play();

        // 파티클이 재생 중인 동안 대기
        while (particleSystem.isPlaying)
        {
            yield return null;
        }

        // 파티클 재생이 끝나면 이펙트 비활성화 후 풀에 반환
        effect.SetActive(false);
        pool?.Enqueue(effect);
    }

    // 몬스터 피격 시 이펙트 처리
    private void HandleMonsterHitEffect(Vector3 position)
    {
        // 피격 이펙트 재생
        Queue<GameObject> targetQueue;
        GameObject hitEffectInstance = GetOrCreateHitEffect(out targetQueue);
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
                StartCoroutine(DisableEffectAfterParticle(hitEffectInstance, particleSystem, targetQueue));
            }
        }
    }

    // 체인 라이트닝 효과를 실행하는 함수
    // startTarget: 체인 라이트닝이 시작되는 몬스터
    void ChainLightning(GameObject startTarget, bool isDeadTarget)
    {
        List<GameObject> hit = new List<GameObject>(); // 이미 체인 라이트닝이 튄 몬스터 리스트
        GameObject current = startTarget;
        // 마지막 위치 저장 (몬스터가 비활성화된 경우를 대비)
        Vector3 lastPosition = startTarget != null ? startTarget.transform.position : transform.position;
        int chainCount = 0; // 실제 체인 라이트닝이 튄 횟수 카운트

        // weaponProjecTile + playerStatus.projecTile 횟수만큼 체인 라이트닝 발동
        while (chainCount <= (int)weaponProjecTile + playerStatus.projecTile)
        {
            // 현재 타겟이 비활성화된 몬스터인 경우
            if (current == null || !current.activeInHierarchy)
            {
                // 이펙트 생성 및 카운트 증가
                if ((isArousal && effectArousal != null) || (!isArousal && effect != null))
                {
                    HandleMonsterHitEffect(lastPosition);
                }
                chainCount++;
                
                // 다음 타겟 찾기
                current = FindNextEnemy(lastPosition, hit);
                if (current == null) break;
                continue;
            }

            hit.Add(current); // 현재 몬스터를 체인 라이트닝이 튄 리스트에 추가
            Damage(current); // 데미지와 이펙트 적용
            chainCount++;

            lastPosition = current.transform.position; // 현재 위치를 마지막 위치로 저장
            current = FindNextEnemy(current.transform.position, hit); // 다음 타겟 찾기
        }
    }

    // 다음으로 체인 라이트닝이 튈 몬스터를 찾는 함수
    // pos: 현재 체인 라이트닝이 튄 위치
    // hit: 이미 체인 라이트닝이 튄 몬스터 리스트
    GameObject FindNextEnemy(Vector3 pos, List<GameObject> hit)
    {
        GameObject next = null;
        float minDist = Mathf.Infinity;
        SphereCollider sphereCollider = playerAttackRange.GetComponent<SphereCollider>();
        float attackRange = sphereCollider.radius * playerStatus.attackRange;

        // 모든 몬스터를 순회하면서 가장 가까운 다음 타겟 찾기
        foreach (GameObject enemy in EnemyManager.Instance.enemies)
        {
            // 비활성화되었거나 이미 체인 라이트닝이 튄 몬스터는 제외
            if (enemy == null || !enemy.activeInHierarchy || hit.Contains(enemy)) continue;

            float dist = Vector3.Distance(pos, enemy.transform.position);
            // attackRange 이내의 가장 가까운 몬스터 선택
            if (dist < minDist && dist <= attackRange)
            {
                minDist = dist;
                next = enemy;
            }
        }

        return next;
    }

    // 적 오브젝트에 MonsterStatus가 있으면, 무기 데미지와 플레이어 정보를 넘겨 데미지를 입힘
    void Damage(GameObject enemy)
    {
        MonsterStatus monster = enemy.GetComponent<MonsterStatus>();
        if (monster != null)
        {
            float finalDamage = isArousal ? weaponDamage * 2 : weaponDamage;
            monster.TakeDamage(finalDamage, player); // 무기 데미지와 플레이어 정보를 전달하여 데미지 적용

            if ((isArousal && effectArousal != null) || (!isArousal && effect != null))
            {
                HandleMonsterHitEffect(enemy.transform.position);
            }
        }
    }
}
