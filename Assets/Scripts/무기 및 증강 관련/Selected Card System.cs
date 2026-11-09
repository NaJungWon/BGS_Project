using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro 사용을 위해 추가

public class SelectedCardSystem : MonoBehaviour
{
    public CardData dfCards;
    public List<CardData> weaponCardData; // 무기 카드 데이터
    public List<CardData> abilityCardData; // 능력치 카드 데이터
    public List<CardData> elseCardData; // 기타 카드 데이터
    public List<RectTransform> cardSpawnPosition; //카드 생성위치 좌표
    public GameObject cardList; //카드 저장소
    public GameObject selectCardUI; //카드선택창 UI;
    public GameObject reloadCardUI; //카드 재생성창 UI;

    //public GameObject dfCards; //플레이어 기본 카드
    public List<GameObject> weaponCards = new List<GameObject>(); //생성된 무기 카드 리스트
    public List<GameObject> abilityCards = new List<GameObject>(); //생성된 능력치 카드 리스트
    public List<GameObject> elseCards = new List<GameObject>(); //생성된 기타 카드 리스트
    
    public GameObject player;
    public PlayerUISetting playerUISetting;

    // 선택된 카드 정보를 저장할 구조체 정의
    [System.Serializable]
    public struct SelectedCardInfo
    {
        public GameObject cardObject;
        public CardInfo cardInfo;

        public SelectedCardInfo(GameObject obj, CardInfo info)
        {
            cardObject = obj;
            cardInfo = info;
        }
    }

    public List<SelectedCardInfo> selectedWeaponCards = new List<SelectedCardInfo>(); //선택된 무기 카드;
    public List<SelectedCardInfo> selectedAbilityCards = new List<SelectedCardInfo>(); //선택된 데이터 카드;

    public List<SelectedCardInfo> selectableWeaponCards = new List<SelectedCardInfo>(); //선택 가능한 무기 카드;
    public List<SelectedCardInfo> selectableAbilityCards = new List<SelectedCardInfo>(); //선택 가능한 능력치 카드;


    void Start()
    {
        // dfCards가 null이 아닐 경우 weaponCardData 리스트에 추가
        if (dfCards != null)
        {
            weaponCardData.Add(dfCards);
        }

        playerUISetting = GetComponent<PlayerUISetting>();
        InputCardSpawnPosition();
        GenerateCards();
    }

    void Update()
    {

    }

    public void InputCardSpawnPosition()//카드생성좌표 리스트 추가
    {
        // cardSpawnPosition 초기화
        cardSpawnPosition = new List<RectTransform>();
        for (int i = 0; i < 3; i++)
        {
            RectTransform spawnPosition = selectCardUI.transform.GetChild(i).GetComponent<RectTransform>();
            if (spawnPosition != null)
            {
                cardSpawnPosition.Add(spawnPosition);
            }
        }
    }

