using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float projectileSpeed = 5f;
    public float damage;
    private Vector3 shootDirection;
    private float lifeTime = 5f;
    private float currentTime = 0f;
    private TrailRenderer[] trailRenderers;
    public Action<GameObject> onRelease;

    void Awake()
    {
        trailRenderers = GetComponentsInChildren<TrailRenderer>();
    }

    public void Setup(Vector3 direction, float attackPower)
    {
        shootDirection = direction;
        damage = attackPower;
        currentTime = 0f;
    }

    void OnEnable()
    {
        // 활성화될 때 TrailRenderer 초기화
        if (trailRenderers != null)
        {
            foreach (var trail in trailRenderers)
            {
                if (trail != null)
                {
                    trail.Clear();
                }
            }
        }
    }

    void OnDisable()
    {
        // 비활성화될 때 TrailRenderer 초기화
        if (trailRenderers != null)
        {
            foreach (var trail in trailRenderers)
            {
                if (trail != null)
                {
                    trail.Clear();
                }
            }
        }
    }

    void Update()
    {
        // 발사 방향으로 지속적으로 이동
        transform.Translate(shootDirection * projectileSpeed * Time.deltaTime, Space.World);

        // 생존 시간 체크
        currentTime += Time.deltaTime;
        if (currentTime >= lifeTime)
        {
            ReleaseToPool();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStatus playerStatus = other.GetComponent<PlayerStatus>();
            if (playerStatus != null)
            {
                // Hitdecision의 ApplyDamage 메서드 호출
                Hitdecision hitdecision = other.GetComponent<Hitdecision>();
                if (hitdecision != null)
                {
                    hitdecision.ApplyDamage(playerStatus, damage); // damage는 projectile의 damage
                }
            }
            ReleaseToPool(); // 발사체 비활성화
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