using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoTitle : MonoBehaviour
{
    public GameManager gameManager;
    public GameObject optionUI;

    // Start is called before the first frame update
    void Start()
    {
        // GameManager 태그를 가진 오브젝트를 찾아서 연결
        GameObject gameManagerObj = GameObject.FindGameObjectWithTag("GameManager");
        if (gameManagerObj != null)
        {
            gameManager = gameManagerObj.GetComponent<GameManager>();
        }
        optionUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // ESC 키 입력 감지
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleOptionUI();
        }
    }

    public void ToggleOptionUI()
    {
        if (optionUI != null)
        {
            optionUI.SetActive(!optionUI.activeSelf);
        }
    }

    public void GoBackRoom()
    {
        //Time.timeScale = 1f;
        gameManager.ReturnRoom();
    }
}
