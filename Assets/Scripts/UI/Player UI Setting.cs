using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro 사용을 위해 추가

public class PlayerUISetting : MonoBehaviour
{
    //public PlayerStatus playerStatus;
    [Header("게임 타이머")]
    public GameObject gameTimerUI;
    public TextMeshProUGUI gameTimer;
    [Header("플레이어 UI")]
    public GameObject backGround;
    public Image experience;
    public GameObject hp;
    public Image playerIcon;
    public TextMeshProUGUI playerLevel;
    public List<GameObject> augmentationSlots = new List<GameObject>();
    public List<GameObject> skillSlots = new List<GameObject>();
    //public GameObject passiveSlots;
    public List<GameObject> abilitySlots = new List<GameObject>();
    public GameObject gameClearUI;
    public GameObject gameOverUI;
    [Header("보스 UI")]
    public GameObject bossUI;
    public Image bossHPImage;
    public TextMeshProUGUI bossHPText;
    [Header("맵 이미지")]
    public GameObject map;

    private Image hpImage;
    private TextMeshProUGUI hpText;

    // Start is called before the first frame update
    void Awake()
    {
        backGround = this.transform.GetChild(0).gameObject;
        backGround.SetActive(false);
        gameTimerUI = this.transform.GetChild(1).GetChild(0).gameObject;
        gameTimer = gameTimerUI.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        bossUI = this.transform.GetChild(1).GetChild(1).gameObject;
        bossHPImage = bossUI.transform.GetChild(1).GetChild(0).GetComponent<Image>();
        bossHPText = bossUI.transform.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>();

        bossUI.SetActive(false);
        // hp 게임 오브젝트의 첫 번째 자식인 Image와 두 번째 자식인 Text 가져오기
        hpImage = hp.transform.GetChild(0).GetComponent<Image>(); // 첫 번째 자식: Image
        hpText = hp.transform.GetChild(1).GetComponent<TextMeshProUGUI>(); // 두 번째 자식: Text
        // 초기 FillAmount 설정
        experience.fillAmount = 0; // 초기값 0으로 설정
        hpImage.fillAmount = 1;
    }

    // Update is called once per frame
    void Update()
    {
        // 게임 타이머 UI 갱신
        if (gameTimer != null)
        {
            gameTimer.text = GameTimer.GetTimerString();
        }

        // 게임 상태에 따른 UI 처리
        if (GameManager.gameStatus == GameManager.GameStatus.Win)
        {
            GameManager.PauseGame();
            gameClearUI.SetActive(true);
            backGround.SetActive(true);
        }
        else if (GameManager.gameStatus == GameManager.GameStatus.GameOver)
        {
            GameManager.PauseGame();
            gameOverUI.SetActive(true);
            backGround.SetActive(true);
        }
        
    }

    public void UpdateHealthUI(PlayerStatus playerStatus)
    {
        // 체력 이미지의 FillAmount 설정
        if (playerStatus.maxHealth > 0) // 최대 체력이 0보다 클 때만 계산
        {
            hpImage.fillAmount = playerStatus.currentHealth / playerStatus.maxHealth; // FillAmount 설정
        }

        // 체력 텍스트 업데이트 - 소수점 제거 및 최대 체력 제한
        float displayHealth = Mathf.Min(Mathf.FloorToInt(playerStatus.currentHealth), playerStatus.maxHealth);
        hpText.text = $"{displayHealth} / {playerStatus.maxHealth}"; // 현재 체력 / 최대 체력 표시
    }

    public void UpdateExperienceUI(PlayerStatus playerStatus)
    {
        // 경험치 비율에 따라 FillAmount 업데이트
        if (playerStatus.needExperience > 0) // 필요 경험치가 0보다 클 때만 계산
        {
            experience.fillAmount = playerStatus.currentExperience / playerStatus.needExperience; // 비율 계산
        }
    }

    public void UpdatePlayerIconUI(CharacterInfo playerInfo)
    {
        playerIcon.sprite = playerInfo.characterIcon;
    }


    public void UpdatePlayerLevelUI(PlayerStatus playerStatus)
    {
        // 플레이어 레벨 UI 업데이트
        playerLevel.text = playerStatus.level.ToString(); // 현재 레벨 표시
    }

