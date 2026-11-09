using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    public Slider BGMslider; //배경음조절 슬라이더
    public Slider SFXslider; //효과음조절 슬라이더

    [Header("배경 사운드 설정")]
    public List<GameObject> sound = new List<GameObject>(); //배경음 사운드 오브젝트
    public List<AudioSource> defaultSoundSource = new List<AudioSource>(); //배경음 사운드 오브젝트의 컴포넌트

    [Header("SFX 사운드 설정")]
    public List<GameObject> soundSource = new List<GameObject>(); //인게임 풀링방식 사운드 오브젝트
    public List<AudioSource> effectSoundSource = new List<AudioSource>(); //인게임 풀링방식 사운드 오브젝트의 컴포넌트
    
    // EffectAudioSync가 적용된 오브젝트들을 관리하는 리스트
    private HashSet<GameObject> effectAudioSyncObjects = new HashSet<GameObject>();
    
    private float currentVolume = 1f; //현재 볼륨
    private Dictionary<GameObject, int> childCountMap = new Dictionary<GameObject, int>(); //자식 개수 저장
    private Dictionary<AudioSource, float> originalVolumes = new Dictionary<AudioSource, float>(); //원래 볼륨 저장

    private float stableTimer = 0f;
    private float stableInterval = 1f;
    private float lastSliderValue = -1f;
    private bool needUpdate = false;

    void Awake()
    {
        // 싱글톤 초기화
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // 슬라이더 범위 설정 (0~100) 및 초기값 100으로 설정
        if (BGMslider != null)
        {
            BGMslider.maxValue = 100f;
            BGMslider.minValue = 0f;
            BGMslider.value = 100f; // 초기값 100으로 설정
        }
        if (SFXslider != null)
        {
            SFXslider.maxValue = 100f;
            SFXslider.minValue = 0f;
            SFXslider.value = 100f; // 초기값 100으로 설정
        }

        // 배경음 AudioSource 수집
        defaultSoundSource.Clear();
        foreach (GameObject soundObj in sound)
        {
            if (soundObj != null)
            {
                foreach (AudioSource source in soundObj.GetComponentsInChildren<AudioSource>())
                {
                    if (!defaultSoundSource.Contains(source))
                        defaultSoundSource.Add(source);
                }
            }
        }
    }

    void OnDestroy()
    {
        // 게임 종료 시 모든 AudioSource의 볼륨을 원래대로 복구
        foreach (var kvp in originalVolumes)
        {
            if (kvp.Key != null)
            {
                kvp.Key.volume = kvp.Value;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // BGM 볼륨 관리 (슬라이더 값 0~100을 AudioSource 볼륨 0~1로 변환)
        if (BGMslider != null)
        {
            float volume = BGMslider.value / 100f; // 0~100을 0~1로 정규화
            foreach (AudioSource source in defaultSoundSource)
            {
                if (source != null)
                    source.volume = volume;
            }
        }

        // SFX 볼륨 관리 (활성화된 오브젝트만, 슬라이더 값 0~100을 AudioSource 볼륨 0~1로 변환)
        if (SFXslider != null)
        {
            float volume = SFXslider.value / 100f; // 0~100을 0~1로 정규화
            effectSoundSource.Clear();
            
            // soundSource 리스트의 오브젝트들 처리
            foreach (GameObject obj in soundSource)
            {
                if (obj != null && obj.activeInHierarchy)
                {
                    foreach (AudioSource source in obj.GetComponentsInChildren<AudioSource>())
                    {
                        if (!effectSoundSource.Contains(source))
                        {
                            source.volume = volume;
                            effectSoundSource.Add(source);
                        }
                    }
                }
            }
            
            // EffectAudioSync가 적용된 오브젝트들 처리
            foreach (GameObject obj in effectAudioSyncObjects)
            {
                if (obj != null && obj.activeInHierarchy)
                {
                    foreach (AudioSource source in obj.GetComponentsInChildren<AudioSource>())
                    {
                        if (!effectSoundSource.Contains(source))
                        {
                            source.volume = volume;
                            effectSoundSource.Add(source);
                        }
                    }
                }
            }
        }
    }

    void FindAllAudioSources()
    {
        soundSource.Clear();
        effectSoundSource.Clear();
        HashSet<AudioSource> uniqueSources = new HashSet<AudioSource>();

        // sound 리스트의 자식 오브젝트들에서만 AudioSource를 찾음
        foreach (GameObject soundObj in sound)
        {
            if (soundObj != null)
            {
                foreach (Transform child in soundObj.transform)
                {
                    AudioSource[] sources = child.GetComponentsInChildren<AudioSource>();
                    foreach (AudioSource source in sources)
                    {
                        if (source != null && uniqueSources.Add(source))
                        {
                            effectSoundSource.Add(source); // AudioSource 리스트에 추가
                            if (!originalVolumes.ContainsKey(source))
                                originalVolumes[source] = source.volume;
                        }
                    }
                }
            }
        }
    }

    void UpdateAllAudioVolumes()
    {
        foreach (AudioSource source in effectSoundSource)
        {
            if (source != null)
            {
                source.volume = currentVolume;
            }
        }
    }

    int CountChildren(GameObject obj)
    {
        int count = 0;
        foreach (Transform child in obj.transform)
        {
            count++;
        }
        return count;
    }

    // 슬라이더 값이 바뀔 때
    void UpdateActiveEffectVolumes()
    {
        if (SFXslider != null)
        {
            float volume = SFXslider.value / 100f; // 0~100을 0~1로 정규화
            foreach (AudioSource source in effectSoundSource)
            {
                if (source != null && source.gameObject.activeInHierarchy)
                {
                    source.volume = volume;
                }
            }
        }
    }

    // EffectAudioSync 오브젝트 등록
    public void RegisterEffectAudioSync(GameObject obj)
    {
        if (obj != null)
        {
            effectAudioSyncObjects.Add(obj);
        }
    }

    // EffectAudioSync 오브젝트 등록 해제
    public void UnregisterEffectAudioSync(GameObject obj)
    {
        if (obj != null)
        {
            effectAudioSyncObjects.Remove(obj);
        }
    }

    /*
    // effect 오브젝트가 활성화될 때(풀에서 꺼낼 때)
    void OnEnable()
    {
        foreach (AudioSource source in GetComponentsInChildren<AudioSource>())
            source.volume = FindObjectOfType<SoundManager>().slider.value;
    }
    */
}