using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meteor_Indicator : MonoBehaviour
{
    public GameObject meteorImpactPrefab;
    public float warningDuration = 2f;

    void Start()
    {
        Invoke(nameof(SpawnMeteor), warningDuration);
    }

    void Update()
    {
        
    }

    void SpawnMeteor()
    {
        GameObject impact = Instantiate(meteorImpactPrefab, transform.position, Quaternion.identity);
        var meteorScript = impact.GetComponent<Meteor_Impact>();
        if (meteorScript != null)
        {
            meteorScript.indicatorObject = gameObject; // 자기 자신을 참조
        }

        //Instantiate(meteorImpactPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject); // 인디케이터 제거
    }
}
