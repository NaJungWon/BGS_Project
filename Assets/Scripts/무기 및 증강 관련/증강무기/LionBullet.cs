using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LionBullet : MonoBehaviour
{
    public GameObject player;
    public float weaponDamage; // 발사체의 공격력
    private HashSet<GameObject> hitMonsters = new HashSet<GameObject>(); // 충돌한 몬스터를 저장

    public event Action<GameObject> OnProjectileDestroy;
    public event Action<Vector3> OnMonsterHitEffect;

    void OnEnable()
    {
        hitMonsters.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        // 충돌한 오브젝트가 몬스터인지 확인
        if (other.CompareTag("Monster"))
        {
            // 이미 충돌한 적이 있는지 확인
            if (!hitMonsters.Contains(other.gameObject))
            {
                // 몬스터에 데미지 적용
                MonsterStatus monster = other.GetComponent<MonsterStatus>();
                CardInfo cardInfo = other.GetComponent<CardInfo>();
                if (monster != null)
                {
                    monster.TakeDamage(weaponDamage, player);
                    // 피격 이펙트 이벤트 발생
                    OnMonsterHitEffect?.Invoke(other.transform.position);
                    //Debug.Log($"Monster hit! Damage dealt: {weaponDamage}");
                }

                // 몬스터를 HashSet에 추가하여 중복 처리 방지
                hitMonsters.Add(other.gameObject);
            }
        }
    }
}
