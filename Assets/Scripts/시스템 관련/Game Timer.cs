using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMeshPro 사용을 위해 추가

public class GameTimer : MonoBehaviour
{
    public static float gameTimer = 0f; // 타이머 변수 (초 단위)
    public float maxTime = 300f;    // 최대 시간(예: 5분 = 300초)
    public TextMeshProUGUI timerText; // Text를 TextMeshProUGUI로 변경

    // Start is called before the first frame update
    void Start()
    {
        gameTimer = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        // GameManager의 상태가 InGame일 때만 타이머가 동작
        if (GameManager.gameStatus == GameManager.GameStatus.InGame)
        {
            if (gameTimer < maxTime)
            {
                gameTimer += Time.deltaTime;
            }
            else
            {
                gameTimer = maxTime;
                // 시간이 다 되었을 때 추가 동작(예: 게임 오버 등) 필요시 여기에 작성
            }
        }

        // 타이머 텍스트 업데이트
        if (timerText != null)
        {
            timerText.text = GetTimerString();
        }
    }

    public static string GetTimerString()
    {
        int minutes = Mathf.FloorToInt(gameTimer / 60f);
        int seconds = Mathf.FloorToInt(gameTimer % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
