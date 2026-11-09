using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonArea : MonoBehaviour
{
    public float poisonDamage = 5; // 독 영역의 데미지량
    public float damageInterval = 1.0f; // 데미지를 주는 최대 간격 (1초)
    public float lifeTime = 5f; // 독 영역 생존 시간
    public Action<GameObject> onRelease;

    private void OnEnable()
    {
        // 활성화될 때 생명주기 코루틴 시작
        StartCoroutine(LifeCycle());
    }

    private IEnumerator LifeCycle()
    {
        // 생존 시간 대기
        yield return new WaitForSeconds(lifeTime);
        
        ReleaseToPool();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("플레이어가 독 영역에 진입");
            StartCoroutine(ApplyPoisonDamage(other));
        }
    }

    private IEnumerator ApplyPoisonDamage(Collider player)
    {
        PlayerStatus playerStatus = player.GetComponent<PlayerStatus>();
        Hitdecision hitdecision = player.GetComponent<Hitdecision>();

        if (playerStatus != null && hitdecision != null)
        {
            while (player.bounds.Intersects(GetComponent<Collider>().bounds))
            {
                Debug.Log($"[PoisonArea] 독 데미지로 {poisonDamage} 데미지!");
                hitdecision.ApplyDamage(playerStatus, poisonDamage); 

                yield return new WaitForSeconds(damageInterval);
            }

            Debug.Log("플레이어가 독 영역을 벗어남");
        }
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
