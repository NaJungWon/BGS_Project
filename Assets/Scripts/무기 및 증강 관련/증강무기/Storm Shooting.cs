using UnityEngine;
using System;

public class StormShooting : MonoBehaviour
{
    public GameObject player; // 투사체를 발사한 플레이어
    public float weaponDamage; // 무기 데미지
    public float speed = 15f; // 투사체 속도
    private float maxDistance; // 최대 이동 거리
    public float lifeTime = 5f; // 투사체 생존 시간(초)

    private Vector3 startPos; // 시작 위치
    private float currentLifeTime; // 현재 생존 시간

    public event Action<GameObject> OnProjectileDestroy;
    public event Action<Vector3> OnMonsterHitEffect; // 몬스터 피격 시 이펙트 위치 전달 이벤트

    void OnEnable()
    {
        startPos = transform.position;
        currentLifeTime = 0f;
    }

    // 투사체의 플레이어, 데미지 설정 함수
    public void Initialize(GameObject player, float damage, float attackRange, float speed)
    {
        this.player = player;
        this.weaponDamage = damage;
        this.maxDistance = attackRange;
        this.speed = speed;
    }

    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
        currentLifeTime += Time.deltaTime;

        if (Vector3.Distance(startPos, transform.position) >= maxDistance || currentLifeTime >= lifeTime)
        {
            OnProjectileDestroy?.Invoke(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 몬스터와 충돌 처리
        if (other.CompareTag("Monster")) // "Monster" 태그가 붙은 오브젝트와 충돌
        {
            // 몬스터에게 데미지 부여
            MonsterStatus monster = other.GetComponent<MonsterStatus>();
            if (monster != null)
            {
                monster.TakeDamage(weaponDamage, player); // 데미지 전달
                // 피격 이펙트 위치 전달
                OnMonsterHitEffect?.Invoke(monster.transform.position);
            }
            OnProjectileDestroy?.Invoke(gameObject);
        }
    }
}
