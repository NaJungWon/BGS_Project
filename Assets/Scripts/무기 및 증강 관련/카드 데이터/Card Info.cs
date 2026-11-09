using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro 사용을 위해 추가

public class CardInfo : MonoBehaviour
{
    public GameObject player;//사용할 플레이어

    public string cardName; //카드드이름
    public Sprite cardImage; //카드 이미지(증강 선택시)
    public CardType cardType; //카드 타입
    public int upgradeCount; //업글횟수

    [Header("무기 데이터일 때")]
    public bool dfWeapon; //기본 무기 일때
    public GameObject weaponPrefab; //무기 프리팹
    private GameObject weaponInstance; // 생성된 무기 인스턴스 저장
    public string weaponDescription; //무기설명
    public float damage; //무기 공격력
    public float attackSpeed; //무기 공격속도
    public float defense; //무기 방어력
    public float projecTile; //무기 투사체 개수
    public float attackRange; //무기 공격범위
    public float duration; //무기 지속시간
    
    public bool isArousal; //진화여부
    public string arousalDescription; //진화 설명
    public List<CardData.WeaponUpgradeData> weaponUpgradeDataList = new List<CardData.WeaponUpgradeData>(); // 무기 업그레이드 데이터 리스트
    //public float increaseStatus; //올릴 무기 능력치 수치

    public TextMeshProUGUI cardDescription; // Text를 TextMeshProUGUI로 변경

    [Header("능력치 데이터일때")]
    public UpgradeAbility upgradeAbility; //해당 카드가 올릴 능력치
    public float increaseAbility; //올릴 능력치 수치

    public enum CardType
    {
        Weapon, Ability, Else
    }

    public enum UpgradeAbility
    {
        None,
        MaxHealth,
        HealthRegeneration,
        Attack,
        AttackSpeed,
        Defense,
        Speed,
        Critical,
        ProjecTile,
        AttackRange,
        Duration,
        AcquisitionRange,
        ExperienceGain,
        Heal,
        Gold
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SelectCard()
    {
        switch (upgradeCount)
        {
            case 0:
                // upgradeCount가 1일 때의 동작
                Debug.Log($"{cardName} 카드가 1회 업그레이드되었습니다.");

                // 카드 타입이 Weapon인 경우 weaponPrefab을 생성하여 player의 3번째 자식의 자식으로 추가
                if (cardType == CardType.Weapon && weaponPrefab != null)
                {
                    weaponInstance = Instantiate(weaponPrefab); // weaponPrefab 인스턴스 생성
                    if (player.transform.childCount >= 3) // 플레이어의 자식이 3개 이상인지 확인
                    {
                        Transform parentTransform = player.transform.GetChild(4); // 4번째 자식 가져오기
                        weaponInstance.transform.SetParent(parentTransform); // 4번째 자식으로 설정
                        weaponInstance.transform.localPosition = Vector3.zero; // 위치를 (0, 0, 0)으로 설정
                        
                        // 초기값 설정
                        Component[] components = weaponInstance.GetComponents<Component>();
                        foreach (var upgradeData in weaponUpgradeDataList)
                        {
                            switch ((UpgradeAbility)upgradeData.upgradeAbility)
                            {
                                case UpgradeAbility.Attack:
                                    SetInitialWeaponValues(components, "weaponDamage", damage);
                                    break;
                                case UpgradeAbility.AttackSpeed:
                                    SetInitialWeaponValues(components, "weaponCooldown", attackSpeed);
                                    break;
                                case UpgradeAbility.Defense:
                                    SetInitialWeaponValues(components, "weaponDefense", defense);
                                    break;
                                case UpgradeAbility.ProjecTile:
                                    SetInitialWeaponValues(components, "weaponProjecTile", projecTile);
                                    break;
                                case UpgradeAbility.AttackRange:
                                    SetInitialWeaponValues(components, "weaponAttackRange", attackRange);
                                    break;
                                case UpgradeAbility.Duration:
                                    SetInitialWeaponValues(components, "weaponDuration", duration);
                                    break;
                            }
                        }
                    }
                    UpgradeWeaponValues();
                    upgradeCount++;
                    //ShowInitialWeaponValues();
                }
                else if(cardType == CardType.Ability)
                {
                    UpgradeAbilityValues();
                    upgradeCount++;
                }
                break;

            case 1:
                // upgradeCount가 2일 때의 동작
                Debug.Log($"{cardName} 카드가 2회 업그레이드되었습니다.");
                if (cardType == CardType.Weapon && weaponInstance != null)
                {
                    UpgradeWeaponValues();
                    upgradeCount++;
                }
                else if (cardType == CardType.Ability)
                {
                    UpgradeAbilityValues();
                    upgradeCount++;
                }
                break;

            case 2:
                // upgradeCount가 3일 때의 동작
                Debug.Log($"{cardName} 카드가 3회 업그레이드되었습니다.");
                if (cardType == CardType.Weapon && weaponInstance != null)
                {
                    UpgradeWeaponValues();
                    upgradeCount++;
                }
                else if (cardType == CardType.Ability)
                {
                    UpgradeAbilityValues();
                    upgradeCount++;
                }
                break;

            case 3:
                // upgradeCount가 4일 때의 동작
                Debug.Log($"{cardName} 카드가 4회 업그레이드되었습니다.");
                if (cardType == CardType.Weapon && weaponInstance != null)
                {
                    UpgradeWeaponValues();
                    upgradeCount++;
                }
                else if (cardType == CardType.Ability)
                {
                    UpgradeAbilityValues();
                    upgradeCount++;
                }
                break;

            case 4:
                // upgradeCount가 5일 때의 동작
                Debug.Log($"{cardName} 카드가 5회 업그레이드되었습니다.");
                if (cardType == CardType.Weapon && weaponInstance != null)
                {
                    UpgradeWeaponValues();
                    upgradeCount++;
                    cardDescription.text = arousalDescription;
                }
                else if (cardType == CardType.Ability)
                {
                    UpgradeAbilityValues();
                    upgradeCount++;
                }
                break;

            case 5:
                if (cardType == CardType.Weapon && weaponInstance != null)
                {
                    UpgradeWeaponValues();
                    Debug.Log("진화 완료");
                    //cardDescription.text = arousalDescription;
                    isArousal = true;
                    Component[] components = weaponInstance.GetComponents<Component>();
                    foreach (var component in components)
                    {
                        var field = component.GetType().GetField("isArousal");
                        if (field != null)
                        {
                            field.SetValue(component, true);
                            break;
                        }
                    }
                }
                // upgradeCount가 5를 달성하고 진화해야할 때 여기 작성할 것
                //Debug.Log($"{cardName} 카드의 업그레이드 횟수가 5를 초과했습니다.");
                break;
        }
    }

