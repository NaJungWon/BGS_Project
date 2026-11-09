using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WizardBullet : MonoBehaviour
{
    public GameObject player;
    public float weaponDamage; //무기 데미지
    public float speed = 10f; //투사체 속도
    private float time = 0f; //투사체 생존 시간
    public float destroyTimer = 5f; //투사체 파괴 시간
    private Vector3 moveDirection; // 이동 방향 저장
    public bool isArousal; // 진화 여부

    public event Action<GameObject> OnProjectileDestroy;
    public event Action<Vector3> OnMonsterHitEffect;

    public void Initialize(GameObject playerObj, Vector3 direction, float damage, float projectileSpeed, float lifetime, bool isArousal)
    {
        player = playerObj;
        weaponDamage = damage;
        speed = projectileSpeed;
        destroyTimer = lifetime;
        moveDirection = direction.normalized;
        this.isArousal = isArousal;
        time = 0f;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;

        // 저장된 방향으로 이동
        transform.Translate(moveDirection * speed * Time.deltaTime);

        if(time >= destroyTimer)
        {
            OnProjectileDestroy?.Invoke(gameObject);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Monster"))
        {
            // MonsterStatus 컴포넌트 가져오기
            MonsterStatus monsterStatus = other.GetComponent<MonsterStatus>();
            if (monsterStatus != null)
            {
                // TakeDamage 메서드 호출
                monsterStatus.TakeDamage(weaponDamage, player);
                // 피격 이펙트 이벤트 발생
                OnMonsterHitEffect?.Invoke(other.transform.position);

                // 진화 상태일 때 몬스터 정지 효과 적용
                if (isArousal)
                {
                    MonsterSetting monsterSetting = monsterStatus.GetComponent<MonsterSetting>();
                    if (monsterSetting != null)
                    {
                        // 몬스터가 활성화 상태인지 확인
                        if (monsterStatus.gameObject.activeInHierarchy)
                        {
                            StartCoroutine(ApplyStopMovement(monsterStatus, monsterSetting));
                        }
                    }
                }
            }
        }
    }

    private IEnumerator ApplyStopMovement(MonsterStatus monsterStatus, MonsterSetting monsterSetting)
    {
        int stopCount = 0;
        
        while (stopCount < 3 && monsterStatus.gameObject.activeInHierarchy)
        {
            monsterSetting.StopMovement(0.5f * player.GetComponent<PlayerStatus>().duration);
            stopCount++;
            //Debug.Log($"{monsterStatus.name}의 정지 횟수: {stopCount}");
            
            // 정지 효과 적용 시 피격 이펙트 발생
            OnMonsterHitEffect?.Invoke(monsterStatus.transform.position);
            
            if (stopCount < 3)
            {
                yield return new WaitForSeconds(1f);
            }
        }
    }
}
