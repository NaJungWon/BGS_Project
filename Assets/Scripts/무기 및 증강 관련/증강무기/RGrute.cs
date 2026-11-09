using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RGrute : MonoBehaviour
{
    public GameObject player;
    public float weaponDamage = 10; // 공격력
    public bool isArousal; // 진화 여부
    
    public float orbitRadius = 3f;
    public float orbitSpeed = 90f;

    public float attackDistance = 7f;     // 공격 거리
    public float attackAngle = 60f;       // 공격 각도 (도 단위)

    public float attackDuration = 6f;   // 공격 지속 시간
    public float attackInterval = 0.5f;   // 공격 간격
    public float attackDelay = 0.5f;       // 공격 딜레이

    private float currentAngle = 0f;
    private Dictionary<MonsterStatus, float> monsterDamageMultipliers = new Dictionary<MonsterStatus, float>(); // 몬스터별 데미지 증가 배율

    // 피격 이펙트 이벤트
    public event Action<Vector3> OnMonsterHitEffect;

    private void Start()
    {
        // Start에서 player 찾기 제거 (RabbitGun에서 설정)
    }

    void Update()
    {
        if (player == null) return;
        attackDelay -= Time.deltaTime;

        // 1. 가장 가까운 타겟을 찾아 각도 계산
        Transform nearest = FindNearestTarget();
        if (nearest != null)
        {
            float targetAngle = GetAngleAroundPlayer(nearest.position);
            float angleDiff = Mathf.DeltaAngle(currentAngle, targetAngle);
            
            // 회전 속도를 각도 차이에 비례하여 조절
            float rotationSpeed = Mathf.Abs(angleDiff) * 2f; // 각도 차이가 클수록 더 빠르게 회전
            rotationSpeed = Mathf.Clamp(rotationSpeed, 30f, orbitSpeed); // 최소 30도, 최대 orbitSpeed로 제한
            
            float direction = Mathf.Sign(angleDiff);
            currentAngle += direction * rotationSpeed * Time.deltaTime;
        }

        // 2. 위치 및 회전 갱신 (항상 플레이어 주변)
        Vector3 offset = new Vector3(Mathf.Cos(currentAngle * Mathf.Deg2Rad), 0.5f, Mathf.Sin(currentAngle * Mathf.Deg2Rad)) * orbitRadius;
        transform.position = player.transform.position + offset;

        // 플레이어의 Y축 위치를 1로 맞춰서 회전
        Vector3 adjustedPlayerPosition = player.transform.position + new Vector3(0, 1f, 0);
        Vector3 dirToPlayer = (adjustedPlayerPosition - transform.position).normalized;
        
        // 부드러운 회전을 위해 Lerp 사용
        Quaternion targetRotation = Quaternion.LookRotation(-dirToPlayer);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10f);

        if (attackDelay <= 0)
        {
            // 3. 원뿔 범위 내에 있는 적 탐색 및 데미지
            DealDamageInCone();
            attackDelay = 0.5f;
        }
    }

    void DealDamageInCone()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Monster");

        foreach (GameObject target in targets)
        {
            Vector3 toTarget = target.transform.position - transform.position;
            float distance = toTarget.magnitude;

            if (distance <= attackDistance)
            {
                // Y축을 무시한 평면 방향 계산
                Vector3 forwardFlat = new Vector3(transform.forward.x, 0, transform.forward.z).normalized; // Y값 제거

                float angle = Vector3.Angle(forwardFlat, toTarget.normalized);
                if (angle <= attackAngle / 2f)
                {
                    // 타겟의 MonsterStatus 컴포넌트에 데미지 전달
                    MonsterStatus monsterStatus = target.GetComponent<MonsterStatus>();
                    if (monsterStatus != null)
                    {
                        float finalDamage = weaponDamage;
                        
                        // 진화 상태일 때 데미지 증가 로직 적용
                        if (isArousal)
                        {
                            if (!monsterDamageMultipliers.ContainsKey(monsterStatus))
                            {
                                monsterDamageMultipliers[monsterStatus] = 1.0f;
                            }
                            
                            // 데미지 배율 증가 (매 타격마다 20%씩 증가)
                            monsterDamageMultipliers[monsterStatus] += 0.2f;
                            finalDamage *= monsterDamageMultipliers[monsterStatus];
                            
                            //Debug.Log($"{target.name}의 현재 데미지 배율: {monsterDamageMultipliers[monsterStatus]:F1}x");
                        }
                        
                        monsterStatus.TakeDamage(finalDamage, player);
                        OnMonsterHitEffect?.Invoke(target.transform.position);
                        //Debug.Log($"{target.name} 에게 {finalDamage:F1} 데미지!");
                    }
                    else
                    {
                        //Debug.LogWarning($"{target.name}에 MonsterStatus 컴포넌트가 없습니다.");
                    }
                }
            }
        }
    }

    Transform FindNearestTarget()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Monster");
        Transform nearest = null;
        float minDist = Mathf.Infinity;

        foreach (GameObject target in targets)
        {
            float dist = Vector3.Distance(player.transform.position, target.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = target.transform;
            }
        }
        return nearest;
    }

    float GetAngleAroundPlayer(Vector3 worldPosition)
    {
        Vector3 dir = (worldPosition - player.transform.position).normalized;
        float angle = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;
        return angle;
    }

    private void OnDrawGizmos()
    {
        if (player == null) return;

        // 공격 범위를 시각적으로 그리기 위한 코드
        Gizmos.color = new Color(1, 0, 0, 0.3f); // 반투명 빨간색

        // Y축을 0.5로 맞춘 현재 위치
        Vector3 gizmoPosition = new Vector3(transform.position.x, 0.5f, transform.position.z);

        // Y축을 무시한 정면 방향
        Vector3 forward = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;

        // 정면 방향
        Gizmos.DrawRay(gizmoPosition, forward * attackDistance);

        // 원뿔의 왼쪽, 오른쪽 경계선
        Quaternion leftRotation = Quaternion.Euler(0, -attackAngle / 2f, 0);
        Quaternion rightRotation = Quaternion.Euler(0, attackAngle / 2f, 0);

        Vector3 leftDirection = leftRotation * forward;
        Vector3 rightDirection = rightRotation * forward;

        Gizmos.DrawRay(gizmoPosition, leftDirection * attackDistance);
        Gizmos.DrawRay(gizmoPosition, rightDirection * attackDistance);

        // 원뿔의 호를 그리기
        int segments = 30; // 호를 그릴 세그먼트 수
        float angleStep = attackAngle / segments;
        Vector3 previousPoint = gizmoPosition + (leftDirection * attackDistance);

        for (int i = 1; i <= segments; i++)
        {
            Quaternion segmentRotation = Quaternion.Euler(0, -attackAngle / 2f + angleStep * i, 0);
            Vector3 currentPoint = gizmoPosition + (segmentRotation * forward * attackDistance);

            Gizmos.DrawLine(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }
    }

    private void OnDisable()
    {
        // 오브젝트가 비활성화될 때 데미지 배율 초기화
        monsterDamageMultipliers.Clear();
    }
}
