using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomber : MonoBehaviour // (폭발 데미지 스킬)
{
    [Header("장착 플레이어")]
    public GameObject player;
    public PlayerStatus playerStatus;
    public UpgradeInGame upgradeInGame;

    [Header("발사체")]
    public GameObject weaponPrefab; //기본 무기

    [Header("무기 설정")]
    public float weaponDamage; //무기 데미지
    public float weaponCooldown = 20f; // 스킬 쿨다운
    public float disableCooldown = 2f; // 비활성화 쿨타임
    private float timer = 0f;
    private GameObject playerAttackRange; //플레이어 공격 범위
    
    [Header("진화 여부")]
    public bool isArousal; //능력 진화 여부

    [Header("이펙트")]
    public GameObject hitEffect; // 몬스터 피격 이펙트
    private Queue<GameObject> hitEffects = new Queue<GameObject>(); // 몬스터 피격 이펙트 풀

    // Start is called before the first frame update
    void Start()
    {
        player = transform.parent.parent.gameObject;
        playerStatus = player.GetComponent<PlayerStatus>();
        upgradeInGame = player.GetComponent<UpgradeInGame>();
        // projectilePrefab을 하나 생성해서 비활성화 상태로 보관
        weaponPrefab = Instantiate(weaponPrefab, transform.position, Quaternion.identity);
        // EnemyManager의 첫 번째 자식 오브젝트의 자식으로 설정
        weaponPrefab.transform.SetParent(EnemyManager.Instance.transform.GetChild(0));
        weaponPrefab.SetActive(false);
        weaponPrefab.transform.localScale = Vector3.one * playerStatus.attackRange;
        BomberBullet bulletScript = weaponPrefab.GetComponent<BomberBullet>();
        bulletScript.Initialize(player, weaponDamage);
        bulletScript.OnMonsterHitEffect += HandleMonsterHitEffect; // 이벤트 구독
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= (weaponCooldown / (1 + (upgradeInGame.abilityCardValues.attackSpeed + upgradeInGame.upgradeAbilityValues.attackSpeed)/100)))
        {
            //currentWeapon = weaponPrefab;
            if (!weaponPrefab.activeSelf)
            {
                GameObject closest = GetClosestEnemy();
                if (closest != null)
                {
                    weaponPrefab.transform.position = closest.transform.position;
                    weaponPrefab.transform.localScale = Vector3.one * playerStatus.attackRange;
                    // 발사체의 isArousal 값 업데이트
                    BomberBullet bulletScript = weaponPrefab.GetComponent<BomberBullet>();
                    if (bulletScript != null)
                    {
                        bulletScript.isArousal = isArousal;
                    }
                    weaponPrefab.SetActive(true);
                    StartCoroutine(DisableAfterSeconds(weaponPrefab, disableCooldown));
                }
            }
        }
    }

    GameObject GetClosestEnemy()
    {
        GameObject closest = null;
        float minDist = Mathf.Infinity;
        foreach (GameObject enemy in EnemyManager.Instance.enemies)
        {
            if (enemy == null) continue;
            float dist = Vector3.Distance(player.transform.position, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = enemy;
            }
        }
        return closest;
    }

    IEnumerator DisableAfterSeconds(GameObject obj, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        obj.SetActive(false);
        timer = 0f; // 비활성화 시점에 timer 초기화
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
