using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldTest : MonoBehaviour
{
    [Header("장착 플레이어")]
    public GameObject player;         // 플레이어 오브젝트 참조
    public PlayerStatus playerStatus;
    public UpgradeInGame upgradeInGame;

    [Header("이펙트")]
    public GameObject effect; // 기본 이펙트
    public GameObject arousalEffect; // 능력 진화 이펙트
    public GameObject effectBoom; // 폭발 이펙트
    public GameObject arousalEffectBoom; // 능력 진화 폭발 이펙트

    public GameObject hitEffect; // 몬스터 피격 이펙트
    public GameObject arousalHitEffect; // 능력 진화 몬스터 피격 이펙트

    [Header("무기 설정")]
    public float weaponDamage;              // 무기 데미지
    public float weaponCooldown = 5;             // 데미지 반복 간격(초)
    
    [Header("진화 여부")]
    public bool isArousal; //능력 진화 여부

    private GameObject effectInstance;
    private GameObject effectBoomInstance;
    private GameObject arousalEffectInstance;
    private GameObject arousalEffectBoomInstance;
    
    public Queue<GameObject> hitEffects = new Queue<GameObject>(); // 몬스터 피격 이펙트 풀
    public Queue<GameObject> arousalHitEffects = new Queue<GameObject>(); // 능력 진화 몬스터 피격 이펙트 풀
    private List<MonsterStatus> monstersInArea = new List<MonsterStatus>(); // 영역 내 몬스터 리스트
    private float shieldTimer = 0f; // 쉴드 재생성 타이머
    private bool prevCanShield = true; // 이전 프레임의 canShield 값

    // Start는 스크립트가 활성화될 때 한 번 실행됨
    void Start()
    {
        // player를 이 스크립트가 붙은 오브젝트의 두 단계 위 부모로 설정
        player = transform.parent.parent.gameObject;
        playerStatus = player.GetComponent<PlayerStatus>();
        upgradeInGame = player.GetComponent<UpgradeInGame>();
        playerStatus.canShield = true;

        // 모든 이펙트 인스턴스 생성
        effectInstance = Instantiate(effect, transform.parent);
        effectBoomInstance = Instantiate(effectBoom, this.transform);
        arousalEffectInstance = Instantiate(arousalEffect, transform.parent);
        arousalEffectBoomInstance = Instantiate(arousalEffectBoom, this.transform);

        // 모든 이펙트 비활성화
        effectInstance.SetActive(false);
        arousalEffectInstance.SetActive(false);
    }

    // Update는 매 프레임마다 실행됨
    void Update()
    {
        this.transform.localScale = Vector3.one * playerStatus.attackRange;

        // isArousal이 true가 되면 기본 이펙트들 삭제
        if (isArousal && hitEffects.Count > 0)
        {
            ClearEffectPool(hitEffects);
        }

        // 기본
        if (playerStatus.canShield)  // 쉴드가 활성화된 상태일 때만 이펙트 재생
        {
            // isArousal 값에 따라 다른 이펙트 활성화
            if (!isArousal)
            {
                // 기본 이펙트 활성화
                if (effectInstance != null && !effectInstance.activeSelf)
                {
                    effectInstance.SetActive(true);
                }
                // 진화 이펙트 비활성화
                if (arousalEffectInstance != null && arousalEffectInstance.activeSelf)
                {
                    arousalEffectInstance.SetActive(false);
                }
            }
            else
            {
                // 진화 이펙트 활성화
                if (arousalEffectInstance != null && !arousalEffectInstance.activeSelf)
                {
                    arousalEffectInstance.SetActive(true);
                }
                // 기본 이펙트 비활성화
                if (effectInstance != null && effectInstance.activeSelf)
                {
                    effectInstance.SetActive(false);
                }
            }
        }
        else  // 쉴드가 비활성화된 상태일 때는 모든 이펙트 중지
        {
            // 모든 이펙트 비활성화
            if (effectInstance != null)
            {
                effectInstance.SetActive(false);
            }
            if (arousalEffectInstance != null)
            {
                arousalEffectInstance.SetActive(false);
            }
        }

        // canShield가 true에서 false로 바뀌는 순간에만 데미지 부여 및 이펙트 처리
        if (prevCanShield && !playerStatus.canShield)
        {
            // isArousal 값에 따라 다른 폭발 이펙트 재생
            if (!isArousal)
            {
                // 기본 폭발 이펙트 재생
                if (effectBoomInstance != null)
                {
                    effectBoomInstance.GetComponent<ParticleSystem>().Play();
                    // 기본 폭발 이펙트 사운드 재생
                    AudioSource boomAudio = effectBoomInstance.GetComponent<AudioSource>();
                    if (boomAudio != null)
                    {
                        boomAudio.Play();
                    }
                }
            }
            else
            {
                // 진화 폭발 이펙트 재생
                if (arousalEffectBoomInstance != null)
                {
                    arousalEffectBoomInstance.GetComponent<ParticleSystem>().Play();
                    // 진화 폭발 이펙트 사운드 재생
                    AudioSource arousalBoomAudio = arousalEffectBoomInstance.GetComponent<AudioSource>();
                    if (arousalBoomAudio != null)
                    {
                        arousalBoomAudio.Play();
                    }
                }
            }

            GiveDamageToAllMonstersInArea();
        }

        // 쉴드가 꺼져있으면 타이머 동작
        if (!playerStatus.canShield)
        {
            shieldTimer += Time.deltaTime;
            if (shieldTimer >= (weaponCooldown / (1 + (upgradeInGame.abilityCardValues.attackSpeed + upgradeInGame.upgradeAbilityValues.attackSpeed)/100)))
            {
                //Debug.Log((weaponCooldown / (1 + (upgradeInGame.abilityCardValues.attackSpeed + upgradeInGame.upgradeAbilityValues.attackSpeed)/100)));
                playerStatus.canShield = true;
                shieldTimer = 0f;
            }
        }
        else
        {
            shieldTimer = 0f;
        }

        // prevCanShield 값 갱신
        prevCanShield = playerStatus.canShield;
    }

    // 영역 내 모든 몬스터에게 데미지 1회 부여
    private void GiveDamageToAllMonstersInArea()
    {
        // 이미 처리된 몬스터를 저장할 HashSet
        HashSet<MonsterStatus> processedMonsters = new HashSet<MonsterStatus>();

        foreach (var monster in monstersInArea)
        {
            if (monster != null && monster.gameObject.activeInHierarchy && !processedMonsters.Contains(monster))
            {
                // 데미지 부여
                monster.TakeDamage(weaponDamage, player);

                // 피격 이펙트 재생
                GameObject hitEffectInstance = GetOrCreateHitEffect();
                if (hitEffectInstance != null)
                {
                    // 몬스터의 실제 위치에 y좌표만 1로 설정
                    Vector3 effectPosition = new Vector3(
                        monster.transform.position.x,
                        1f,
                        monster.transform.position.z
                    );
                    hitEffectInstance.transform.position = effectPosition;
                    hitEffectInstance.SetActive(true);
                    
                    // 파티클 시스템 가져오기
                    ParticleSystem particleSystem = hitEffectInstance.GetComponent<ParticleSystem>();
                    if (particleSystem != null)
                    {
                        // 파티클 재생이 끝나면 자동으로 비활성화하는 코루틴 시작
                        StartCoroutine(DisableEffectAfterParticle(hitEffectInstance, particleSystem, isArousal));
                    }
                }

                // 처리된 몬스터 추가
                processedMonsters.Add(monster);

                // isArousal이 true일 때 몬스터의 이동속도 감소
                if (isArousal)
                {
                    StartCoroutine(ReduceMonsterSpeed(monster));
                }
            }
        }
    }

    // 피격 이펙트 풀링 관리
    private GameObject GetOrCreateHitEffect()
    {
        Queue<GameObject> targetPool = isArousal ? arousalHitEffects : hitEffects;
        GameObject prefab = isArousal ? arousalHitEffect : hitEffect;

        GameObject effectInstance = null;

        while (targetPool.Count > 0 && effectInstance == null)
        {
            effectInstance = targetPool.Dequeue();
        }

        if (effectInstance == null)
        {
            effectInstance = Instantiate(prefab);
            effectInstance.transform.SetParent(EnemyManager.Instance.transform.GetChild(3));
        }

        effectInstance.SetActive(false);
        return effectInstance;
    }

    // 파티클 재생이 끝나면 이펙트를 비활성화하는 코루틴
    private IEnumerator DisableEffectAfterParticle(GameObject effect, ParticleSystem particleSystem, bool isArousalEffect)
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
        EnqueueEffect(effect, isArousalEffect);
    }

    private void EnqueueEffect(GameObject effect, bool isArousalEffect)
    {
        if (effect == null) return;
        Queue<GameObject> targetPool = isArousalEffect ? arousalHitEffects : hitEffects;
        targetPool.Enqueue(effect);
    }

    private void ClearEffectPool(Queue<GameObject> pool)
    {
        while (pool.Count > 0)
        {
            GameObject effect = pool.Dequeue();
            if (effect != null)
            {
                Destroy(effect);
            }
        }
    }

    // 몬스터의 이동속도를 일시적으로 감소시키는 코루틴
    private IEnumerator ReduceMonsterSpeed(MonsterStatus monster)
    {
        float originalSpeed = monster.moveSpeed;
        float reducedSpeed = originalSpeed * 0.7f; // 30% 감소

        // 이동속도 감소
        monster.moveSpeed = reducedSpeed;

        // 3초 대기
        yield return new WaitForSeconds(3f * playerStatus.duration);

        // 이동속도 복구
        if (monster != null && monster.gameObject.activeInHierarchy)
        {
            monster.moveSpeed = originalSpeed;
        }
    }

    // 몬스터가 영역에 들어오면 리스트에 추가
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Monster"))
        {
            MonsterStatus monsterStatus = other.GetComponent<MonsterStatus>();
            if (monsterStatus != null && !monstersInArea.Contains(monsterStatus))
            {
                monstersInArea.Add(monsterStatus);
            }
        }
    }

    // 몬스터가 영역에서 나가면 리스트에서 제거
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Monster"))
        {
            MonsterStatus monsterStatus = other.GetComponent<MonsterStatus>();
            if (monsterStatus != null && monstersInArea.Contains(monsterStatus))
            {
                monstersInArea.Remove(monsterStatus);
            }
        }
    }
}
