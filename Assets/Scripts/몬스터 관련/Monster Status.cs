using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MonsterStatus : MonoBehaviour
{
    public enum MonsterType
    {
        Normale, Elite, Middleboss, Boss, None
    }
    public enum AttackType
    {
        Melee, Ranged, Special, None
    }
    [Header("몬스터 기본 세팅")]
    public Animator anim;
    public MonsterType monsterType = MonsterType.None;
    public AttackType attackType = AttackType.None;
    public GameObject rangeAttack;

    [Header("몬스터 스탯 세팅")]
    public float attackPower;
    public float attackSpeed;
    public float attackRange;
    public float maxHealth;
    public float currentHealth;
    public float moveSpeed;

    [Header("기본 설정")]
    public bool monsterAlive = false;
    //public bool isMove = false;
    public bool isAttack = false;

    public GameObject expPrefab; // 경험치 프리팹
    public GameObject goldPrefab; // 골드 프리팹
    public GameObject healItemPrefab; // 회복 아이템 프리팹
    public GameObject magnetItemPrefab; // 자석 아이템 프리팹
    public GameObject damageTextUI; // 데미지 텍스트 UI

    public bool hitExtinction = false;

    public MonsterSpawn.MonsterSpawnData.MonsterType spawnPoolType = MonsterSpawn.MonsterSpawnData.MonsterType.None;
    public int spawnPoolIndex = -1;
    public bool isInPool = false;

    public int indexNumber; //생성시의 생성번호

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private GameObject GetOrCreateDamageText()
    {
        Transform parent = EnemyManager.Instance.transform.GetChild(4);
        return EnemyManager.Instance.GetDamageTextFromPool(damageTextUI, parent);
    }

    public void TakeDamage(float weaponDamage, GameObject player)
    {
        UpgradeInGame upgradeInGame = player.GetComponent<UpgradeInGame>();
        PlayerStatus playerStatus = player.GetComponent<PlayerStatus>();
        PlayerSystem playerSystem = player.GetComponent<PlayerSystem>();

        if (upgradeInGame == null || playerStatus == null || playerSystem == null)
            return;

        // 데미지 계산: (weaponDamage * upgradeAbilityValues.attack) + playerStatus.attack
        float upgradeAttack = upgradeInGame.upgradeAbilityValues.attack;
        float playerAttack = playerStatus.attack;
        float finalDamage = (weaponDamage * upgradeAttack) + playerAttack;

        // 크리티컬 확률 적용
        float criticalChance = playerStatus.critical; // 0~100
        float criticalDamageMultiplier = 1f;
        bool isCritical = false;

        if (Random.Range(0f, 100f) < criticalChance)
        {
            // 크리티컬 발생
            criticalDamageMultiplier = playerStatus.criticalDamage;
            isCritical = true;
        }

        finalDamage *= criticalDamageMultiplier;

        // === 공격 범위 내에 있을 때만 데미지 텍스트 생성 ===
        bool inAttackRange = false;
        if (playerSystem.playerAttackRange != null)
        {
            Collider rangeCol = playerSystem.playerAttackRange.GetComponent<Collider>();
            if (rangeCol != null)
            {
                inAttackRange = rangeCol.bounds.Contains(transform.position);
            }
        }

        if (damageTextUI != null && inAttackRange)
        {
            // 몬스터의 정확한 위치에 생성 (y는 0으로 고정)
            GameObject damageText = GetOrCreateDamageText();
            Vector3 damageTextPosition = new Vector3(this.transform.position.x, 0, this.transform.position.z);
            damageText.transform.position = damageTextPosition;
            
            // 캔버스의 월드 스페이스 설정 확인
            Canvas canvas = damageText.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.renderMode = RenderMode.WorldSpace;
            }
            
            TextMeshProUGUI damageTextMesh = damageText.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
            if (damageTextMesh != null)
            {
                damageTextMesh.text = Mathf.Round(finalDamage).ToString();
                // 크리티컬 여부에 따라 색상 설정
                damageTextMesh.color = isCritical ? new Color(1f, 0.5f, 0f) : Color.white; // 크리티컬: 주황색, 일반: 흰색
            }

            damageText.SetActive(true);
        }

        currentHealth -= finalDamage;
        
        if (currentHealth <= 0)
        {
            if (monsterType != MonsterType.Boss)
            {
                if (TryGetComponent<PoisonMonster>(out var poison)) poison.MarkAsDead();

                if (TryGetComponent<ExplodingMonster>(out var exploding))
                {
                    exploding.ExplodeImmediately(); // 💥 먼저 폭발
                }

                if (TryGetComponent<SplittingMonster>(out var splitting))
                {
                    splitting.SpawnChild();
                }

                Drop();
                Dead(); // 일반 몬스터만
            }
        }
    }

    // 사망 처리 메서드
    void Dead()
    {
        monsterAlive = false;
        gameObject.SetActive(false);
        if (MonsterSpawn.Instance != null)
        {
            MonsterSpawn.Instance.ReturnMonsterToPool(this.gameObject);
        }
    }

    void Drop()
    {
        // 현재 오브젝트의 x, z 좌표를 사용하고 y 좌표는 1로 설정
        Vector3 dropPosition = new Vector3(transform.position.x, 1, transform.position.z);
        
        // 경험치 오브젝트 풀링
        GameObject expObject = GetOrCreateExpObject();
        if (expObject != null)
        {
            expObject.transform.position = dropPosition;
            Collider expCollider = expObject.GetComponent<Collider>();
            if (expCollider != null)
            {
                expCollider.isTrigger = false;
            }
            ExpItem expItem = expObject.GetComponent<ExpItem>();
            expItem.ResetValue(); // 재사용 구의 값을 원래대로 되돌린 뒤 배율 적용
            if(hitExtinction)
            {
                expItem.expValue *= 1.5f;
                hitExtinction = false;
            }
            expObject.SetActive(true);
            EnemyManager.Instance.MarkExpActive(expObject);
        }

        // 20% 확률로 골드 드랍
        if (Random.Range(0, 100) < 20)
        {
            // 골드 드랍 위치를 경험치 드랍 위치에서 약간 오른쪽으로 이동
            Vector3 goldDropPosition = new Vector3(transform.position.x + 0.5f, 1, transform.position.z);
            
            // 골드 오브젝트 풀링
            GameObject goldObject = GetOrCreateGoldObject();
            if (goldObject != null)
            {
                goldObject.transform.position = goldDropPosition;
                Collider goldCollider = goldObject.GetComponent<Collider>();
                if (goldCollider != null)
                {
                    goldCollider.isTrigger = false;
                }
                goldObject.SetActive(true);
                EnemyManager.Instance.MarkGoldActive(goldObject);
            }
        }

        // MiddleBoss 타입일 경우 힐 또는 자석 아이템 드랍
        if (monsterType == MonsterType.Middleboss)
        {
            // 50% 확률로 힐 아이템, 50% 확률로 자석 아이템
            bool isHealItem = Random.Range(0, 2) == 0;
            Vector3 itemDropPosition = new Vector3(transform.position.x - 0.5f, 1, transform.position.z);

            if (isHealItem)
            {
                // 힐 아이템 생성
                GameObject healItem = Instantiate(healItemPrefab);
                healItem.transform.position = itemDropPosition;
            }
            else
            {
                // 자석 아이템 생성
                GameObject magnetItem = Instantiate(magnetItemPrefab);
                magnetItem.transform.position = itemDropPosition;
            }
        }
    }

    private GameObject GetOrCreateExpObject()
    {
        Transform parent = EnemyManager.Instance.transform.GetChild(2);
        return EnemyManager.Instance.GetExpFromPool(expPrefab, parent);
    }

    private GameObject GetOrCreateGoldObject()
    {
        Transform parent = EnemyManager.Instance.transform.GetChild(2);
        return EnemyManager.Instance.GetGoldFromPool(goldPrefab, parent);
    }
}

