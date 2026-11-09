using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "CardData")]
public class CardData : ScriptableObject
{
    public string cardName; //카드드이름
    public Sprite cardImage; //카드 이미지(증강 선택시)
    public CardType cardType;
    public GameObject cardCase;
    public int upgradeCount; //업글횟수

    [Header("무기 데이터일 때")]
    public GameObject weaponPrefab; //무기 프리팹
    public string weaponDescription; //무기설명
    public float damage; //무기 공격력
    public float attackSpeed; //무기 공격속도
    public float defense; //무기 방어력
    public float projecTile; //무기 투사체 개수
    public float attackRange; //무기 공격범위
    public float duration; //무기 지속시간
    public bool isArousal; //진화여부
    public string arousalDescription; //진화 설명
    public List<WeaponUpgradeData> weaponUpgradeDataList = new List<WeaponUpgradeData>();

    [Header("능력치 데이터일때")]
    public UpgradeAbility upgradeAbility; //해당 카드가 올릴 능력치
    public float increaseAbility; //올릴 능력치 수치
    public string needArousalAbility; //진화 필요 능력치

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

    [System.Serializable]
    public class WeaponUpgradeData
    {
        public UpgradeAbility upgradeAbility; //업그레이드할 무기의 능력치
        public float increaseStatus; //올릴 능력치 수치
    }
}
