using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class BossAttackPatterns : MonoBehaviour
{
    public NavMeshAgent agent;
    public GameObject bulletPrefab;
    public GameObject minionPrefab;
    public float bulletSpeed = 5f;
    public float minionSummonInterval = 10f;

    public GameObject bombPrefab;  // 포격 프리팹
    public float bombSpeed = 10f;  // 포격 속도
    public float bulletDamage = 20f; // 포격 피해

    public float flyDuration = 10f;   // 공중으로 떠 있는 시간
    public float attackInterval = 100f;   // 포격 간격
    public float flyHeight = 10f;         // 공중으로 떠있는 높이
    public float flySpeed = 5f;           // 공중으로 이동하는 속도

    private Transform playerTransform;
    private bool isFlying = false;
    private bool isAttacking = false;

    

    [SerializeField] private float chargeSpeed = 20f;
    [SerializeField] private float chargeDuration = 1.2f;
    [SerializeField] private float chargeCooldown = 5f;

    private bool isCharging = false;
    private bool isDead = false;

    private Animator bossAnimator;
    

    void Start()
    {
        bossAnimator = GetComponentInChildren<Animator>();
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        agent = GetComponent<NavMeshAgent>();

        

        if (playerTransform == null)
        {
            Debug.LogError("Player 태그를 가진 오브젝트를 찾을 수 없습니다.");
            return;
        }

        StartCoroutine(PatternLoop());
    }

    void Update()
    {
        // 공격 패턴 루프를 시작할 타이밍
        if (isFlying && !isAttacking)
        {
            StartCoroutine(DropBombs());
        }
    }
   
    private Vector3 GetPlayerPosition()
    {
        return playerTransform.position;
    }

    public IEnumerator PatternLoop()
    {
        while (!isDead)
        {
            yield return StartCoroutine(ChargeAttack());
            yield return StartCoroutine(BulletHell());
            yield return StartCoroutine(SummonMinions());
            yield return StartCoroutine(ExplosionZoneAttack());
            yield return StartCoroutine(ChainEffectDebuff());
            //yield return StartCoroutine(flyAttack());
        }
    }

    private IEnumerator ChargeAttack()
    {
        if (isCharging || playerTransform == null) yield break;
        isCharging = true;

        bossAnimator.SetTrigger("Charge");

        Vector3 chargeDirection = (GetPlayerPosition() - transform.position).normalized;
        float timer = 0f;

        // 돌진 중 충돌 무시
        Collider bossCollider = GetComponent<Collider>();
        if (bossCollider != null) bossCollider.isTrigger = true;

        // 이동
        while (timer < chargeDuration)
        {
            transform.position += chargeDirection * chargeSpeed * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }

        // 충돌 다시 활성화
        if (bossCollider != null) bossCollider.isTrigger = false;

        isCharging = false;
        yield return new WaitForSeconds(chargeCooldown);
    }

    private IEnumerator BulletHell()
    {
        if (bulletPrefab != null)
        {
            float angleStep = 360f / 10f;
            for (int i = 0; i < 10; i++)
            {
                float angle = i * angleStep;
                Vector3 bulletDirection = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad));
                GameObject bullet = Instantiate(bulletPrefab, transform.position + Vector3.up, Quaternion.identity);
                Rigidbody rb = bullet.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = bulletDirection * bulletSpeed;
                }
                yield return new WaitForSeconds(0.2f);
            }
        }
        yield return null;
    }

    private IEnumerator SummonMinions()
    {
        if (minionPrefab != null)
        {
            Instantiate(minionPrefab, transform.position + new Vector3(2f, 0, 0), Quaternion.identity);
            Debug.Log("소환된 미니언!");
            yield return new WaitForSeconds(minionSummonInterval);
        }
        yield return null;
    }

    private IEnumerator ExplosionZoneAttack()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 5f);
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Player"))
            {
                Debug.Log("플레이어에게 폭발 데미지 적용!");
                // 실제 데미지는 여기에 구현
            }
        }
        yield return new WaitForSeconds(3f);
    }

    private IEnumerator ChainEffectDebuff()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.ApplySlow(0.5f, 5f);
            Debug.Log("체인 효과: 플레이어 속도 감소!");
            yield return new WaitForSeconds(5f);
        }
        yield return null;
    }



private IEnumerator flyAttack()
{
    if (isFlying) yield break;  // 이미 비행 중이라면 무시
    isFlying = true;

    bossAnimator.SetTrigger("Fly"); // 애니메이션 시작

    // 보스를 공중으로 이동 (z축 상승)
    float elapsedTime = 0f;
    Vector3 startPosition = transform.position;
    Vector3 targetPosition = new Vector3(startPosition.x, startPosition.y + flyHeight, startPosition.z);

    while (elapsedTime < flyDuration)
    {
        transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / flyDuration);
        elapsedTime += Time.deltaTime;
        yield return null;
    }
    transform.position = targetPosition; // 최종 위치로 강제 설정

    // 포격 떨어뜨리기 시작
    isAttacking = true;

    yield return new WaitForSeconds(attackInterval); // 포격 간격 대기

    // 포격을 떨어뜨리는 공격 반복
    yield return StartCoroutine(DropBombs());

    // 공격 후 보스가 내려옵니다
    yield return StartCoroutine(Descend());
}
    private void OnTriggerEnter(Collider other)
    {
       /* if (other.CompareTag("Player"))
        {
            // 플레이어에게 데미지 적용
            other.GetComponent<MonsterStatus>().TakeDamage(50);
            Destroy(gameObject);  // 폭탄 파괴
        }
        else if (other.CompareTag("Ground"))
        {
            Destroy(gameObject);  // 지면에 닿으면 폭탄 사라짐
        }
       */
    }

        // 공중에서 포격 떨어뜨리기 
    private IEnumerator DropBombs()
{
        while (isFlying)
        {
        // 랜덤하게 떨어질 위치 계산
            Vector3 dropPosition = playerTransform.position;  // 예시: 플레이어 위치로 떨어지게 할 때
            GameObject bullet = Instantiate(bombPrefab, dropPosition + Vector3.up * 10f, Quaternion.identity);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();

            if (rb != null)
            {
                Vector3 direction = (playerTransform.position - bullet.transform.position).normalized;
                rb.velocity = direction * bombSpeed;
            }

        // 포격 발사 간격
            yield return new WaitForSeconds(attackInterval);
        }
}

// 보스가 내려오는 애니메이션 및 이동 처리
private IEnumerator Descend()
{
    Vector3 startPosition = transform.position;
    Vector3 targetPosition = new Vector3(startPosition.x, startPosition.y - flyHeight, startPosition.z);
    float elapsedTime = 0f;

    while (elapsedTime < flyDuration)
    {
        transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / flyDuration);
        elapsedTime += Time.deltaTime;
        yield return null;
    }

    transform.position = targetPosition; // 최종 위치로 강제 설정

    isFlying = false; // 비행 종료
}

    
}
