using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meteor_Impact : MonoBehaviour
{
    public float damage = 100f;
    public float effectDuration = 2f;
    public GameObject indicatorObject;
    private SphereCollider sphereCollider;

    void Start()
    {
        sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider != null)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, sphereCollider.radius);
            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    PlayerStatus playerStatus = hit.GetComponent<PlayerStatus>();
                    Hitdecision hitdecision = hit.GetComponent<Hitdecision>();

                    if (playerStatus != null && hitdecision != null)
                    {
                        hitdecision.ApplyDamage(playerStatus, damage);
                    }
                }
            }
        }

        StartCoroutine(DestroyWithIndicator());
    }

    private IEnumerator DestroyWithIndicator()
    {
        yield return new WaitForSeconds(effectDuration);
        if (indicatorObject != null)
            Destroy(indicatorObject);
        Destroy(gameObject);
    }
}
