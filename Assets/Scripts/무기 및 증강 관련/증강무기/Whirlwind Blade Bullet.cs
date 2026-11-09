using UnityEngine;
using System;

public class WhirlwindBladeBullet : MonoBehaviour
{
    public GameObject player;  // 플레이어
    public float weaponDamage; // 공격력
    public bool isArousal; // 진화 여부
    public float pushForce = 0.2f; // 밀어내는 힘

    public event Action<Vector3> OnMonsterHitEffect; // 몬스터 피격 시 이펙트 위치 전달 이벤트

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Monster"))  // 몬스터와 충돌 시
        {
            MonsterStatus enemy = other.GetComponent<MonsterStatus>();
            if (enemy != null)
            {
                enemy.TakeDamage(weaponDamage, player);  // 적에게 피해 주기
                
                // 진화된 무기일 경우 몬스터를 밀어냄
                if (isArousal)
                {
                    Rigidbody monsterRigidbody = other.GetComponent<Rigidbody>();
                    if (monsterRigidbody != null)
                    {
                        // 몬스터에서 플레이어 방향의 반대 방향으로 밀어내기
                        Vector3 pushDirection = (other.transform.position - player.transform.position).normalized;
                        monsterRigidbody.AddForce(pushDirection * pushForce, ForceMode.Impulse);
                    }
                }
                
                // 피격 이펙트 위치 전달
                OnMonsterHitEffect?.Invoke(other.transform.position);
            }
        }
    }
}