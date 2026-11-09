using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Boomerang : MonoBehaviour
{
    public GameObject player;
    public float speed = 10f;
    public float maxDistance = 10f;

    public float weaponDamage;
    private Vector3 startPoint;
    public Transform playerTransform;
    private Vector3 direction;
    private bool returning = false;
    private float fixedY; // y값 고정용
    private bool isArousal = false; // 진화 여부
    private bool isSplitBoomerang = false; // 분열 여부

    public event Action<GameObject> OnBoomerangReturn;
    public event Action<Vector3> OnMonsterHit; // 몬스터 피격 시 좌표 전달 이벤트
    public event Action<Vector3> OnMonsterHitEffect; // 몬스터 피격 시 이펙트 위치 전달 이벤트

    private HashSet<MonsterStatus> monstersInTrigger = new HashSet<MonsterStatus>(); // 트리거 영역 내 몬스터 저장
    private Coroutine damageCoroutine; // 데미지 코루틴 참조

    public void Initialize(Transform playerTransform, Vector3 shootDirection, GameObject player, float weaponDamage, float attackRange, bool isArousal, bool isSplitBoomerang)
    {
        this.player = player;
        this.weaponDamage = weaponDamage;
        this.playerTransform = playerTransform;
        startPoint = transform.position; // 시작점을 현재 위치로 설정
        direction = shootDirection;
        fixedY = transform.position.y; // 부메랑 생성 시 y값 고정
        maxDistance = attackRange;
        returning = false;
        this.isArousal = isArousal;
        this.isSplitBoomerang = isSplitBoomerang;
    }

    void Update()
    {
        if (!returning)
        {
            Vector3 nextPos = transform.position + direction * speed * Time.deltaTime;
            nextPos.y = fixedY; // y값 고정
            transform.position = nextPos;

            if (Vector3.Distance(startPoint, transform.position) >= maxDistance)
            {
                returning = true;
            }
        }
        else
        {
            Vector3 returnDir = (playerTransform.position - transform.position).normalized;
            Vector3 nextPos = transform.position + returnDir * speed * Time.deltaTime;
            transform.position = nextPos;

            if (Vector3.Distance(transform.position, playerTransform.position) < 0.5f)
            {
                if (isSplitBoomerang)
                {
                    // 분열 부메랑은 HandleSplitBoomerangReturn으로 처리
                    OnBoomerangReturn?.Invoke(gameObject);
                    //gameObject.SetActive(false);
                }
                else
                {
                    // 일반 부메랑은 HandleBoomerangReturn으로 처리
                    OnBoomerangReturn?.Invoke(gameObject);
                }
            }
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
                OnMonsterHitEffect?.Invoke(other.transform.position);
                
                // 트리거 영역 내 몬스터 추가
                monstersInTrigger.Add(monster);
                
                // 데미지 코루틴이 실행 중이 아니라면 시작
                if (damageCoroutine == null)
                {
                    damageCoroutine = StartCoroutine(DealDamageOverTime());
                }
                
                // 진화된 부메랑이고 분열 부메랑이 아닐때만 비활성화 및 이벤트 발생
                if (isArousal && !isSplitBoomerang)
                {
                    OnMonsterHit?.Invoke(transform.position); // 피격 좌표 전달
                    OnBoomerangReturn?.Invoke(gameObject);
                    gameObject.SetActive(false);
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Monster"))
        {
            MonsterStatus monster = other.GetComponent<MonsterStatus>();
            if (monster != null)
            {
                // 트리거 영역을 벗어난 몬스터 제거
                monstersInTrigger.Remove(monster);
                
                // 트리거 영역 내 몬스터가 없으면 코루틴 중지
                if (monstersInTrigger.Count == 0 && damageCoroutine != null)
                {
                    StopCoroutine(damageCoroutine);
                    damageCoroutine = null;
                }
            }
        }
    }

    private IEnumerator DealDamageOverTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f); // 1초 대기

            // 트리거 영역 내 모든 몬스터에게 데미지
            List<MonsterStatus> monstersToRemove = new List<MonsterStatus>(); // 제거할 몬스터 리스트

            foreach (MonsterStatus monster in monstersInTrigger)
            {
                if (monster != null && monster.gameObject.activeSelf)
                {
                    monster.TakeDamage(weaponDamage, player);
                    OnMonsterHitEffect?.Invoke(monster.transform.position);
                    // 진화된 부메랑이고 분열 부메랑이 아닐때만 비활성화 및 이벤트 발생
                    if (isArousal && !isSplitBoomerang)
                    {
                        OnMonsterHit?.Invoke(transform.position); // 피격 좌표 전달
                        OnBoomerangReturn?.Invoke(gameObject);
                        gameObject.SetActive(false);
                    }
                    
                }
                else
                {
                    // 비활성화된 몬스터는 제거 리스트에 추가
                    monstersToRemove.Add(monster);
                }
            }

            // 비활성화된 몬스터 제거
            foreach (MonsterStatus monster in monstersToRemove)
            {
                monstersInTrigger.Remove(monster);
            }

            // 모든 몬스터가 제거되었다면 코루틴 중지
            if (monstersInTrigger.Count == 0)
            {
                damageCoroutine = null;
                yield break;
            }
        }
    }
}
