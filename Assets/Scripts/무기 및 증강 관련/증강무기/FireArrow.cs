using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FireArrow : MonoBehaviour
{
    [Header("장착 플레이어")]
    public GameObject player;
    public PlayerStatus playerStatus;
    public UpgradeInGame upgradeInGame;

    [Header("발사체")]
    public GameObject weaponPrefab; //기본 무기
    public GameObject weaponArousalPrefab; //진화 무기

    [Header("무기 설정")]
    public float weaponDamage; // 초기 데미지
    public float weaponCooldown; //쿨타임
    public float damageInterval = 1f; // 데미지 주는 간격
    public float groundEffectDamage = 10f; // 지면 효과 데미지
    public float weaponProjecTile = 3; // 최대 타겟 수
    public float weaponDuration = 5f; // 무기 지속시간
    private float timer = 0f;

    private GameObject playerAttackRange; //플레이어 공격 범위

    [Header("이펙트")]
    public GameObject hitEffect; // 몬스터 피격 이펙트
    public GameObject hitEffectArousal; // 몬스터 피격 이펙트
    private Queue<GameObject> hitEffects = new Queue<GameObject>(); // 몬스터 피격 이펙트 풀
    private Queue<GameObject> hitEffectsArousal = new Queue<GameObject>(); // 몬스터 피격 진화 이펙트 풀

    [Header("진화 여부")]
    public bool isArousal; //능력 진화 여부

    public List<GameObject> activeArrows = new List<GameObject>(); // 활성화된 투사체
    public Queue<GameObject> inactiveArrows = new Queue<GameObject>(); // 비활성화된 투사체

    public List<GameObject> activeArrowsArousal = new List<GameObject>(); // 활성화된 진화 투사체
    public Queue<GameObject> inactiveArrowsArousal = new Queue<GameObject>(); // 비활성화된 진화 투사체

    private Dictionary<Vector3, float> recentEffectPositions = new Dictionary<Vector3, float>();
    private float effectCooldown = 0.5f; // 0.5초 이내엔 같은 위치에 중복 생성 안함

    void Start()
    {
        player = transform.parent.parent.gameObject;
        playerStatus = player.GetComponent<PlayerStatus>();
        upgradeInGame = player.GetComponent<UpgradeInGame>();
        playerAttackRange = player.GetComponent<PlayerSystem>().playerAttackRange;
        groundEffectDamage = weaponDamage / 5;
    }

    void Update()
    {
        // 진화 상태가 변경되었을 때 기존 투사체들 정리
        if (isArousal)
        {
            if (inactiveArrows.Count > 0)
            {
                // 비활성화된 기본 투사체들 제거
                foreach (var arrow in inactiveArrows)
                {
                    if (arrow != null)
                    {
                        Destroy(arrow);
                    }
                }
                inactiveArrows.Clear();
            }
            if (inactiveArrowsArousal.Count > 0)
            {
                // 기존 이펙트들 정리
                CleanupNormalEffects();
            }
        }

        timer += Time.deltaTime;
        if ((timer >= weaponCooldown / (1 + (upgradeInGame.abilityCardValues.attackSpeed + upgradeInGame.upgradeAbilityValues.attackSpeed)/100)))
        {
            StartCoroutine(ActivateSkill());
            timer = 0f;
        }
    }

    private void CleanupNormalEffects()
    {
        int poolCount = hitEffects.Count;
        for (int i = 0; i < poolCount; i++)
        {
            var effect = hitEffects.Dequeue();
            if (effect != null && !effect.activeSelf)
            {
                Destroy(effect);
            }
            else if (effect != null)
            {
                hitEffects.Enqueue(effect);
            }
        }
    }

    IEnumerator ActivateSkill()
    {
        SphereCollider sphereCollider = playerAttackRange.GetComponent<SphereCollider>();
        float attackRange = sphereCollider.radius * playerStatus.attackRange;

        // 장판 피해는 발사 시점의 공격력 기준(첫 타격의 1/5) — 강화로 오른 공격력을 반영
        groundEffectDamage = weaponDamage / 5f;

        // 플레이어 주변의 몬스터 찾기
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);

        var closestMonsters = hitColliders
            .Where(c => c.CompareTag("Monster"))
            .Select(c => new
            {
                Monster = c.GetComponent<MonsterStatus>(),
                Distance = Vector3.Distance(transform.position, c.transform.position)
            })
            .Where(m => m.Monster != null)
            .OrderBy(m => m.Distance)
            .Take((int)weaponProjecTile + playerStatus.projecTile)
            .ToList();

        for (int i = 0; i < (int)weaponProjecTile + playerStatus.projecTile; i++)
        {
            if (i < closestMonsters.Count)
            {
                var entry = closestMonsters[i];
                
                if (isArousal)
                {
                    // 진화 상태일 때
                    if (inactiveArrowsArousal.Count == 0)
                    {
                        // 새로운 진화 화살 생성
                        GameObject newArrow = Instantiate(weaponArousalPrefab, entry.Monster.transform.position, Quaternion.identity);
                        // EnemyManager의 첫 번째 자식 오브젝트의 자식으로 설정
                        newArrow.transform.SetParent(EnemyManager.Instance.transform.GetChild(0));
                        newArrow.SetActive(false);
                        inactiveArrowsArousal.Enqueue(newArrow);
                    }

                    // 비활성화된 진화 화살 중 하나를 가져와서 활성화
                    GameObject arrow = inactiveArrowsArousal.Dequeue();

                    arrow.transform.position = entry.Monster.transform.position;
                    arrow.transform.localScale = Vector3.one * playerStatus.attackRange * 1.5f;

                    FireArrowBullet bulletScript = arrow.GetComponent<FireArrowBullet>();
                    if (bulletScript != null)
                    {
                        bulletScript.Initialize(player, weaponDamage * 1.5f, groundEffectDamage * 1.5f, weaponDuration * playerStatus.duration * 1.5f, damageInterval);
                        bulletScript.OnArrowDestroy += HandleArrowDestroyArousal;
                        bulletScript.OnMonsterHitEffect += HandleMonsterHitEffect;
                    }

                    arrow.SetActive(true);
                    activeArrowsArousal.Add(arrow);

                    // 지속시간 후 비활성화
                    StartCoroutine(DeactivateArrowAfterDurationArousal(arrow, weaponDuration * playerStatus.duration));
                }
                else
                {
                    // 기본 상태일 때
                    if (inactiveArrows.Count == 0)
                    {
                        // 새로운 화살 생성
                        GameObject newArrow = Instantiate(weaponPrefab, entry.Monster.transform.position, Quaternion.identity);
                        // EnemyManager의 첫 번째 자식 오브젝트의 자식으로 설정
                        newArrow.transform.SetParent(EnemyManager.Instance.transform.GetChild(0));
                        newArrow.SetActive(false);
                        inactiveArrows.Enqueue(newArrow);
                    }

                    // 비활성화된 화살 중 하나를 가져와서 활성화
                    GameObject arrow = inactiveArrows.Dequeue();

                    arrow.transform.position = entry.Monster.transform.position;
                    arrow.transform.localScale = Vector3.one * playerStatus.attackRange;

                    FireArrowBullet bulletScript = arrow.GetComponent<FireArrowBullet>();
                    if (bulletScript != null)
                    {
                        bulletScript.Initialize(player, weaponDamage, groundEffectDamage, weaponDuration * playerStatus.duration, damageInterval);
                        bulletScript.OnArrowDestroy += HandleArrowDestroy;
                        bulletScript.OnMonsterHitEffect += HandleMonsterHitEffect;
                    }

                    arrow.SetActive(true);
                    activeArrows.Add(arrow);

                    // 지속시간 후 비활성화
                    StartCoroutine(DeactivateArrowAfterDuration(arrow, weaponDuration * playerStatus.duration));
                }
            }
        }
        yield return null;
    }

    private void HandleArrowDestroy(GameObject arrow)
    {
        if (activeArrows.Contains(arrow))
        {
            activeArrows.Remove(arrow);
            arrow.SetActive(false);
            inactiveArrows.Enqueue(arrow);
        }
    }

    private void HandleArrowDestroyArousal(GameObject arrow)
    {
        if (activeArrowsArousal.Contains(arrow))
        {
            activeArrowsArousal.Remove(arrow);
            arrow.SetActive(false);
            inactiveArrowsArousal.Enqueue(arrow);
        }
    }

    private IEnumerator DeactivateArrowAfterDuration(GameObject arrow, float duration)
    {
        yield return new WaitForSeconds(duration);
        HandleArrowDestroy(arrow);
    }

    private IEnumerator DeactivateArrowAfterDurationArousal(GameObject arrow, float duration)
    {
        yield return new WaitForSeconds(duration);
        HandleArrowDestroyArousal(arrow);
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
            hitEffectsArousal.Enqueue(effect);
        }
        else
        {
            hitEffects.Enqueue(effect);
        }
    }

    // 몬스터 피격 시 이펙트 처리
    private void HandleMonsterHitEffect(Vector3 position)
    {
        // 좌표를 소수점 한 자리로 반올림해서 근처 좌표도 묶어줌 (중복 방지)
        Vector3 roundedPos = new Vector3(
            Mathf.Round(position.x * 10f) / 10f,
            Mathf.Round(position.y * 10f) / 10f,
            Mathf.Round(position.z * 10f) / 10f
        );

        float now = Time.time;
        // 이미 최근에 생성된 좌표라면 무시
        if (recentEffectPositions.TryGetValue(roundedPos, out float lastTime))
        {
            if (now - lastTime < effectCooldown)
                return;
        }
        recentEffectPositions[roundedPos] = now;

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

    void OnDrawGizmos()
    {
        SphereCollider sphereCollider = playerAttackRange.GetComponent<SphereCollider>();
        float attackRange = sphereCollider.radius * playerStatus.attackRange;
        // 스킬 범위 표시
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
