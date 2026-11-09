using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MonsterSpawn : MonoBehaviour
{
    public static MonsterSpawn Instance { get; private set; }

    public PlayerUISetting playerUISetting;
    [System.Serializable]
    public class MonsterSpawnData
    {
        public enum MonsterType //몬스터 타입
        {
            Default, Sub, Elite, SubBoss, Boss, None
        }
        public float triggerTime; //해당 정보가 적용될 시간
        public MonsterType monsterType; //메인 몬스터 타입
        public int MonsterIndex; //메인 몬스터 인덱스 번호
        public float spawnInterval; //해당 시간 생성 간격
        public int MonsterCount; //몬스터 타입이 엘리트,중간보스일때 생성할 개수
        public MonsterType submonsterType; //서브 몬스터 타입
        public int subMonsterIndex; //서브 몬스터 인덱스 번호
        public float subspawnInterval; //해당 시간 생성 간격
        public int subMonsterCount; //서브 몬스터 타입이 엘리트,중간보스일때 생성할 개수
    }
    
    [Header("몬스터 생성주기를 관리하는 리스트")]
    public List<MonsterSpawnData> spawnDataList = new List<MonsterSpawnData>();

    [Header("몬스터 정보 리스트 기본/특수/중간보스/보스")]
    public List<MonsterInfo> dfMonsterInfoList; //기본몹 정보
    public List<MonsterInfo> sbMonsterInfoList; //서브몹 정보
    public List<MonsterInfo> specialMonsterInfoList; //특수몹 정보
    public List<MonsterInfo> sbBossMonsterInfoList; //중간보스 정보
    public List<MonsterInfo> BossMonsterInfoList; //중간보스 정보

    [Header("몬스터 생성 리스트 기본/특수/중간보스/보스")]
    public List<GameObject> DfMonsters = new List<GameObject>();
    public List<GameObject> SubMonsters = new List<GameObject>();
    public List<GameObject> SpecialMonsters = new List<GameObject>();
    public List<GameObject> SubBossMonsters = new List<GameObject>();
    public List<GameObject> BossMonsters = new List<GameObject>();

    // 타입 + 인덱스별 풀
    private readonly Dictionary<int, Queue<GameObject>> defaultPools = new Dictionary<int, Queue<GameObject>>();
    private readonly Dictionary<int, Queue<GameObject>> subPools = new Dictionary<int, Queue<GameObject>>();
    private readonly Dictionary<int, Queue<GameObject>> specialPools = new Dictionary<int, Queue<GameObject>>();
    private readonly Dictionary<int, Queue<GameObject>> subBossPools = new Dictionary<int, Queue<GameObject>>();
    private readonly Dictionary<int, Queue<GameObject>> bossPools = new Dictionary<int, Queue<GameObject>>();

    [Header("플레이어 리스트")]
    public List<GameObject> playerList = new List<GameObject>();

    [Header("몬스터 생성 영역 관련")]
    public GameObject ground;
    public GameObject spawnArea;
    public BoxCollider groundTrigger;

    [Header("몬스터 스폰 관련")]
    public bool monsterSpawn;
    public bool spawnSub = false;

    [Header("몬스터 생성 관련")]
    public float mainSpawnInterval = 3f; // 초기 생성 간격
    public float mainSpawnTimer = 0f;
    public float subSpawnInterval = 5f;
    public float subSpawnTimer = 0f;
    public float safeDistance = 10f; // 플레이어와의 안전 거리
    public float timer;

    [Header("스폰 완료된 데이터 추적")]
    private HashSet<int> spawnedDataIndices = new HashSet<int>();

    [Header("보스 관련")]
    public bool bossAlive; //보스 생존 여부
    public GameObject bossSpawnPosition; //보스 스폰 위치
    public MonsterStatus currentBossStatus; //보스 스탯
    public GameObject bossRoom; //보스 방

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        monsterSpawn = true;
        // ground를 이 스크립트가 컴포넌트된 게임 오브젝트의 0번째 자식으로 설정
        ground = transform.GetChild(0).gameObject;
        spawnArea = transform.GetChild(1).gameObject; //풀링으로 생성된 몹들 집어넣을 곳
        
        if (ground != null && ground.transform.childCount > 0)
        {
            groundTrigger = ground.transform.GetChild(0).GetComponent<BoxCollider>();
            if (groundTrigger == null)
            {
                Debug.LogError("Ground의 첫 번째 자식 오브젝트에 BoxCollider가 없습니다!");
            }
            else
            {
                // groundTrigger 내의 'Player' 태그를 가진 오브젝트를 players 리스트에 추가
                Collider[] hitColliders = Physics.OverlapBox(groundTrigger.bounds.center, groundTrigger.bounds.extents, Quaternion.identity);
                foreach (var hitCollider in hitColliders)
                {
                    if (hitCollider.CompareTag("Player"))
                    {
                        playerList.Add(hitCollider.gameObject);
                    }
                }
            }
        }

        DefaultMonsterSetting();
    }

    void Update()
    {
        MonsterSpawnSetting();
        CheckBossStatus();
    }

    // 보스 UI/승패 판단을 프레임마다 갱신
    void CheckBossStatus()
    {
        if (bossAlive && currentBossStatus != null)
        {
            // 보스가 활성화되어 있고 상태가 존재하면 UI 업데이트
            if (currentBossStatus.gameObject.activeInHierarchy)
            {
                playerUISetting.UpdateBossHealthUI(currentBossStatus);
                
                // 보스 체력이 0이 되었을 때 승리 조건 체크
                if (currentBossStatus.currentHealth <= 0)
                {
                    bool allPlayersAlive = true;
                    foreach (var player in playerList)
                    {
                        PlayerStatus playerStatus = player.GetComponent<PlayerStatus>();
                        if (playerStatus != null && !playerStatus.isAlive)
                        {
                            allPlayersAlive = false;
                            break;
                        }
                    }
                    
                    if (allPlayersAlive)
                    {
                        GameManager.gameStatus = GameManager.GameStatus.Win;
                    }
                }
            }
            else
            {
                // 보스가 비활성화되면 상태 초기화
                bossAlive = false;
                currentBossStatus = null;
            }
        }
        
        // 플레이어 전원 사망 체크
        bool allPlayersDead = true;
        foreach (var player in playerList)
        {
            PlayerStatus playerStatus = player.GetComponent<PlayerStatus>();
            if (playerStatus != null && playerStatus.isAlive)
            {
                allPlayersDead = false;
                break;
            }
        }
        
        if (allPlayersDead)
        {
            GameManager.gameStatus = GameManager.GameStatus.GameOver;
        }
    }

    // 모든 몬스터 프리팹을 Instantiate 후 타입+인덱스 큐에 채워 넣는 초기화
    public void DefaultMonsterSetting()
    {
        CreatePools(dfMonsterInfoList, MonsterSpawnData.MonsterType.Default, defaultPools, 50, DfMonsters);
        CreatePools(sbMonsterInfoList, MonsterSpawnData.MonsterType.Sub, subPools, 50, SubMonsters);
        CreatePools(specialMonsterInfoList, MonsterSpawnData.MonsterType.Elite, specialPools, 200, SpecialMonsters);
        CreatePools(sbBossMonsterInfoList, MonsterSpawnData.MonsterType.SubBoss, subBossPools, 5, SubBossMonsters);
        CreatePools(BossMonsterInfoList, MonsterSpawnData.MonsterType.Boss, bossPools, 1, BossMonsters);
        }

    // 스폰 스케줄을 돌면서 triggerTime이 온 항목을 처리
    public void MonsterSpawnSetting() //스케쥴에 따른 스폰 설정
    {
        if (!monsterSpawn || GameManager.gameStatus != GameManager.GameStatus.InGame)
            return;

        // 스케줄 모두 순회
        for (int i = 0; i < spawnDataList.Count; i++)
        {
            var data = spawnDataList[i];
            // triggerTime에 도달했을 때
            if (GameTimer.gameTimer >= data.triggerTime)
            {
                // 이미 지나간 triggerTime의 데이터들과 비교
                for (int j = 0; j < i; j++)
                {
                    var prevData = spawnDataList[j];
                    if (GameTimer.gameTimer > prevData.triggerTime)
                    {
                        // Default나 Sub 타입일 경우에만 데이터 교체
                        if (prevData.monsterType == data.monsterType && 
                            (data.monsterType == MonsterSpawnData.MonsterType.Default || 
                             data.monsterType == MonsterSpawnData.MonsterType.Sub))
                        {
                            prevData.MonsterIndex = data.MonsterIndex;
                            prevData.spawnInterval = data.spawnInterval;
                        }
                        if (prevData.submonsterType == data.submonsterType && 
                            (data.submonsterType == MonsterSpawnData.MonsterType.Default || 
                             data.submonsterType == MonsterSpawnData.MonsterType.Sub))
                        {
                            prevData.subMonsterIndex = data.subMonsterIndex;
                            prevData.subspawnInterval = data.subspawnInterval;
                        }
                    }
                }

                HandleMainSpawn(data, i);

                // 서브 몬스터 스폰
                if (data.submonsterType != MonsterSpawnData.MonsterType.None)
                {
                    HandleSubSpawn(data, i);
                }
            }
        }
    }

    // 특정 타입 리스트(infoList)를 돌며 index별 큐를 만들어 지정된 수(countPerIndex)만큼 채운다
    private void CreatePools(List<MonsterInfo> infoList, MonsterSpawnData.MonsterType poolType, Dictionary<int, Queue<GameObject>> targetPools, int countPerIndex, List<GameObject> registry)
    {
        if (infoList == null)
            return;

        for (int index = 0; index < infoList.Count; index++)
        {
            if (!targetPools.ContainsKey(index))
            {
                targetPools[index] = new Queue<GameObject>();
            }

            for (int i = 0; i < countPerIndex; i++)
            {
                GameObject monster = CreateMonster(poolType, infoList[index], index, addToPool: true);
                targetPools[index].Enqueue(monster);
                registry?.Add(monster);
                }
        }
    }

    // 메인 스폰(기본/서브/엘리트/보스)을 유형별로 처리
    private void HandleMainSpawn(MonsterSpawnData data, int dataIndex)
    {
                    if (data.monsterType == MonsterSpawnData.MonsterType.Default || 
                        data.monsterType == MonsterSpawnData.MonsterType.Sub)
                    {
                        mainSpawnTimer += Time.deltaTime;
                        if (mainSpawnTimer >= data.spawnInterval)
                        {
                Vector3 spawnPos = GetRandomSpawnPosition();
                ActivateMonsterFromPool(data.monsterType, data.MonsterIndex, spawnPos, Quaternion.identity);
                            mainSpawnTimer = 0f;
                        }
            return;
        }

        if (spawnedDataIndices.Contains(dataIndex))
            return;

        if (data.monsterType == MonsterSpawnData.MonsterType.Elite ||
            data.monsterType == MonsterSpawnData.MonsterType.SubBoss)
                    {
            SpawnMultipleFromPool(data.monsterType, data.MonsterIndex, data.MonsterCount);
            spawnedDataIndices.Add(dataIndex);
            return;
                    }

        if (data.monsterType == MonsterSpawnData.MonsterType.Boss)
                    {
            SpawnBoss(data);
            spawnedDataIndices.Add(dataIndex);
                                            }
                                        }
                                        
    // 보조 스폰(submonsterType) 로직
    private void HandleSubSpawn(MonsterSpawnData data, int dataIndex)
                    {
        if (data.submonsterType == MonsterSpawnData.MonsterType.None)
            return;

                        if (data.submonsterType == MonsterSpawnData.MonsterType.Default || 
                            data.submonsterType == MonsterSpawnData.MonsterType.Sub)
                        {
                            subSpawnTimer += Time.deltaTime;
                            if (subSpawnTimer >= data.subspawnInterval)
                            {
                Vector3 spawnPos = GetRandomSpawnPosition();
                ActivateMonsterFromPool(data.submonsterType, data.subMonsterIndex, spawnPos, Quaternion.identity);
                                subSpawnTimer = 0f;
                            }
            return;
        }

        if (spawnedDataIndices.Contains(dataIndex))
            return;

        SpawnMultipleFromPool(data.submonsterType, data.subMonsterIndex, data.subMonsterCount);
        spawnedDataIndices.Add(dataIndex);
    }

    // 프리팹 Instantiate 후 MonsterStatus 초기값을 세팅하고 spawnArea 자식에 붙인다
    private GameObject CreateMonster(MonsterSpawnData.MonsterType poolType, MonsterInfo selectedInfo, int poolIndex, bool addToPool)
    {
        GameObject newMonster = Instantiate(selectedInfo.monstermodel, Vector3.zero, Quaternion.identity);
        newMonster.SetActive(false);

        MonsterStatus monsterStatus = newMonster.GetComponent<MonsterStatus>();
        if (monsterStatus != null)
        {
            monsterStatus.monsterType = (MonsterStatus.MonsterType)selectedInfo.monsterType;
            monsterStatus.attackType = (MonsterStatus.AttackType)selectedInfo.attackType;
            monsterStatus.maxHealth = selectedInfo.hp;
            monsterStatus.currentHealth = selectedInfo.hp;
            monsterStatus.attackPower = selectedInfo.damage;
            monsterStatus.attackSpeed = selectedInfo.attackSpeed;
            monsterStatus.moveSpeed = selectedInfo.moveSpeed;
            monsterStatus.attackRange = selectedInfo.attackRange;
            monsterStatus.indexNumber = poolIndex;
            monsterStatus.spawnPoolType = poolType;
            monsterStatus.spawnPoolIndex = poolIndex;
            monsterStatus.isInPool = addToPool;
            }

        MonsterSetting monsterSetting = newMonster.GetComponent<MonsterSetting>();
        if (monsterSetting != null)
        {
            monsterSetting.ground = ground;
        }

        if (spawnArea.transform.childCount >= 5)
        {
            int parentIndex = poolType switch
            {
                MonsterSpawnData.MonsterType.Default => 0,
                MonsterSpawnData.MonsterType.Sub => 1,
                MonsterSpawnData.MonsterType.Elite => 2,
                MonsterSpawnData.MonsterType.SubBoss => 3,
                MonsterSpawnData.MonsterType.Boss => 4,
                _ => -1
            };

            if (parentIndex >= 0 && parentIndex < spawnArea.transform.childCount)
            {
                newMonster.transform.SetParent(spawnArea.transform.GetChild(parentIndex));
            }
        }

        return newMonster;
    }

    // 큐에서 몬스터를 하나 꺼내 위치/회전 세팅 후 활성화 (없으면 새로 생성)
    private GameObject ActivateMonsterFromPool(MonsterSpawnData.MonsterType poolType, int index, Vector3 position, Quaternion rotation)
    {
        Queue<GameObject> pool = GetOrCreatePool(poolType, index);
        GameObject monster = null;

        while (pool.Count > 0 && monster == null)
        {
            monster = pool.Dequeue();
        }

        if (monster == null)
        {
            MonsterInfo info = GetMonsterInfo(poolType, index);
            if (info == null)
                return null;

            monster = CreateMonster(poolType, info, index, addToPool: false);

            switch (poolType)
            {
                case MonsterSpawnData.MonsterType.Default:
                    DfMonsters.Add(monster);
                    break;
                case MonsterSpawnData.MonsterType.Sub:
                    SubMonsters.Add(monster);
                    break;
                case MonsterSpawnData.MonsterType.Elite:
                    SpecialMonsters.Add(monster);
                    break;
                case MonsterSpawnData.MonsterType.SubBoss:
                    SubBossMonsters.Add(monster);
                    break;
                case MonsterSpawnData.MonsterType.Boss:
                    BossMonsters.Add(monster);
                    break;
            }
        }

        MonsterStatus status = monster.GetComponent<MonsterStatus>();
        if (status != null)
        {
            status.isInPool = false;
            status.monsterAlive = true;
        }

        monster.transform.position = position;
        monster.transform.rotation = rotation;
        monster.SetActive(true);
        return monster;
    }

    // 외부 스크립트가 큐에서 몬스터를 직접 꺼내 쓰고 싶을 때 사용
    public GameObject SpawnMonsterFromPool(MonsterSpawnData.MonsterType poolType, int index, Vector3 position, Quaternion rotation)
    {
        return ActivateMonsterFromPool(poolType, index, position, rotation);
    }

    // 동일 타입/인덱스 몬스터를 count만큼 연속 스폰
    private void SpawnMultipleFromPool(MonsterSpawnData.MonsterType poolType, int index, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPos = GetRandomSpawnPosition();
            ActivateMonsterFromPool(poolType, index, spawnPos, Quaternion.identity);
        }
    }

    // 보스 스폰 시 전용 위치/세팅 및 나머지 몬스터 정리
    private void SpawnBoss(MonsterSpawnData data)
    {
        for (int i = 0; i < data.MonsterCount; i++)
        {
            GameObject monster = ActivateMonsterFromPool(MonsterSpawnData.MonsterType.Boss, data.MonsterIndex, bossSpawnPosition.transform.position, Quaternion.Euler(0f, 180f, 0f));
            if (monster == null)
                continue;

            MonsterStatus monsterStatus = monster.GetComponent<MonsterStatus>();
            BossSetting bossSetting = monster.GetComponent<BossSetting>();
            if (bossSetting != null)
            {
                bossSetting.ground = ground;
                bossSetting.groundTrigger = groundTrigger;
                bossSetting.monsterStatus = monsterStatus;
                bossSetting.bossRoom = bossRoom;
            }

            if (bossRoom != null)
            {
                BoxCollider bossCollider = bossRoom.GetComponent<BoxCollider>();
                if (bossCollider != null)
                {
                    bossCollider.isTrigger = true;
                }
                bossRoom.transform.GetChild(0).gameObject.SetActive(false);
                bossRoom.transform.GetChild(1).gameObject.SetActive(false);
            }

            monsterSpawn = false;
            DisableAllNonBossMonsters();

            bossAlive = true;
            currentBossStatus = monsterStatus;
            playerUISetting.gameTimerUI.gameObject.SetActive(false);
            playerUISetting.bossUI.SetActive(true);
        }
    }

    // 보스 등장 시 일반 몬스터를 모두 비활성화하고 풀로 돌려보냄
    private void DisableAllNonBossMonsters()
    {
        foreach (var monster in DfMonsters)
        {
            if (monster != null && monster.activeInHierarchy)
            {
                monster.SetActive(false);
                MonsterStatus status = monster.GetComponent<MonsterStatus>();
                if (status != null)
                    status.monsterAlive = false;
                ReturnMonsterToPool(monster);
            }
        }
        foreach (var monster in SubMonsters)
        {
            if (monster != null && monster.activeInHierarchy)
            {
                monster.SetActive(false);
                MonsterStatus status = monster.GetComponent<MonsterStatus>();
                if (status != null)
                    status.monsterAlive = false;
                ReturnMonsterToPool(monster);
            }
        }
        foreach (var monster in SpecialMonsters)
        {
            if (monster != null && monster.activeInHierarchy)
            {
                monster.SetActive(false);
                MonsterStatus status = monster.GetComponent<MonsterStatus>();
                if (status != null)
                    status.monsterAlive = false;
                ReturnMonsterToPool(monster);
            }
        }
        foreach (var monster in SubBossMonsters)
            {
            if (monster != null && monster.activeInHierarchy)
            {
                monster.SetActive(false);
                MonsterStatus status = monster.GetComponent<MonsterStatus>();
                if (status != null)
                    status.monsterAlive = false;
                ReturnMonsterToPool(monster);
            }
        }
    }

    // 타입에 맞는 큐 딕셔너리 반환
    private Dictionary<int, Queue<GameObject>> GetPoolDictionary(MonsterSpawnData.MonsterType poolType)
    {
        return poolType switch
        {
            MonsterSpawnData.MonsterType.Default => defaultPools,
            MonsterSpawnData.MonsterType.Sub => subPools,
            MonsterSpawnData.MonsterType.Elite => specialPools,
            MonsterSpawnData.MonsterType.SubBoss => subBossPools,
            MonsterSpawnData.MonsterType.Boss => bossPools,
            _ => null
        };
    }

    // (타입, 인덱스) 조합의 큐를 찾아보고 없으면 새로 만든다
    private Queue<GameObject> GetOrCreatePool(MonsterSpawnData.MonsterType poolType, int index)
    {
        var dict = GetPoolDictionary(poolType);
        if (dict == null)
            return null;

        if (!dict.TryGetValue(index, out var queue))
        {
            queue = new Queue<GameObject>();
            dict[index] = queue;
        }

        return queue;
    }

    // 타입/인덱스에 해당하는 MonsterInfo 반환
    private MonsterInfo GetMonsterInfo(MonsterSpawnData.MonsterType poolType, int index)
    {
        List<MonsterInfo> list = poolType switch
        {
            MonsterSpawnData.MonsterType.Default => dfMonsterInfoList,
            MonsterSpawnData.MonsterType.Sub => sbMonsterInfoList,
            MonsterSpawnData.MonsterType.Elite => specialMonsterInfoList,
            MonsterSpawnData.MonsterType.SubBoss => sbBossMonsterInfoList,
            MonsterSpawnData.MonsterType.Boss => BossMonsterInfoList,
            _ => null
        };

        if (list == null || index < 0 || index >= list.Count)
            return null;

        return list[index];
    }

    // MonsterStatus.Dead() 등에서 호출되어 오브젝트를 다시 큐로 돌려보냄
    public void ReturnMonsterToPool(GameObject monster)
    {
        if (monster == null)
            return;

        MonsterStatus status = monster.GetComponent<MonsterStatus>();
        if (status == null || status.spawnPoolType == MonsterSpawnData.MonsterType.None)
            return;

        if (status.isInPool)
            return;

        var pool = GetOrCreatePool(status.spawnPoolType, status.spawnPoolIndex);
        if (pool == null)
            return;

        status.isInPool = true;
        pool.Enqueue(monster);
    }

    Vector3 GetRandomSpawnPosition() //몬스터를 생성할 랜덤 좌표 계산
    {
        Vector3 spawnPosition = Vector3.zero;
        UnityEngine.AI.NavMeshHit hit;
        bool validPosition = false;

        while (!validPosition)
        {
            float x = Random.Range(-100, 100); // 랜덤 x 좌표 생성
            float z = Random.Range(-100, 100); // 랜덤 z 좌표 생성
            Vector3 randomPoint = new Vector3(x, 0, z);

            if (UnityEngine.AI.NavMesh.SamplePosition(randomPoint, out hit, 1.0f, UnityEngine.AI.NavMesh.AllAreas))
            {
                spawnPosition = hit.position;
                bool isSafe = true;
                foreach (var player in playerList)
                {
                    if (Vector3.Distance(spawnPosition, player.transform.position) < safeDistance)
                    {
                        isSafe = false;
                        break;
                    }
                }
                if (isSafe)
                {
                    validPosition = true;
                }
            }
        }
        return spawnPosition;
    }
}