    public void GenerateCards() //최초의 카드 생성
    {
        // 기존 카드 제거
        foreach (GameObject card in abilityCards)
        {
            Destroy(card);
        }
        abilityCards.Clear();

        foreach (GameObject card in weaponCards)
        {
            Destroy(card);
        }
        weaponCards.Clear();

        // 무기 카드 생성
        foreach (CardData weaponData in weaponCardData)
        {
            if (weaponData.cardCase != null)
            {
                // 카드 오브젝트 생성 및 cardList의 자식으로 설정
                GameObject newCard = Instantiate(weaponData.cardCase, cardList.transform);
                
                // CardInfo 스크립트 가져오기
                CardInfo cardInfo = newCard.GetComponent<CardInfo>();
                if (cardInfo != null)
                {
                    // 카드 타입에 따라 값 설정
                    cardInfo.cardType = CardInfo.CardType.Weapon;
                    cardInfo.cardName = weaponData.cardName;
                    cardInfo.cardImage = weaponData.cardImage;
                    cardInfo.increaseAbility = weaponData.increaseAbility;
                    cardInfo.upgradeCount = weaponData.upgradeCount;
                    cardInfo.weaponPrefab = weaponData.weaponPrefab;
                    cardInfo.damage = weaponData.damage;
                    cardInfo.attackSpeed = weaponData.attackSpeed;
                    cardInfo.defense = weaponData.defense;
                    cardInfo.projecTile = weaponData.projecTile;
                    cardInfo.attackRange = weaponData.attackRange;
                    cardInfo.duration = weaponData.duration;
                    cardInfo.isArousal = weaponData.isArousal;
                    cardInfo.weaponDescription = weaponData.weaponDescription;
                    cardInfo.arousalDescription = weaponData.arousalDescription;
                    cardInfo.weaponUpgradeDataList = weaponData.weaponUpgradeDataList; // 무기 업그레이드 데이터 리스트 전달
                    cardInfo.cardDescription = newCard.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                    cardInfo.upgradeAbility = (CardInfo.UpgradeAbility)weaponData.upgradeAbility;
                    // player를 selectedCardSystem의 player로 설정
                    cardInfo.player = player;

                    // dfCards 정보가 있을 경우 selectCard() 호출 및 UI 설정
                    if (dfCards != null && dfCards.cardName == weaponData.cardName) // dfCards와 비교
                    {
                        cardInfo.dfWeapon = true;
                        float dfattackSpeed = player.GetComponent<UpgradeInGame>().playerInfo.attackSpeed;
                        cardInfo.attackSpeed = dfattackSpeed;
                        // 카드 정보 설정을 먼저 하고
                        SettingCardInfo(newCard, cardInfo);
                        // 그 다음에 SelectCard 호출
                        cardInfo.SelectCard(); // SelectCard 호출
                        SelectCardplayerUISetting(cardInfo, newCard); // newCard와 cardInfo를 함께 전달
                    }
                    else
                    {
                        // dfCards가 아닌 경우 기존대로 처리
                        SettingCardInfo(newCard, cardInfo);
                    }
                }

                weaponCards.Add(newCard); // 무기 카드 리스트에 추가
            }
        }

        // 능력치 카드 생성
        foreach (CardData abilityData in abilityCardData)
        {
            if (abilityData.cardCase != null)
            {
                // 카드 오브젝트 생성 및 cardList의 자식으로 설정
                GameObject newCard = Instantiate(abilityData.cardCase, cardList.transform);
                
                // CardInfo 스크립트 가져오기
                CardInfo cardInfo = newCard.GetComponent<CardInfo>();
                if (cardInfo != null)
                {
                    // 카드 타입에 따라 값 설정
                    cardInfo.cardType = CardInfo.CardType.Ability;
                    cardInfo.cardName = abilityData.cardName;
                    cardInfo.cardImage = abilityData.cardImage;
                    cardInfo.increaseAbility = abilityData.increaseAbility;
                    cardInfo.upgradeCount = abilityData.upgradeCount;
                    cardInfo.weaponDescription = abilityData.weaponDescription;
                    cardInfo.upgradeAbility = (CardInfo.UpgradeAbility)abilityData.upgradeAbility;
                    cardInfo.cardDescription = newCard.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                    // player를 selectedCardSystem의 player로 설정
                    cardInfo.player = player;
                }

                // 카드 정보 설정
                SettingCardInfo(newCard, cardInfo);
                abilityCards.Add(newCard); // 능력치 카드 리스트에 추가
            }
        }

        // elseCard 생성 및 설정
        foreach (CardData elseData in elseCardData)
        {
            if (elseData != null && elseData.cardCase != null)
            {
                // 카드 오브젝트 생성 및 cardList의 자식으로 설정
                GameObject elseCard = Instantiate(elseData.cardCase, cardList.transform);
                
                // CardInfo 스크립트 가져오기
                CardInfo cardInfo = elseCard.GetComponent<CardInfo>();
                if (cardInfo != null)
                {
                    // 카드 타입에 따라 값 설정
                    cardInfo.cardType = CardInfo.CardType.Else;
                    cardInfo.cardName = elseData.cardName;
                    cardInfo.cardImage = elseData.cardImage;
                    cardInfo.weaponDescription = elseData.weaponDescription;
                    cardInfo.upgradeAbility = (CardInfo.UpgradeAbility)elseData.upgradeAbility; // upgradeAbility 값 설정
                    cardInfo.cardDescription = elseCard.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                    // player를 selectedCardSystem의 player로 설정
                    cardInfo.player = player;

                    // 카드 정보 설정
                    SettingCardInfo(elseCard, cardInfo);
                }

                // elseCards 리스트에 추가
                elseCards.Add(elseCard);
            }
        }
    }

