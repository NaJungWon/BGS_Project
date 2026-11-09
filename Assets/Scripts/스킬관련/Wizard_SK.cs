using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wizard_SK : MonoBehaviour
{
    [Header("플레이어 관련")]
    public GameObject player;
    public PlayerSystem playerSystem;
    public PlayerStatus playerStatus;
    public AimingSetting aimSet;
    public PlayerController playerController;
    public Animator anim;

    //public Sprite passiveImage;

    [Header("E 스킬 관련")]
    public GameObject magicBullet;
    public GameObject eSkillHitEffect; // E스킬 몬스터 피격 이펙트
    private List<GameObject> magicBulletPool = new List<GameObject>(); // E스킬 매직 불릿 풀
    private Queue<GameObject> eSkillHitEffects = new Queue<GameObject>(); // E스킬 몬스터 피격 이펙트 풀
    private HashSet<Vector3> eSkillHitPositions = new HashSet<Vector3>(); // E스킬 이미 이펙트가 터진 위치 추적
    public Sprite eSkillImageBG;
    public Sprite eSkillImage;
    public float eSkillDamage; //e스킬 데미지 기본계수
    public float eSkillCooldown = 5f; // E 스킬 쿨다운 시간
    public float eSkillDelay = 1f; // E 스킬 지연 시간
    public float eSkillAnimSpeed = 1f; // E 스킬 애니메이션 재생 속도

    [Header("Q 스킬 관련")]
    public GameObject qSkillEffect;
    public GameObject qSkillHitEffect; // Q스킬 몬스터 피격 이펙트
    private Queue<GameObject> qSkillHitEffects = new Queue<GameObject>(); // Q스킬 몬스터 피격 이펙트 풀
    private HashSet<Vector3> qSkillHitPositions = new HashSet<Vector3>(); // Q스킬 이미 이펙트가 터진 위치 추적
    public Sprite qSkillImageBG;
    public Sprite qSkillImage;
    public float qSkillDamage; //q스킬 데미지
    public float qSkillRadius = 5f; // Q 스킬 범위 반경
    public float qSkillCooldown = 10f; // Q 스킬 쿨다운 시간
    public float qSkillDelay = 1f; // Q 스킬 지연 시간
    public float qSkillAnimSpeed = 1f; // Q 스킬 애니메이션 재생 속도
    private Vector3 qSkillTargetPosition; // Q 스킬의 타겟 위치

    public float currentQCooldown = 0f; // Q 스킬 현재 쿨다운
    public float currentECooldown = 0f; // E 스킬 현재 쿨다운

    private GameObject qSkillEffectInstance;
    private ParticleSystem qSkillParticle;

    // Start is called before the first frame update
    void Start()
    {
        // 현재 스크립트가 컴포넌트된 게임오브젝트의 부모 오브젝트를 player로 설정
        player = transform.parent.gameObject;
        playerStatus = player.GetComponent<PlayerStatus>();
        aimSet = player.GetComponent<AimingSetting>();
        playerController = player.GetComponent<PlayerController>();
        playerSystem = player.GetComponent<PlayerSystem>();
        anim = player.transform.GetChild(0).GetComponent<Animator>();
        //qSkillEffect = Resources.Load<GameObject>("WizardQ_Effect");
        //eSkillEffect = Resources.Load<GameObject>("WizardE_Effect");

        // qSkillEffect, eSkillEffect를 자식으로 생성 및 파티클 참조 저장
        if (qSkillEffect != null)
        {
            qSkillEffectInstance = Instantiate(qSkillEffect, this.transform);
            qSkillParticle = qSkillEffectInstance.GetComponent<ParticleSystem>();
            if (qSkillParticle != null)
                qSkillParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 쿨다운 감소
        if (currentQCooldown > 0f)
        {
            currentQCooldown -= Time.deltaTime;
        }
        if (currentECooldown > 0f)
        {
            currentECooldown -= Time.deltaTime;
        }

        // 쿨다운 UI 업데이트
        if (playerSystem != null && playerSystem.playerUISetting != null)
        {
            playerSystem.playerUISetting.UpdateSkillCooldownUI(gameObject);
        }
        
        UseSkill();
    }

    void UseSkill()
    {
        if (Input.GetKeyDown(KeyCode.E) && currentECooldown <= 0f)
        {
            StartCoroutine(WizardESkill());
            currentECooldown = eSkillCooldown; // E 스킬 쿨다운 설정
            Debug.Log("E 스킬 발동");
        }
        if (Input.GetKeyDown(KeyCode.Q) && currentQCooldown <= 0f)
        {
            StartCoroutine(WizardQSkill());
            currentQCooldown = qSkillCooldown; // Q 스킬 쿨다운 설정
            Debug.Log("Q 스킬 발동");
        }
    }

    // E스킬 매직 불릿 풀링 관리
    private GameObject GetOrCreateMagicBullet()
    {
        // 비활성화된 불릿 찾기
        foreach (GameObject bullet in magicBulletPool)
        {
            if (!bullet.activeSelf)
            {
                return bullet;
            }
        }

        // 비활성화된 불릿이 없으면 새로 생성
        GameObject newBullet = Instantiate(magicBullet);
        // EnemyManager의 첫 번째 자식 오브젝트의 자식으로 설정
        newBullet.transform.SetParent(EnemyManager.Instance.transform.GetChild(0));
        newBullet.SetActive(false);
        magicBulletPool.Add(newBullet);
        return newBullet;
    }

    // E스킬 피격 이펙트 풀링 관리
    private GameObject GetOrCreateESkillHitEffect()
    {
        GameObject effectInstance = null;

        while (eSkillHitEffects.Count > 0 && effectInstance == null)
        {
            effectInstance = eSkillHitEffects.Dequeue();
        }

        if (effectInstance == null)
        {
            GameObject newEffect = Instantiate(eSkillHitEffect);
            newEffect.transform.SetParent(EnemyManager.Instance.transform.GetChild(3));
            newEffect.SetActive(false);
            effectInstance = newEffect;
        }

        effectInstance.SetActive(false);
        return effectInstance;
    }

    // Q스킬 피격 이펙트 풀링 관리
    private GameObject GetOrCreateQSkillHitEffect()
    {
        GameObject effectInstance = null;

        while (qSkillHitEffects.Count > 0 && effectInstance == null)
        {
            effectInstance = qSkillHitEffects.Dequeue();
        }

        if (effectInstance == null)
        {
            GameObject newEffect = Instantiate(qSkillHitEffect);
            newEffect.transform.SetParent(EnemyManager.Instance.transform.GetChild(3));
            newEffect.SetActive(false);
            effectInstance = newEffect;
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

        // 파티클 재생이 끝나면 이펙트 비활성화
        effect.SetActive(false);
        if (pool != null)
        {
            pool.Enqueue(effect);
        }
    }

    // E스킬 몬스터 피격 시 이펙트 처리
    public void HandleESkillMonsterHitEffect(Vector3 position)
    {
        // 이미 이펙트가 터진 위치인지 확인
        if (eSkillHitPositions.Contains(position))
        {
            return;
        }

        // 피격 이펙트 재생
        GameObject hitEffectInstance = GetOrCreateESkillHitEffect();
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
                StartCoroutine(DisableEffectAfterParticle(hitEffectInstance, particleSystem, eSkillHitEffects));
            }

            // 이펙트가 터진 위치 기록
            eSkillHitPositions.Add(position);
        }
    }

    // Q스킬 몬스터 피격 시 이펙트 처리
    public void HandleQSkillMonsterHitEffect(Vector3 position)
    {
        // 이미 이펙트가 터진 위치인지 확인
        if (qSkillHitPositions.Contains(position))
        {
            return;
        }

        // 피격 이펙트 재생
        GameObject hitEffectInstance = GetOrCreateQSkillHitEffect();
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
                StartCoroutine(DisableEffectAfterParticle(hitEffectInstance, particleSystem, qSkillHitEffects));
            }

            // 이펙트가 터진 위치 기록
            qSkillHitPositions.Add(position);
        }
    }

    IEnumerator WizardESkill()
    {
        Debug.Log("E 스킬 시작");

        anim.SetLayerWeight(2, 1);
        anim.SetFloat("ESkillSpeed", eSkillAnimSpeed);  // ESkill 애니메이션 속도만 설정
        anim.SetTrigger("ESkill");
        
        // 애니메이션이 실제로 "ESkill" 상태로 전환될 때까지 기다림
        yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(2).IsName("ESkill"));
        // 이제 확실히 "ESkill" 애니메이션이 재생 중이므로 true로 설정
        playerController.useESkill = true;
        playerController.canMove = false; //재생중 이동 불가

        // === 몸체를 aimSet 방향으로 회전 ===
        Vector3 direction = aimSet.aimingdirection.transform.forward;
        direction.y = 0f;
        player.transform.GetChild(0).rotation = Quaternion.LookRotation(direction, Vector3.up);

        yield return new WaitForSeconds(eSkillDelay); // 지연 시간 후 이펙트 파티클 재생

        Vector3 mouseWorldPosition = aimSet.GetMouseWorldPosition();
        mouseWorldPosition.y = transform.position.y;

        Vector3 spawnPosition = transform.position + direction; // 플레이어 앞에 생성
        spawnPosition.y = player.transform.position.y + 1; // y축 위치 고정

        GameObject projectile = GetOrCreateMagicBullet();
        projectile.transform.position = spawnPosition;
        projectile.transform.rotation = Quaternion.identity;
        // 투사체 크기를 playerStatus.attackRange에 따라 조정
        projectile.transform.localScale = Vector3.one * playerStatus.attackRange;
        projectile.SetActive(true);
        
        Magic_Bullet magic_Bullet = projectile.GetComponent<Magic_Bullet>();

        if (magic_Bullet != null)
        {
            magic_Bullet.Initialize(direction, eSkillDamage, player); // 투사체를 초기화
            magic_Bullet.OnMonsterHitEffect += HandleESkillMonsterHitEffect;
        }

        // 애니메이션이 종료될 때까지 기다림
        yield return new WaitUntil(() => !anim.GetCurrentAnimatorStateInfo(2).IsName("ESkill"));
        
        // 애니메이션이 종료되면 false로 설정
        playerController.useESkill = false;
        playerController.canMove = true;
    }

    IEnumerator WizardQSkill()
    {
        Debug.Log("Q 스킬 시작");
        anim.SetLayerWeight(2, 1);
        anim.SetFloat("QSkillSpeed", qSkillAnimSpeed);  // QSkill 애니메이션 속도만 설정
        anim.SetTrigger("QSkill");

        // 애니메이션이 실제로 "QSkill" 상태로 전환될 때까지 기다림
        yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(2).IsName("QSkill"));
        // 이제 확실히 "QSkill" 애니메이션이 재생 중이므로 true로 설정
        playerController.useQSkill = true;
        playerController.canMove = false;

        // === 몸체를 aimSet 방향으로 회전 ===
        Vector3 direction = aimSet.aimingdirection.transform.forward;
        direction.y = 0f;
        player.transform.GetChild(0).rotation = Quaternion.LookRotation(direction, Vector3.up);

        yield return new WaitForSeconds(qSkillDelay); // 지연 시간 후 이펙트 파티클 재생

        // 마우스 위치를 가져옴
        Vector3 worldPosition = aimSet.GetMouseWorldPosition();
        worldPosition.y = transform.position.y;
        qSkillTargetPosition = worldPosition;

        // === Q스킬 이펙트 파티클 Play ===
        if (qSkillParticle != null && qSkillEffectInstance != null)
        {
            qSkillEffectInstance.transform.parent = null; // 부모 해제
            qSkillEffectInstance.transform.position = qSkillTargetPosition;
            qSkillEffectInstance.transform.rotation = Quaternion.identity;
            // Q스킬 이펙트 크기를 각 축별로 playerStatus.attackRange를 더함
            Vector3 currentScale = qSkillEffectInstance.transform.localScale;
            /*
            qSkillEffectInstance.transform.localScale = new Vector3(
                currentScale.x * playerStatus.attackRange,
                currentScale.y * playerStatus.attackRange,
                currentScale.z * playerStatus.attackRange
            );
            */
            qSkillParticle.Play();
            qSkillParticle.GetComponent<AudioSource>().Play();
            StartCoroutine(ReparentParticleWhenDone());
        }

        var enemyList = EnemyManager.Instance != null ? EnemyManager.Instance.GetEnemyList() : null;
        if (enemyList != null)
        {
            var enemyArray = enemyList.ToArray();
            foreach (var enemyObj in enemyArray)
            {
                if (enemyObj == null || !enemyObj.activeInHierarchy) continue;
                MonsterStatus monsterStatus = enemyObj.GetComponent<MonsterStatus>();
                if (monsterStatus == null) continue;

                float dist = Vector3.Distance(qSkillTargetPosition, enemyObj.transform.position);
                // Q스킬 범위를 playerStatus.attackRange에 따라 조정
                if (dist <= qSkillRadius * playerStatus.attackRange)
                {
                    monsterStatus.TakeDamage(qSkillDamage, player);
                    HandleQSkillMonsterHitEffect(enemyObj.transform.position);
                    Debug.Log($"몬스터에게 {qSkillDamage} 데미지를 줌: {enemyObj.name}");
                }
            }
        }

        // 애니메이션이 종료될 때까지 기다림
        yield return new WaitUntil(() => !anim.GetCurrentAnimatorStateInfo(2).IsName("QSkill"));
        
        // 애니메이션이 종료되면 false로 설정
        playerController.useQSkill = false;
        playerController.canMove = true;
    }

    void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            // Q스킬 범위 (마우스 타겟 위치 기준)
            Gizmos.color = Color.blue;
            // Q스킬 범위를 playerStatus.attackRange에 따라 조정
            Gizmos.DrawWireSphere(qSkillTargetPosition, qSkillRadius * (playerStatus != null ? playerStatus.attackRange : 1f));
        }
    }

    IEnumerator ReparentParticleWhenDone()
    {
        while (qSkillParticle != null && qSkillParticle.isPlaying)
        {
            yield return null;
        }
        if (qSkillEffectInstance != null)
            qSkillEffectInstance.transform.parent = this.transform;
    }
}
