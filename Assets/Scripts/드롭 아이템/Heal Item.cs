using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealItem : MonoBehaviour
{
    public GameObject healEffect;
    private float healPercentage = 0.2f; // 20% 회복
    public List<GameObject> effectList = new List<GameObject>();

    private GameObject GetOrCreateHealEffect()
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
        GameObject newEffect = Instantiate(healEffect);
        effectList.Add(newEffect);
        return newEffect;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStatus playerStatus = other.GetComponent<PlayerStatus>();
            if (playerStatus != null)
            {
                // 최대 체력의 20%만큼 회복
                float healAmount = playerStatus.maxHealth * healPercentage;
                playerStatus.currentHealth = Mathf.Min(playerStatus.currentHealth + healAmount, playerStatus.maxHealth);

                // 회복 이펙트 표시 (풀링 방식)
                if (healEffect != null)
                {
                    GameObject effect = GetOrCreateHealEffect();
                    Vector3 effectPos = other.transform.position;
                    effectPos.y = 1f;
                    effect.transform.position = effectPos;
                    effect.SetActive(true);
                    StartCoroutine(DisableEffectAfterDelay(effect, 1f));
                }

                // 아이템 삭제
                Destroy(gameObject);
            }
        }
    }

    private IEnumerator DisableEffectAfterDelay(GameObject effect, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (effect != null)
        {
            effect.SetActive(false);
        }
    }
}
