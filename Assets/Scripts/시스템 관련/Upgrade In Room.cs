using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro 사용을 위해 추가

public class UpgradeInRoom : MonoBehaviour
{
    public GameManager gameManager;
    public List<UpgradeAbilityData> abilityData;
    public List<GameObject> abilitySlots;
    public UpgradeAbilityValues currentAbilityValues; //업글 능력치 구조체상태로 저장
    public bool haveAbilityData = false;
    public TextMeshProUGUI goldUI; // Text를 TextMeshProUGUI로 변경

    /// <summary>
    /// 능력치 값들을 저장하는 구조체
    /// </summary>
    public void Awake()
    {
        // GameManager 태그를 가진 오브젝트를 찾아서 연결
        GameObject gameManagerObj = GameObject.FindGameObjectWithTag("GameManager");
        if (gameManagerObj != null)
        {
            gameManager = gameManagerObj.GetComponent<GameManager>();
        }
    }
    public struct UpgradeAbilityValues //게임에 적용시키기 위해 GameManager에 보내기 위한 데이터화 구조체
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
        public float goldGain;
    }

    // Start is called before the first frame update
    public void StartAbilitySetting()
    {
        // GameManager의 데이터를 먼저 가져옴
        if (haveAbilityData && gameManager == null)
        {
            gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
            // 저장된 능력치 값 복원
            currentAbilityValues = gameManager.upgradeAbilityValues;
            /*
            Debug.Log("=== GameManager에서 복원된 데이터 ===");
            Debug.Log("복원된 능력치 값:");
            Debug.Log($"maxHealth: {currentAbilityValues.maxHealth}");
            Debug.Log($"healthRegeneration: {currentAbilityValues.healthRegeneration}");
            Debug.Log($"attack: {currentAbilityValues.attack}");
            Debug.Log($"attackSpeed: {currentAbilityValues.attackSpeed}");
            Debug.Log($"defense: {currentAbilityValues.defense}");
            Debug.Log($"speed: {currentAbilityValues.speed}");
            Debug.Log($"critical: {currentAbilityValues.critical}");
            Debug.Log($"projecTile: {currentAbilityValues.projecTile}");
            Debug.Log($"attackRange: {currentAbilityValues.attackRange}");
            Debug.Log($"duration: {currentAbilityValues.duration}");
            Debug.Log($"acquisitionRange: {currentAbilityValues.acquisitionRange}");
            Debug.Log($"experienceGain: {currentAbilityValues.experienceGain}");
            Debug.Log($"goldGain: {currentAbilityValues.goldGain}");
            Debug.Log("============================");
            */
        }
        else
        {
            // 초기 능력치 값 설정
            currentAbilityValues = new UpgradeAbilityValues
            {
                maxHealth = 1f,
                healthRegeneration = 0,
                attack = 1f,
                attackSpeed = 0,
                defense = 0,
                speed = 0,
                critical = 0,
                projecTile = 0,
                attackRange = 0,
                duration = 0,
                acquisitionRange = 0,
                experienceGain = 0,
                goldGain = 0
            };

            // 초기값 설정 후 GameManager에 값을 전달
            if (gameManager != null)
            {
                gameManager.UpgradeAbilityData(currentAbilityValues);
            }
        }
            
        // 이 스크립트가 적용된 오브젝트의 모든 자식 오브젝트를 abilitySlots 리스트에 추가합니다.
        abilitySlots.Clear(); // 기존 슬롯 초기화
        for (int i = 0; i < transform.childCount; i++)
        {
            abilitySlots.Add(transform.GetChild(i).gameObject);
        }

        // GameManager의 데이터를 가져온 후 UI 업데이트
        UpdateAbilitySetting();
    }

    //시작할때 UI및 텍스트 기초 세팅 메서드
    public void UpdateAbilitySetting()
    {
        // abilityData의 0번부터 차례대로 abilitySlots의 0번부터 현재 데이터가 있는 개수까지 슬롯의 첫 번째 자식 오브젝트의 Image 컴포넌트를 data의 abilityImage로 변경합니다.
        for (int i = 0; i < abilityData.Count && i < abilitySlots.Count; i++)
        {
            //슬롯 이미지 동기화화
            Image slotImage = abilitySlots[i].transform.GetChild(0).GetComponent<Image>();
            if (slotImage != null)
            {
                slotImage.sprite = abilityData[i].abilityImage;
            }

            //슬롯 텍스트 강화수치 출력
            TextMeshProUGUI slotText = abilitySlots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            if (slotText != null)
            {
                slotText.text = $"{abilityData[i].abilityName}";
            }

            //슬롯 텍스트 강화필요 골드 출력
            TextMeshProUGUI goldText = abilitySlots[i].transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            if (goldText != null)
            {
                goldText.text = $"{abilityData[i].needGold} 골드";
            }

            //슬롯의 능력치에 데이터 값들 적용
            UpgradeAbilityStatus abilityStatus = abilitySlots[i].GetComponent<UpgradeAbilityStatus>();
            if (abilityStatus != null)
            {
                abilityStatus.upgradeInRoom = this; // upgradeInRoom 초기화
                abilityStatus.abilityName = abilityData[i].abilityName;
                abilityStatus.upgradeCount = abilityData[i].upgradeCount;
                abilityStatus.maxCount = abilityData[i].maxUpgradeCount;
                abilityStatus.upgradeAbilityCategory = (UpgradeAbilityStatus.UpgradeAbilityCategory)abilityData[i].upgradeAbilityCategory;
                abilityStatus.upgradeValueType = (UpgradeAbilityStatus.UpgradeValueType)abilityData[i].upgradeValueType;
                abilityStatus.upgradeEfficiency = abilityData[i].efficiency;
                abilityStatus.needGold = abilityData[i].needGold;
                abilityStatus.defaultGold = abilityData[i].needGold;
                abilityStatus.increaseNeedGoldValue = abilityData[i].increaseNeedGoldValue;
                abilityStatus.SetUpgradeSlot(); // 업데이트 슬롯개수 연동

                // haveAbilityData가 true이고 GameManager에 저장된 upgradeCount가 있으면 그것을 사용
                if (haveAbilityData && gameManager.upgradeCountData.upgradeCounts != null)
                {
                    // 현재 abilityStatus의 upgradeAbilityCategory와 저장된 category가 일치하는지 확인
                    if (gameManager.upgradeCountData.upgradeCounts.ContainsKey(abilityStatus.upgradeAbilityCategory))
                    {
                        abilityStatus.upgradeCount = gameManager.upgradeCountData.upgradeCounts[abilityStatus.upgradeAbilityCategory];
                        //Debug.Log($"복원된 업그레이드 카운트 - 카테고리: {abilityStatus.upgradeAbilityCategory}, 카운트: {abilityStatus.upgradeCount}");
                        abilityStatus.UpdateAbilityUI(); // UI만 업데이트
                    }
                }
            }
        }
    }

    public void UpgradeAbilityToGold(int slotIndex) //원클릭 버튼설정 돈으로 업글 메서드
    {
        // 버튼을 누른 오브젝트에 할당된 UpgradeAbilityStatus 스크립트의 UpgradeAbility 메서드를 실행
        UpgradeAbilityStatus abilityStatus = abilitySlots[slotIndex].GetComponent<UpgradeAbilityStatus>();
        if (abilityStatus != null && abilityStatus.upgradeCount < abilityStatus.maxCount)
        {
            // 골드가 충분한지 확인
            if (gameManager != null && gameManager.gold >= abilityStatus.needGold)
            {
                gameManager.gold -= abilityStatus.needGold;
                if (goldUI != null)
                {
                    goldUI.text = gameManager.gold.ToString();
                }
                abilityStatus.upgradeCount++;
                abilityStatus.UpgradeAbility();
            }
            // else: 골드 부족 시 아무 동작 안 함 (필요시 안내 메시지 추가 가능)
        }
    }

    public void ResetUpgradeInRoom() //업그레이드 리셋 메서드
    {
        // 모든 능력치 값을 초기화
        currentAbilityValues = new UpgradeAbilityValues
        {
            maxHealth = 1f,
            healthRegeneration = 0,
            attack = 1f,
            attackSpeed = 0,
            defense = 0,
            speed = 0,
            critical = 0,
            projecTile = 0,
            attackRange = 0,
            duration = 0,
            acquisitionRange = 0,
            experienceGain = 0,
            goldGain = 0
        };

        // 각 슬롯에 적용된 UpgradeAbilityStatus의 upgradeCount를 0으로 설정
        foreach (GameObject slot in abilitySlots)
        {
            UpgradeAbilityStatus abilityStatus = slot.GetComponent<UpgradeAbilityStatus>();
            if (abilityStatus != null)
            {
                abilityStatus.upgradeCount = 0;
                abilityStatus.UpdateAbilityUI(); //업그레이드했던 UI도 초기화

                // 슬롯의 1번째 자식의 텍스트를 abilityName으로 설정 (초기화)
                TextMeshProUGUI slotText = slot.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
                if (slotText != null)
                {
                    slotText.text = abilityStatus.abilityName;
                }
            }
        }

        // GameManager의 업그레이드 카운트, 구매 능력치 값 초기화
        if (gameManager != null)
        {
            gameManager.upgradeCountData.upgradeCounts = new Dictionary<UpgradeAbilityStatus.UpgradeAbilityCategory, int>();
            gameManager.UpgradeAbilityData(currentAbilityValues);
        }
        
        haveAbilityData = false;
        //Debug.Log("UpgradeInRoom이 초기화되었습니다.");
    }
}
