using System.Collections;
using UnityEngine;

public class ChainEffect : MonoBehaviour
{
    public float effectRadius = 5f;
    public float slowAmount = 0.5f;
    public float effectDuration = 3f;

    void Start()
    {
        ApplyEffect();
    }

    void ApplyEffect()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, effectRadius);
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Player"))
            {
                PlayerController controller = col.GetComponent<PlayerController>();
                if (controller != null)
                {
                    controller.ApplySlow(slowAmount, effectDuration);
                }
            }
        }

        Destroy(gameObject, 1f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, effectRadius);
    }
}
