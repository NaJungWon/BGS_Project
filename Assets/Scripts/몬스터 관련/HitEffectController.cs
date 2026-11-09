using UnityEngine;
using System.Collections;

public class HitEffectController : MonoBehaviour
{
    public void PlayHitEffect(GameObject hitEffect, int hitSoundNumber)
    {
        if (hitEffect != null && hitSoundNumber < hitEffect.transform.childCount)
        {
            hitEffect.SetActive(true);
            ParticleSystem hitParticle = hitEffect.transform.GetChild(hitSoundNumber).GetComponent<ParticleSystem>();
            if (hitParticle != null)
            {
                // URP에서 파티클의 렌더링 우선순위 설정
                var renderer = hitParticle.GetComponent<ParticleSystemRenderer>();
                if (renderer != null)
                {
                    renderer.sortingOrder = 100; // 높은 값을 설정하여 항상 앞에 보이도록
                }

                hitParticle.Play();
                StartCoroutine(DisableHitEffectAfterParticle(hitEffect, hitParticle));
            }
        }
    }

    private IEnumerator DisableHitEffectAfterParticle(GameObject hitEffect, ParticleSystem particle)
    {
        while (particle.isPlaying)
        {
            yield return null;
        }
        hitEffect.SetActive(false);
    }
} 