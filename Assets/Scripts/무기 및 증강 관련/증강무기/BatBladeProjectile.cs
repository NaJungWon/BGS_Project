using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatBladeProjectile : MonoBehaviour
{
    public GameObject player;
    private Vector3 direction; // 발사체의 이동 방향
    private float speed; // 발사체 속도
    private float lifetime; // 발사체의 남은 생존 시간
    private int reflectionsRemaining; // 남은 반사 횟수
    public float damage; // 발사체의 공격력
    public bool isArousal; // 진화 여부
    public float timer;

    private BatBlade batBlade; // BatBlade 참조

    public event Action<GameObject> OnProjectileDestroy;
    public event Action<Vector3> OnMonsterHitEffect; // 몬스터 피격 시 이펙트 위치 전달 이벤트

    public void Initialize(GameObject player, Vector3 initialDirection, float initialSpeed, float initialLifetime, int maxReflections, float weaponDamage, bool isArousal)
    {
        this.player = player;
        this.damage = weaponDamage;
        this.isArousal = isArousal;
        direction = initialDirection.normalized; // 방향 벡터 정규화
        speed = initialSpeed;
        lifetime = initialLifetime;
        reflectionsRemaining = maxReflections;
        timer = 0f;
        batBlade = player.GetComponentInChildren<BatBlade>(); // BatBlade 컴포넌트 가져오기
    }

    private void Update()
    {
        // 발사체 이동
        transform.position += direction * speed * Time.deltaTime;

        // 생존 시간 감소
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            OnProjectileDestroy?.Invoke(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 몬스터와 충돌 처리
        if (other.CompareTag("Monster")) // "Monster" 태그가 붙은 오브젝트와 충돌
        {
            // 몬스터에게 데미지 부여
            MonsterStatus monster = other.GetComponent<MonsterStatus>();
            if (monster != null)
            {
                monster.TakeDamage(damage, player); // 데미지 전달
                // 피격 이펙트 위치 전달
                OnMonsterHitEffect?.Invoke(other.transform.position);
            }
        }
        // 벽과 충돌 처리
        if (other.CompareTag("Wall")) // "Wall" 태그가 붙은 오브젝트와 충돌
        {
            if (reflectionsRemaining > 0)
            {
                // 충돌 지점에서 Raycast로 반사 방향 계산
                RaycastHit hit;
                if (Physics.Raycast(transform.position, direction, out hit))
                {
                    Vector3 normal = hit.normal; // 충돌 표면의 법선 벡터

                    // 반사 방향 계산
                    direction = Vector3.Reflect(direction, normal);

                    // 충돌 지점에서 약간 떨어진 위치로 이동
                    transform.position = hit.point + normal * 0.1f;

                    // 반사 횟수 감소
                    reflectionsRemaining--;

                    // 진화된 발사체일 경우 데미지 증가
                    if (isArousal)
                    {
                        damage *= 1.5f;
                    }
                }
            }
            else
            {
                OnProjectileDestroy?.Invoke(gameObject);
            }
        }
    }
}
