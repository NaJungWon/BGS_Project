using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPattern : MonoBehaviour
{
    public BossSetting bossSetting; // 보스 설정 컴포넌트

    public Bs_DashAttack bs_DashAttack; // 대시 공격 컴포넌트

    public GameObject tornadoPrefab;
    public float tornadoSpawnInterval = 5f; // 토네이도 생성 간격
    public float tornadoTimer = 0f; // 토네이도 타이머

    public float meteorInterval = 1.5f;

    public GameObject meteorIndicatorPrefab; // 메테오 표시 프리팹
    public Animator animator;

    public float meteorAreaRadius = 18f; // 메테오가 생성될 전체 영역 반지름
    public float bossExcludeRadius = 0f; // 보스 주변 제외 반지름
    public int meteorCount = 2; // 한 번에 생성될 메테오 개수
    

    // Start is called before the first frame update
    void Start()
    {
        bossSetting = this.gameObject.GetComponent<BossSetting>();
        if (animator != null)
        {
            // 초기 상태를 Idle로 설정
            animator.SetTrigger("Idle");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (bossSetting != null)
        {
            if (bossSetting.playerInRoom)
            {
                // 플레이어가 방에 들어왔을 때만 애니메이션 활성화
                if (animator != null)
                {
                    animator.enabled = true;
                }
            }
            else
            {
                // 플레이어가 방에 없을 때는 애니메이션 비활성화
                if (animator != null)
                {
                    animator.enabled = false;
                    // Idle 상태로 강제 전환
                    animator.SetTrigger("Idle");
                }
            }
        }

        if (bossSetting != null && bossSetting.playerInRoom)
        {
            // 메테오 패턴
            meteorInterval -= Time.deltaTime;
            if (meteorInterval <= 0f)
            {
                meteorInterval = 1.5f; // 초기화
                SummonMeteor();
            }
        }
    }

    public void SummonTornado()
    {
        if (!bossSetting.playerInRoom) return;

        // 1. 플레이어 위치 찾기
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        // 2. 방향 벡터 계산
        Vector3 bossPos = transform.position;
        Vector3 playerPos = player.transform.position;
        Vector3 dir = (playerPos - bossPos).normalized;

        // 3. 생성 위치 계산 (보스로부터 3만큼 떨어진 곳, 필요시 거리 조정)
        float spawnDistance = 5f;
        Vector3 spawnPos = bossPos + dir * spawnDistance;

        // 4. 토네이도 오브젝트 생성
        GameObject tornado = Instantiate(tornadoPrefab, spawnPos, Quaternion.identity);
        tornado.GetComponent<Bs_Tornado>().Initialize(3f, 10f, 1f, this); // 데미지 값

        // 2. 랜덤 방향 토네이도
        Vector2 randCircle = Random.insideUnitCircle.normalized; // 랜덤 방향
        Vector3 dirRandom = new Vector3(randCircle.x, 0, randCircle.y);
        Vector3 spawnPosRandom = bossPos + dirRandom * spawnDistance;
        GameObject tornado2 = Instantiate(tornadoPrefab, spawnPosRandom, Quaternion.identity);
        tornado2.GetComponent<Bs_Tornado>().Initialize(3f, 10f, 1f, this);
    }

    public void OnTornadoDestroyed()
    {
        StartCoroutine(RestartTornadoDelay(5f));
    }

    private IEnumerator RestartTornadoDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (bossSetting != null && bossSetting.playerInRoom && animator != null)
        {
            animator.SetTrigger("StartTornado");
        }
    }

    public void SummonMeteor()
    {
        if (!bossSetting.playerInRoom) return;

        Vector3 bossPos = transform.position;

        for (int i = 0; i < meteorCount; i++)
        {
            Vector3 spawnPos = GetRandomPosition(bossPos, meteorAreaRadius, bossExcludeRadius);
            Instantiate(meteorIndicatorPrefab, spawnPos, Quaternion.identity);
        }
    }

    Vector3 GetRandomPosition(Vector3 center, float areaRadius, float excludeRadius)
    {
        Vector2 randCircle;
        float dist;
        do
        {
            randCircle = Random.insideUnitCircle * areaRadius;
            dist = randCircle.magnitude;
        } while (dist < excludeRadius);

        Vector3 pos = center + new Vector3(randCircle.x, 0, randCircle.y);
        pos.y = 0.1f; // 필요한 y축 조정(바닥 높이)
        return pos;
    }

    public void OnDashAttackFinished()
    {
        if (bossSetting != null && bossSetting.playerInRoom)
        {
            StartCoroutine(RestartDashAttackDelay(5f)); // 5초 딜레이 적용
        }
    }

    private IEnumerator RestartDashAttackDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (bossSetting != null && bossSetting.playerInRoom && animator != null)
        {
            animator.SetTrigger("Fly");
            animator.SetTrigger("StartDash");
            animator.SetTrigger("Land");
        }
    }
}
