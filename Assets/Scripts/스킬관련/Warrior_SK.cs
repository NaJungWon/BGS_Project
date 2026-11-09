using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using UnityEngine;
using UnityEngine.UI;

public class Warrior_SK : MonoBehaviour
{
    [Header("플레이어 관련")]
    public GameObject player;
    public PlayerSystem playerSystem;
    public PlayerStatus playerStatus;
    public AimingSetting aimSet;
    public PlayerController playerController;
    public Animator anim;
    //public UpgradeInGame upgradeInGame;
    public Vector3 playerPosition;
    //public Sprite passiveImage;
    private GameObject playerAttackRange; // 플레이어 공격 범위

    [Header("E 스킬 관련")]
    public GameObject eSkillEffect;
    public Sprite eSkillImageBG;
    public Sprite eSkillImage;
    public float eSkillCooldown = 5f; // E 스킬 쿨다운 시간
    public float eSkillDelay = 1f; // E 스킬 지연 시간
    public float dashSpeed = 40f; // 대시 속도
    public float dashDuration = 0.2f; // 대시 지속 시간
    public float eSkillDamage = 50f; // E 스킬 데미지
    public float eSkillAnimSpeed = 1f; // E 스킬 애니메이션 재생 속도
    public bool isDashing = false;

    [Header("Q 스킬 관련")]
    public GameObject qSkillEffect;
    public Sprite qSkillImageBG;
    public Sprite qSkillImage;
    public float qSkillCooldown = 10f; // Q 스킬 쿨다운 시간
    public float qSkillDelay = 1f; // Q 스킬 지연 시간
    public float qSkillDamage = 200f; // Q 스킬 데미지
    public float qSkillAnimSpeed = 1f; // Q 스킬 애니메이션 재생 속도
    private Vector3 qSkillBaseScale; // Q스킬 이펙트의 기본 스케일

    public float currentQCooldown = 0f; // Q 스킬 현재 쿨다운
    public float currentECooldown = 0f; // E 스킬 현재 쿨다운

    private HashSet<GameObject> hitMonsters = new HashSet<GameObject>();
    private GameObject eSkillEffectInstance;
    private ParticleSystem eSkillParticle;
    private GameObject qSkillEffectInstance;
    private ParticleSystem qSkillParticle;

    [Header("이펙트")]
    public GameObject hitEffect; // 몬스터 피격 이펙트
    private List<GameObject> hitEffects = new List<GameObject>(); // 몬스터 피격 이펙트 리스트
    private HashSet<Vector3> hitPositions = new HashSet<Vector3>(); // 이미 이펙트가 터진 위치 추적

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
        playerAttackRange = player.GetComponent<PlayerSystem>().playerAttackRange;

