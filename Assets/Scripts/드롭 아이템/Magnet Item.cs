using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagnetItem : MonoBehaviour
{
    public GameObject magnetEffect;
    public List<GameObject> effectList = new List<GameObject>();
    private bool isMagnetActive = false;
    private GameObject player;

    private GameObject GetOrCreateMagnetEffect()
    {
        // 비활성화된 이펙트 찾기
        foreach (GameObject effect in effectList)
        {
            if (effect != null && !effect.activeInHierarchy)
            {
                return effect;
            }
        }

        // 비활성화된 이펙트가 없으면 새로 생성
        GameObject newEffect = Instantiate(magnetEffect);
        effectList.Add(newEffect);
        return newEffect;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isMagnetActive)
        {
            player = other.gameObject;
            isMagnetActive = true;

            // 이펙트 표시
            if (magnetEffect != null)
            {
                GameObject effect = GetOrCreateMagnetEffect();
                Vector3 effectPos = other.transform.position;
                effectPos.y = 1f;
                effect.transform.position = effectPos;
                effect.SetActive(true);
                StartCoroutine(DisableEffectAfterDelay(effect, 1f));
            }
            
            // EnemyManager에 자석 효과 활성화 요청
            EnemyManager.Instance.ActivateMagnetEffect(player);

            // 자석 아이템 삭제
            Destroy(gameObject);
        }
    }

    // 이펙트 비활성화
    private IEnumerator DisableEffectAfterDelay(GameObject effect, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (effect != null)
        {
            effect.SetActive(false);
        }
    }
}