    public void SettingCardInfo(GameObject card, CardInfo cardInfo) //카드를 데이터 기반 동기화 메서드
    {
        // 카드 이름 설정
        TextMeshProUGUI cardName = card.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        if (cardName != null)
        {
            cardName.text = cardInfo.cardName;
        }

        // 카드 이미지 설정
        // cardCase의 1번째 자식의 1번째 자식의 Image 컴포넌트의 sprite를 설정
        Image cardImage = card.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Image>();
        if (cardImage != null)
        {
            cardImage.sprite = cardInfo.cardImage;
        }

        // 카드 설명 설정
        TextMeshProUGUI cardDescription = card.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        if (cardDescription != null)
        {
            cardDescription.text = cardInfo.weaponDescription;
        }
    }

    public void SelectCard(int cardIndex) // 카드 선택 메서드 (원클릭 설정)
    {
        // 선택된 카드의 인덱스에 해당하는 cardSpawnPosition의 자식 오브젝트 가져오기
        if (cardIndex >= 0 && cardIndex < cardSpawnPosition.Count)
        {
            // cardSpawnPosition의 자식 오브젝트를 가져옴
            GameObject selectedCard = cardSpawnPosition[cardIndex].GetChild(0).gameObject; // 첫 번째 자식 오브젝트를 가져옴
            CardInfo cardInfo = selectedCard.GetComponent<CardInfo>();

            if (cardInfo != null)
            {
                if (cardInfo.cardType == CardInfo.CardType.Else)
                {
                    if (cardInfo.upgradeAbility == CardInfo.UpgradeAbility.Heal)
                    {
                        // 체력 회복 로직
                        PlayerStatus playerStatus = player.GetComponent<PlayerStatus>();
                        if (playerStatus != null)
                        {
                            float healAmount = playerStatus.maxHealth / 4f; // 최대 체력의 1/4
                            playerStatus.currentHealth = Mathf.Min(playerStatus.currentHealth + healAmount, playerStatus.maxHealth);
                            playerStatus.playerSystem.playerUISetting.UpdateHealthUI(playerStatus);
                            Debug.Log($"체력 회복: {healAmount}, 현재 체력: {playerStatus.currentHealth}");
                        }
                    }
                    else if (cardInfo.upgradeAbility == CardInfo.UpgradeAbility.Gold)
                    {
                        player.GetComponent<PlayerStatus>().playerSystem.gameManager.gold += 150;
                        Debug.Log("골드 획득: 150");
                    }
                }
                else
                {
                    // upgradeCount 증가
                    cardInfo.SelectCard(); // SelectCard 메서드 호출

                    // 카드 타입에 따라 UI 설정
                    SelectCardplayerUISetting(cardInfo, selectedCard);
                }
            }
        }

        // cardSpawnPosition의 모든 자식 카드들을 cardList로 되돌려 보냄
        for (int i = 0; i < cardSpawnPosition.Count; i++)
        {
            for (int j = 0; j < cardSpawnPosition[i].childCount; j++)
            {
                GameObject cardToReturn = cardSpawnPosition[i].GetChild(j).gameObject;
                cardToReturn.transform.SetParent(cardList.transform); // cardList의 자식으로 설정
            }
        }

        // UI 비활성화
        selectCardUI.SetActive(false);
        reloadCardUI.SetActive(false);
        playerUISetting.backGround.SetActive(false);
        GameManager.gameStatus = GameManager.GameStatus.InGame; // 게임상태 인게임으로 바꾸기
        GameManager.ResumeGame(); // 게임 일시정지 해제
        Debug.Log("카드 선택 완료");
    }

