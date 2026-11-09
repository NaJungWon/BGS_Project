using UnityEngine;
using System.Collections.Generic;
using System;

public class ProjectileBounce : MonoBehaviour
{
    public GameObject player; // 투사체를 발사한 플레이어
    public float weaponDamage; // 무기 데미지
    public float speed = 10f; // 투사체 속도
    public int maxBounces = 3; // 최대 튕기는 횟수
    public float bounceRange = 5f; // 튕길 수 있는 최대 거리
    public bool isArousal; // 진화 여부

    private int currentBounces = 0; // 현재 튕긴 횟수
    private GameObject currentTarget; // 현재 타겟
    private List<GameObject> hitEnemies = new List<GameObject>(); // 이미 맞은 적 리스트

    public event Action<GameObject> OnProjectileDestroy;
    public event Action<Vector3> OnMonsterHitEffect; // 몬스터 피격 시 이펙트 위치 전달 이벤트

    // 투사체의 타겟, 플레이어, 데미지 설정 함수
    public void DfSetting(Transform target, GameObject player, float damage, float maxBounces, float bounceRange, bool isArousal)
    {
        currentTarget = target.gameObject;
        this.player = player;
        weaponDamage = damage;
        this.maxBounces = (int)maxBounces + 2;
        this.bounceRange = bounceRange;
        this.isArousal = isArousal;
        currentBounces = 0;
        hitEnemies.Clear();
    }

    void Update()
    {
        // 타겟이 존재하고 활성화된 경우
        if (currentTarget != null && currentTarget.activeInHierarchy)
        {
            Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
            
            // 발사체의 정면 방향을 타겟 방향으로 설정
            transform.forward = direction;

            if (Vector3.Distance(transform.position, currentTarget.transform.position) < 0.3f)
            {
                OnHit(currentTarget); // 타겟에 도달하면 OnHit 실행
            }
            else if (!currentTarget.activeInHierarchy) // 타겟이 비활성화된 경우
            {
                return; // 즉시 반환
            }
        }
        // 타겟이 없거나 비활성화된 경우
        else
        {
            // 다음 타겟을 찾아보기
            GameObject nextTarget = FindNextTarget(transform.position);
            if (nextTarget != null)
            {
                currentTarget = nextTarget;
            }
            else
            {
                OnProjectileDestroy?.Invoke(gameObject);
                return;
            }
        }

        // 안전장치: 너무 오래 살아있는 발사체 제거
        if (currentBounces > 100)
        {
            OnProjectileDestroy?.Invoke(gameObject);
            return;
        }
    }

    // 적을 맞췄을 때 실행되는 함수
    void OnHit(GameObject enemy)
    {
        // 적에게 데미지 적용
        enemy.GetComponent<MonsterStatus>()?.TakeDamage(weaponDamage, player);

        // 피격 이펙트 위치 전달
        OnMonsterHitEffect?.Invoke(enemy.transform.position);

        hitEnemies.Add(enemy); // 맞은 적 리스트에 추가
        currentBounces++;

        // 진화된 발사체가 아닌 경우에만 최대 튕김 횟수 체크
        if (!isArousal && currentBounces >= maxBounces)
        {
            OnProjectileDestroy?.Invoke(gameObject);
            return;
        }

        GameObject nextTarget = FindNextTarget(enemy.transform.position); // 다음 타겟 탐색

        if (nextTarget != null)
        {
            currentTarget = nextTarget;
        }
        else
        {
            OnProjectileDestroy?.Invoke(gameObject);
        }
    }

    // EnemyManager를 활용하여 다음으로 튕길 적을 찾는 함수
    GameObject FindNextTarget(Vector3 fromPosition)
    {
        GameObject next = null;
        float minDist = Mathf.Infinity;

        foreach (GameObject enemy in EnemyManager.Instance.enemies)
        {
            if (enemy == null || !enemy.activeInHierarchy) continue;
            if (hitEnemies.Contains(enemy)) continue; // 이미 맞은 적은 제외

            float dist = Vector3.Distance(fromPosition, enemy.transform.position);
            if (dist <= bounceRange && dist < minDist)
            {
                minDist = dist;
                next = enemy;
            }
        }

        return next;
    }
}
