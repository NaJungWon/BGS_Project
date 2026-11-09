using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSetting : MonoBehaviour
{
    public GameObject ground;   //보스 방 영역
    public BoxCollider groundTrigger;   //보스 방 트리거
    public MonsterStatus monsterStatus; //보스 스탯 컴포넌트
    public GameObject bossRoom; //보스 방 영역
    public float playerDistance = 20f; //플레이어와의 거리
    public bool playerInRoom = false; //플레이어 방 내부 여부
    //public GameObject nearPlayer;

    private Dictionary<PlayerStatus, float> playersInTrigger = new Dictionary<PlayerStatus, float>();
    private float damageInterval = 1f; // 데미지 간격을 1초로 설정

    // Start is called before the first frame update
    void Start()
    {
        groundTrigger = ground.transform.GetChild(0).GetComponent<BoxCollider>();
        monsterStatus = this.GetComponent<MonsterStatus>();

        // Start에서도 체력 초기화
        if (monsterStatus != null)
        {
            monsterStatus.currentHealth = monsterStatus.maxHealth;
        }
        //DefaultSetting();
    }

    // Update is called once per frame
    void Update()
    {
        // 보스방 내 플레이어 감지
        if (bossRoom != null && !playerInRoom)
        {
            BoxCollider bossRoomCollider = bossRoom.GetComponent<BoxCollider>();
            if (bossRoomCollider != null)
            {
                Collider[] colliders = Physics.OverlapBox(
                    bossRoomCollider.bounds.center,
                    bossRoomCollider.bounds.extents,
                    bossRoom.transform.rotation
                );

                foreach (Collider collider in colliders)
                {
                    if (collider.CompareTag("Player"))
                    {
                        // 플레이어와의 거리 체크
                        float distance = Vector3.Distance(collider.transform.position, this.transform.position);
                        if (distance < playerDistance)
                        {
                            // 보스방 자식 오브젝트들 활성화
                            if (bossRoom.transform.childCount >= 2)
                            {
                                bossRoom.transform.GetChild(0).gameObject.SetActive(true);
                                bossRoom.transform.GetChild(1).gameObject.SetActive(true);
                                playerInRoom = true;

                            }
                        }
                        break;
                    }
                }
            }
        }

        if (monsterStatus.monsterAlive && groundTrigger != null)
        {
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

            //MoveSetting();
        }
    }

    void OnEnable()
    {
        if (ground == null)
        {
            // 필요한 ground를 할당하는 코드 추가
        }
        if (ground != null && ground.transform.childCount > 0)
        {
            groundTrigger = ground.transform.GetChild(0).GetComponent<BoxCollider>();
        }
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
