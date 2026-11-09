using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldTest : MonoBehaviour
{
    [Header("장착 플레이어")]
    public GameObject player;         // 플레이어 오브젝트 참조
    public PlayerStatus playerStatus;
    
    [Header("무기 설정")]
    public float weaponDamage;              // 무기 데미지
    public float weaponAttackRange;         // 무기 공격범위
    public int timer = 1;             // 데미지 반복 간격(초)

    [Header("진화 여부")]
    public bool isArousal; //능력 진화 여부

    [Header("무기 이펙트 기본/진화")]
    public GameObject fieldEffect;
    
    public GameObject hitEffect; // 몬스터 피격 이펙트
    private Queue<GameObject> hitEffects = new Queue<GameObject>(); // 몬스터 피격 이펙트 풀
    private HashSet<Vector3> hitPositions = new HashSet<Vector3>(); // 이미 이펙트가 터진 위치 추적

    // 영역 내에 있는 몬스터별로 데미지 타이머를 관리하는 딕셔너리
    private Dictionary<MonsterStatus, float> monsterTimers = new Dictionary<MonsterStatus, float>();

    // Start는 스크립트가 활성화될 때 한 번 실행됨
    void Start()
    {
        // player를 이 스크립트가 붙은 오브젝트의 두 단계 위 부모로 설정
        player = transform.parent.parent.gameObject;
        playerStatus = player.GetComponent<PlayerStatus>();

        // 현재 y좌표에서 -0.99f를 뺌
        Vector3 position = transform.position;
        position.y -= 0.99f;
        transform.position = position;

        if (fieldEffect != null)
        {
            GameObject effectInstance = Instantiate(fieldEffect, this.transform.position, Quaternion.identity, this.transform);
            fieldEffect = effectInstance;
            fieldEffect.SetActive(false);
            // 진화 상태가 아닐 때는 0번 자식 비활성화
            if (fieldEffect.transform.childCount > 0)
            {
                fieldEffect.transform.GetChild(0).gameObject.SetActive(false);
            }
        }
    }

    // Update는 매 프레임마다 실행됨
    void Update()
    {
        // 이펙트 파티클 제어
        fieldEffect.SetActive(true);
        this.transform.localScale = Vector3.one * (weaponAttackRange + playerStatus.attackRange);

        // 진화 상태에 따른 이펙트 제어
        if (fieldEffect.transform.childCount > 0)
        {
            fieldEffect.transform.GetChild(0).gameObject.SetActive(isArousal);
        }

        // 현재 영역 내에 있는 모든 몬스터에 대해 반복
        List<MonsterStatus> monsters = new List<MonsterStatus>(monsterTimers.Keys);
        foreach (var monster in monsters)
        {
            //비활성화된 경우 타이머 관리에서 제거
            if (!monster.gameObject.activeInHierarchy)
            {
                monsterTimers.Remove(monster);
                continue;
            }

            // 타이머 증가
            monsterTimers[monster] += Time.deltaTime;
            // 타이머가 timer(초) 이상이 되면 데미지 부여
            if (monsterTimers[monster] >= timer)
            {
                monster.TakeDamage(weaponDamage, player); // 몬스터에게 데미지 부여
                HandleMonsterHitEffect(monster.transform.position); // 피격 이펙트 재생

                // 진화 상태일 때 몬스터 이동 정지
                if (isArousal)
                {
                    MonsterSetting monsterSetting = monster.GetComponent<MonsterSetting>();
                    if (monsterSetting != null)
                    {
                        monsterSetting.StopMovement(0.5f);
                    }
                }

                monsterTimers[monster] = 0f; // 타이머 초기화
            }
        }

        // 이펙트 파티클 사운드 제어
        if(GameManager.gameStatus == GameManager.GameStatus.InGame && fieldEffect.GetComponent<AudioSource>().isPlaying == false)
        {
            fieldEffect.GetComponent<AudioSource>().Play();
        }
        else if(GameManager.gameStatus != GameManager.GameStatus.InGame && fieldEffect.GetComponent<AudioSource>().isPlaying == true)
        {
            fieldEffect.GetComponent<AudioSource>().Stop();
        }
    }

    // 몬스터가 영역에 처음 들어왔을 때 실행됨
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Monster"))
        {
            MonsterStatus monsterStatus = other.GetComponent<MonsterStatus>();
            // 영역에 처음 들어온 몬스터라면 타이머 등록
            if (monsterStatus != null && !monsterTimers.ContainsKey(monsterStatus))
            {
                monsterTimers[monsterStatus] = 0f;
            }
        }
    }

    // 몬스터가 영역에서 나갔을 때 실행됨
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Monster"))
        {
            MonsterStatus monsterStatus = other.GetComponent<MonsterStatus>();
            // 영역에서 나간 몬스터는 타이머 관리에서 제거
            if (monsterStatus != null && monsterTimers.ContainsKey(monsterStatus))
            {
                monsterTimers.Remove(monsterStatus);
            }
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
    private void HandleMonsterHitEffect(Vector3 position)
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