    private void SelectCardplayerUISetting(CardInfo cardInfo, GameObject selectedCard)
    {
        // 카드 타입에 따라 리스트에 추가
        if (cardInfo.cardType == CardInfo.CardType.Weapon)
        {
            // 중복 체크
            bool isDuplicate = false;
            int existingIndex = -1;
            
            for (int i = 0; i < selectedWeaponCards.Count; i++)
            {
                if (selectedWeaponCards[i].cardObject == selectedCard)
                {
                    isDuplicate = true;
                    existingIndex = i;
                    break;
                }
            }

            if (!isDuplicate)
            {
                // 중복이 없는 경우: 리스트에 추가하고 UI 설정
                selectedWeaponCards.Add(new SelectedCardInfo(selectedCard, cardInfo)); // 무기 카드 리스트에 추가

                // playerUISetting의 augmentationSlots에 0번부터 차례대로 이미지 설정
                int slotIndex = selectedWeaponCards.Count - 1; // 현재 선택된 무기 카드의 개수로 인덱스 설정
                if (slotIndex < playerUISetting.augmentationSlots.Count)
                {
                    Image slotImage = playerUISetting.augmentationSlots[slotIndex].transform.GetChild(0).GetComponent<Image>();
                    if (slotImage != null)
                    {
                        slotImage.sprite = cardInfo.cardImage; // CardInfo의 cardImage로 변경
                        slotImage.gameObject.SetActive(true); // 0번째 자식 오브젝트 활성화
                    }

                    // 레벨 능력치 레벨 텍스트 변경
                    TextMeshProUGUI upgradeLevelText = playerUISetting.augmentationSlots[slotIndex].transform.GetChild(2).GetChild(1).GetComponent<TextMeshProUGUI>();
                    if (upgradeLevelText != null)
                    {
                        upgradeLevelText.text = cardInfo.upgradeCount.ToString(); // upgradeCount 수치로 변경
                    }
                }
            }
            else
            {
                // 중복이 있는 경우: 해당 카드의 인덱스를 찾아서 레벨만 업데이트
                if (existingIndex >= 0 && existingIndex < playerUISetting.augmentationSlots.Count)
                {
                    TextMeshProUGUI upgradeLevelText = playerUISetting.augmentationSlots[existingIndex].transform.GetChild(2).GetChild(1).GetComponent<TextMeshProUGUI>();
                    if (upgradeLevelText != null)
                    {
                        upgradeLevelText.text = cardInfo.upgradeCount.ToString(); // upgradeCount 수치로 변경
                    }
                }
            }
        }
        else if (cardInfo.cardType == CardInfo.CardType.Ability)
        {
            // 중복 체크
            bool isDuplicate = false;
            int existingIndex = -1;
            
            for (int i = 0; i < selectedAbilityCards.Count; i++)
            {
                if (selectedAbilityCards[i].cardObject == selectedCard)
                {
                    isDuplicate = true;
                    existingIndex = i;
                    break;
                }
            }

            if (!isDuplicate)
            {
                // 중복이 없는 경우: 리스트에 추가하고 UI 설정
                selectedAbilityCards.Add(new SelectedCardInfo(selectedCard, cardInfo)); // 능력치 카드 리스트에 추가

                // playerUISetting의 abilitySlots에 0번부터 차례대로 이미지 설정
                int slotIndex = selectedAbilityCards.Count - 1; // 현재 선택된 능력치 카드의 개수로 인덱스 설정
                if (slotIndex < playerUISetting.abilitySlots.Count)
                {
                    Image slotImage = playerUISetting.abilitySlots[slotIndex].transform.GetChild(0).GetComponent<Image>();
                    if (slotImage != null)
                    {
                        slotImage.sprite = cardInfo.cardImage; // CardInfo의 cardImage로 변경
                        slotImage.gameObject.SetActive(true); // 0번째 자식 오브젝트 활성화
                    }

                    // 레벨 능력치 레벨 텍스트 변경
                    TextMeshProUGUI upgradeLevelText = playerUISetting.abilitySlots[slotIndex].transform.GetChild(2).GetChild(1).GetComponent<TextMeshProUGUI>();
                    if (upgradeLevelText != null)
                    {
                        upgradeLevelText.text = cardInfo.upgradeCount.ToString(); // upgradeCount 수치로 변경
                    }
                }
            }
            else
            {
                // 중복이 있는 경우: 해당 카드의 인덱스를 찾아서 레벨만 업데이트
                if (existingIndex >= 0 && existingIndex < playerUISetting.abilitySlots.Count)
                {
                    TextMeshProUGUI upgradeLevelText = playerUISetting.abilitySlots[existingIndex].transform.GetChild(2).GetChild(1).GetComponent<TextMeshProUGUI>();
                    if (upgradeLevelText != null)
                    {
                        upgradeLevelText.text = cardInfo.upgradeCount.ToString(); // upgradeCount 수치로 변경
                    }
                }
            }
        }
    }