    private void SetWeaponComponentValue(Component[] components, string fieldName, float value)
    {
        foreach (var component in components)
        {
            // 리플렉션을 사용하여 컴포넌트의 필드 값을 설정
            var field = component.GetType().GetField(fieldName);
            if (field != null)
            {
                float currentValue = (float)field.GetValue(component);
                
                if (upgradeCount == 0)
                {
                    return;
                }
                else
                {
                    // upgradeCount가 1보다 클 때는 기존 값에 누적
                    if (fieldName == "weaponCooldown")
                    {
                        // 초기값인 attackSpeed를 기준으로 계산
                        field.SetValue(component, attackSpeed / (1 + (value * upgradeCount)/100));
                    }
                    else if (fieldName == "weaponDuration")
                    {
                        // value가 10이면 10% 증가, 즉 110% 적용
                        field.SetValue(component, duration * (1 + (value * upgradeCount)/100));
                    }
                    else
                    {
                        field.SetValue(component, currentValue + value);
                    }
                }
                break;
            }
        }
    }

    private void UpgradeWeaponValues()
    {
        // weaponInstance의 모든 컴포넌트를 가져옵니다
        Component[] components = weaponInstance.GetComponents<Component>();
        string description = "";
        
        foreach (var upgradeData in weaponUpgradeDataList)
        {
            switch ((UpgradeAbility)upgradeData.upgradeAbility)
            {
                case UpgradeAbility.Attack:
                    float currentDamage = damage + (upgradeData.increaseStatus * (upgradeCount));
                    float newDamage = damage + (upgradeData.increaseStatus * (upgradeCount + 1));
                    description += $"공격력 {currentDamage} -> {newDamage}\n";
                    SetWeaponComponentValue(components, "weaponDamage", upgradeData.increaseStatus);
                    break;
                case UpgradeAbility.AttackSpeed:
                    float currentCooldown = attackSpeed / (1 + (upgradeData.increaseStatus * (upgradeCount))/100);
                    float newCooldown = attackSpeed / (1 + (upgradeData.increaseStatus * (upgradeCount + 1))/100);
                    description += $"공격속도 {currentCooldown:F2}초 -> {newCooldown:F2}초\n";
                    SetWeaponComponentValue(components, "weaponCooldown", upgradeData.increaseStatus);
                    // dfWeapon일 때 attackSpeedValue 설정
                    if (dfWeapon)
                    {
                        SetWeaponComponentValue(components, "attackSpeedValue", upgradeData.increaseStatus);
                    }
                    break;
                case UpgradeAbility.Defense:
                    float currentDefense = defense + (upgradeData.increaseStatus * (upgradeCount));
                    float newDefense = defense + (upgradeData.increaseStatus * (upgradeCount + 1));
                    description += $"방어력 {currentDefense} -> {newDefense}\n";
                    SetWeaponComponentValue(components, "weaponDefense", upgradeData.increaseStatus);
                    break;
                case UpgradeAbility.ProjecTile:
                    float currentProjectile = projecTile + (upgradeData.increaseStatus * (upgradeCount));
                    float newProjectile = projecTile + (upgradeData.increaseStatus * (upgradeCount + 1));
                    description += $"투사체 개수 {currentProjectile} -> {newProjectile}\n";
                    SetWeaponComponentValue(components, "weaponProjecTile", upgradeData.increaseStatus);
                    break;
                case UpgradeAbility.AttackRange:
                    float currentRange = attackRange + (upgradeData.increaseStatus * (upgradeCount));
                    float newRange = attackRange + (upgradeData.increaseStatus * (upgradeCount + 1));
                    description += $"공격범위 {currentRange} -> {newRange}\n";
                    SetWeaponComponentValue(components, "weaponAttackRange", upgradeData.increaseStatus);
                    break;
                case UpgradeAbility.Duration:
                    float currentDuration = duration * (1 + (upgradeData.increaseStatus * (upgradeCount))/100);
                    float newDuration = duration * (1 + (upgradeData.increaseStatus * (upgradeCount + 1))/100);
                    description += $"지속시간 {currentDuration:F2}초 -> {newDuration:F2}초\n";
                    SetWeaponComponentValue(components, "weaponDuration", upgradeData.increaseStatus);
                    break;
            }
        }

        if (cardDescription != null)
        {
            cardDescription.text = description;
        }
    }

