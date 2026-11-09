using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UI;

[CreateAssetMenu(menuName = "UpgradeAbilityData")]
public class UpgradeAbilityData : ScriptableObject
{
    [Header("능력치이름/이미지/업글수치/업글최대치/업글효율/필요골드")]
    public string abilityName;
    public Sprite abilityImage;
    public UpgradeAbilityCategory upgradeAbilityCategory;
    public int upgradeCount;
    public int maxUpgradeCount;
    public int needGold;
    public int increaseNeedGoldValue;
    [Header("업글 타입 깡 수치 / % 수치")]
    public UpgradeValueType upgradeValueType;
    public float efficiency;

    public enum UpgradeValueType
    {
        Flat, // 깡 수치
        Percent, // % 수치
        Multiply // 배수 수치
    }
    
    public enum UpgradeAbilityCategory
    {
        maxHealth, // 최대 체력
        healthRegeneration, // 체력 재생
        attack, // 공격력
        attackSpeed, //공격 속도
        defense, // 방어력
        speed, // 이동 속도
        critical, // 크리티컬 확률%
        projecTile, // 투사체 증가
        attackRange, // 공격 범위 증가%
        duration, // 지속시간 증가%
        acquisitionRange, // 획득 범위 증가%
        experienceGain, // 경험치 획득량 증가%
        goldGain, //골드 획득량 증가%
    }
}
