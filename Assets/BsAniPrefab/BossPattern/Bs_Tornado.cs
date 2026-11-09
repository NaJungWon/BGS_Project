using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bs_Tornado : MonoBehaviour
{
    public float speed = 3f;
    public float lifetime = 8f;
    public float damage = 50f;
    public float damageInterval = 0.5f;

    private Transform target;

    private Coroutine damageCoroutine; // 데미지 코루틴 변수

    BossPattern bossPattern;

    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null) return;

        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;
        transform.LookAt(target); // 타겟 회전
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (damageCoroutine == null)
                damageCoroutine = StartCoroutine(ApplyTornadoDamage(other));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (damageCoroutine != null)
            {
                StopCoroutine(damageCoroutine);
                damageCoroutine = null;
            }
        }
    }

    private IEnumerator ApplyTornadoDamage(Collider player)
    {
        PlayerStatus playerStatus = player.GetComponent<PlayerStatus>();
        Hitdecision hitdecision = player.GetComponent<Hitdecision>();

        if (playerStatus != null && hitdecision != null)
        {
            while (player.bounds.Intersects(GetComponent<Collider>().bounds))
            {
                hitdecision.ApplyDamage(playerStatus, damage);

                yield return new WaitForSeconds(damageInterval);
            }
        }
    }

    public void Initialize(float speed, float lifetime, float damageInterval, BossPattern bossPattern)
    {
        this.speed = speed;
        this.lifetime = lifetime;
        this.damageInterval = damageInterval;
        this.bossPattern = bossPattern;

        // Start()에서 하던 초기화 작업을 여기서 처리
        Destroy(gameObject, lifetime);
    }

    private void OnDestroy()
    {
        if (bossPattern != null)
        {
            bossPattern.OnTornadoDestroyed();
        }
    }
}
