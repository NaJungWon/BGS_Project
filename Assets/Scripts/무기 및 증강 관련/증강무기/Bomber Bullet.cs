using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BomberBullet : MonoBehaviour
{
    public GameObject player;
    public float weaponDamage;
    public List<MonsterStatus> monstersInRange = new List<MonsterStatus>();
    private bool isActive = false;
    public bool isArousal;
    
    public event Action<Vector3> OnMonsterHitEffect; // 몬스터 피격 시 이펙트 위치 전달 이벤트
    
    public void Initialize(GameObject player, float weaponDamage)
    {
        this.player = player;
        this.weaponDamage = weaponDamage;
    }

    void OnEnable()
    {
        isActive = true;
        // 활성화 시점에 트리거 내 모든 몬스터를 리스트에 저장
        monstersInRange.Clear();
        Collider[] colliders = Physics.OverlapSphere(transform.position, GetTriggerRadius());
        foreach (var col in colliders)
        {
            if (col.CompareTag("Monster"))
            {
                MonsterStatus monster = col.GetComponent<MonsterStatus>();
                if (monster != null && !monstersInRange.Contains(monster))
                {
                    monstersInRange.Add(monster);
                    // 피격 이펙트 위치 전달
                    OnMonsterHitEffect?.Invoke(monster.transform.position);
                }
            }
        }
        // 리스트 내 모든 몬스터에게 데미지 1회 적용
        foreach (var monster in monstersInRange)
        {
            if (monster != null)
            {
                monster.TakeDamage(weaponDamage, player);
                if(isArousal)
                {
                    monster.hitExtinction = true;
                }
            }
        }
    }

    void OnDisable()
    {
        isActive = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (isActive) return; // 활성화 중에는 리스트 변동 금지
        if (other.CompareTag("Monster"))
        {
            MonsterStatus monster = other.GetComponent<MonsterStatus>();
            if (monster != null && !monstersInRange.Contains(monster))
            {
                monstersInRange.Add(monster);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (isActive) return; // 활성화 중에는 리스트 변동 금지
        if (other.CompareTag("Monster"))
        {
            MonsterStatus monster = other.GetComponent<MonsterStatus>();
            if (monster != null && monstersInRange.Contains(monster))
            {
                monstersInRange.Remove(monster);
            }
        }
    }

    // 트리거 콜라이더의 반지름을 구하는 함수 (SphereCollider만 지원)
    float GetTriggerRadius()
    {
        SphereCollider sphere = GetComponent<SphereCollider>();
        if (sphere != null && sphere.isTrigger)
            return sphere.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
        return 0.5f; // 기본값
    }
}