        // qSkillEffect, eSkillEffect를 자식으로 생성 및 파티클 참조 저장
        if (qSkillEffect != null)
        {
            qSkillEffectInstance = Instantiate(qSkillEffect, this.transform);
            qSkillParticle = qSkillEffectInstance.GetComponent<ParticleSystem>();
            if (qSkillParticle != null)
            {
                qSkillParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                qSkillBaseScale = qSkillEffectInstance.transform.localScale; // 기본 스케일 저장
            }
        }
        if (eSkillEffect != null)
        {
            eSkillEffectInstance = Instantiate(eSkillEffect, this.transform);
            eSkillParticle = eSkillEffectInstance.GetComponent<ParticleSystem>();
            if (eSkillParticle != null)
                eSkillParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
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
            StartCoroutine(WarriorESkill());
            currentECooldown = eSkillCooldown; // E 스킬 쿨다운 설정
            //Debug.Log("E 스킬 발동");
        }
        if (Input.GetKeyDown(KeyCode.Q) && currentQCooldown <= 0f)
        {
            StartCoroutine(WarriorQSkill());
            currentQCooldown = qSkillCooldown; // Q 스킬 쿨다운 설정
            //Debug.Log("Q 스킬 발동");
        }
    }

    // 피격 이펙트 풀링 관리
    private GameObject GetOrCreateHitEffect()
    {
        // 비활성화된 이펙트 찾기
        foreach (GameObject effect in hitEffects)
        {
            if (!effect.activeSelf)
            {
                return effect;
            }
        }

        // 비활성화된 이펙트가 없으면 새로 생성
        GameObject newEffect = Instantiate(hitEffect);
        // EnemyManager의 첫 번째 자식 오브젝트의 자식으로 설정
        newEffect.transform.SetParent(EnemyManager.Instance.transform.GetChild(3));
        newEffect.SetActive(false);
        hitEffects.Add(newEffect);
        return newEffect;
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
    }

    // 몬스터 피격 시 이펙트 처리
    private void HandleMonsterHitEffect(Vector3 position)
    {
        // y좌표를 1로 고정한 위치 생성
        Vector3 fixedPosition = new Vector3(position.x, 1f, position.z);

        // 이미 이펙트가 터진 위치인지 확인
        if (hitPositions.Contains(fixedPosition))
        {
            return;
        }

        // 피격 이펙트 재생
        GameObject hitEffectInstance = GetOrCreateHitEffect();
        if (hitEffectInstance != null)
        {
            hitEffectInstance.transform.position = fixedPosition;
            hitEffectInstance.SetActive(true);
            
            // 파티클 시스템 가져오기
            ParticleSystem particleSystem = hitEffectInstance.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                // 파티클 재생이 끝나면 자동으로 비활성화하는 코루틴 시작
                StartCoroutine(DisableEffectAfterParticle(hitEffectInstance, particleSystem));
            }

            // 이펙트가 터진 위치 기록
            hitPositions.Add(fixedPosition);
        }
    }

    IEnumerator WarriorESkill()
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
        playerStatus.isInvincible = true;

        // === 몸체를 aimSet 방향으로 회전 ===
        Vector3 direction = aimSet.aimingdirection.transform.forward;
        direction.y = 0f;
        player.transform.GetChild(0).rotation = Quaternion.LookRotation(direction, Vector3.up);

        yield return new WaitForSeconds(eSkillDelay); // 지연 시간 후 이펙트 파티클 재생

        // === E스킬 이펙트 파티클 Play ===
        if (eSkillParticle != null)
        {
            eSkillEffectInstance.transform.position = player.transform.position;
            eSkillEffectInstance.transform.rotation = Quaternion.identity;
            eSkillParticle.Play();
            eSkillParticle.GetComponent<AudioSource>().Play();
        }

        Vector3 mouseWorldPosition = aimSet.GetMouseWorldPosition();
        mouseWorldPosition.y = player.transform.position.y;

        Vector3 directionToMouse = (mouseWorldPosition - player.transform.position).normalized;
        float startTime = Time.time;
        isDashing = true;
        hitMonsters.Clear();

        while (Time.time < startTime + dashDuration)
        {
            player.transform.Translate(directionToMouse * dashSpeed * Time.deltaTime, Space.World);

            // EnemyManager에서 몬스터 리스트 복사본 가져오기
            var enemyList = EnemyManager.Instance != null ? EnemyManager.Instance.GetEnemyList() : null;
            float dashHitRadius = 2f;

            if (enemyList != null)
            {
                var enemyArray = enemyList.ToArray();
                foreach (var enemyObj in enemyArray)
                {
                    if (enemyObj == null || !enemyObj.activeInHierarchy) continue;
                    MonsterStatus monsterStatus = enemyObj.GetComponent<MonsterStatus>();
                    if (monsterStatus == null) continue;

                    float dist = Vector3.Distance(player.transform.position, enemyObj.transform.position);
                    if (dist <= dashHitRadius && !hitMonsters.Contains(enemyObj))
                    {
                        monsterStatus.TakeDamage(eSkillDamage, player);
                        HandleMonsterHitEffect(enemyObj.transform.position);
                        hitMonsters.Add(enemyObj);
                        Debug.Log("몬스터에게 " + (eSkillDamage) + " 데미지를 줌: " + enemyObj.name);
                    }
                }
            }
            yield return null;
        }
        isDashing = false;
        hitMonsters.Clear();

        // 애니메이션이 종료될 때까지 기다림
        yield return new WaitUntil(() => !anim.GetCurrentAnimatorStateInfo(2).IsName("ESkill"));
        
        // 애니메이션이 종료되면 false로 설정
        playerController.useESkill = false;
        playerStatus.isInvincible = false;
        playerController.canMove = true;
    }

    IEnumerator WarriorQSkill()
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
        qSkillParticle.GetComponent<AudioSource>().Play();

        // === 몸체를 aimSet 방향으로 회전 ===
        Vector3 direction = aimSet.aimingdirection.transform.forward;
        direction.y = 0f;
        player.transform.GetChild(0).rotation = Quaternion.LookRotation(direction, Vector3.up);

        yield return new WaitForSeconds(qSkillDelay); // 지연 시간 후 이펙트 파티클 재생

        // === Q스킬 이펙트 파티클 Play ===
        if (qSkillParticle != null)
        {
            Vector3 effectPosition = player.transform.position;
            effectPosition.y = 1f;
            qSkillEffectInstance.transform.position = effectPosition;
            qSkillEffectInstance.transform.rotation = Quaternion.identity;
            
            // 기본 스케일에 attackRange를 곱하여 설정
            //Debug.Log($"기본 스케일: {qSkillBaseScale}");
            //Debug.Log($"현재 attackRange: {playerStatus.attackRange}");
            qSkillEffectInstance.transform.localScale = new Vector3(
                qSkillBaseScale.x * playerStatus.attackRange,
                qSkillBaseScale.y * playerStatus.attackRange,
                qSkillBaseScale.z * playerStatus.attackRange
            );
            //Debug.Log($"Q스킬 이펙트 스케일: {qSkillEffectInstance.transform.localScale}");
            
            qSkillParticle.Play();
            //qSkillParticle.GetComponent<AudioSource>().Play();
        }

        // 공격 범위 계산
        SphereCollider sphereCollider = playerAttackRange.GetComponent<SphereCollider>();
        float attackRange = (sphereCollider.radius / 2 * playerStatus.attackRange);

        // EnemyManager에서 몬스터 리스트 가져오기
        var enemyList = EnemyManager.Instance != null ? EnemyManager.Instance.GetEnemyList() : null;
        if (enemyList != null)
        {
            foreach (var enemyObj in enemyList)
            {
                if (enemyObj == null || !enemyObj.activeInHierarchy) continue;
                MonsterStatus monsterStatus = enemyObj.GetComponent<MonsterStatus>();
                if (monsterStatus == null) continue;

                // 공격 범위 내의 모든 적에게 데미지
                float dist = Vector3.Distance(player.transform.position, enemyObj.transform.position);
                if (dist <= attackRange)
                {
                    monsterStatus.TakeDamage(qSkillDamage, player);
                    HandleMonsterHitEffect(enemyObj.transform.position);
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
            // Q스킬 범위 (플레이어 위치 기준)
            Gizmos.color = Color.red;
            if (playerAttackRange != null)
            {
                SphereCollider sphereCollider = playerAttackRange.GetComponent<SphereCollider>();
                float attackRange = sphereCollider.radius / 2 * playerStatus.attackRange;
                Gizmos.DrawWireSphere(transform.position, attackRange);
            }
        }
    }
}
