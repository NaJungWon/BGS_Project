using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Launcher_Bullet : MonoBehaviour
{
    public GameObject player;
    public float weaponDamage; // 총알의 공격력
    private float speed = 30f;  // 총알 속도

    private Vector3 startPoint;
    public Transform playerTransform;
    private Vector3 direction;
    private float maxDistance;
    
    private float fixedY; // y값 고정용

    public event Action<GameObject> OnProjectileDestroy;
    public event Action<Vector3> OnMonsterHitEffect; // 몬스터 피격 시 이펙트 위치 전달 이벤트

    public void Initialize(Transform playerTransform, Vector3 shootDirection, GameObject player, float weaponDamage, float attackRange)
    {
        this.player = player;
        this.weaponDamage = weaponDamage;
        this.playerTransform = playerTransform;
        startPoint = playerTransform.position;
        direction = shootDirection;
        fixedY = transform.position.y; // 부메랑 생성 시 y값 고정
        maxDistance = attackRange;

        // TrailRenderer 초기화
        TrailRenderer[] trails = GetComponentsInChildren<TrailRenderer>();
        foreach (TrailRenderer trail in trails)
        {
            trail.Clear();
        }
    }

    private void Start()
    {

    }

    private void Update()
    {
        // direction 방향으로 이동
        transform.position += direction * speed * Time.deltaTime;
        
        // 이동 방향으로 회전
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        // 최대 거리 체크
        float distanceTraveled = Vector3.Distance(startPoint, transform.position);
        if (distanceTraveled >= maxDistance)
        {
            OnProjectileDestroy?.Invoke(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 몬스터와 충돌했는지 확인
        if (other.CompareTag("Monster"))
        {
            MonsterStatus monsterStatus = other.GetComponent<MonsterStatus>();
            if (monsterStatus != null)
            {
                // 몬스터에게 데미지 적용
                monsterStatus.TakeDamage(weaponDamage, player);
                
                // 피격 이펙트 위치 전달
                OnMonsterHitEffect?.Invoke(other.transform.position);
            }

            // 총알 비활성화
            OnProjectileDestroy?.Invoke(gameObject);
        }
    }
}
