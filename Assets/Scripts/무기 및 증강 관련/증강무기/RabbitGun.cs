using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RabbitGun : MonoBehaviour
{
    [Header("장착 플레이어")]
    public GameObject player;
    public PlayerStatus playerStatus;
    public UpgradeInGame upgradeInGame;
    public Transform playerTransform;  // 플레이어의 Transform (위치, 회전, 크기 정보)
    [Header("발사체")]
    public GameObject weaponPrefab; //기본 무기
    public GameObject weaponPrefabArousal; //진화 무기
    [Header("이펙트")]
    public GameObject hitEffect; // 몬스터 피격 이펙트
    public GameObject hitEffectArousal; // 몬스터 피격 이펙트

    private Queue<GameObject> hitEffects = new Queue<GameObject>(); // 몬스터 피격 이펙트 풀
    private Queue<GameObject> hitEffectsArousal = new Queue<GameObject>(); // 진화 이펙트 풀
    private HashSet<Vector3> hitPositions = new HashSet<Vector3>(); // 이미 이펙트가 터진 위치 추적

    [Header("무기 데미지/ 발사체 속도/ 지속시간")]
    public float weaponDamage;
    public float weaponCooldown; //스킬 쿨타임
    public float weaponDuration = 10f; //무기 지속시간
    private float timer;

    [Header("진화 여부")]
    public bool isArousal; //능력 진화 여부

    private GameObject weaponInstance;
    private GameObject weaponInstanceArousal;

    private void Start()
    {
        // player를 이 스크립트가 컴포넌트된 부모 오브젝트로 설정
        player = transform.parent.parent.gameObject;
        playerStatus = player.GetComponent<PlayerStatus>();
        upgradeInGame = player.GetComponent<UpgradeInGame>();
        playerTransform = player.transform;
        timer = 0f; // 처음엔 바로 발사 가능

        // rGrutePrefab을 자식으로 하나만 생성 후 비활성화
        if (weaponPrefab != null)
        {
            weaponInstance = Instantiate(weaponPrefab);
            // EnemyManager의 첫 번째 자식 오브젝트의 자식으로 설정
            weaponInstance.transform.SetParent(EnemyManager.Instance.transform.GetChild(0));
            weaponInstance.SetActive(false);
        }
        if (weaponPrefabArousal != null)
        {
            weaponInstanceArousal = Instantiate(weaponPrefabArousal);
            weaponInstanceArousal.transform.SetParent(EnemyManager.Instance.transform.GetChild(0));
            weaponInstanceArousal.SetActive(false);
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= (weaponCooldown / (1 + (upgradeInGame.abilityCardValues.attackSpeed + upgradeInGame.upgradeAbilityValues.attackSpeed)/100)))
        {
            if (isArousal && !weaponInstanceArousal.activeSelf)
            {
                // 진화 상태로 전환 시 기존 이펙트 정리
                CleanupHitEffects();
                
                // 진화 무기 활성화
                weaponInstanceArousal.transform.position = playerTransform.position;
                weaponInstanceArousal.SetActive(true);
                
                // RGrute에 필요한 정보 전달
                RGrute rGrute = weaponInstanceArousal.GetComponent<RGrute>();
                rGrute.player = player;
                rGrute.weaponDamage = weaponDamage;
                rGrute.attackDuration = weaponDuration * playerStatus.duration;
                rGrute.isArousal = isArousal;
                rGrute.OnMonsterHitEffect += HandleMonsterHitEffect;
                
                StartCoroutine(DeactivateWeaponAfterTime(weaponInstanceArousal, weaponDuration * playerStatus.duration));
            }
            else if (!isArousal && !weaponInstance.activeSelf)
            {
                // 일반 무기 활성화
                weaponInstance.transform.position = playerTransform.position;
                weaponInstance.SetActive(true);
                
                // RGrute에 필요한 정보 전달
                RGrute rGrute = weaponInstance.GetComponent<RGrute>();
                rGrute.player = player;
                rGrute.weaponDamage = weaponDamage;
                rGrute.attackDuration = weaponDuration * playerStatus.duration;
                rGrute.isArousal = isArousal;
                rGrute.OnMonsterHitEffect += HandleMonsterHitEffect;
                
                StartCoroutine(DeactivateWeaponAfterTime(weaponInstance, weaponDuration * playerStatus.duration));
            }
        }
    }

    private IEnumerator DeactivateWeaponAfterTime(GameObject weapon, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (weapon != null)
        {
            timer = 0f;
            weapon.SetActive(false);
        }
    }

    // 기존 이펙트 정리 (비활성화된 이펙트만)
    private void CleanupHitEffects()
    {
        if (hitEffects.Count == 0) return;  // 큐가 비어있으면 종료
        Queue<GameObject> tempQueue = new Queue<GameObject>();
        while (hitEffects.Count > 0)
        {
            GameObject effect = hitEffects.Dequeue();
            if (effect != null && !effect.activeSelf)
            {
                Destroy(effect);
            }
            else if (effect != null)
            {
                tempQueue.Enqueue(effect);
            }
        }
        hitEffects = tempQueue;
    }

    // 피격 이펙트 풀링 관리
    private GameObject GetOrCreateHitEffect()
    {
        if (isArousal)
        {
            GameObject effectInstance = null;
            while (hitEffectsArousal.Count > 0 && effectInstance == null)
            {
                effectInstance = hitEffectsArousal.Dequeue();
            }
            if (effectInstance == null)
            {
                effectInstance = Instantiate(hitEffectArousal);
                effectInstance.transform.SetParent(EnemyManager.Instance.transform.GetChild(3));
            }
            effectInstance.SetActive(false);
            return effectInstance;
        }
        else
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
        if (isArousal)
        {
            hitEffectsArousal.Enqueue(effect); // 이펙트 풀에 추가
        }
        else
        {
            hitEffects.Enqueue(effect); // 이펙트 풀에 추가
        }
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
