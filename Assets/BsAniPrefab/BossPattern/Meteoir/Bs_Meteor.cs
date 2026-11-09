//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class Bs_Meteor : MonoBehaviour
//{
//    public GameObject meteorIndicatorPrefab;
//    public int meteorCount = 5;
//    public float radius = 10f;
//    public float excludeCenterRadius = 3f;
//    public Transform bossCenter;

//    // Start is called before the first frame update
//    void Start()
//    {
        
//    }

//    // Update is called once per frame
//    void Update()
//    {
        
//    }

//    public void CastMeteorShower()
//    {
//        for (int i = 0; i < meteorCount; i++)
//        {
//            Vector3 pos;
//            do
//            {
//                Vector2 rnd = Random.insideUnitCircle * radius;
//                pos = bossCenter.position + new Vector3(rnd.x, 0, rnd.y);
//            } while (Vector3.Distance(pos, bossCenter.position) < excludeCenterRadius);

//            Instantiate(meteorIndicatorPrefab, pos, Quaternion.identity);
//        }
//    }
//}
