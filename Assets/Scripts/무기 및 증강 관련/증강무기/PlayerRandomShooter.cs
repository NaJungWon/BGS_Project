using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerRandomShooter : MonoBehaviour
{
    [Header("장착 플레이어")]
    public GameObject player;
    public PlayerStatus playerStatus;
    public UpgradeInGame upgradeInGame;

    [Header("발사체")]
    public GameObject weaponPrefab; //기본 무기

    [Header("무기 설정")]
    public float weaponDamage;
    public float weaponCooldown = 5f;     // 발사 쿨타임(초)
    private float cooldownTimer = 0f;   // 쿨타임 타이머
    public float weaponProjecTile = 5;      // 한 번에 발사할 총알 개수
    public float arcAngle = 40f;        // 퍼지는 각도(도)
    public float projectileSpeed = 15f; // 투사체 속도
    
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
        player = transform.parent.parent.gameObject;
        playerStatus = player.GetComponent<PlayerStatus>();
        upgradeInGame = player.GetComponent<UpgradeInGame>();
        playerAttackRange = player.GetComponent<PlayerSystem>().playerAttackRange;
    }

    void Update()
    {
        cooldownTimer += Time.deltaTime;
        if (cooldownTimer >= (weaponCooldown / (1 + (upgradeInGame.abilityCardValues.attackSpeed + upgradeInGame.upgradeAbilityValues.attackSpeed)/100)))
        {
            FireArc();
            cooldownTimer = 0f;
        }
    }

    // 무작위 방향을 기준으로 총알을 퍼뜨리는 함수
    void FireArc()
    {
        // 기준 방향: 무작위
        Vector3 baseDirection = Random.insideUnitSphere;
        baseDirection.y = 0f;
        baseDirection.Normalize();

        // 총 투사체 개수 계산
        int totalProjectiles;
        float currentArcAngle;
        
        if (isArousal)
        {
            // 진화 시: 투사체 +30, arcAngle 360도
            totalProjectiles = (int)(weaponProjecTile + playerStatus.projecTile + 30);
            currentArcAngle = 360f;
        }
        else
        {
            // 기존 코드 유지
            totalProjectiles = (int)(weaponProjecTile + playerStatus.projecTile);
            currentArcAngle = arcAngle * playerStatus.attackRange;
        }

        float startAngle = -currentArcAngle / 2f;
        float angleStep = currentArcAngle / (totalProjectiles - 1);

        // attackRange 계산
        SphereCollider sphereCollider = playerAttackRange.GetComponent<SphereCollider>();
        float attackRange = sphereCollider.radius * playerStatus.attackRange;

        for (int i = 0; i < totalProjectiles; i++)
        {
            float angle = startAngle + angleStep * i;
            Vector3 dir = Quaternion.Euler(0, angle, 0) * baseDirection;

            // 비활성화된 투사체가 있는지 확인
            if (inactiveProjectiles.Count == 0)
            {
                // 새로운 투사체 생성
                GameObject newProjectile = Instantiate(weaponPrefab, transform.position, Quaternion.LookRotation(dir));
                // EnemyManager의 첫 번째 자식 오브젝트의 자식으로 설정
                newProjectile.transform.SetParent(EnemyManager.Instance.transform.GetChild(0));
                newProjectile.SetActive(false);
                inactiveProjectiles.Enqueue(newProjectile);
            }

            // 비활성화된 투사체 중 하나를 가져와서 활성화
            GameObject projectile = inactiveProjectiles.Dequeue();

            projectile.transform.position = transform.position;
            projectile.transform.rotation = Quaternion.LookRotation(dir);

            IciclesShooting shooting = projectile.GetComponent<IciclesShooting>();
            if (shooting != null)
            {
                shooting.Initialize(player, weaponDamage, attackRange, projectileSpeed, isArousal);
                shooting.OnProjectileDestroy += HandleProjectileDestroy;
                shooting.OnMonsterHitEffect += HandleMonsterHitEffect;
            }

            projectile.SetActive(true);
            activeProjectiles.Add(projectile);
        }
    }

    private void HandleProjectileDestroy(GameObject projectile)
    {
        if (activeProjectiles.Contains(projectile))
        {
            // 이벤트 구독 해제
            IciclesShooting shooting = projectile.GetComponent<IciclesShooting>();
            if (shooting != null)
            {
                shooting.OnProjectileDestroy -= HandleProjectileDestroy;
                shooting.OnMonsterHitEffect -= HandleMonsterHitEffect;
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
