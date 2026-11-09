using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static EnemyManager Instance;

    // 현재 존재하는 적들의 리스트
    public List<GameObject> enemies = new List<GameObject>();
    
    public Queue<GameObject> enemyDfProjectiles = new Queue<GameObject>(); // 기본공격 이펙트
    public Queue<GameObject> enemyExplosions = new Queue<GameObject>(); // 폭발 이펙트
    public Queue<GameObject> enemyPoisonAreas = new Queue<GameObject>(); // 독 영역
    public Queue<GameObject> enemyRangedProjectiles = new Queue<GameObject>(); // 포격 이펙트
    public List<GameObject> enemySpecialProjectiles = new List<GameObject>(); // 특수 투사체
    public List<GameObject> enemyHitEffects = new List<GameObject>(); // 히트 이펙트
    public Queue<GameObject> enemyDamageTexts = new Queue<GameObject>(); // 데미지 텍스트

    // 비활성/활성 경험치/골드 풀
    public Queue<GameObject> enemyExps = new Queue<GameObject>();
    public Queue<GameObject> enemyGolds = new Queue<GameObject>();
    private readonly Queue<GameObject> activeExpDrops = new Queue<GameObject>(); // 활성화된 경험치 풀
    private readonly Queue<GameObject> activeGoldDrops = new Queue<GameObject>(); // 활성화된 골드 풀

    void Awake()
    {
        // 싱글톤 초기화
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public List<GameObject> GetEnemyList()
    {
        return new List<GameObject>(enemies);
    }

    // 적 추가
    public void RegisterEnemy(GameObject enemy)
    {
        if (!enemies.Contains(enemy))
        {
            enemies.Add(enemy);
        }
    }

    // 적 제거
    public void UnregisterEnemy(GameObject enemy)
    {
        if (enemies.Contains(enemy))
        {
            enemies.Remove(enemy);
        }
    }

    // 풀에서 경험치 오브젝트 하나 꺼내오기 (없으면 새로 생성 후 보관)
    public GameObject GetExpFromPool(GameObject prefab, Transform parent = null)
    {
        if (enemyExps.Count == 0)
        {
            GameObject newExp = Instantiate(prefab, parent);
            newExp.SetActive(false);
            enemyExps.Enqueue(newExp);
        }

        GameObject exp = enemyExps.Dequeue();
        if (parent != null) exp.transform.SetParent(parent);
        return exp;
    }

    // 기본 몹 이펙트
    public GameObject GetDfProjectileFromPool(GameObject prefab, Transform parent = null)
    {
        return GetOrCreateFromPool(enemyDfProjectiles, prefab, parent);
    }

    // 기본 몹 이펙트 반환
    public void ReturnDfProjectile(GameObject projectile)
    {
        ReturnToPool(enemyDfProjectiles, projectile);
    }

    // 폭발 이펙트
    public GameObject GetExplosionFromPool(GameObject prefab, Transform parent = null)
    {
        return GetOrCreateFromPool(enemyExplosions, prefab, parent);
    }

    // 폭발 이펙트 반환
    public void ReturnExplosion(GameObject explosion)
    {
        ReturnToPool(enemyExplosions, explosion);
    }

    // 독 영역
    public GameObject GetPoisonAreaFromPool(GameObject prefab, Transform parent = null)
    {
        return GetOrCreateFromPool(enemyPoisonAreas, prefab, parent);
    }

    // 독 영역 반환
    public void ReturnPoisonArea(GameObject poisonArea)
    {
        ReturnToPool(enemyPoisonAreas, poisonArea);
    }

    // 포격 이펙트
    public GameObject GetRangedProjectileFromPool(GameObject prefab, Transform parent = null)
    {
        return GetOrCreateFromPool(enemyRangedProjectiles, prefab, parent);
    }

    // 포격 이펙트 반환
    public void ReturnRangedProjectile(GameObject projectile)
    {
        ReturnToPool(enemyRangedProjectiles, projectile);
    }

    // 풀에서 골드 오브젝트 하나 꺼내오기 (없으면 새로 생성 후 보관)
    public GameObject GetGoldFromPool(GameObject prefab, Transform parent = null)
    {
        if (enemyGolds.Count == 0)
        {
            GameObject newGold = Instantiate(prefab, parent);
            newGold.SetActive(false);
            enemyGolds.Enqueue(newGold);
        }

        GameObject gold = enemyGolds.Dequeue();
        if (parent != null) gold.transform.SetParent(parent);
        return gold;
    }

    // 경험치 반환
    public void ReturnExp(GameObject exp)
    {
        if (exp == null) return;
        RemoveFromActiveQueue(activeExpDrops, exp);
        exp.SetActive(false);
        enemyExps.Enqueue(exp);
    }

    // 골드 반환
    public void ReturnGold(GameObject gold)
    {
        if (gold == null) return;
        RemoveFromActiveQueue(activeGoldDrops, gold);
        gold.SetActive(false);
        enemyGolds.Enqueue(gold);
    }

    // 경험치 활성화
    public void MarkExpActive(GameObject exp)
    {
        EnqueueActive(activeExpDrops, exp);
    }

    // 골드 활성화
    public void MarkGoldActive(GameObject gold)
    {
        EnqueueActive(activeGoldDrops, gold);
    }

    // 특수 투사체 추가
    public void RegisterSpecialProjectile(GameObject specialProjectile)
    {
        if (!enemySpecialProjectiles.Contains(specialProjectile))
        {
            enemySpecialProjectiles.Add(specialProjectile);
        }
    }

    // 특수 투사체 제거
    public void UnregisterSpecialProjectile(GameObject specialProjectile)
    {
        if (enemySpecialProjectiles.Contains(specialProjectile))
        {
            enemySpecialProjectiles.Remove(specialProjectile);
        }
    }

    // 히트 이펙트 추가
    public void RegisterHitEffect(GameObject hitEffect)
    {
        if (!enemyHitEffects.Contains(hitEffect))
        {
            enemyHitEffects.Add(hitEffect);
        }
    }

    // 히트 이펙트 제거
    public void UnregisterHitEffect(GameObject hitEffect)
    {
        if (enemyHitEffects.Contains(hitEffect))
        {
            enemyHitEffects.Remove(hitEffect);
        }
    }

    // 풀에서 데미지 텍스트 오브젝트 하나 꺼내오기 (없으면 새로 생성 후 보관)
    public GameObject GetDamageTextFromPool(GameObject prefab, Transform parent)
    {
        if (enemyDamageTexts.Count == 0)
        {
            GameObject newText = Instantiate(prefab, parent);
            newText.SetActive(false);
            enemyDamageTexts.Enqueue(newText);
        }

        GameObject text = enemyDamageTexts.Dequeue();
        if (parent != null) text.transform.SetParent(parent);
        return text;
    }

    // 데미지 텍스트 반환
    public void ReturnDamageText(GameObject damageText)
    {
        if (damageText == null) return;
        if (damageText.activeSelf)
            damageText.SetActive(false);
        enemyDamageTexts.Enqueue(damageText);
    }

    // 자석 효과 활성화
    public void ActivateMagnetEffect(GameObject player)
    {
        if (magnetEffectCoroutine != null)
        {
            StopCoroutine(magnetEffectCoroutine);
        }

        activeDrops.Clear();
        CollectActiveDrops(activeExpDrops, activeDrops);
        CollectActiveDrops(activeGoldDrops, activeDrops);

        if (activeDrops.Count == 0)
            return;

        magnetEffectCoroutine = StartCoroutine(MagnetEffectCoroutine(player, activeDrops));
    }

    private Coroutine magnetEffectCoroutine; // 자석 효과 코루틴
    private float magnetMoveSpeed = 20f; // 자석 효과 이동 속도
    private List<GameObject> activeDrops = new List<GameObject>(); // 활성화된 경험치,골드 아이템 리스트

    // 자석 효과 코루틴
    private IEnumerator MagnetEffectCoroutine(GameObject player, List<GameObject> targets)
    {
        while (targets.Count > 0)
        {
            bool hasActive = false;
            for (int i = targets.Count - 1; i >= 0; i--)
            {
                GameObject item = targets[i];
                if (item != null && item.activeInHierarchy)
                {
                    hasActive = true;
                    Vector3 direction = (player.transform.position - item.transform.position).normalized;
                    item.transform.Translate(direction * magnetMoveSpeed * Time.deltaTime, Space.World);
                }
                else
                {
                    targets.RemoveAt(i);
                }
            }

            if (!hasActive)
            {
                break;
            }

            yield return null;
        }

        targets.Clear();
    }

    // 활성화된 큐에 오브젝트 추가
    private void EnqueueActive(Queue<GameObject> queue, GameObject obj)
    {
        if (obj == null) return;
        int count = queue.Count;
        for (int i = 0; i < count; i++)
        {
            GameObject existing = queue.Dequeue();
            queue.Enqueue(existing);
            if (existing == obj)
                return;
        }
        queue.Enqueue(obj);
    }

    // 활성화된 큐에서 특정 오브젝트 제거
    private void RemoveFromActiveQueue(Queue<GameObject> queue, GameObject target)
    {
        if (target == null || queue.Count == 0)
            return;

        int count = queue.Count;
        for (int i = 0; i < count; i++)
        {
            GameObject obj = queue.Dequeue();
            if (obj == null || obj == target)
            {
                if (obj != target && obj != null && obj.activeInHierarchy)
                {
                    queue.Enqueue(obj);
                }
            }
            else
            {
                queue.Enqueue(obj);
            }
        }
    }

    // 활성화된 큐에서 특정 오브젝트 제거
    private void CollectActiveDrops(Queue<GameObject> sourceQueue, List<GameObject> buffer)
    {
        foreach (GameObject obj in sourceQueue)
        {
            if (obj != null && obj.activeInHierarchy)
            {
                buffer.Add(obj);
            }
        }
    }

    private GameObject GetOrCreateFromPool(Queue<GameObject> pool, GameObject prefab, Transform parent)
    {
        if (prefab == null)
            return null;

        if (pool.Count == 0)
        {
            GameObject newObject = Instantiate(prefab, parent);
            newObject.SetActive(false);
            pool.Enqueue(newObject);
        }

        GameObject pooledObject = pool.Dequeue();
        if (parent != null && pooledObject != null)
        {
            pooledObject.transform.SetParent(parent);
        }

        return pooledObject;
    }

    private void ReturnToPool(Queue<GameObject> pool, GameObject obj)
    {
        if (obj == null)
            return;

        if (obj.activeSelf)
            obj.SetActive(false);

        pool.Enqueue(obj);
    }
}
