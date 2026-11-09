using UnityEngine;

public class ExpItem : MonoBehaviour
{
    public float expValue = 50; // 경험치 값

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player")) // 플레이어와 충돌 시
        {
            PlayerStatus playerStatus = other.gameObject.GetComponent<PlayerStatus>();
            if (playerStatus != null)
            {
                // experienceGain 수치만큼 경험치 증가 (소수점 제외)
                float finalExpValue = Mathf.Floor(expValue * (1 + playerStatus.experienceGain / 100f));
                other.gameObject.GetComponent<PlayerSystem>().GainExp(finalExpValue);
            }
            EnemyManager.Instance?.ReturnExp(gameObject);
        }
    }
}
