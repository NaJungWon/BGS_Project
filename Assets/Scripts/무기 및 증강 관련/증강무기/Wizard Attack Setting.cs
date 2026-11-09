using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WizardAttackSetting : MonoBehaviour
{
    [Header("장착 플레이어")]
    public GameObject player;
    public PlayerStatus playerStatus;
    public UpgradeInGame upgradeInGame;
    public PlayerController playerController;

    [Header("발사체")]
    public GameObject weaponPrefab; //기본 무기
    
    [Header("이펙트")]
    public GameObject hitEffect; // 몬스터 피격 이펙트
    private Queue<GameObject> hitEffects = new Queue<GameObject>(); // 몬스터 피격 이펙트 풀
    private HashSet<Vector3> hitPositions = new HashSet<Vector3>(); // 이미 이펙트가 터진 위치 추적
    [Header("무기 설정")]
    public float weaponDamage;
    public float weaponCooldown; //공격 속도
    public float weaponProjecTile; // 한 번에 발사할 총알 개수
    public float waitRotate; //애니메이션 연동을 위한 딜레이시간
    public float orbitRadius = 1f; // 궤도 반지름
    public float projectileSpeed = 10f; // 투사체 속도
    public float projectileLifetime = 5f; // 투사체 생존 시간
    public bool isShoot; //발사 여부
    private float timer = 0f; // 타이머 변수 추가
    [Header("진화 여부")]
    public bool isArousal; //능력 진화 여부

    private float originCooldown;
    public float attackSpeedValue; //공속 증가 값

    public List<GameObject> activeProjectiles = new List<GameObject>(); //활성화된 투사체
    public Queue<GameObject> inactiveProjectiles = new Queue<GameObject>(); //비활성화된 투사체
    
    void Start()
    {
        // player를 이 스크립트가 컴포넌트된 부모 오브젝트로 설정
        player = transform.parent.parent.gameObject; // 부모 오브젝트에서 플레이어를 찾는 방법
        playerController = player.GetComponent<PlayerController>();
        playerStatus = player.GetComponent<PlayerStatus>();
        upgradeInGame = player.GetComponent<UpgradeInGame>();
        originCooldown = playerStatus.attackSpeed;
    }

    void Update()
    {
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
        if (playerController.isAttack && !isShoot)
        {
            timer += Time.deltaTime; // 타이머 증가
            if (timer >= waitRotate)
            {
                timer = 0f; // 타이머 초기화
                LaunchProjectile();
                isShoot = true;
            }
        }
        else if (!playerController.isAttack)
        {
            isShoot = false;
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

    private void LaunchProjectile()
    {
        // 마우스 방향 가져오기
        Vector3 mouseDirection = playerController.attackDirection * Vector3.forward;
        mouseDirection.y = 0f;
        mouseDirection.Normalize();

        // 총 투사체 개수 계산
        int totalProjectiles = 1 + (int)(weaponProjecTile + playerStatus.projecTile);

        // 필요한 발사체 개수만큼 비활성화된 발사체가 있는지 확인
        if (inactiveProjectiles.Count < totalProjectiles)
        {
            // 부족한 만큼 새로운 발사체 생성
            int needToCreate = totalProjectiles - inactiveProjectiles.Count;
            for (int i = 0; i < needToCreate; i++)
            {
                GameObject newProjectile = Instantiate(weaponPrefab, Vector3.zero, Quaternion.identity);
                // EnemyManager의 첫 번째 자식 오브젝트의 자식으로 설정
                newProjectile.transform.SetParent(EnemyManager.Instance.transform.GetChild(0));
                newProjectile.SetActive(false);
                inactiveProjectiles.Enqueue(newProjectile);
            }
        }

        // 모든 투사체를 궤도상에 생성
        for (int i = 0; i < totalProjectiles; i++)
        {
            // 각도 계산 (360도를 투사체 개수로 나눔)
            float angle = (360f / totalProjectiles) * i;
            
            // 궤도상의 위치 계산
            Vector3 orbitOffset = Quaternion.Euler(0, angle, 0) * mouseDirection * (orbitRadius * playerStatus.attackRange);
            Vector3 spawnPosition = player.transform.position + orbitOffset;
            spawnPosition.y = player.transform.position.y + 1f; // 플레이어 키 높이에 맞춤

            // 비활성화된 발사체 중 하나를 가져와서 활성화
            if (inactiveProjectiles.Count > 0)
            {
                GameObject projectile = inactiveProjectiles.Dequeue();

                projectile.transform.position = spawnPosition;
                projectile.transform.localScale = Vector3.one * playerStatus.attackRange;

                WizardBullet wizardBullet = projectile.GetComponent<WizardBullet>();
                // 첫 번째 투사체는 마우스 방향으로, 나머지는 궤도 방향으로 발사
                Vector3 direction = (i == 0) ? mouseDirection : Quaternion.Euler(0, angle, 0) * mouseDirection;
                wizardBullet.Initialize(player, direction, weaponDamage, projectileSpeed, projectileLifetime, isArousal);
                wizardBullet.OnProjectileDestroy += HandleProjectileDestroy;
                wizardBullet.OnMonsterHitEffect += HandleMonsterHitEffect;

                projectile.SetActive(true);
                activeProjectiles.Add(projectile);
            }
        }
    }

    private void HandleProjectileDestroy(GameObject projectile)
    {
        if (activeProjectiles.Contains(projectile))
        {
            activeProjectiles.Remove(projectile);
            projectile.SetActive(false);
            inactiveProjectiles.Enqueue(projectile);
        }
    }
}
