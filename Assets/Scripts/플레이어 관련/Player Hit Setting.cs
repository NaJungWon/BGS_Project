using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitSetting : MonoBehaviour
{
    public List<GameObject> bodyMaterial = new List<GameObject>();
    private Coroutine currentHitEffect;

    public void hitPlayer()
    {
        // 이전 효과가 실행 중이면 중지
        if (currentHitEffect != null)
        {
            StopCoroutine(currentHitEffect);
        }
        // 새로운 효과 시작
        currentHitEffect = StartCoroutine(HitEffect());
    }

    private IEnumerator HitEffect()
    {
        // 모든 bodyMaterial의 Material을 빨간색으로 설정
        foreach (GameObject obj in bodyMaterial)
        {
            if (obj != null)
            {
                Material mat = obj.GetComponent<Renderer>().material;
                if (mat != null)
                {
                    mat.SetColor("_EmissionColor", new Color(1f, 0f, 0f));
                }
            }
        }
        this.gameObject.GetComponent<AudioSource>().Play();

        float elapsedTime = 0f;
        float duration = 1f;

        // 1초에 걸쳐 Emission을 검은색으로 변화
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            
            foreach (GameObject obj in bodyMaterial)
            {
                if (obj != null)
                {
                    Material mat = obj.GetComponent<Renderer>().material;
                    if (mat != null)
                    {
                        Color currentColor = Color.Lerp(new Color(1f, 0f, 0f), new Color(0f, 0f, 0f), t);
                        mat.SetColor("_EmissionColor", currentColor);
                    }
                }
            }

            yield return null;
        }

        // 최종적으로 Emission을 검은색으로 설정
        foreach (GameObject obj in bodyMaterial)
        {
            if (obj != null)
            {
                Material mat = obj.GetComponent<Renderer>().material;
                if (mat != null)
                {
                    mat.SetColor("_EmissionColor", new Color(0f, 0f, 0f));
                }
            }
        }

        currentHitEffect = null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
