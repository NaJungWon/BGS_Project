using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterParticle : MonoBehaviour
{
    void Start()
    {
        var ps = GetComponent<ParticleSystem>();
        if (ps != null)
            Destroy(gameObject, 1f);
    }
}
