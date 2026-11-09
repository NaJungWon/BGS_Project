using UnityEngine;
using System.Collections;

public class ExplodingMonster : MonoBehaviour
{
    public GameObject explosionEffect;
    public float explosionRadius = 3f;
    private bool isExploding = false;
    public float damage;
    private MonsterStatus monsterStatus;

    void Start()
    {
        monsterStatus = GetComponent<MonsterStatus>();
        damage = monsterStatus.attackPower;
    }

    public void ExplodeImmediately()
    {
        // ✅ 폭발 위치 보정
        Vector3 explosionPosition = new Vector3(transform.position.x, 0.5f, transform.position.z);

        if (explosionEffect != null)
        {
            GameObject explosion = GetOrCreateExplosion();
            if (explosion != null)
            {
                explosion.transform.position = explosionPosition;
                explosion.transform.rotation = Quaternion.identity;
                
                ExplosionZone explosionZone = explosion.GetComponent<ExplosionZone>();
                if (explosionZone != null)
                {
                    explosionZone.damage = damage;
                }
                
                explosion.SetActive(true);
            }
        }
    }

    private GameObject GetOrCreateExplosion()
    {
        Transform parent = EnemyManager.Instance.transform.GetChild(1);
        GameObject explosion = EnemyManager.Instance.GetExplosionFromPool(explosionEffect, parent);
        if (explosion != null)
        {
            ExplosionZone zone = explosion.GetComponent<ExplosionZone>();
            if (zone != null)
            {
                zone.onRelease = EnemyManager.Instance.ReturnExplosion;
            }
        }
        return explosion;
    }
}
