using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro 사용을 위해 추가

public class UpgradeAbilityStatus : MonoBehaviour
{
    public UpgradeInRoom upgradeInRoom;

    [Header("현재 업글 수치 / 최대 업글 수치 / 필요 골드")]
    public string abilityName;
    public int upgradeCount;
    public int maxCount;
    public int needGold;
    public int defaultGold;
    public int increaseNeedGoldValue;
    [Header("업글 내용 / 업글 수치 타입 / 업글 효율")]
    public UpgradeAbilityCategory upgradeAbilityCategory;
    public UpgradeValueType upgradeValueType;
    public float upgradeEfficiency;

    public List<GameObject> upgradeUI;
    public TextMeshProUGUI goldUI; // Text를 TextMeshProUGUI로 변경

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
        none
    }

    public void Start()
    {
        //upgradeInRoom = transform.parent.GetComponent<UpgradeInRoom>();
    }

    public void SetUpgradeSlot() //시작시 슬롯 개수 연동
    {
        goldUI = transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        // 4번째 자식 오브젝트의 모든 자식 오브젝트를 비활성화
        Transform thirdChild = transform.GetChild(3);
        for (int j = 0; j < thirdChild.childCount; j++)
        {
            thirdChild.GetChild(j).gameObject.SetActive(false);
        }

        // maxCount 수치만큼의 개수만 활성화하고 upgradeUI 리스트에 저장
        upgradeUI.Clear();
        for (int j = 0; j < maxCount; j++)
        {
            if (j < thirdChild.childCount)
            {
                GameObject slot = thirdChild.GetChild(j).gameObject;
                slot.SetActive(true);
                upgradeUI.Add(slot);
            }
        }
    }

    public void UpgradeAbility() // 업그레이드한 능력치 카테고리에 맞게 값을 재전송
    {
        // 업그레이드 카테고리에 따라 구조체 값을 직접 업데이트합니다.
        switch (upgradeAbilityCategory)
        {
            case UpgradeAbilityCategory.maxHealth:
                upgradeInRoom.currentAbilityValues.maxHealth += upgradeEfficiency;
                break;
            case UpgradeAbilityCategory.healthRegeneration:
                upgradeInRoom.currentAbilityValues.healthRegeneration += (int)upgradeEfficiency;
                break;
            case UpgradeAbilityCategory.attack:
                upgradeInRoom.currentAbilityValues.attack += upgradeEfficiency;
                break;
            case UpgradeAbilityCategory.attackSpeed:
                upgradeInRoom.currentAbilityValues.attackSpeed += upgradeEfficiency;
                break;
            case UpgradeAbilityCategory.defense:
                upgradeInRoom.currentAbilityValues.defense += (int)upgradeEfficiency;
                break;
            case UpgradeAbilityCategory.speed:
                upgradeInRoom.currentAbilityValues.speed += upgradeEfficiency;
                break;
            case UpgradeAbilityCategory.critical:
                upgradeInRoom.currentAbilityValues.critical += upgradeEfficiency;
                break;
            case UpgradeAbilityCategory.projecTile:
                upgradeInRoom.currentAbilityValues.projecTile += (int)upgradeEfficiency;
                break;
            case UpgradeAbilityCategory.attackRange:
                upgradeInRoom.currentAbilityValues.attackRange += upgradeEfficiency;
                break;
            case UpgradeAbilityCategory.duration:
                upgradeInRoom.currentAbilityValues.duration += upgradeEfficiency;
                break;
            case UpgradeAbilityCategory.acquisitionRange:
                upgradeInRoom.currentAbilityValues.acquisitionRange += upgradeEfficiency;
                break;
            case UpgradeAbilityCategory.experienceGain:
                upgradeInRoom.currentAbilityValues.experienceGain += upgradeEfficiency;
                break;
            case UpgradeAbilityCategory.goldGain:
                upgradeInRoom.currentAbilityValues.goldGain += upgradeEfficiency;
                break;
        }

        // 업데이트된 구조체 값을 GameManager에 전달
        upgradeInRoom.gameManager.UpgradeAbilityData(upgradeInRoom.currentAbilityValues);

        // UI를 업데이트합니다.
        UpdateAbilityUI();
    }

    public void UpdateAbilityUI() //업그레이드 카운트에 따라 강화효과 표기 Text변경
    {
        // 슬롯 텍스트를 abilityName과 UpgradeInRoom의 변동된 값으로 표시
        TextMeshProUGUI slotText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        if (slotText != null)
        {
            string value = "";

            // 업그레이드 카테고리에 따라 구조체에서 값을 가져옵니다.
            switch (upgradeAbilityCategory)
            {
                case UpgradeAbilityCategory.maxHealth:
                    value = upgradeInRoom.currentAbilityValues.maxHealth.ToString();
                    break;
                case UpgradeAbilityCategory.healthRegeneration:
                    value = upgradeInRoom.currentAbilityValues.healthRegeneration.ToString();
                    break;
                case UpgradeAbilityCategory.attack:
                    value = upgradeInRoom.currentAbilityValues.attack.ToString();
                    break;
                case UpgradeAbilityCategory.attackSpeed:
                    value = upgradeInRoom.currentAbilityValues.attackSpeed.ToString();
                    break;
                case UpgradeAbilityCategory.defense:
                    value = upgradeInRoom.currentAbilityValues.defense.ToString();
                    break;
                case UpgradeAbilityCategory.speed:
                    value = upgradeInRoom.currentAbilityValues.speed.ToString();
                    break;
                case UpgradeAbilityCategory.critical:
                    value = upgradeInRoom.currentAbilityValues.critical.ToString();
                    break;
                case UpgradeAbilityCategory.projecTile:
                    value = upgradeInRoom.currentAbilityValues.projecTile.ToString();
                    break;
                case UpgradeAbilityCategory.attackRange:
                    value = upgradeInRoom.currentAbilityValues.attackRange.ToString();
                    break;
                case UpgradeAbilityCategory.duration:
                    value = upgradeInRoom.currentAbilityValues.duration.ToString();
                    break;
                case UpgradeAbilityCategory.acquisitionRange:
                    value = upgradeInRoom.currentAbilityValues.acquisitionRange.ToString();
                    break;
                case UpgradeAbilityCategory.experienceGain:
                    value = upgradeInRoom.currentAbilityValues.experienceGain.ToString();
                    break;
                case UpgradeAbilityCategory.goldGain:
                    value = upgradeInRoom.currentAbilityValues.goldGain.ToString();
                    break;
            }

            string prefix = "";
            string suffix = "";
            switch (upgradeValueType)
            {
                case UpgradeValueType.Flat:
                    prefix = "+";
                    break;
                case UpgradeValueType.Percent:
                    prefix = "+";
                    suffix = "%";
                    break;
                case UpgradeValueType.Multiply:
                    prefix = "*";
                    break;
            }
            slotText.text = $"{abilityName} {prefix} {value}{suffix}";
        }

        // needGold UI 갱신
        if (goldUI != null)
        {
            needGold = defaultGold + (upgradeCount * increaseNeedGoldValue);
            goldUI.text = needGold.ToString() + " 골드";
        }

        // upgradeCount에 따라 리스트의 게임 오브젝트의 0번째 자식 오브젝트를 활성화
        for (int i = 0; i < upgradeUI.Count; i++)
        {
            if (i < upgradeCount)
            {
                upgradeUI[i].transform.GetChild(0).gameObject.SetActive(true);
            }
            else
            {
                upgradeUI[i].transform.GetChild(0).gameObject.SetActive(false);
            }
        }
    }
}
