using UnityEngine;

public class EffectAudioSync : MonoBehaviour
{
    void OnEnable()
    {
        // SoundManager에 자신을 등록
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.RegisterEffectAudioSync(gameObject);
            
            // 즉시 볼륨 적용
            if (SoundManager.Instance.SFXslider != null)
            {
                float volume = SoundManager.Instance.SFXslider.value / 100f; // 0~100을 0~1로 정규화
                foreach (AudioSource source in GetComponentsInChildren<AudioSource>())
                    source.volume = volume;
            }
        }
    }

    void OnDisable()
    {
        // SoundManager에서 등록 해제
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.UnregisterEffectAudioSync(gameObject);
        }
    }
}