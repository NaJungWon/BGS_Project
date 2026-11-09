using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 플레이어의 조준 방향을 마우스 위치에 맞춰 회전시키는 스크립트
public class AimingSetting : MonoBehaviour
{
    // 플레이어 이동 및 상태 제어 스크립트 참조
    public PlayerController player;

    // 화면 상 마우스 위치 저장 (pixel 좌표)
    public Vector3 mousePosition;

    public GameObject autoAimingRange;
    // 조준 방향을 표시할 자식 게임오브젝트
    public GameObject aimingdirection;
    // 조준 방향 회전 속도 (값이 클수록 빠르게 회전)
    public float aimrotateSpeed = 30;

    // Start is called before the first frame update
    void Start()
    {
        // PlayerController 컴포넌트 가져오기
        player = GetComponent<PlayerController>();
        // 4번째 자식(인덱스 3)을 조준 방향용 오브젝트로 설정
        aimingdirection = transform.GetChild(3).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        // 매 프레임 마우스 화면 좌표 갱신
        mousePosition = Input.mousePosition;

        // 플레이어가 자동 조준 상태가 아니면 직접 마우스를 향해 조준 회전 수행
        if (!player.AutoAiming)
        {
            RotateAimingdirectionToMouse();
        }
        else if(player.AutoAiming)
        {
            AutoAimingToNearestMonster();
        }
    }

    // 마우스 위치를 기준으로 aimingdirection 오브젝트를 회전시키는 메서드
    void RotateAimingdirectionToMouse()
    {
        // 카메라에서 마우스 좌표로 레이 발사
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        // y=0 평면(지면)을 생성
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayLength;

        // 레이가 평면과 충돌하면 충돌 지점까지의 거리 계산
        if (groundPlane.Raycast(ray, out rayLength))
        {
            // 마우스가 가리키는 월드 좌표
            Vector3 pointToLook = ray.GetPoint(rayLength);
            // 조준 오브젝트와 충돌 지점을 연결하는 방향 벡터
            Vector3 direction = pointToLook - aimingdirection.transform.position;
            // y축으로 회전하지 않도록 y 성분 고정
            direction.y = 0f;
            // 회전할 쿼터니언 계산
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            // 부드럽게 회전 적용
            aimingdirection.transform.rotation =
                Quaternion.Lerp(
                    aimingdirection.transform.rotation,
                    lookRotation,
                    aimrotateSpeed * Time.deltaTime
                );
        }
    }

    // 자동 조준 범위 내의 가장 가까운 몬스터를 조준하는 메서드
    void AutoAimingToNearestMonster()
    {
        // autoAimingRange의 콜라이더 가져오기
        Collider aimingRangeCollider = autoAimingRange.GetComponent<Collider>();
        if (aimingRangeCollider == null) return;

        // 자동 조준 범위 내의 몬스터 찾기
        Collider[] colliders = Physics.OverlapBox(
            aimingRangeCollider.bounds.center,
            aimingRangeCollider.bounds.extents,
            aimingRangeCollider.transform.rotation
        );

        GameObject closestMonster = null;
        float closestDistance = float.MaxValue;

        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Monster"))
            {
                float distance = Vector3.Distance(this.transform.position, collider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestMonster = collider.gameObject;
                }
            }
        }

        // 가장 가까운 몬스터가 있으면 해당 몬스터를 조준
        if (closestMonster != null)
        {
            Vector3 direction = closestMonster.transform.position - aimingdirection.transform.position;
            direction.y = 0f; // y축 회전 제한
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            aimingdirection.transform.rotation = Quaternion.Lerp(
                aimingdirection.transform.rotation,
                lookRotation,
                aimrotateSpeed * Time.deltaTime
            );
        }
    }

    // 화면상의 마우스 위치를 지면 위 월드 좌표(Vector3)로 변환하여 반환
    public Vector3 GetMouseWorldPosition()
    {
        // 카메라에서 마우스 좌표로 레이 발사
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayLength;

        // 레이가 지면과 충돌하면 해당 위치 반환
        if (groundPlane.Raycast(ray, out rayLength))
        {
            Vector3 worldPosition = ray.GetPoint(rayLength);
            // 캐릭터가 약간 떠 있는 높이에 맞춰 y값 고정
            worldPosition.y = 0.5f;
            return worldPosition;   
        }

        // 레이가 지면을 통과하지 못하면 (예외 처리) (0,0,0) 반환
        return Vector3.zero;
    }
}