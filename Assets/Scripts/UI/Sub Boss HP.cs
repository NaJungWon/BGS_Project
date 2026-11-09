using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SubBossHP : MonoBehaviour
{
    public MonsterStatus monsterStatus;
    private Camera mainCamera;
    public Image hpBar;
    public TextMeshProUGUI hpText;

    // Start is called before the first frame update
    void Start()
    {
        // 메인 카메라 참조 가져오기
        mainCamera = Camera.main;
        monsterStatus = transform.parent.GetComponent<MonsterStatus>();
        hpBar = this.gameObject.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Image>();
        hpText = this.gameObject.transform.GetChild(0).GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
        
        // 초기 HP 바와 텍스트 설정
        UpdateHPBar();
    }

    void OnEnable()
    {
        // 활성화될 때 HP 바와 텍스트 업데이트
        UpdateHPBar();
    }

    void LateUpdate()
    {
        if (mainCamera != null)
        {
            // 텍스트가 항상 카메라를 향하도록 회전
            transform.rotation = mainCamera.transform.rotation;
        }

        // HP 바와 텍스트 실시간 업데이트
        UpdateHPBar();
    }

    void UpdateHPBar()
    {
        if (monsterStatus != null && hpBar != null && hpText != null)
        {
            // HP 바 업데이트 (0~1 사이 값)
            float hpRatio = monsterStatus.currentHealth / monsterStatus.maxHealth;
            hpBar.fillAmount = hpRatio;

            // HP 텍스트 업데이트 (현재 HP/최대 HP)
            hpText.text = $"{Mathf.Round(monsterStatus.currentHealth)}/{monsterStatus.maxHealth}";
        }
    }
}