    private float GetComponentValue(Component[] components, string fieldName)
    {
        foreach (var component in components)
        {
            var field = component.GetType().GetField(fieldName);
            if (field != null)
            {
                return (float)field.GetValue(component);
            }
        }
        return 0f;
    }

    private void UpgradeAbilityValues()
    {
        // UpgradeInGame 스크립트 가져오기
        UpgradeInGame upgradeInGame = player.GetComponent<UpgradeInGame>();
        if (upgradeInGame != null)
        {
            string description = "";
            // UpgradeAbilityCardValues 구조체에 값 전달
            switch (upgradeAbility)
            {
                case UpgradeAbility.MaxHealth:
                    upgradeInGame.abilityCardValues.maxHealth += increaseAbility;
                    float currentMaxHealth = upgradeInGame.abilityCardValues.maxHealth;
                    float newMaxHealth = upgradeInGame.abilityCardValues.maxHealth + (increaseAbility);
                    description += $"최대체력 {currentMaxHealth} -> {newMaxHealth}\n";
                    break;
                case UpgradeAbility.HealthRegeneration:
                    upgradeInGame.abilityCardValues.healthRegeneration += (int)increaseAbility;
                    int currentHealthRegen = upgradeInGame.abilityCardValues.healthRegeneration;
                    int newHealthRegen = upgradeInGame.abilityCardValues.healthRegeneration + (int)(increaseAbility);
                    description += $"체력회복 {currentHealthRegen} -> {newHealthRegen}\n";
                    break;
                case UpgradeAbility.Attack:
                    upgradeInGame.abilityCardValues.attack += increaseAbility;
                    float currentAttack = upgradeInGame.abilityCardValues.attack;
                    float newAttack = upgradeInGame.abilityCardValues.attack + (increaseAbility);
                    description += $"공격력 {currentAttack} -> {newAttack}\n";
                    break;
                case UpgradeAbility.AttackSpeed:
                    upgradeInGame.abilityCardValues.attackSpeed += increaseAbility;
                    float currentAttackSpeed = upgradeInGame.abilityCardValues.attackSpeed;
                    float newAttackSpeed = upgradeInGame.abilityCardValues.attackSpeed + (increaseAbility);
                    description += $"공격속도 {currentAttackSpeed}% -> {newAttackSpeed}%\n";
                    break;
                case UpgradeAbility.Defense:
                    upgradeInGame.abilityCardValues.defense += (int)increaseAbility;
                    int currentDefense = upgradeInGame.abilityCardValues.defense;
                    int newDefense = upgradeInGame.abilityCardValues.defense + (int)(increaseAbility);
                    description += $"방어력 {currentDefense} -> {newDefense}\n";
                    break;
                case UpgradeAbility.Speed:
                    upgradeInGame.abilityCardValues.speed += increaseAbility;
                    float currentSpeed = upgradeInGame.abilityCardValues.speed;
                    float newSpeed = upgradeInGame.abilityCardValues.speed + (increaseAbility);
                    description += $"이동속도 {currentSpeed}% -> {newSpeed}%\n";
                    break;
                case UpgradeAbility.Critical:
                    upgradeInGame.abilityCardValues.critical += increaseAbility;
                    float currentCritical = upgradeInGame.abilityCardValues.critical;
                    float newCritical = upgradeInGame.abilityCardValues.critical + (increaseAbility);
                    description += $"치명타 {currentCritical}% -> {newCritical}%\n";
                    break;
                case UpgradeAbility.ProjecTile:
                    upgradeInGame.abilityCardValues.projecTile += (int)increaseAbility;
                    int currentProjectile = upgradeInGame.abilityCardValues.projecTile;
                    int newProjectile = upgradeInGame.abilityCardValues.projecTile + (int)(increaseAbility);
                    description += $"투사체 개수 {currentProjectile} -> {newProjectile}\n";
                    break;
                case UpgradeAbility.AttackRange:
                    upgradeInGame.abilityCardValues.attackRange += increaseAbility;
                    float currentAttackRange = upgradeInGame.abilityCardValues.attackRange;
                    float newAttackRange = upgradeInGame.abilityCardValues.attackRange + (increaseAbility);
                    description += $"공격범위 {currentAttackRange}% -> {newAttackRange}%\n";
                    break;
                case UpgradeAbility.Duration:
                    upgradeInGame.abilityCardValues.duration += increaseAbility;
                    float currentDuration = upgradeInGame.abilityCardValues.duration;
                    float newDuration = upgradeInGame.abilityCardValues.duration + (increaseAbility);
                    description += $"지속시간 {currentDuration}% -> {newDuration}%\n";
                    break;
                case UpgradeAbility.AcquisitionRange:
                    upgradeInGame.abilityCardValues.acquisitionRange += increaseAbility;
                    float currentAcquisitionRange = upgradeInGame.abilityCardValues.acquisitionRange;
                    float newAcquisitionRange = upgradeInGame.abilityCardValues.acquisitionRange + (increaseAbility);
                    description += $"획득범위 {currentAcquisitionRange}% -> {newAcquisitionRange}%\n";
                    break;
                case UpgradeAbility.ExperienceGain:
                    upgradeInGame.abilityCardValues.experienceGain += increaseAbility;
                    float currentExpGain = upgradeInGame.abilityCardValues.experienceGain;
                    float newExpGain = upgradeInGame.abilityCardValues.experienceGain + (increaseAbility);
                    description += $"경험치 획득 {currentExpGain}% -> {newExpGain}%\n";
                    break;
            }

            if (cardDescription != null)
            {
                cardDescription.text = description;
            }

            // PlayerStatusUpgradeSetting 호출 시 필요한 파라미터들을 전달
            upgradeInGame.PlayerStatusUpgradeSetting(
                upgradeInGame.playerInfo,
                upgradeInGame.upgradeWeaponValues,
                upgradeInGame.upgradeAbilityValues,
                upgradeInGame.abilityCardValues,
                upgradeInGame.abilityPassiveValues
            );
        }
    }

    private void SetInitialWeaponValues(Component[] components, string fieldName, float value)
    {
        foreach (var component in components)
        {
            var field = component.GetType().GetField(fieldName);
            if (field != null)
            {
                field.SetValue(component, value);
                break;
            }
        }
    }
}
