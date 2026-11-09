using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class Icon_Description : MonoBehaviour
{
    public GameObject inGameUI;
    public GameObject descriptionUI; //설명 UI
    private GameManager gameManager;
    private SelectedCardSystem selectedCardSystem;
    private GraphicRaycaster graphicRaycaster;
    private EventSystem eventSystem;
    
    [Header("UI 요소")]
    public Image iconImage;
    public Image iconOutline;
    public TextMeshProUGUI iconName;
    public TextMeshProUGUI iconDescription;
    public TextMeshProUGUI needArousalAbility;

    public Sprite weaponIconOutline;
    public Sprite abilityIconOutline;

    [Header("체크할 UI 리스트")]
    public List<GameObject> targetUIList = new List<GameObject>(); // 마우스 체크할 UI GameObject 리스트
    public GameObject leftUI;
    public GameObject useSkillUI;
    public GameObject useAbilityUI;
    public GameObject useChrSkillUI;
    public List<GameObject> leftUIList; //왼쪽 UI 리스트
    public List<GameObject> useSkillUIList; //사용중인 스킬 UI 리스트
    public List<GameObject> useAbilityUIList; //사용중인 능력치 UI 리스트
    public List<GameObject> useChrSkillUIList; //사용중인 캐릭터 스킬 UI 리스트

    public List<CardData> cardIconDescriptionData;
    public CharacterInfo playerCharacterInfo;

    // GameManager의 playerCharacterInfo에서 가져온 스킬 이미지
    public Sprite currentESkillSprite;
    public Sprite currentQSkillSprite;
    
    // Start is called before the first frame update
    void Start()
    {
        descriptionUI.SetActive(false);
        gameManager = inGameUI.GetComponent<GoTitle>().gameManager;
        selectedCardSystem = inGameUI.GetComponent<SelectedCardSystem>();
        
        // GraphicRaycaster 가져오기 (Canvas에 있음)
        graphicRaycaster = FindObjectOfType<GraphicRaycaster>();
        eventSystem = EventSystem.current;

        // leftUI의 자식 수집 순서:
        // 1. leftUI의 자식을 모두 구한다 (자식1)
        // 2. 자식1들의 자식을 모두 구한다 (자식2)
        // 3. 자식2들의 자식을 모두 구한다 (자식3)
        // 4. 자식3들의 0번째 자식들을 모두 leftUIList에 추가한다
        if (leftUI != null && leftUIList != null)
        {
            leftUIList.Clear();
            
            // 1. leftUI의 모든 직계 자식 구하기 (자식1)
            List<Transform> children1 = new List<Transform>();
            for (int i = 0; i < leftUI.transform.childCount; i++)
            {
                Transform directChild = leftUI.transform.GetChild(i);
                if (directChild != null)
                {
                    children1.Add(directChild);
                }
            }
            
            // 2. 자식1들의 모든 자식 구하기 (자식2) - 재귀적으로
            List<Transform> children2 = new List<Transform>();
            foreach (Transform child1 in children1)
            {
                if (child1 != null)
                {
                    CollectAllChildrenTransforms(child1, children2);
                }
            }
            
            // 3. 자식2들의 0번째 자식들을 모두 leftUIList에 추가
            foreach (Transform child2 in children2)
            {
                if (child2 != null && child2.childCount > 0)
                {
                    Transform firstChild = child2.GetChild(0);
                    if (firstChild != null && firstChild.gameObject != null)
                    {
                        leftUIList.Add(firstChild.gameObject);
                    }
                }
            }
        }

        // useSkillUI의 모든 직계 자식의 0번째 자식만 useSkillUIList에 추가
        if (useSkillUI != null && useSkillUIList != null)
        {
            useSkillUIList.Clear();
            // useSkillUI의 모든 직계 자식 가져오기
            for (int i = 0; i < useSkillUI.transform.childCount; i++)
            {
                Transform directChild = useSkillUI.transform.GetChild(i);
                if (directChild != null && directChild.childCount > 0)
                {
                    // 각 직계 자식의 0번째 자식만 추가
                    Transform firstChild = directChild.GetChild(0);
                    if (firstChild != null && firstChild.gameObject != null)
                    {
                        useSkillUIList.Add(firstChild.gameObject);
                    }
                }
            }
        }

        // useAbilityUI의 모든 직계 자식의 0번째 자식만 useAbilityUIList에 추가
        if (useAbilityUI != null && useAbilityUIList != null)
        {
            useAbilityUIList.Clear();
            // useAbilityUI의 모든 직계 자식 가져오기
            for (int i = 0; i < useAbilityUI.transform.childCount; i++)
            {
                Transform directChild = useAbilityUI.transform.GetChild(i);
                if (directChild != null && directChild.childCount > 0)
                {
                    // 각 직계 자식의 0번째 자식만 추가
                    Transform firstChild = directChild.GetChild(0);
                    if (firstChild != null && firstChild.gameObject != null)
                    {
                        useAbilityUIList.Add(firstChild.gameObject);
                    }
                }
            }
        }

        // useChrSkillUI의 모든 직계 자식의 0번째 자식만 useChrSkillUIList에 추가
        if (useChrSkillUI != null && useChrSkillUIList != null)
        {
            useChrSkillUIList.Clear();
            // useChrSkillUI의 모든 직계 자식 가져오기
            for (int i = 0; i < useChrSkillUI.transform.childCount; i++)
            {
                Transform directChild = useChrSkillUI.transform.GetChild(i);
                if (directChild != null && directChild.childCount > 0)
                {
                    // 각 직계 자식의 0번째 자식만 추가
                    Transform firstChild = directChild.GetChild(1);
                    if (firstChild != null && firstChild.gameObject != null)
                    {
                        useChrSkillUIList.Add(firstChild.gameObject);
                    }
                }
            }
        }

        // 4개 리스트의 모든 항목을 targetUIList에 추가
        if (targetUIList != null)
        {
            targetUIList.Clear();
            
            // leftUIList 추가
            if (leftUIList != null)
            {
                foreach (GameObject obj in leftUIList)
                {
                    if (obj != null && !targetUIList.Contains(obj))
                    {
                        targetUIList.Add(obj);
                    }
                }
            }
            
            // useSkillUIList 추가
            if (useSkillUIList != null)
            {
                foreach (GameObject obj in useSkillUIList)
                {
                    if (obj != null && !targetUIList.Contains(obj))
                    {
                        targetUIList.Add(obj);
                    }
                }
            }
            
            // useAbilityUIList 추가
            if (useAbilityUIList != null)
            {
                foreach (GameObject obj in useAbilityUIList)
                {
                    if (obj != null && !targetUIList.Contains(obj))
                    {
                        targetUIList.Add(obj);
                    }
                }
            }
            
            // useChrSkillUIList 추가
            if (useChrSkillUIList != null)
            {
                foreach (GameObject obj in useChrSkillUIList)
                {
                    if (obj != null && !targetUIList.Contains(obj))
                    {
                        targetUIList.Add(obj);
                    }
                }
            }
        }

        // GameManager의 playerCharacterInfo에서 스킬 이미지 가져오기
        currentESkillSprite = null;
        currentQSkillSprite = null;

        if (GameManager.Instance != null && GameManager.Instance.playerCharacterInfo != null)
        {
            CharacterInfo charInfo = GameManager.Instance.playerCharacterInfo;
            playerCharacterInfo = charInfo; // public 필드에도 저장

            if (charInfo.chracterSkill != null)
            {
                // CharacterSkill GameObject의 스킬 컴포넌트 가져오기
                MonoBehaviour skillComponent = charInfo.chracterSkill.GetComponent<MonoBehaviour>();
                if (skillComponent != null)
                {
                    // eSkillImage 가져오기
                    var eSkillField = skillComponent.GetType().GetField("eSkillImage");
                    if (eSkillField != null)
                    {
                        currentESkillSprite = (Sprite)eSkillField.GetValue(skillComponent);
                    }

                    // qSkillImage 가져오기
                    var qSkillField = skillComponent.GetType().GetField("qSkillImage");
                    if (qSkillField != null)
                    {
                        currentQSkillSprite = (Sprite)qSkillField.GetValue(skillComponent);
                    }
                }
            }
        }
    }

    // 재귀적으로 모든 자식 오브젝트를 수집하는 메서드
    private void CollectAllChildren(Transform parent, List<GameObject> targetList)
    {
        // 모든 직계 자식 가져오기
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child != null && child.gameObject != null)
            {
                targetList.Add(child.gameObject);
                // 재귀적으로 자식의 자식도 수집
                CollectAllChildren(child, targetList);
            }
        }
    }

    // 재귀적으로 모든 자식 Transform을 수집하는 메서드
    private void CollectAllChildrenTransforms(Transform parent, List<Transform> targetList)
    {
        // 모든 직계 자식 가져오기
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child != null)
            {
                targetList.Add(child);
                // 재귀적으로 자식의 자식도 수집
                CollectAllChildrenTransforms(child, targetList);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        CheckMouseOverCard();
    }

    private void CheckMouseOverCard()
    {
        // UI 위에 마우스가 있는지 확인
        if (eventSystem == null || !eventSystem.IsPointerOverGameObject())
        {
            // UI 위에 없으면 설명 초기화 및 descriptionUI 비활성화
            if (descriptionUI != null)
            {
                descriptionUI.SetActive(false);
            }
            if (iconDescription != null)
            {
                iconImage.sprite = null;
                iconOutline.sprite = null;
                iconName.text = "";
                iconDescription.text = "";
                needArousalAbility.text = "";
            }
            return;
        }

        // GraphicRaycaster로 마우스 위치의 UI 요소 감지
        if (graphicRaycaster == null)
        {
            // GraphicRaycaster가 없으면 descriptionUI 비활성화
            if (descriptionUI != null)
            {
                descriptionUI.SetActive(false);
            }
            return;
        }

        PointerEventData pointerEventData = new PointerEventData(eventSystem);
        pointerEventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        graphicRaycaster.Raycast(pointerEventData, results);

        // 감지된 UI 요소 중 targetUIList에 포함된 것만 체크
        foreach (RaycastResult result in results)
        {
            // result.gameObject가 targetUIList에 포함되어 있는지 확인 (자식 포함)
            if (!IsInTargetUIList(result.gameObject))
                continue;

            Image image = result.gameObject.GetComponent<Image>();
            if (image != null && image.sprite != null)
            {
                // useChrSkillUIList에 포함된 UI인지 확인
                if (IsInChrSkillUIList(result.gameObject))
                {
                    // 저장된 스킬 이미지와 비교
                    if (playerCharacterInfo != null)
                    {
                        // UI의 sprite가 저장된 E 스킬 sprite와 일치하는 경우
                        if (image.sprite == currentESkillSprite && currentESkillSprite != null)
                        {
                            iconImage.sprite = currentESkillSprite; // UI의 sprite를 iconImage에 설정
                            iconOutline.sprite = weaponIconOutline;
                            iconName.text = playerCharacterInfo.eskillName;
                            iconDescription.text = playerCharacterInfo.eskillDescription;
                            needArousalAbility.text = "";
                            if (descriptionUI != null)
                            {
                                descriptionUI.SetActive(true);
                            }
                            return; // 찾으면 종료
                        }
                        // UI의 sprite가 저장된 Q 스킬 sprite와 일치하는 경우
                        else if (image.sprite == currentQSkillSprite && currentQSkillSprite != null)
                        {
                            iconImage.sprite = currentQSkillSprite; // UI의 sprite를 iconImage에 설정
                            iconOutline.sprite = weaponIconOutline;
                            iconName.text = playerCharacterInfo.qskillName;
                            iconDescription.text = playerCharacterInfo.qskillDescription;
                            needArousalAbility.text = "";
                            if (descriptionUI != null)
                            {
                                descriptionUI.SetActive(true);
                            }
                            return; // 찾으면 종료
                        }
                    }
                }
                else
                {
                    // cardIconDescriptionData에서 동일한 sprite를 가진 CardData 찾기
                    CardData matchingCardData = FindCardDataByImage(image.sprite);
                    if (matchingCardData != null && iconDescription != null)
                    {
                        iconImage.sprite = matchingCardData.cardImage;
                        iconName.text = matchingCardData.cardName;
                        iconDescription.text = matchingCardData.weaponDescription;
                        if( matchingCardData.needArousalAbility != null)
                        {
                            needArousalAbility.text = "각성 필요 능력치: " + matchingCardData.needArousalAbility;
                            iconOutline.sprite = weaponIconOutline;
                        }
                        else
                        {
                            needArousalAbility.text = "";
                            iconOutline.sprite = abilityIconOutline;
                        }
                        if (descriptionUI != null)
                        {
                            descriptionUI.SetActive(true);
                        }
                        return; // 찾으면 종료
                    }
                }
            }
        }

        // 일치하는 sprite를 찾지 못하면 설명 초기화 및 descriptionUI 비활성화
        if (iconDescription != null)
        {
            iconImage.sprite = null;
            iconOutline.sprite = null;
            iconName.text = "";
            iconDescription.text = "";
            needArousalAbility.text = "";
            descriptionUI.SetActive(false);
        }
    }

    // GameObject가 targetUIList에 포함되어 있는지 확인 (자식 포함)
    private bool IsInTargetUIList(GameObject targetObject)
    {
        if (targetUIList == null || targetUIList.Count == 0)
            return true; // 리스트가 비어있으면 모든 UI 체크

        // 직접 포함되어 있는지 확인
        if (targetUIList.Contains(targetObject))
            return true;

        // 자식으로 포함되어 있는지 확인
        foreach (GameObject uiObject in targetUIList)
        {
            if (uiObject != null && targetObject.transform.IsChildOf(uiObject.transform))
            {
                return true;
            }
        }

        return false;
    }

    // GameObject가 useChrSkillUIList에 포함되어 있는지 확인
    private bool IsInChrSkillUIList(GameObject targetObject)
    {
        if (useChrSkillUIList == null || useChrSkillUIList.Count == 0)
            return false;

        // 직접 포함되어 있는지 확인
        if (useChrSkillUIList.Contains(targetObject))
            return true;

        // 자식으로 포함되어 있는지 확인
        foreach (GameObject uiObject in useChrSkillUIList)
        {
            if (uiObject != null && targetObject.transform.IsChildOf(uiObject.transform))
            {
                return true;
            }
        }

        return false;
    }


    private CardData FindCardDataByImage(Sprite targetImage)
    {
        if (targetImage == null || cardIconDescriptionData == null)
            return null;

        foreach (CardData cardData in cardIconDescriptionData)
        {
            if (cardData != null && cardData.cardImage == targetImage)
            {
                return cardData;
            }
        }
        return null;
    }
}