    public void CardSpawn() //카드를 좌표에 소환하는 메서드
    {
        List<GameObject> selectedCards = new List<GameObject>();
        System.Random random = new System.Random();

        // 선택 가능한 카드 리스트 초기화 및 업데이트
        selectableWeaponCards.Clear();
        selectableAbilityCards.Clear();

        // 선택된 무기 카드와 능력치 카드를 선택 가능한 리스트에 추가
        foreach (var weaponCard in selectedWeaponCards)
        {
            selectableWeaponCards.Add(weaponCard);
        }
        foreach (var abilityCard in selectedAbilityCards)
        {
            selectableAbilityCards.Add(abilityCard);
        }

        // upgradeCount가 5이고 isArousal이 true인 카드 제거
        selectableWeaponCards.RemoveAll(card => card.cardInfo.upgradeCount == 5 && card.cardInfo.isArousal);
        selectableAbilityCards.RemoveAll(card => card.cardInfo.upgradeCount == 5);

        // upgradeCount가 5인 무기 카드 중 upgradeAbility와 동일한 값을 가진 능력치 카드가 없는 경우 제거
        selectableWeaponCards.RemoveAll(weaponCard => 
        {
            if (weaponCard.cardInfo.upgradeCount == 5)
            {
                bool hasMatchingAbility = false;
                foreach (var abilityCard in selectedAbilityCards)
                {
                    if (weaponCard.cardInfo.upgradeAbility == abilityCard.cardInfo.upgradeAbility)
                    {
                        hasMatchingAbility = true;
                        break;
                    }
                }
                return !hasMatchingAbility; // 매칭되는 능력치 카드가 없으면 제거
            }
            return false; // upgradeCount가 5가 아닌 경우는 제거하지 않음
        });

        int weaponCount = selectedWeaponCards.Count; // upgradeCount가 1 이상인 무기 카드 수
        int abilityCount = selectedAbilityCards.Count; // upgradeCount가 1 이상인 능력치 카드 수

        // 경우에 따라 카드 선택
        switch (true)
        {
            case bool _ when weaponCount >= 5 && abilityCount >= 6:
                // 경우 1: 무기 카드 5개 이상, 능력치 카드 6개 이상
                bool hasSelectableWeapon = selectableWeaponCards.Count > 0;
                bool hasSelectableAbility = selectableAbilityCards.Count > 0;

                if (hasSelectableWeapon && hasSelectableAbility)
                {
                    // 둘 다 있을 때
                    int totalSelectableCards = selectableWeaponCards.Count + selectableAbilityCards.Count;
                    int targetCount = Mathf.Min(3, totalSelectableCards); // 최대 3개 또는 선택 가능한 총 카드 수 중 작은 값

                    while (selectedCards.Count < targetCount)
                    {
                        int listSelector = random.Next(0, 2);
                        List<GameObject> selectedList = listSelector == 0 ?
                            selectableWeaponCards.ConvertAll(x => x.cardObject) :
                            selectableAbilityCards.ConvertAll(x => x.cardObject);
                        if (selectedList.Count == 0) continue;
                        int cardIndex = random.Next(0, selectedList.Count);
                        GameObject selectedCard = selectedList[cardIndex];
                        if (!selectedCards.Contains(selectedCard))
                            selectedCards.Add(selectedCard);
                    }
                }
                else if (!hasSelectableWeapon && hasSelectableAbility)
                {
                    // 무기카드 없음, 능력치카드만 있을 때
                    List<GameObject> abilityList = selectableAbilityCards.ConvertAll(x => x.cardObject);
                    int count = Mathf.Min(3, abilityList.Count);
                    List<int> usedIdx = new List<int>();
                    while (selectedCards.Count < count)
                    {
                        int idx = random.Next(0, abilityList.Count);
                        if (!usedIdx.Contains(idx))
                        {
                            selectedCards.Add(abilityList[idx]);
                            usedIdx.Add(idx);
                        }
                    }
                }
                else if (hasSelectableWeapon && !hasSelectableAbility)
                {
                    // 능력치카드 없음, 무기카드만 있을 때
                    List<GameObject> weaponList = selectableWeaponCards.ConvertAll(x => x.cardObject);
                    int count = Mathf.Min(3, weaponList.Count);
                    List<int> usedIdx = new List<int>();
                    while (selectedCards.Count < count)
                    {
                        int idx = random.Next(0, weaponList.Count);
                        if (!usedIdx.Contains(idx))
                        {
                            selectedCards.Add(weaponList[idx]);
                            usedIdx.Add(idx);
                        }
                    }
                }
                else
                {
                    Debug.Log("선택 가능한 카드가 없습니다.");
                    // 모든 elseCard 선택
                    if (elseCards.Count > 0)
                    {
                        foreach (GameObject elseCard in elseCards)
                        {
                            selectedCards.Add(elseCard);
                            elseCard.SetActive(true);
                            Debug.Log($"elseCard {elseCard.name}가 선택되었습니다.");
                        }
                    }
                    break;
                }
                break;

            case bool _ when weaponCount >= 5 && abilityCount < 6:
                // 경우 2: 무기 카드 5개 이상, 능력치 카드 6개 이하
                
                // 선택 가능한 무기 카드 리스트
                List<GameObject> selectableWeaponList = selectableWeaponCards.ConvertAll(x => x.cardObject);
                
                // 선택 가능한 능력치 카드 리스트 생성
                List<GameObject> selectableAbilityList = new List<GameObject>();
                selectableAbilityList.Clear();
                foreach (var card in abilityCards)
                {
                    CardInfo cardInfo = card.GetComponent<CardInfo>();
                    if (cardInfo != null && cardInfo.upgradeCount < 5)
                    {
                        selectableAbilityList.Add(card);
                    }
                }

                if (selectableWeaponList.Count > 0)
                {
                    // 무기 카드가 있으면 무기 + 능력치 카드에서 3개 선택
                    while (selectedCards.Count < 3)
                    {
                        int listSelector = random.Next(0, 2);
                        List<GameObject> selectedList = listSelector == 0 ? selectableWeaponList : selectableAbilityList;

                        if (selectedList.Count == 0) continue;

                        int cardIndex = random.Next(0, selectedList.Count);
                        GameObject selectedCard = selectedList[cardIndex];

                        if (!selectedCards.Contains(selectedCard))
                        {
                            selectedCards.Add(selectedCard);
                        }
                    }
                }
                else
                {
                    // 무기 카드가 없으면 능력치 카드에서 3개 선택
                    List<int> usedIdx = new List<int>();
                    while (selectedCards.Count < 3)
                    {
                        int idx = random.Next(0, selectableAbilityList.Count);
                        if (!usedIdx.Contains(idx))
                        {
                            selectedCards.Add(selectableAbilityList[idx]);
                            usedIdx.Add(idx);
                        }
                    }
                }
                break;

            case bool _ when weaponCount < 5 && abilityCount >= 6:
                // 경우 3: 무기 카드 5개 이하, 능력치 카드 6개 이상
                
                // 선택 가능한 무기 카드 리스트 생성
                List<GameObject> selectableWeaponList2 = new List<GameObject>();
                selectableWeaponList2.Clear();
                foreach (var card in weaponCards)
                {
                    CardInfo cardInfo = card.GetComponent<CardInfo>();
                    if (cardInfo != null)
                    {
                        bool isSelectable = true;
                        
                        // upgradeCount가 5이고 진화가 true인 경우 제외
                        if (cardInfo.upgradeCount == 5 && cardInfo.isArousal)
                        {
                            isSelectable = false;
                        }
                        // upgradeCount가 5인 경우, 해당하는 능력치 카드가 있는지 확인
                        else if (cardInfo.upgradeCount == 5)
                        {
                            bool hasMatchingAbility = false;
                            foreach (var abilityCard in selectedAbilityCards)
                            {
                                if (cardInfo.upgradeAbility == abilityCard.cardInfo.upgradeAbility)
                                {
                                    hasMatchingAbility = true;
                                    break;
                                }
                            }
                            if (!hasMatchingAbility)
                            {
                                isSelectable = false;
                            }
                        }

                        if (isSelectable)
                        {
                            selectableWeaponList2.Add(card);
                        }
                    }
                }

                // 선택 가능한 능력치 카드 리스트
                List<GameObject> selectableAbilityList2 = selectableAbilityCards.ConvertAll(x => x.cardObject);

                if (selectableAbilityList2.Count > 0)
                {
                    // 능력치 카드가 있으면 능력치 + 무기 카드에서 3개 선택
                    while (selectedCards.Count < 3)
                    {
                        int listSelector = random.Next(0, 2);
                        List<GameObject> selectedList = listSelector == 0 ? selectableAbilityList2 : selectableWeaponList2;

                        if (selectedList.Count == 0) continue;

                        int cardIndex = random.Next(0, selectedList.Count);
                        GameObject selectedCard = selectedList[cardIndex];

                        if (!selectedCards.Contains(selectedCard))
                        {
                            selectedCards.Add(selectedCard);
                        }
                    }
                }
                else
                {
                    // 능력치 카드가 없으면 무기 카드에서 3개 선택
                    List<int> usedIdx = new List<int>();
                    while (selectedCards.Count < 3)
                    {
                        int idx = random.Next(0, selectableWeaponList2.Count);
                        if (!usedIdx.Contains(idx))
                        {
                            selectedCards.Add(selectableWeaponList2[idx]);
                            usedIdx.Add(idx);
                        }
                    }
                }
                break;

            default:
                // 경우 4: 무기 카드 5개 이하, 능력치 카드 6개 이하
                
                // 선택 가능한 무기 카드 리스트 생성
                List<GameObject> selectableWeaponList3 = new List<GameObject>();
                selectableWeaponList3.Clear();
                foreach (var card in weaponCards)
                {
                    CardInfo cardInfo = card.GetComponent<CardInfo>();
                    if (cardInfo != null)
                    {
                        bool isSelectable = true;
                        
                        // upgradeCount가 5이고 진화가 true인 경우 제외
                        if (cardInfo.upgradeCount == 5 && cardInfo.isArousal)
                        {
                            isSelectable = false;
                        }
                        // upgradeCount가 5인 경우, 해당하는 능력치 카드가 있는지 확인
                        else if (cardInfo.upgradeCount == 5)
                        {
                            bool hasMatchingAbility = false;
                            foreach (var abilityCard in selectedAbilityCards)
                            {
                                if (cardInfo.upgradeAbility == abilityCard.cardInfo.upgradeAbility)
                                {
                                    hasMatchingAbility = true;
                                    break;
                                }
                            }
                            if (!hasMatchingAbility)
                            {
                                isSelectable = false;
                            }
                        }

                        if (isSelectable)
                        {
                            selectableWeaponList3.Add(card);
                        }
                    }
                }

                // 선택 가능한 능력치 카드 리스트 생성
                List<GameObject> selectableAbilityList3 = new List<GameObject>();
                selectableAbilityList3.Clear();
                foreach (var card in abilityCards)
                {
                    CardInfo cardInfo = card.GetComponent<CardInfo>();
                    if (cardInfo != null && cardInfo.upgradeCount < 5)
                    {
                        selectableAbilityList3.Add(card);
                    }
                }

                // 무기 + 능력치 카드에서 3개 선택
                while (selectedCards.Count < 3)
                {
                    int listSelector = random.Next(0, 2);
                    List<GameObject> selectedList = listSelector == 0 ? selectableWeaponList3 : selectableAbilityList3;

                    if (selectedList.Count == 0) break;

                    int cardIndex = random.Next(0, selectedList.Count);
                    GameObject selectedCard = selectedList[cardIndex];

                    if (!selectedCards.Contains(selectedCard))
                    {
                        selectedCards.Add(selectedCard);
                    }
                }
                break;
        }

        // 선택된 카드의 위치를 cardSpawnPosition에 설정하고, cardSpawnPosition의 자식으로 추가
        // cardSpawnPosition의 모든 자식 카드들을 cardList로 되돌려 보냄
        for (int i = 0; i < cardSpawnPosition.Count; i++)
        {
            if (cardSpawnPosition[i].childCount > 0)  // 자식이 존재할 때만 실행
            {
                for (int j = 0; j < cardSpawnPosition[i].childCount; j++)
                {
                    GameObject cardToReturn = cardSpawnPosition[i].GetChild(j).gameObject;
                    cardToReturn.transform.SetParent(cardList.transform); // cardList의 자식으로 설정
                }
            }
        }

        // 새로운 카드들의 위치 설정 및 부모 설정
        for (int i = 0; i < selectedCards.Count; i++)
        {
            if (i < cardSpawnPosition.Count)
            {
                // 카드의 위치를 cardSpawnPosition의 위치로 설정
                selectedCards[i].transform.position = cardSpawnPosition[i].position;

                // 카드의 부모를 cardSpawnPosition로 설정
                selectedCards[i].transform.SetParent(cardSpawnPosition[i]);
            }
        }

        // cardSpawnPosition 활성화/비활성화 관리
        for (int i = 0; i < cardSpawnPosition.Count; i++)
        {
            // 자식이 있으면 활성화, 없으면 비활성화
            cardSpawnPosition[i].gameObject.SetActive(cardSpawnPosition[i].childCount > 0);
        }

        // 업그레이드된 카드 정보 출력
        Debug.Log("업그레이드된 무기 카드 목록:");
        foreach (var card in selectedWeaponCards)
        {
            if (card.cardInfo != null)
            {
                Debug.Log($"무기 카드: {card.cardInfo.cardName}, 업그레이드 횟수: {card.cardInfo.upgradeCount}");
            }
        }

        Debug.Log("업그레이드된 능력치 카드 목록:");
        foreach (var card in selectedAbilityCards)
        {
            if (card.cardInfo != null)
            {
                Debug.Log($"능력치 카드: {card.cardInfo.cardName}, 업그레이드 횟수: {card.cardInfo.upgradeCount}");
            }
        }

        selectCardUI.SetActive(true);
        reloadCardUI.SetActive(true);
    }
}
