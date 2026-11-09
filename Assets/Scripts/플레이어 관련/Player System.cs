using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSystem : MonoBehaviour
{
    [Header("메인 3개")]
    public GameManager gameManager;
    public GameObject player;
    public GameObject inGameCanvas;

    [Header("자동 입력 값")]
    public PlayerStatus playerStatus;
    public GameObject playerAttackRange; // 플레이어 공격 범위 오브젝트
    public GameObject expGainRange; // 경험치 획득 범위 오브젝트
    public PlayerUISetting playerUISetting;
    public GameObject selectCardUI; // 카드 선택 UI 창
    public SelectedCardSystem selectedCardSystem; //카드 선택 시스템 스크립트

    private float originAttackRange;
    private float originAcquisitionRange;

    void Awake()
    {
        playerStatus = player.GetComponent<PlayerStatus>();
        playerUISetting = inGameCanvas.GetComponent<PlayerUISetting>();
        selectedCardSystem = inGameCanvas.GetComponent<SelectedCardSystem>();
        selectCardUI = inGameCanvas.transform.GetChild(2).gameObject;
        playerAttackRange = player.transform.GetChild(4).gameObject;
        expGainRange = player.transform.GetChild(5).gameObject;

        // 초기 범위 값과 scale 저장
        originAttackRange = playerStatus.attackRange;
        originAcquisitionRange = playerStatus.acquisitionRange;

        // selectedCardSystem의 player를 현재 PlayerSystem이 컴포넌트된 게임 오브젝트로 설정
        selectedCardSystem.player = this.gameObject;

        // gameManager를 씬에서 GameManager 인스턴스로 초기화
        gameManager = FindObjectOfType<GameManager>();
        
        // dfCards에 dfWeaponCard 정보를 할당
        selectedCardSystem.dfCards = gameManager.playerCharacterInfo.dfWeaponCard;
    }

    public void Update()
    {
        // attackRange 값이 변경되었는지 확인
        if (originAttackRange != playerStatus.attackRange)
        {
            playerAttackRange.transform.localScale = Vector3.one * playerStatus.attackRange;
            originAttackRange = playerStatus.attackRange;
        }

        // acquisitionRange 값이 변경되었는지 확인
        if (originAcquisitionRange != playerStatus.acquisitionRange)
        {
            expGainRange.transform.localScale = Vector3.one * playerStatus.acquisitionRange;
            originAcquisitionRange = playerStatus.acquisitionRange;
        }
    }

    public void PlayerStatusSetting(CharacterInfo playerInfo, UpgradeInRoom.UpgradeAbilityValues upgradeValues) //시작시 플레이어 스탯 동기화
    {
        //여기서 캐릭터 기본 스탯과 상점에서 구매한 업그레이드 능력치를 Status에 적용시킨다.
        if (playerStatus != null)
        {
            // starterCharacter의 데이터를 playerStatus에 입력
            playerStatus.characterName = playerInfo.characterName;
            playerStatus.level = playerInfo.level;
            playerStatus.characterOccupation = (PlayerStatus.CharacterOccupation)playerInfo.characterOccupation;
            playerStatus.needExperience = playerInfo.needExperience;
            playerStatus.maxHealth = playerInfo.health * upgradeValues.maxHealth;
            playerStatus.healthRegeneration = playerInfo.healthRegeneration + upgradeValues.healthRegeneration;
            playerStatus.attack = playerInfo.attack * upgradeValues.attack;
            playerStatus.attackSpeed = playerInfo.attackSpeed / (1 + upgradeValues.attackSpeed / 100); //캐릭터 공속에서 업글공속만큼 나누어 적용
            playerStatus.defense = playerInfo.defense + upgradeValues.defense;
            playerStatus.speed = playerInfo.speed * (1 + upgradeValues.speed / 100);
            playerStatus.critical = playerInfo.critical + upgradeValues.critical;
            playerStatus.criticalDamage = playerInfo.criticalDamage;
            playerStatus.projecTile = playerInfo.projecTile + upgradeValues.projecTile;
            playerStatus.attackRange = playerInfo.attackRange * (1 + upgradeValues.attackRange / 100);
            playerStatus.duration = playerInfo.duration * (1 + upgradeValues.duration / 100);
            playerStatus.acquisitionRange = playerInfo.acquisitionRange * (1 + upgradeValues.acquisitionRange / 100);
            playerStatus.experienceGain = playerInfo.experienceGain * (1 + upgradeValues.experienceGain / 100);
            playerStatus.isAlive = true; // 생존 여부 설정
            playerStatus.HpSynchronization();
            Staminasynchronization();
        }
    }

    public void GainExp(float amount)
    {
        playerStatus.currentExperience += amount;
        playerUISetting.UpdateExperienceUI(playerStatus);

        if (playerStatus.currentExperience >= playerStatus.needExperience)
        {
            LevelUp();
        }
    }

    public void LevelUp()
    {
        playerStatus.currentExperience = 0; // 경험치 초기화
        playerStatus.needExperience += playerStatus.needExperience * 0.05f; // 다음 레벨까지의 경험치 증가
        playerStatus.level++;
        
        playerUISetting.UpdatePlayerLevelUI(playerStatus); // 플레이어 레벨UI업데이트
        playerUISetting.UpdateExperienceUI(playerStatus); // 경험치 UI 업데이트
        selectedCardSystem.CardSpawn();

        GameManager.gameStatus = GameManager.GameStatus.Pause; //게임상태 일시정지로 변경
        GameManager.PauseGame(); //게임 일시정지
        
        Debug.Log("카드 선택을 위해 게임 일시정지");
        playerUISetting.backGround.SetActive(true);
        selectCardUI.SetActive(true); // 스킬 UI 활성화
    }

    //플레이어 UI설정동기화
    public void Staminasynchronization()
    {
        playerUISetting.UpdateHealthUI(playerStatus);
        playerUISetting.UpdateExperienceUI(playerStatus);
        playerUISetting.UpdatePlayerLevelUI(playerStatus);
        playerUISetting.UpdatePlayerIconUI(gameManager.playerCharacterInfo);
    }

    public void PlayerSkillSetting(CharacterInfo playerInfo)
    {
        if (playerInfo.chracterSkill != null)
        {
            // characterSkill을 player의 자식으로 생성
            GameObject skillInstance = Instantiate(playerInfo.chracterSkill, player.transform);
            skillInstance.transform.SetParent(player.transform);
            
            // UI 업데이트
            playerUISetting.UpdatePlayerSkillUI(skillInstance);
        }
    }
}
