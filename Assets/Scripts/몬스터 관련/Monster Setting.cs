using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterSetting : MonoBehaviour
{
    public GameObject ground;
    public BoxCollider groundTrigger;
    public NavMeshAgent agent;
    public MonsterStatus monsterStatus;
    public GameObject nearPlayer;
    
    private Dictionary<PlayerStatus, float> playersInTrigger = new Dictionary<PlayerStatus, float>();
    private float damageInterval = 1f; // 데미지 간격을 1초로 설정

    // Start is called before the first frame update
    void Start()
    {
        // ground의 0번 자식으로 groundTrigger 설정
        groundTrigger = ground.transform.GetChild(0).GetComponent<BoxCollider>();
        monsterStatus = GetComponent<MonsterStatus>();

        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent가 없습니다!");
        }

        // Start에서도 체력 초기화
        if (monsterStatus != null)
        {
            monsterStatus.currentHealth = monsterStatus.maxHealth;
        }
        DefaultSetting();
    }

    // Update is called once per frame
    void Update()
    {

        if (monsterStatus.monsterAlive && groundTrigger != null)
        {
            //DefaultSetting(); // 몬스터 기본 설정 실시간 업데이트

            Collider[] hitColliders = Physics.OverlapBox(groundTrigger.bounds.center, groundTrigger.bounds.extents, Quaternion.identity);
            float closestDistance = Mathf.Infinity;
            GameObject closestPlayer = null;

            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Player"))
                {
                    float distance = Vector3.Distance(transform.position, hitCollider.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestPlayer = hitCollider.gameObject;
                    }
                }
            }
            if (closestPlayer != null && !monsterStatus.isAttack)
            {
                nearPlayer = closestPlayer;
                if (agent != null && agent.isActiveAndEnabled)
                {
                    agent.SetDestination(nearPlayer.transform.position);
                }
            }

            // 트리거 영역 내의 플레이어들에게 지속 데미지 적용
            List<PlayerStatus> playersToRemove = new List<PlayerStatus>();
            var playersCopy = new Dictionary<PlayerStatus, float>(playersInTrigger);
            
            foreach (var player in playersCopy)
            {
                if (player.Key != null)
                {
                    float currentTime = playersInTrigger[player.Key] + Time.deltaTime;
                    if (currentTime >= damageInterval)
                    {
                        Hitdecision hitdecision = player.Key.GetComponent<Hitdecision>();
                        if (hitdecision != null)
                        {
                            hitdecision.ApplyDamage(player.Key, monsterStatus.attackPower);
                        }
                        playersInTrigger[player.Key] = 0f;
                    }
                    else
                    {
                        playersInTrigger[player.Key] = currentTime;
                    }
                }
                else
                {
                    playersToRemove.Add(player.Key);
                }
            }

            // null이 된 플레이어 제거
            foreach (var player in playersToRemove)
            {
                playersInTrigger.Remove(player);
            }

            MoveSetting();
        }
    }

    public void DefaultSetting()
    {
        agent.stoppingDistance = monsterStatus.attackRange;
        agent.speed = monsterStatus.moveSpeed;
    }

    public void MoveSetting()
    {
        if (nearPlayer == null)
        {
            // 방어: null일 경우 아무것도 하지 않음
            return;
        }

        // NavMeshAgent의 velocity가 Vector3.zero가 아닐 때 움직임으로 판단
        if (agent.velocity != Vector3.zero && !monsterStatus.isAttack)
        {
            monsterStatus.anim.SetBool("IsMove", true);
        }
        else
        {
            monsterStatus.anim.SetBool("IsMove", false);
        }
    }

    // 몬스터 이동 일시 정지 메서드
    public void StopMovement(float duration)
    {
        // 게임오브젝트가 활성화 상태일 때만 코루틴 실행
        if (this.gameObject.activeInHierarchy)
        {
            StartCoroutine(StopMovementCoroutine(duration));
        }
    }

    private IEnumerator StopMovementCoroutine(float duration)
    {
        // 현재 속도 저장
        float originalSpeed = agent.speed;
        
        // 이동 정지
        agent.speed = 0f;
        agent.velocity = Vector3.zero;
        agent.isStopped = true;

        // 지정된 시간 동안 대기
        yield return new WaitForSeconds(duration);

        // 이동 재개
        agent.speed = originalSpeed;
        agent.isStopped = false;
    }

    void OnEnable()
    {
        // monsterStatus가 이미 할당되어 있는지 확인
        if (monsterStatus != null)
        {
            monsterStatus.currentHealth = monsterStatus.maxHealth;
            monsterStatus.monsterAlive = true;
        }
        // 몬스터가 활성화될 때 EnemyManager에 등록
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.RegisterEnemy(this.gameObject);
        }
    }

    void OnDisable()
    {
        // 몬스터가 비활성화될 때 EnemyManager에서 제거
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.UnregisterEnemy(this.gameObject);
        }

        // 기존의 딕셔너리 초기화 로직
        playersInTrigger.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStatus playerStatus = other.GetComponent<PlayerStatus>();
            if (playerStatus != null)
            {
                // Hitdecision의 ApplyDamage 메서드 호출
                Hitdecision hitdecision = other.GetComponent<Hitdecision>();
                if (hitdecision != null)
                {
                    hitdecision.ApplyDamage(playerStatus, monsterStatus.attackPower);
                    // 트리거 영역에 들어온 플레이어를 딕셔너리에 추가
                    if (!playersInTrigger.ContainsKey(playerStatus))
                    {
                        playersInTrigger.Add(playerStatus, 0f);
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStatus playerStatus = other.GetComponent<PlayerStatus>();
            if (playerStatus != null)
            {
                // 트리거 영역을 벗어난 플레이어를 딕셔너리에서 제거
                playersInTrigger.Remove(playerStatus);
            }
        }
    }
}
