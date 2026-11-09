using System;
using System.Collections;
using UnityEngine;

public class ExplosionZone : MonoBehaviour
{
    public float damage = 20; //데미지
    public float damageDelay = 0.25f; //데미지 적용 딜레이
    public float lifeTime = 3f; // 폭발 이펙트 생존 시간
    public Action<GameObject> onRelease;

    private void OnEnable()
    {
        // 활성화될 때 코루틴 시작
        StartCoroutine(ExplosionLifeCycle());
    }

    private IEnumerator ExplosionLifeCycle()
    {
        // 데미지 적용
        yield return StartCoroutine(ApplyDamageCoroutine());
        
        // 생존 시간 대기
        yield return new WaitForSeconds(lifeTime);
        
        ReleaseToPool();
    }

    private IEnumerator ApplyDamageCoroutine()
    {
        // 딜레이 대기
        yield return new WaitForSeconds(damageDelay);

        // 현재 오브젝트의 localScale을 반경으로 사용
        float radius = transform.localScale.x / 2f; // x축 scale을 반경으로 사용 (원형이므로 x, z 중 하나만 사용)
        
        Collider[] targets = Physics.OverlapSphere(transform.position, radius);
        foreach (Collider col in targets)
        {
            if (col.CompareTag("Player"))
            {
                Hitdecision hit = col.GetComponent<Hitdecision>();
                PlayerSystem system = col.GetComponent<PlayerSystem>();
                if (hit != null && system != null)
                {
                    hit.ApplyDamage(system.playerStatus, damage);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 현재 오브젝트의 localScale을 반경으로 사용하여 기즈모 표시
        float radius = transform.localScale.x / 2f;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    private void ReleaseToPool()
    {
        if (onRelease != null)
        {
            onRelease.Invoke(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
