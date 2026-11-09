using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class WhirlwindBlade : MonoBehaviour
{
    [Header("장착 플레이어")]
    public GameObject player; // 플레이어
    public PlayerStatus playerStatus;
    [Header("발사체")]
    public GameObject weaponPrefab; // 기본 궤도 무기 프리팹
    private GameObject currentWeapon; //현재 사용중인 무기
    [Header("무기 데미지/투사체 개수")]
    public float weaponDamage;
    public float weaponProjecTile = 1; // 기본 무기 수
    public float orbitRadius = 2.5f; // 궤도 반지름
    public float orbitSpeed = 100f; // 궤도 속도

    [Header("이펙트")]
    public GameObject hitEffect; // 몬스터 피격 이펙트
    private Queue<GameObject> hitEffects = new Queue<GameObject>(); // 몬스터 피격 이펙트 풀

    public List<GameObject> activeWeapons = new List<GameObject>(); // 활성화된 무기 리스트
    public List<GameObject> inactiveWeapons = new List<GameObject>(); // 비활성화된 무기 리스트
    private float currentAngle = 0f; // 현재 각도
    private float previousProjectTile; // 이전 투사체 개수를 저장할 변수

    [Header("진화 여부")]
    public bool isArousal; //능력 진화 여부

    void Start()
    {
        // player를 이 스크립트가 컴포넌트된 부모 오브젝트로 설정
        player = transform.parent.parent.gameObject;
        playerStatus = player.GetComponent<PlayerStatus>();
        previousProjectTile = weaponProjecTile; // 초기값 저장
        UpdateWeapons(weaponPrefab);
    }

    void Update()
    {
        // weaponProjectTile 값이 변경되었는지 확인
        float currentProjectTile = weaponProjecTile + playerStatus.projecTile;
        if (isArousal)
        {
            currentProjectTile = 2f + weaponProjecTile + playerStatus.projecTile; // 진화 시 투사체 개수 +2
            orbitSpeed = 300f; // 진화 시 궤도 속도 2배
        }

        if (previousProjectTile != currentProjectTile)
        {
            currentWeapon = weaponPrefab;
            UpdateWeapons(currentWeapon);
            previousProjectTile = currentProjectTile;
        }

        // 궤도 각도 업데이트
        currentAngle += orbitSpeed * Time.deltaTime;

        // 각 무기 위치 업데이트
        for (int i = 0; i < activeWeapons.Count; i++)
        {
            if (activeWeapons[i] == null) continue;

            // 각 무기 위치 계산
            float angleOffset = (360f / activeWeapons.Count) * i; // 각 무기 위치 오프셋
            float radian = (currentAngle + angleOffset) * Mathf.Deg2Rad;

            // 무기 위치 설정
            Vector3 weaponPosition = new Vector3(
                player.transform.position.x + Mathf.Cos(radian) * (orbitRadius * playerStatus.attackRange),
                player.transform.position.y,
                player.transform.position.z + Mathf.Sin(radian) * (orbitRadius * playerStatus.attackRange)
            );

            // 무기 위치 업데이트 (Y값만 1 증가)
            activeWeapons[i].transform.position = new Vector3(weaponPosition.x, weaponPosition.y + 1f, weaponPosition.z);

            // 무기가 플레이어를 바라보게 설정 (Y값도 1 증가)
            Vector3 lookAtPosition = new Vector3(
                player.transform.position.x,
                player.transform.position.y + 1f,
                player.transform.position.z
            );
            activeWeapons[i].transform.LookAt(lookAtPosition);

            // 무기 크기를 실시간으로 업데이트
            activeWeapons[i].transform.localScale = weaponPrefab.transform.localScale * playerStatus.attackRange;
        }
    }

    void UpdateWeapons(GameObject currentWeapon)
    {
        int totalWeapons = (int)(weaponProjecTile + playerStatus.projecTile);

        // 기존 활성화된 무기들을 비활성화 리스트로 이동
        foreach (var weapon in activeWeapons)
        {
            if (weapon != null)
            {
                // 이벤트 구독 해제
                WhirlwindBladeBullet bullet = weapon.GetComponent<WhirlwindBladeBullet>();
                if (bullet != null)
                {
                    bullet.OnMonsterHitEffect -= HandleMonsterHitEffect;
                }

                weapon.SetActive(false);
                inactiveWeapons.Add(weapon);
            }
        }
        activeWeapons.Clear();

        // 필요한 만큼의 무기가 있는지 확인하고 부족하면 생성
        while (inactiveWeapons.Count < totalWeapons)
        {
            GameObject newWeapon = Instantiate(currentWeapon, transform);
            // EnemyManager의 첫 번째 자식 오브젝트의 자식으로 설정
            newWeapon.transform.SetParent(EnemyManager.Instance.transform.GetChild(0));
            newWeapon.SetActive(false);
            inactiveWeapons.Add(newWeapon);
        }

        // 필요한 만큼의 무기를 활성화
        for (int i = 0; i < totalWeapons; i++)
        {
            if (inactiveWeapons.Count > 0)
            {
                GameObject weapon = inactiveWeapons[0];
                inactiveWeapons.RemoveAt(0);

                float angleOffset = (360f / totalWeapons) * i;
                float radian = angleOffset * Mathf.Deg2Rad;
                
                // 생성 위치 계산
                Vector3 spawnPosition = new Vector3(
                    player.transform.position.x + Mathf.Cos(radian) * (orbitRadius * playerStatus.attackRange),
                    player.transform.position.y + 1f,
                    player.transform.position.z + Mathf.Sin(radian) * (orbitRadius * playerStatus.attackRange)
                );

                weapon.transform.position = spawnPosition;
                weapon.transform.localScale = currentWeapon.transform.localScale * playerStatus.attackRange;
                
                WhirlwindBladeBullet bullet = weapon.GetComponent<WhirlwindBladeBullet>();
                if (bullet != null)
                {
                    bullet.player = player;
                    bullet.weaponDamage = weaponDamage;
                    bullet.isArousal = isArousal; // 진화 상태 전달
                    bullet.OnMonsterHitEffect += HandleMonsterHitEffect; // 이벤트 구독
                }

                weapon.SetActive(true);
                activeWeapons.Add(weapon);
            }
        }
    }

    // 피격 이펙트 풀링 관리
    private GameObject GetOrCreateHitEffect()
    {
        GameObject effectInstance = null;

        while (hitEffects.Count > 0 && effectInstance == null)
        {
            effectInstance = hitEffects.Dequeue();
        }

        // 비활성화된 이펙트가 없으면 새로 생성
        if (effectInstance == null)
        {
            effectInstance = Instantiate(hitEffect);
            // EnemyManager의 첫 번째 자식 오브젝트의 자식으로 설정
            effectInstance.transform.SetParent(EnemyManager.Instance.transform.GetChild(3));
        }

        effectInstance.SetActive(false);
        return effectInstance;
    }

    // 파티클 재생이 끝나면 이펙트를 비활성화하는 코루틴
    private IEnumerator DisableEffectAfterParticle(GameObject effect, ParticleSystem particleSystem)
    {
        // 파티클 재생
        particleSystem.Play();

        // 파티클이 재생 중인 동안 대기
        while (particleSystem.isPlaying)
        {
            yield return null;
        }

        // 파티클 재생이 끝나면 이펙트 비활성화
        effect.SetActive(false);
        hitEffects.Enqueue(effect);
    }

    // 몬스터 피격 시 이펙트 처리
    private void HandleMonsterHitEffect(Vector3 position)
    {
        // 피격 이펙트 재생
        GameObject hitEffectInstance = GetOrCreateHitEffect();
        if (hitEffectInstance != null)
        {
            // 이펙트 위치 설정 (y좌표 1로 설정)
            Vector3 effectPosition = position;
            effectPosition.y = 1f;
            hitEffectInstance.transform.position = effectPosition;
            
            hitEffectInstance.SetActive(true);
            
            // 파티클 시스템 가져오기
            ParticleSystem particleSystem = hitEffectInstance.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                // 파티클 재생이 끝나면 자동으로 비활성화하는 코루틴 시작
                StartCoroutine(DisableEffectAfterParticle(hitEffectInstance, particleSystem));
            }
        }
    }
}
