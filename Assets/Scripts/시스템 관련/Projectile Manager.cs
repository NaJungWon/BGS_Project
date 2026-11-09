using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static ProjectileManager Instance;

    // 현재 존재하는 투사체들의 리스트
    public List<GameObject> projectiles = new List<GameObject>();

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

    public List<GameObject> GetProjectileList()
    {
        return new List<GameObject>(projectiles);
    }

    // 투사체 추가
    public void RegisterProjectile(GameObject projectile)
    {
        if (!projectiles.Contains(projectile))
        {
            projectiles.Add(projectile);
        }
    }

    // 투사체 제거
    public void UnregisterProjectile(GameObject projectile)
    {
        if (projectiles.Contains(projectile))
        {
            projectiles.Remove(projectile);
        }
    }
}
