using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // 씬 관리를 위해 추가
using TMPro; // TextMeshPro 사용을 위해 추가

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public enum GameStatus 
    {
        Title, // 타이틀 상태
        Ready, // 준비 상태
        InGame, // 게임 진행 중 상태
        Pause, // 일시 정지 상태
        GameOver, // 게임 오버 상태
        Win // 승리 상태
    }
    public MainMenuSystem mainMenuSystem; //메인 메뉴 시스템
    public UpgradeInRoom upgradeInRoom; //능력치 업그레이드 부분
    public static GameStatus gameStatus; // 게임 상태 변수
    //public List<GameObject> playerList; //게임에 있는 플레이어 리스트
    public CharacterInfo playerCharacterInfo;//플레이어 시작 캐릭터 정보
    public UpgradeInRoom.UpgradeAbilityValues upgradeAbilityValues; //구매한 능력치 값 데이터
    public UpgradeCountData upgradeCountData; // 구매한 능력치 구매 횟수 데이터
    public GameObject mainCanvas;
    public float gold; //현재 보유 골드

    // 업그레이드 카운트 정보를 저장할 클래스
    [System.Serializable]
    public class UpgradeCountData
    {
        public Dictionary<UpgradeAbilityStatus.UpgradeAbilityCategory, int> upgradeCounts;

        public UpgradeCountData()
        {
            upgradeCounts = new Dictionary<UpgradeAbilityStatus.UpgradeAbilityCategory, int>();
        }
    }

    void Awake()
    {
        // 싱글톤 패턴 적용  한 씬에 같은 오브젝트가 중복시 삭제
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            upgradeCountData = new UpgradeCountData();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start() 
    {
        // 초기 상태를 Title로 설정
        gameStatus = GameStatus.Title;

        // gameStatus가 Title일 때 "Main Menu Canvas"를 찾아서 mainMenuSystem을 지정합니다.
        if (gameStatus == GameStatus.Title)
        {
            GameObject mainMenuCanvas = GameObject.Find("Main Menu Canvas");
            if (mainMenuCanvas != null)
            {
                mainMenuSystem = mainMenuCanvas.GetComponent<MainMenuSystem>();
                // upgradeInRoom 컴포넌트를 가져옵니다.
                upgradeInRoom = mainMenuSystem.transform.GetChild(2).GetChild(8).gameObject.GetComponent<UpgradeInRoom>();
                upgradeInRoom.goldUI.text = gold.ToString();
            }
        }
    }

    //시작 UI세팅
    public void StartUISetting(MainMenuSystem mainMenuSystem)
    {
        // 모든 UI 오브젝트 비활성화
        mainMenuSystem.createRoom.SetActive(false);
        mainMenuSystem.selectCharacter.SetActive(false);
        mainMenuSystem.upgradeStat.SetActive(false);
        mainMenuSystem.optionSet.SetActive(false);
    }

    public void GameStart()
    {
        // 게임 시간 초기화
        //Time.timeScale = 1f;
        
        // "InGame" 씬으로 이동
        SceneManager.LoadScene("Df Chr System"); // 이동할 씬의 이름을 지정

        // 씬이 로드된 후 InstantiatePlayer 호출
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Debug.Log($"Scene loaded: {scene.name}"); // 씬 로드 로그 출력
        if (scene.name == "Df Chr System")
        {
            //Debug.Log("scene loaded successfully."); // 이동 완료 로그 출력
            SceneManager.sceneLoaded -= OnSceneLoaded; // 이벤트 리스너 제거
            ResumeGame();
            //gameStatus = GameStatus.InGame;
            Debug.Log($"GameStatus changed to: {gameStatus}"); // GameStatus 변경 로그 출력

            // PlayerUI 태그를 가진 게임오브젝트를 찾아 mainCanvas에 설정
            GameObject playerUI = GameObject.FindGameObjectWithTag("PlayerUI");
            if (playerUI != null)
            {
                mainCanvas = playerUI;
            }

            InstantiatePlayer(); // 플레이어 인스턴스화 호출
        }
    }

    public void InstantiatePlayer() // 호출시 플레이어 캐릭터를 씬에 생성하는 부분
    {
        // 게임 상태가 InGame일 때 플레이어를 생성합니다.
        if (gameStatus == GameStatus.InGame)
        {
            // starterCharacter와 캐릭터 모델이 존재하면 플레이어를 생성합니다.
            if (playerCharacterInfo != null && playerCharacterInfo.characterModel != null)
            {
                // starterCharacter.characterModel을 (0, 0, 0) 좌표에 생성합니다.
                GameObject playerInstance = Instantiate(playerCharacterInfo.characterModel, new Vector3(0, 0, 0), Quaternion.identity);
                DontDestroyOnLoad(playerInstance); // playerInstance에 대해 DontDestroyOnLoad 호출합니다.

                // Player 게임 오브젝트를 찾습니다.
                GameObject player = GameObject.Find("Player");
                if (player != null)
                {
                    // playerInstance의 부모를 Player로 설정하고 첫 번째 자식으로 추가합니다.
                    playerInstance.transform.SetParent(player.transform);
                    playerInstance.transform.SetAsFirstSibling(); // 첫 번째 자식으로 설정합니다.

                    // PlayerSystem 컴포넌트를 가져와서 게임 매니저와 플레이어 게임 설정을 수행합니다.
                    PlayerSystem playerSystem = player.GetComponent<PlayerSystem>();
                    playerSystem.gameManager = this;
                    playerSystem.PlayerStatusSetting(playerCharacterInfo, upgradeAbilityValues); //플레이어 스테이터스 계산 메서드
                    playerSystem.PlayerSkillSetting(playerCharacterInfo); // 플레이어 스킬 설정 메서드
                }
            }
        }
    }

    public void UpgradeAbilityData(UpgradeInRoom.UpgradeAbilityValues values) //상점에서 산 능력치 저장 메서드
    {
        // 받은 데이터를 upgradeAbilityValues 변수에 저장합니다.
        upgradeAbilityValues = values;

        // 저장된 values 값을 Debug.Log로 출력합니다.
        //Debug.Log($"UpgradeAbilityData: maxHealth={values.maxHealth}, healthRegeneration={values.healthRegeneration}, attack={values.attack}, attackSpeed={values.attackSpeed}, defense={values.defense}, speed={values.speed}, critical={values.critical}, projecTile={values.projecTile}, attackRange={values.attackRange}, duration={values.duration}, acquisitionRange={values.acquisitionRange}, experienceGain={values.experienceGain}, goldGain={values.goldGain}");
    }

    void Update()
    {
        
    }

    public void SaveUpgradeCounts() //업그레이드 능력치 카운트를 저장
    {
        if (mainMenuSystem == null) return;
        
        upgradeCountData.upgradeCounts.Clear();
        
        // upgradeStat의 모든 자식 오브젝트에서 UpgradeAbilityStatus를 찾아 저장
        Transform upgradeStatTransform = mainMenuSystem.upgradeStat.transform;
        for (int i = 0; i < upgradeStatTransform.childCount; i++)
        {
            UpgradeAbilityStatus status = upgradeStatTransform.GetChild(i).GetComponent<UpgradeAbilityStatus>();
            if (status != null && status.upgradeAbilityCategory != UpgradeAbilityStatus.UpgradeAbilityCategory.none)
            {
                upgradeCountData.upgradeCounts[status.upgradeAbilityCategory] = status.upgradeCount;
                //Debug.Log($"저장된 업그레이드 카운트 - 카테고리: {status.upgradeAbilityCategory}, 카운트: {status.upgradeCount}");
            }
        }
    }

    public void ReturnRoom() //게임 도중 준비 화면으로 돌아가기
    {
        // 게임 상태를 Title로 변경
        gameStatus = GameStatus.Title;
        
        // 타이틀 씬으로 이동
        SceneManager.LoadScene("Title");
        
        // 씬이 로드된 후 필요한 초기화 작업 수행
        SceneManager.sceneLoaded += OnTitleSceneLoaded;
    }

    private void OnTitleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Title")
        {
            // 이벤트 리스너 제거
            SceneManager.sceneLoaded -= OnTitleSceneLoaded;
            
            // 메인 메뉴 시스템 초기화
            GameObject mainMenuCanvas = GameObject.Find("Main Menu Canvas");
            if (mainMenuCanvas != null)
            {
                mainMenuSystem = mainMenuCanvas.GetComponent<MainMenuSystem>();
                upgradeInRoom = mainMenuSystem.transform.GetChild(2).GetChild(8).gameObject.GetComponent<UpgradeInRoom>();
                
                // MainMenuSystem의 Start 메서드가 실행될 때까지 대기
                StartCoroutine(WaitForMainMenuSystem());
            }
        }
    }

    private IEnumerator WaitForMainMenuSystem()
    {
        // MainMenuSystem의 Start 메서드가 실행될 때까지 대기
        yield return new WaitForEndOfFrame();
        
        // CreateRoom 실행
        if (mainMenuSystem != null)
        {   
            // UpgradeInRoom이 완전히 초기화될 때까지 대기
            yield return new WaitForEndOfFrame();
            
            // haveAbilityData를 true로 설정하고 저장된 데이터 복원
            if (upgradeInRoom != null)
            {
                upgradeInRoom.haveAbilityData = true;
                upgradeInRoom.currentAbilityValues = upgradeAbilityValues;
                // 골드 UI 업데이트
                if (upgradeInRoom.goldUI != null)
                {
                    upgradeInRoom.goldUI.text = gold.ToString();
                }
            }
            mainMenuSystem.CreateRoom();
        }
    }

    public static void PauseGame() //게임 일시정지 메서드
    {
        gameStatus = GameStatus.Pause;
        Time.timeScale = 0; // 게임 시간 멈춤
        // UI 오버레이 활성화
        // 예: pauseMenu.SetActive(true);
    }

    public static void ResumeGame() //일시정지 해제 메서드
    {
        gameStatus = GameStatus.InGame;
        Time.timeScale = 1; // 게임 시간 재개
        // UI 오버레이 비활성화
        // 예: pauseMenu.SetActive(false);
    }
}
