using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.AI;

public class SplittingMonster : MonoBehaviour
{
    public MonsterSpawn monsterSpawn;
    public MonsterStatus monsterStatus;
    public MonsterSetting monsterSetting;

    public int minSplitCount = 2; // 최소 분열 개수
    public int maxSplitCount = 4; // 최대 분열 개수
    public float spawnRadius = 1.5f; // 생성 위치 범위
    //[Header("분열 몬스터 설정")]
    //public MonsterSpawn.MonsterSpawnData.MonsterType childMonsterType = MonsterSpawn.MonsterSpawnData.MonsterType.Elite;
    //public int childMonsterIndex = 0;


    void Start()
    {
        monsterStatus = GetComponent<MonsterStatus>();
        monsterSetting = GetComponent<MonsterSetting>();
        monsterSpawn = monsterSetting.ground.transform.parent.GetComponent<MonsterSpawn>();
    }

    public void SpawnChild()
    {
        //Debug.Log("큰 몬스터 죽음! 작은 몬스터 생성 중...");

        // 랜덤한 개수만큼 활성화
        int spawnCount = Random.Range(minSplitCount, maxSplitCount + 1);
        
        for (int i = 0; i < spawnCount; i++)
        {
            // 랜덤한 위치 계산
            Vector3 spawnPosition = transform.position + (Vector3)Random.insideUnitCircle * spawnRadius;
            spawnPosition.y = transform.position.y;

            // NavMesh 위의 위치 찾기
            NavMeshHit hit;
            if (NavMesh.SamplePosition(spawnPosition, out hit, 2f, NavMesh.AllAreas))
            {
                spawnPosition = hit.position;
            }
            else
            {
                // NavMesh 위의 위치를 찾지 못하면 현재 위치 사용
                spawnPosition = transform.position;
            }

            // 풀에서 자식 몬스터 활성화
            GameObject child = monsterSpawn.SpawnMonsterFromPool(MonsterSpawn.MonsterSpawnData.MonsterType.Elite, 0, spawnPosition, Quaternion.identity);
            if (child != null)
            {
                SmallMonster smallMonsterScript = child.GetComponent<SmallMonster>();
                if (smallMonsterScript != null)
                {
                    smallMonsterScript.invincible = true;
                }
            }
        }

        this.gameObject.SetActive(false); // 큰 몬스터 제거
    }
}
