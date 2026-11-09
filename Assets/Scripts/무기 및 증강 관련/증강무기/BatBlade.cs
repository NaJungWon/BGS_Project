using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatBlade : MonoBehaviour
{
    [Header("장착 플레이어")]
    public GameObject player;
    public PlayerStatus playerStatus;
    public UpgradeInGame upgradeInGame;
    public Transform playerTransform;  // 플레이어의 좌표
    [Header("발사체")]
    public GameObject weaponPrefab; //기본 무기
    [Header("무기 데미지/ 투사체 개수/ 발사체 속도/ 생존 시간/ 반사 횟수")]
    public float weaponDamage; //무기데미지
    public float weaponProjecTile = 2; // 투사체 개수
    public float projectileSpeed = 10f; // 발사체 속도
    public float projectileLifetime = 3f; // 발사체의 최대 생존 시간
    public int maxReflections = 5; // 발사체의 최대 반사 횟수
    [Header("발사 속도")]
    public float weaponCooldown = 0.5f; //발사 쿨타임
    public float timer;

    [Header("진화 여부")]
    public bool isArousal; //능력 진화 여부

    public List<GameObject> activeProjectiles = new List<GameObject>(); //활성화된 투사체
    public Queue<GameObject> inactiveProjectiles = new Queue<GameObject>(); //비활성화된 투사체

    [Header("이펙트")]
    public GameObject hitEffect; // 몬스터 피격 이펙트
    public Queue<GameObject> hitEffects = new Queue<GameObject>(); // 몬스터 피격 이펙트 풀

    private void Start()
    {
        // player를 이 스크립트가 컴포넌트된 부모 오브젝트로 설정
        player = transform.parent.parent.gameObject;
        playerTransform = player.transform; // 플레이어의 Transform을 찾음
        playerStatus = player.GetComponent<PlayerStatus>();
        upgradeInGame = player.GetComponent<UpgradeInGame>();
    }

    private void Update()
    {
        timer += Time.deltaTime; // 쿨타임 감소
        // 쿨타임이 끝나면 발사체 발사
        if (timer >= (weaponCooldown / (1 + (upgradeInGame.abilityCardValues.attackSpeed + upgradeInGame.upgradeAbilityValues.attackSpeed)/100)))
        {
            timer = 0;
            LaunchProjectile();
        }
    }

    private void LaunchProjectile()
    {
        int requiredProjectiles = (int)weaponProjecTile + playerStatus.projecTile;

        // 필요한 발사체 개수만큼 비활성화된 발사체가 있는지 확인
        if (inactiveProjectiles.Count < requiredProjectiles)
        {
            // 부족한 만큼 새로운 발사체 생성
            int needToCreate = requiredProjectiles - inactiveProjectiles.Count;
            for (int i = 0; i < needToCreate; i++)
            {
                GameObject newProjectile = Instantiate(weaponPrefab, playerTransform.position, Quaternion.identity);
                // EnemyManager의 첫 번째 자식 오브젝트의 자식으로 설정
                newProjectile.transform.SetParent(EnemyManager.Instance.transform.GetChild(0));
                newProjectile.SetActive(false);
                inactiveProjectiles.Enqueue(newProjectile);
            }
        }

        for (int i = 0; i < requiredProjectiles; i++)
        {
            // 360도 중 랜덤한 방향으로 발사
            float randomAngle = Random.Range(0f, 360f);
            Vector3 direction = new Vector3(Mathf.Cos(randomAngle * Mathf.Deg2Rad), 0, Mathf.Sin(randomAngle * Mathf.Deg2Rad));

            // 비활성화된 발사체 중 하나를 가져와서 활성화
            if (inactiveProjectiles.Count > 0)
            {
                GameObject projectile = inactiveProjectiles.Dequeue();

                projectile.transform.position = playerTransform.position + Vector3.up * 0.5f;
                BatBladeProjectile batProjectile = projectile.GetComponent<BatBladeProjectile>();
                batProjectile.Initialize(player, direction, projectileSpeed, projectileLifetime, maxReflections, weaponDamage, isArousal);
                batProjectile.OnProjectileDestroy += HandleProjectileDestroy;
                batProjectile.OnMonsterHitEffect += HandleMonsterHitEffect; // 이펙트 이벤트 구독

                projectile.SetActive(true);
                activeProjectiles.Add(projectile);
            }
        }
    }

    private void HandleProjectileDestroy(GameObject projectile)
    {
        if (activeProjectiles.Contains(projectile))
        {
            BatBladeProjectile batProjectile = projectile.GetComponent<BatBladeProjectile>();
            batProjectile.OnProjectileDestroy -= HandleProjectileDestroy;
            batProjectile.OnMonsterHitEffect -= HandleMonsterHitEffect; // 이벤트 구독 해제

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
        hitEffects.Enqueue(effect); // 이펙트 풀에 추가
    }

    // 몬스터 피격 시 이펙트 처리
    public void HandleMonsterHitEffect(Vector3 position)
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
