using UnityEngine;

public class BossSummon : MonoBehaviour
{
    public GameObject minionPrefab;
    public Transform[] spawnPoints;

    public void SummonMinions()
    {
        foreach (Transform spawnPoint in spawnPoints)
        {
            Instantiate(minionPrefab, spawnPoint.position, Quaternion.identity);
        }
    }
}
