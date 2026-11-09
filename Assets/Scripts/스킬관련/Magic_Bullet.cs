using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Magic_Bullet : MonoBehaviour
{
    public GameObject player;
    public float projectileSpeed = 5.0f;
    public float eSkillDamage;
    public float explosionRadius = 5f;
    public float hitRadius = 2f;
    public GameObject explosionEffect;

    private Vector3 shootDirection;
    private bool hasExploded = false; // 중복 방지
    private float lifeTime = 5f; // 수명
    private float currentLifeTime = 0f; // 현재 경과 시간

    public event Action<Vector3> OnMonsterHitEffect;

    public void Initialize(Vector3 direction, float damage, GameObject player)
    {
        this.shootDirection = direction;
        this.eSkillDamage = damage;
        this.player = player;
        hasExploded = false;
        currentLifeTime = 0f;
        //Debug.Log("Magic_Bullet Initialized: Direction = " + direction + ", Damage = " + eSkillDamage);
    }

    void OnEnable()
    {
        hasExploded = false;
        currentLifeTime = 0f;
    }

    void Update()
    {
        if (hasExploded) return;

        currentLifeTime += Time.deltaTime;
        if (currentLifeTime >= lifeTime)
        {
            gameObject.SetActive(false);
            return;
        }

        transform.Translate(shootDirection * projectileSpeed * Time.deltaTime, Space.World);

        // 1. 몬스터 리스트에서 hitRadius 이내에 있는 몬스터가 있는지 검사
        var enemyList = EnemyManager.Instance != null ? EnemyManager.Instance.GetEnemyList() : null;
        if (enemyList != null)
        {
            var enemyArray = enemyList.ToArray();
            foreach (var enemyObj in enemyArray)
            {
                if (enemyObj == null || !enemyObj.activeInHierarchy) continue;
                float dist = Vector3.Distance(transform.position, enemyObj.transform.position);
                if (dist <= hitRadius)
                {
                    // 2. 폭발 발생: explosionRadius 이내의 모든 몬스터에게 데미지
                    // === 폭발 이펙트 생성 ===
                    if (explosionEffect != null)
                    {
                        GameObject effect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
                        ParticleSystem ps = effect.GetComponent<ParticleSystem>();
                        if (ps != null)
                        {
                            Destroy(effect, ps.main.duration + ps.main.startLifetime.constantMax);
                        }
                        else
                        {
                            Destroy(effect, 1f); // 파티클 없으면 1초 후 삭제
                        }
                    }
                    foreach (var targetObj in enemyArray)
                    {
                        if (targetObj == null || !targetObj.activeInHierarchy) continue;
                        float explosionDist = Vector3.Distance(transform.position, targetObj.transform.position);
                        if (explosionDist <= explosionRadius)
                        {
                            MonsterStatus monsterStatus = targetObj.GetComponent<MonsterStatus>();
                            if (monsterStatus != null)
                            {
                                monsterStatus.TakeDamage(eSkillDamage, player);
                                // 피격 이펙트 이벤트 발생
                                OnMonsterHitEffect?.Invoke(targetObj.transform.position);
                                Debug.Log("폭발 범위 몬스터에게 " + eSkillDamage + " 데미지를 줌: " + targetObj.name);
                            }
                        }
                    }
                    hasExploded = true;
                    break; // 한 번만 폭발
                }
            }
        }

        if (hasExploded)
        {
            gameObject.SetActive(false);
        }
    }
    void OnDrawGizmos()
    {
        // 투사체의 닿는 범위(hitRadius) - 빨간색
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hitRadius);

        // 투사체의 폭발 범위(explosionRadius) - 노란색
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
