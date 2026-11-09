using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bs_Breath : MonoBehaviour
{
    public Transform mouthTransform; // 입술 기준 위치 (입)
    public GameObject breathVFXPrefab; // 브레스 이펙트 프리팹
    public BossPattern bossPattern;
    public float breathDuration = 3f;
    public float damageInterval = 0.5f;
    public float damagePerTick = 100f;
    public Vector3 boxSize = new Vector3(3f, 3f, 20f);

    private GameObject activeBreathVFX;
    private Coroutine damageCoroutine;
    private ParticleSystem breathParticleSystem;

    private void Start()
    {
        bossPattern = GetComponent<BossPattern>();
    }

    public void BeginBreath()
    {
        activeBreathVFX = Instantiate(breathVFXPrefab, mouthTransform.position, mouthTransform.rotation, mouthTransform);
        breathParticleSystem = activeBreathVFX.GetComponent<ParticleSystem>();
        damageCoroutine = StartCoroutine(DoDamage());
    }

    public void EndBreath()
    {
        if (breathParticleSystem != null)
        {
            var emission = breathParticleSystem.emission;
            emission.enabled = false; // 파티클만 중지
            Destroy(activeBreathVFX, breathParticleSystem.main.startLifetime.constantMax); // 남은 파티클이 사라질 때 Destroy
        }
        else if (activeBreathVFX)
        {
            Destroy(activeBreathVFX);
        }
        if (damageCoroutine != null) StopCoroutine(damageCoroutine);

        if (bossPattern != null && bossPattern.animator != null)
            StartCoroutine(RestartBreathPatternAfterDelay(8f));
    }

    private IEnumerator DoDamage()
    {
        float timer = 0f;

        while (timer < breathDuration)
        {
            Vector3 boxCenter = mouthTransform.position + mouthTransform.forward * (boxSize.z / 2);
            Collider[] hits = Physics.OverlapBox(boxCenter, boxSize / 2, mouthTransform.rotation);

            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    PlayerStatus playerStatus = hit.GetComponent<PlayerStatus>();
                    Hitdecision hitdecision = hit.GetComponent<Hitdecision>();

                    // 데미지: 플레이어에게 데미지
                    if (hitdecision != null && playerStatus != null)
                    {
                        hitdecision.ApplyDamage(playerStatus, damagePerTick);

                    }
                }
            }

            yield return new WaitForSeconds(damageInterval);
            timer += damageInterval;
        }

        EndBreath(); // 브레스 종료
    }

    private IEnumerator RestartBreathPatternAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (bossPattern == null || bossPattern.bossSetting == null || 
            !bossPattern.bossSetting.playerInRoom || bossPattern.animator == null) yield break;
        bossPattern.animator.SetTrigger("StartBreath");
    }

    private void OnDrawGizmosSelected()
    {
        if (mouthTransform)
        {
            Gizmos.color = Color.red;
            Gizmos.matrix = Matrix4x4.TRS(mouthTransform.position + mouthTransform.forward * (boxSize.z / 2), mouthTransform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, boxSize);
        }
    }

}
