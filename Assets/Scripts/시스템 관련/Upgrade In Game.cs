using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeInGame : MonoBehaviour
{
    public GameManager gameManager;
    public PlayerSystem playerSystem;
    public PlayerStatus playerStatus;
    public CharacterInfo playerInfo; //플레이어 기본 스탯
    public UpgradeInRoom.UpgradeAbilityValues upgradeAbilityValues; //기본 상점 능력치 증가값
    public UpgradeWeaponValues upgradeWeaponValues; //기본 무기 전용

    public struct UpgradeWeaponValues //기본무기 업그레이드 능력치 증가값
    {
        public float weaponAttackSpeed;
        
    }

    public struct UpgradeAbilityCardValues //카드능력치증강선택으로 인한 능력치 증가값
    {
        public float maxHealth;
        public int healthRegeneration;
        public float attack;
        public float attackSpeed;
        public int defense;
        public float speed;
        public float critical;
        public int projecTile;
        public float attackRange;
        public float duration;
        public float acquisitionRange;
        public float experienceGain;
    }

    public struct UpgradeAbilityPassiveValues //패시브스킬로 인한 능력치 증가값
    {
        public float maxHealth;
        public int healthRegeneration;
        public float attack;
        public float attackSpeed;
        public int defense;
        public float speed;
        public float critical;
        public int projecTile;
        public float attackRange;
        public float duration;
        public float acquisitionRange;
        public float experienceGain;
    }

    public UpgradeAbilityCardValues abilityCardValues; // 카드 증강으로 인한 능력치 증가값
    public UpgradeAbilityPassiveValues abilityPassiveValues; //패시브로 인한 능력치 증가값

    // Start is called before the first frame update
    void Awake()
    {
        playerSystem = GetComponent<PlayerSystem>();
        playerStatus = GetComponent<PlayerStatus>();
        // GameManager 찾기
        gameManager = playerSystem.gameManager;
        if (gameManager != null)
        {
            playerInfo = gameManager.playerCharacterInfo;
            upgradeAbilityValues = gameManager.upgradeAbilityValues;
        }
    }

    public void PlayerStatusUpgradeSetting(CharacterInfo playerInfo,UpgradeWeaponValues upgradeWeaponValues, UpgradeInRoom.UpgradeAbilityValues upgradeValues,
    UpgradeAbilityCardValues abilityCardValues, UpgradeAbilityPassiveValues upgradePassiveValues) //게임 중 스테이터스 업그레이드 설정
    {
        //여기서 캐릭터 기본 스탯과 상점에서 구매한 업그레이드 능력치를 Status에 적용시킨다.
        if (playerStatus != null)
        {
            playerStatus.maxHealth = (playerInfo.health * upgradeValues.maxHealth) + (abilityCardValues.maxHealth * upgradeValues.maxHealth) + (upgradePassiveValues.maxHealth * upgradeValues.maxHealth);
            playerStatus.healthRegeneration = playerInfo.healthRegeneration + upgradeValues.healthRegeneration + abilityCardValues.healthRegeneration + upgradePassiveValues.healthRegeneration;
            playerStatus.attack = (playerInfo.attack * upgradeValues.attack) + (abilityCardValues.attack * upgradeValues.attack) + (upgradePassiveValues.attack * upgradeValues.attack);
            playerStatus.attackSpeed = playerInfo.attackSpeed / (1 + (upgradeValues.attackSpeed + upgradeWeaponValues.weaponAttackSpeed + abilityCardValues.attackSpeed + upgradePassiveValues.attackSpeed) / 100);
            playerStatus.defense = playerInfo.defense + upgradeValues.defense + abilityCardValues.defense + upgradePassiveValues.defense;
            playerStatus.speed = playerInfo.speed * (1 + (upgradeValues.speed + abilityCardValues.speed + upgradePassiveValues.speed) / 100);
            playerStatus.critical = playerInfo.critical + upgradeValues.critical + abilityCardValues.critical + upgradePassiveValues.critical;
            playerStatus.projecTile = playerInfo.projecTile + upgradeValues.projecTile + abilityCardValues.projecTile + upgradePassiveValues.projecTile;
            playerStatus.attackRange = playerInfo.attackRange * (1 + (upgradeValues.attackRange + abilityCardValues.attackRange + upgradePassiveValues.attackRange) / 100);
            playerStatus.duration = playerInfo.duration * (1 + (upgradeValues.duration + abilityCardValues.duration + upgradePassiveValues.duration) / 100);
            playerStatus.acquisitionRange = playerInfo.acquisitionRange * (1 + (upgradeValues.acquisitionRange + abilityCardValues.acquisitionRange + upgradePassiveValues.acquisitionRange) / 100);
            playerStatus.experienceGain = playerInfo.experienceGain * (1 + (upgradeValues.experienceGain + abilityCardValues.experienceGain + upgradePassiveValues.experienceGain) / 100);
            playerSystem.Staminasynchronization();
        }
    }
}
