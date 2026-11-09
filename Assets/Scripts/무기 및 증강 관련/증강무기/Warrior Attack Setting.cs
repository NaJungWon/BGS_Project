using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarriorAttackSetting : MonoBehaviour
{
    [Header("장착 플레이어")]
    public GameObject player;
    public PlayerStatus playerStatus;
    public UpgradeInGame upgradeInGame;
    public PlayerController playerController;

    [Header("무기 설정")]
    public float weaponDamage; //무기 데미지
    public float weaponCooldown; //무기 공속
    public float weaponAttackRange; //무기 공격 범위
    public float waitRotate; //무기 회전 대기 시간
    public Collider attackCollider; //무기 콜라이더
    private float timer = 0f; // 타이머 변수 추가
    private List<MonsterStatus> hitMonsters = new List<MonsterStatus>(); // 현재 공격에 맞은 몬스터 리스트

    [Header("진화 여부")]
    public bool isArousal; //능력 진화 여부

    [Header("무기 이펙트 기본/진화")]
    public GameObject effect; // 기본 이펙트
    public GameObject arousalEffect; // 능력 진화 이펙트
    
    public GameObject hitEffect; // 몬스터 피격 이펙트
    private bool effectSpawned = false; // 이펙트 중복 생성을 막기 위한 플래그
    
    private float originCooldown;
    public float attackSpeedValue; //공속 증가 값

    private GameObject effectInstance; // effect 인스턴스 보관
    private GameObject arousalEffectInstance; // arousalEffect 인스턴스 보관
    private ParticleSystem effectParticle; // 파티클 시스템 참조
    private ParticleSystem arousalEffectParticle; // 진화 파티클 시스템 참조
    
    private Queue<GameObject> hitEffects = new Queue<GameObject>(); // 몬스터 피격 이펙트 풀

    void Start()
    {
        // player를 이 스크립트가 컴포넌트된 부모 오브젝트로 설정
        player = transform.parent.parent.gameObject; // 부모 오브젝트에서 플레이어를 찾는 방법
        upgradeInGame = player.GetComponent<UpgradeInGame>();
        playerController = player.GetComponent<PlayerController>();
        playerStatus = player.GetComponent<PlayerStatus>();
        // attackCollider를 현재 스크립트의 특정 콜라이더로 설정
        attackCollider = GetComponent<Collider>();
        originCooldown = playerStatus.attackSpeed;
        // effect를 this.gameObject의 자식으로 생성
        if (effect != null) //기본 이펙트 설정
        {
            effectInstance = Instantiate(effect, this.transform);
            effectParticle = effectInstance.GetComponent<ParticleSystem>();
            if (effectParticle != null)
                effectParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        if (arousalEffect != null) //진화 이펙트 설정
        {
            arousalEffectInstance = Instantiate(arousalEffect, this.transform);
            arousalEffectParticle = arousalEffectInstance.GetComponent<ParticleSystem>();
            if (arousalEffectParticle != null)
                arousalEffectParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    void Update()
    {
        this.gameObject.transform.localScale = Vector3.one * playerStatus.attackRange;

        // isArousal 상태에 따른 이펙트 전환
        if (isArousal && effectInstance != null)
        {
            Destroy(effectInstance);
            effectInstance = null;
            effectParticle = null;
        }

        // weaponCooldown 값이 변경되었는지 확인
        if (originCooldown != weaponCooldown)
        {
            originCooldown = weaponCooldown;
            if (upgradeInGame != null)
            {
                upgradeInGame.upgradeWeaponValues.weaponAttackSpeed = attackSpeedValue;
                upgradeInGame.PlayerStatusUpgradeSetting(
                    upgradeInGame.playerInfo,
                    upgradeInGame.upgradeWeaponValues,
                    upgradeInGame.upgradeAbilityValues,
                    upgradeInGame.abilityCardValues,
                    upgradeInGame.abilityPassiveValues
                );
            }
        }

        // Update 메서드에서 공격 상태에 따라 타이머 및 콜라이더 처리
        if (playerController.isAttack)
        {
            timer += Time.deltaTime; // 타이머 증가
            if (timer >= waitRotate)
            {
                timer = 0f; // 타이머 초기화
                // 현재 스크립트 오브젝트를 playerController의 attackDirection으로 회전
                transform.rotation = playerController.attackDirection;
                attackCollider.enabled = true; // Collider 활성화

                // === 공격 이펙트 파티클 Play ===
                if (!effectSpawned)
                {
                    if (isArousal && arousalEffectParticle != null)
                    {
                        arousalEffectParticle.Play();
                        // 진화 공격 이펙트 사운드 재생
                        AudioSource arousalAudio = arousalEffectInstance.GetComponent<AudioSource>();
                        if (arousalAudio != null)
                        {
                            arousalAudio.Play();
                        }
                    }
                    else if (effectParticle != null)
                    {
                        effectParticle.Play();
                        // 기본 공격 이펙트 사운드 재생
                        AudioSource effectAudio = effectInstance.GetComponent<AudioSource>();
                        if (effectAudio != null)
                        {
                            effectAudio.Play();
                        }
                    }
                    effectSpawned = true;
                }
            }
        }
        else if (!playerController.isAttack)
        {
            attackCollider.enabled = false; // 공격 상태가 아닐 때 Collider 비활성화
            if (effectSpawned)
            {
                if (isArousal && arousalEffectParticle != null)
                {
                    arousalEffectParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
                else if (effectParticle != null)
                {
                    effectParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }
            effectSpawned = false; // 공격이 끝나면 플래그 초기화
            hitMonsters.Clear(); // 맞은 몬스터 리스트 초기화
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

        // 비활성화된 이펙트가 없으면 새로 생성
        if (effectInstance == null)
        {
            effectInstance = Instantiate(hitEffect);
            // EnemyManager의 세 번째 자식 오브젝트의 자식으로 설정
            effectInstance.transform.SetParent(EnemyManager.Instance.transform.GetChild(3));
        }

        effectInstance.SetActive(false);
        return effectInstance;
    }

    private void OnTriggerEnter(Collider other)
    {
        // playerController의 isAttack이 true일 때만 트리거 활성화
        if (other.CompareTag("Monster"))
        {
            // MonsterStatus 컴포넌트 가져오기
            MonsterStatus monsterStatus = other.GetComponent<MonsterStatus>();
            if (monsterStatus != null && !hitMonsters.Contains(monsterStatus))
            {
                hitMonsters.Add(monsterStatus);
                monsterStatus.TakeDamage(weaponDamage, player); // 필요한 데미지 값을 인자로 전달할 수 있습니다.
                if(isArousal)
                {
                    playerStatus.currentHealth += ((weaponDamage * playerStatus.attack) / 100 * hitMonsters.Count);
                    if(playerStatus.currentHealth > playerStatus.maxHealth)
                    {
                        playerStatus.currentHealth = playerStatus.maxHealth;
                    }
                    playerStatus.playerSystem.playerUISetting.UpdateHealthUI(playerStatus);
                }
                
                // 피격 이펙트 재생
                GameObject hitEffectInstance = GetOrCreateHitEffect();
                if (hitEffectInstance != null)
                {
                    Vector3 effectPosition = monsterStatus.transform.position;
                    effectPosition.y = 1f; // y축을 1로 설정
                    hitEffectInstance.transform.position = effectPosition;
                    hitEffectInstance.SetActive(true);
                    
                    // 파티클 시스템 가져오기
                    ParticleSystem particleSystem = hitEffectInstance.GetComponent<ParticleSystem>();
                    if (particleSystem != null)
                    {
                        // 파티클 재생이 끝나면 자동으로 비활성화하는 코루틴 시작
                        StartCoroutine(DisableEffectAfterParticle(hitEffectInstance, particleSystem));
                    }

                    // AudioSource 재생
                    AudioSource audioSource = hitEffectInstance.GetComponent<AudioSource>();
                    if (audioSource != null)
                    {
                        audioSource.Play();
                    }
                }
            }
        }
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

        // 파티클 재생이 끝나면 이펙트 비활성화 (Destroy 대신)
        effect.SetActive(false);
        hitEffects.Enqueue(effect); // 이펙트 풀에 추가
    }
}