    public void UpdatePlayerSkillUI(GameObject skillInstance)
    {
        if (skillInstance != null)
        {
            // 모든 직업의 스킬 컴포넌트를 가져오기
            MonoBehaviour skillComponent = skillInstance.GetComponent<MonoBehaviour>();
            if (skillComponent != null)
            {   
                // 0번 슬롯 (E 스킬)
                if (skillSlots.Count > 0)
                {
                    GameObject eSkillSlot = skillSlots[0];
                    if (eSkillSlot.transform.childCount > 1)
                    {
                        // 0번째 자식 (배경) 활성화 및 이미지 설정
                        GameObject eSkillBG = eSkillSlot.transform.GetChild(0).gameObject;
                        eSkillBG.SetActive(true);
                        Image eSkillBGImage = eSkillBG.GetComponent<Image>();
                        if (eSkillBGImage != null)
                        {
                            eSkillBGImage.sprite = (Sprite)skillComponent.GetType().GetField("eSkillImageBG").GetValue(skillComponent);
                        }

                        // 1번째 자식 (스킬) 활성화 및 이미지 설정
                        GameObject eSkillChild = eSkillSlot.transform.GetChild(1).gameObject;
                        eSkillChild.SetActive(true);
                        Image eSkillImage = eSkillChild.GetComponent<Image>();
                        if (eSkillImage != null)
                        {
                            eSkillImage.sprite = (Sprite)skillComponent.GetType().GetField("eSkillImage").GetValue(skillComponent);
                        }
                    }
                }

                // 1번 슬롯 (Q 스킬)
                if (skillSlots.Count > 1)
                {
                    GameObject qSkillSlot = skillSlots[1];
                    if (qSkillSlot.transform.childCount > 1)
                    {
                        // 0번째 자식 (배경) 활성화 및 이미지 설정
                        GameObject qSkillBG = qSkillSlot.transform.GetChild(0).gameObject;
                        qSkillBG.SetActive(true);
                        Image qSkillBGImage = qSkillBG.GetComponent<Image>();
                        if (qSkillBGImage != null)
                        {
                            qSkillBGImage.sprite = (Sprite)skillComponent.GetType().GetField("qSkillImageBG").GetValue(skillComponent);
                        }

                        // 1번째 자식 (스킬) 활성화 및 이미지 설정
                        GameObject qSkillChild = qSkillSlot.transform.GetChild(1).gameObject;
                        qSkillChild.SetActive(true);
                        Image qSkillImage = qSkillChild.GetComponent<Image>();
                        if (qSkillImage != null)
                        {
                            qSkillImage.sprite = (Sprite)skillComponent.GetType().GetField("qSkillImage").GetValue(skillComponent);
                        }
                    }
                }
            }
        }
    }

    public void UpdateSkillCooldownUI(GameObject skillInstance) //스킬 쿨타임에 따른 UI 지속 업데이트 메서드
    {
        if (skillInstance != null)
        {
            // 모든 직업의 스킬 컴포넌트를 가져오기
            MonoBehaviour skillComponent = skillInstance.GetComponent<MonoBehaviour>();
            if (skillComponent != null)
            {
                // E 스킬 쿨다운 UI 업데이트
                if (skillSlots.Count > 0)
                {
                    GameObject eSkillSlot = skillSlots[0];
                    if (eSkillSlot.transform.childCount > 1)
                    {
                        GameObject eSkillChild = eSkillSlot.transform.GetChild(1).gameObject;
                        Image eSkillImage = eSkillChild.GetComponent<Image>();
                        if (eSkillImage != null)
                        {
                            float currentECooldown = (float)skillComponent.GetType().GetField("currentECooldown").GetValue(skillComponent);
                            float eSkillCooldown = (float)skillComponent.GetType().GetField("eSkillCooldown").GetValue(skillComponent);
                            float eCooldownRatio = 1 - (currentECooldown / eSkillCooldown);
                            eSkillImage.fillAmount = eCooldownRatio;
                        }
                    }
                }

                // Q 스킬 쿨다운 UI 업데이트
                if (skillSlots.Count > 1)
                {
                    GameObject qSkillSlot = skillSlots[1];
                    if (qSkillSlot.transform.childCount > 1)
                    {
                        GameObject qSkillChild = qSkillSlot.transform.GetChild(1).gameObject;
                        Image qSkillImage = qSkillChild.GetComponent<Image>();
                        if (qSkillImage != null)
                        {
                            float currentQCooldown = (float)skillComponent.GetType().GetField("currentQCooldown").GetValue(skillComponent);
                            float qSkillCooldown = (float)skillComponent.GetType().GetField("qSkillCooldown").GetValue(skillComponent);
                            float qCooldownRatio = 1 - (currentQCooldown / qSkillCooldown);
                            qSkillImage.fillAmount = qCooldownRatio;
                        }
                    }
                }
            }
        }
    }

    public void UpdateBossHealthUI(MonsterStatus bossStatus)
    {
        if (bossStatus != null)
        {
            // 보스 체력이 0 이하가 되지 않도록 Mathf.Max 사용
            float displayHealth = Mathf.Max(0, Mathf.FloorToInt(bossStatus.currentHealth));
            bossHPText.text = $"{displayHealth} / {bossStatus.maxHealth}";
            
            // 보스 체력 이미지 업데이트
            if (bossStatus.maxHealth > 0)
            {
                bossHPImage.fillAmount = bossStatus.currentHealth / bossStatus.maxHealth;
            }
        }
    }
}
