using UnityEngine;
using System.Collections;

public class SmallMonster : MonoBehaviour
{
    public MonsterStatus monsterStatus;

    public bool invincible = false;
    public float invincibleTime = 1f;
    public Collider monsterCollider;
    private float timer = 0f;

    void Start()
    {
        monsterStatus = GetComponent<MonsterStatus>();
        monsterCollider = GetComponent<Collider>();
    }

    void Update()
    {
        if (invincible)
        {
            monsterCollider.enabled = false;
            timer += Time.deltaTime;
            if (timer >= invincibleTime)
            {
                monsterCollider.enabled = true;
                invincible = false;
                timer = 0f;
            }
        }
    }
}
