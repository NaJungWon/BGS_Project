using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuSystem : MonoBehaviour
{
    public GameManager gameManager;
    public GameObject createRoom; // 방 생성창 오브젝트
    public GameObject selectCharacter; // 캐릭터 선택창 오브젝트
    public GameObject upgradeStat; // 스탯 업그레이드창 오브젝트
    public GameObject optionSet; // 옵션 설정창 오브젝트
    public List<GameObject> RoomSlot; //유저 슬롯 오브젝트 리스트
    public List<CharacterInfo> dfCharData;//기본 캐릭터 데이터 리스트

    // Start is called before the first frame update
    void Start()
    {
        // GameManager 태그를 가진 오브젝트를 찾아서 연결
        GameObject gameManagerObj = GameObject.FindGameObjectWithTag("GameManager");
        if (gameManagerObj != null)
        {
            gameManager = gameManagerObj.GetComponent<GameManager>();
        }

        createRoom = transform.GetChild(2).gameObject;
        selectCharacter = createRoom.transform.GetChild(7).gameObject;
        upgradeStat = createRoom.transform.GetChild(8).gameObject;
        optionSet = transform.GetChild(3).gameObject;

        // CreateRoom의 2번 자식 오브젝트의 모든 자식 오브젝트를 RoomSlot 리스트에 추가합니다.
        for (int i = 0; i < createRoom.transform.GetChild(2).childCount; i++)
        {
            RoomSlot.Add(createRoom.transform.GetChild(2).GetChild(i).gameObject);
        }

        gameManager.StartUISetting(this);
    }

    public void ChooseCharacter(int charIndex) // 캐릭터 선택해서 바꾸는 부분
    {
        // 선택된 캐릭터 인덱스에 따라 starterCharacter를 설정합니다.
        if (dfCharData != null && charIndex >= 0 && charIndex < dfCharData.Count)
        {
            gameManager.playerCharacterInfo = dfCharData[charIndex];

            // RoomSlot이 존재하면 선택된 캐릭터의 이미지를 RoomSlot의 첫 번째 슬롯에 표시합니다.
            if (RoomSlot.Count > 0)
            {
                // RoomSlot의 첫 번째 자식 오브젝트의 Image 컴포넌트를 가져옵니다.
                Image roomImage = RoomSlot[0].transform.GetChild(0).GetComponent<Image>();
                if (roomImage != null)
                {
                    roomImage.sprite = gameManager.playerCharacterInfo.characterImage;
                    roomImage.gameObject.SetActive(true);
                    selectCharacter.SetActive(false);
                }
            }
        }
        else
        {
            // 유효하지 않은 캐릭터 인덱스가 선택된 경우 경고 메시지를 출력합니다.
            Debug.LogWarning("Invalid character index selected.");
        }
    }

    public void BaseCharacterSetting() // 게임 시작시 플레이어 기본 캐릭터 설정하는부분
    {
        // 게임 상태가 Ready일 때 기본 캐릭터 설정을 수행합니다.
        if (GameManager.gameStatus == GameManager.GameStatus.Ready)
        {
            // RoomSlot과 기본 캐릭터 데이터가 존재하면 첫 번째 캐릭터의 이미지를 RoomSlot의 첫 번째 슬롯에 표시합니다.
            if (RoomSlot.Count > 0 && dfCharData.Count > 0)
            {
                Image roomImage = RoomSlot[0].transform.GetChild(0).GetComponent<Image>();
                if (roomImage != null)
                {
                    // 기본 캐릭터 데이터가 존재하면 게임매니저의 플레이어 시작 캐릭터정보 입력
                    gameManager.playerCharacterInfo = dfCharData[0];
                    roomImage.sprite = dfCharData[0].characterImage;
                    roomImage.gameObject.SetActive(true);
                }
            }
        }
    }

    public void CreateRoom()
    {
        bool isActive = createRoom.activeSelf; // 현재 방 생성 오브젝트의 활성화 상태 확인
        createRoom.SetActive(!isActive); // 상태 반전

        // createRoom이 활성화되면 gameStatus를 Ready로 변경
        if (createRoom.activeSelf)
        {
            GameManager.gameStatus = GameManager.GameStatus.Ready; // 게임 상태를 Ready로 변경
            Debug.Log($"GameStatus changed to: {GameManager.gameStatus}"); // GameStatus 변경 로그 출력
            BaseCharacterSetting();

            // upgradeStat 게임오브젝트의 UpgradeInRoom 컴포넌트의 StartAbilitySetting 메서드 실행
            UpgradeInRoom upgradeInRoom = upgradeStat.GetComponent<UpgradeInRoom>();
            if (upgradeInRoom != null)
            {
                upgradeInRoom.StartAbilitySetting();
            }
        }
    }

    public void Exit()
    {
        // 애플리케이션 종료
        Application.Quit();

        // 에디터에서 실행 중일 때는 에디터를 종료하는 코드 (옵션)
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // 에디터에서 플레이 모드 종료
        #endif
    }

    public void SelectCharacterWindow()
    {
        bool isActive = selectCharacter.activeSelf; // 현재 캐릭터 선택 오브젝트의 활성화 상태 확인
        selectCharacter.SetActive(!isActive); // 상태 반전
    }

    public void OptionSetting()
    {
        bool isActive = optionSet.activeSelf; // 현재 옵션 설정 오브젝트의 활성화 상태 확인
        optionSet.SetActive(!isActive); // 상태 반전
    }

    public void UpgradeStatus()
    {
        bool isActive = upgradeStat.activeSelf; // 현재 스탯 업그레이드 오브젝트의 활성화 상태 확인
        upgradeStat.SetActive(!isActive); // 상태 반전
        // 스탯 업그레이드 로직
    }

    public void GameStartSetting()
    {
        // 게임 시작 전 업그레이드 카운트 저장
        gameManager.SaveUpgradeCounts();
        gameManager.GameStart();
    }
}
