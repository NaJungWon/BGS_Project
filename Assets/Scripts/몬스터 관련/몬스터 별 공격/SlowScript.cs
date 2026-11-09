using UnityEngine;

public class SlowScript : MonoBehaviour
{
    public float damage;
    public float projectileSpeed = 5f;
    public float slowAmount = 0.5f;
    public float slowDuration = 3f;
    public float lifeTime = 3f;
    private Vector3 shootDirection;
    private float currentTime = 0f;

    public void Setup(Vector3 direction, float attackPower)
    {
        shootDirection = direction;
        damage = attackPower;
        currentTime = 0f;
    }

    void OnEnable()
    {
        currentTime = 0f;
    }

    void Update()
    {
        // 발사 방향으로 지속적으로 이동
        transform.Translate(shootDirection * projectileSpeed * Time.deltaTime, Space.World);

        // 생존 시간 체크
        currentTime += Time.deltaTime;
        if (currentTime >= lifeTime)
        {
            gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStatus playerStatus = other.GetComponent<PlayerStatus>();
            Hitdecision hitdecision = other.GetComponent<Hitdecision>();
            PlayerController movement = other.GetComponent<PlayerController>();
            if (movement != null)
            {
                hitdecision.ApplyDamage(playerStatus, damage);
                movement.ApplySlow(slowAmount, slowDuration);
            }
            gameObject.SetActive(false);
        }
    }
}
  