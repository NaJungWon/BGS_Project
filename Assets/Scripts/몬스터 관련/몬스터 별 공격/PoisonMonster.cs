using UnityEngine;
using System.Collections;

public class PoisonMonster : MonoBehaviour
{
    public MonsterStatus monsterStatus;
    [Header("독 영역 설정")]
    public GameObject poisonAreaPrefab;
    public float poisonDuration = 5f;

    // 몬스터가 죽었을 때 "독 영역을" 생성하기 위한 변수
    private bool isDead = false;

    void Start()
    {
        monsterStatus = GetComponent<MonsterStatus>();
    }

    public void MarkAsDead()
    {
        isDead = true;
    }

    void OnDisable()
    {
        // 1. 에디터에서 비활성화될 때는 실행 안 함
        if (!Application.isPlaying) return;

        // 2. 몬스터가 죽었을 때만 실행
        if (isDead)
        {
            SpawnPoisonArea();
        }
    }

    private GameObject GetOrCreatePoisonArea()
    {
        Transform parent = EnemyManager.Instance.transform.GetChild(1);
        GameObject poisonArea = EnemyManager.Instance.GetPoisonAreaFromPool(poisonAreaPrefab, parent);
        if (poisonArea != null)
        {
            PoisonArea areaComponent = poisonArea.GetComponent<PoisonArea>();
            if (areaComponent != null)
            {
                areaComponent.onRelease = EnemyManager.Instance.ReturnPoisonArea;
            }
        }
        return poisonArea;
    }

    void SpawnPoisonArea()
    {
        if (poisonAreaPrefab != null)
        {
            Vector3 spawnPos = transform.position;
            spawnPos.y = 0f;

            GameObject poisonArea = GetOrCreatePoisonArea();
            if (poisonArea != null)
            {
                poisonArea.transform.position = spawnPos;
                poisonArea.transform.rotation = Quaternion.identity;
                
                PoisonArea poisonAreaScript = poisonArea.GetComponent<PoisonArea>();
                if (poisonAreaScript != null)
                {
                    poisonAreaScript.poisonDamage = monsterStatus.attackPower;
                    poisonAreaScript.damageInterval = 1.0f;
                    poisonAreaScript.lifeTime = poisonDuration;
                }
                
                poisonArea.SetActive(true);
            }

            Debug.Log("PoisonMonster 죽음과 동시에 독 영역 생성!");
        }
        else
        {
            Debug.LogWarning("PoisonMonster: poisonAreaPrefab이 할당되지 않았습니다.");
        }
    }
}
