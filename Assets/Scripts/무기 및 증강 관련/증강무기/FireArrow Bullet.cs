using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireArrowBullet : MonoBehaviour
{
    public GameObject player;
    public float weaponDamage; //데미지
    public float groundEffectDamage; //장판 데미지
    public float groundEffectDuration; //장판 지속시간
    public float damageInterval; //장판 데미지 적용 간격
    public float arrowLifetime; // 화살 수명

    public List<MonsterStatus> monstersInRange = new List<MonsterStatus>();
    private Coroutine groundEffectCoroutine;
    private Coroutine lifetimeCoroutine;
    private bool isActive = false;
    private float timer = 0f;

    public event Action<GameObject> OnArrowDestroy;
    public event Action<Vector3> OnMonsterHitEffect; // 몬스터 피격 시 이펙트 위치 전달 이벤트

    public void Initialize(GameObject player, float weaponDamage, float groundEffectDamage, float weaponDuration, float damageInterval)
    {
        this.player = player;
        this.weaponDamage = weaponDamage;
        this.groundEffectDamage = groundEffectDamage;
        this.groundEffectDuration = weaponDuration;
        this.damageInterval = damageInterval;
    }

    public void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 1f && isActive)
        {
            this.gameObject.transform.GetChild(1).gameObject.GetComponent<AudioSource>().Play();
            timer = 0f;
        }
    }

    void OnEnable()
    {
        if (player == null) return;  // player가 null이면 실행하지 않음
        
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
                    monster.TakeDamage(weaponDamage, player);
                    OnMonsterHitEffect?.Invoke(monster.transform.position);
                    // 데미지를 받은 후 비활성화되었는지 확인
                    if (!monster.gameObject.activeInHierarchy)
                    {
                        monstersInRange.Remove(monster);
                    }
                }
            }
        }
        // 지속 데미지 코루틴 시작
        groundEffectCoroutine = StartCoroutine(GroundEffectCoroutine());
    }

    void OnDisable()
    {
        isActive = false;
        timer = 0f;
        if (groundEffectCoroutine != null)
            StopCoroutine(groundEffectCoroutine);
        monstersInRange.Clear();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;
        if (other.CompareTag("Monster"))
        {
            MonsterStatus monster = other.GetComponent<MonsterStatus>();
            if (monster != null && !monstersInRange.Contains(monster))
            {
                monstersInRange.Add(monster);
                OnMonsterHitEffect?.Invoke(monster.transform.position);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!isActive) return;
        if (other.CompareTag("Monster"))
        {
            MonsterStatus monster = other.GetComponent<MonsterStatus>();
            if (monster != null && monstersInRange.Contains(monster))
            {
                monstersInRange.Remove(monster);
            }
        }
    }

    IEnumerator GroundEffectCoroutine()
    {
        float elapsedTime = 0f;
        while (elapsedTime < groundEffectDuration)
        {
            // 리스트 복사본을 순회하며 비활성화된 몬스터는 리스트에서 삭제
            for (int i = monstersInRange.Count - 1; i >= 0; i--)
            {
                var monster = monstersInRange[i];
                if (monster == null || !monster.gameObject.activeInHierarchy)
                {
                    monstersInRange.RemoveAt(i);
                    continue;
                }
                monster.TakeDamage(groundEffectDamage, player);
                OnMonsterHitEffect?.Invoke(monster.transform.position);
                // 데미지를 받은 후 비활성화되었는지 확인
                if (!monster.gameObject.activeInHierarchy)
                {
                    monstersInRange.RemoveAt(i);
                }
            }
            elapsedTime += damageInterval;
            yield return new WaitForSeconds(damageInterval);
        }
        OnArrowDestroy?.Invoke(gameObject);
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
