using UnityEngine;

public class ExpItem : MonoBehaviour
{
    public float expValue = 50; // 경험치 값
    private float baseExpValue = -1f; // 처음 값(풀에서 재사용할 때 되돌릴 기준)

    // 풀에서 꺼내 다시 떨어뜨릴 때마다 원래 경험치 값으로 되돌림 (절멸 1.5배가 누적되지 않도록)
    public void ResetValue()
    {
        if (baseExpValue < 0f) baseExpValue = expValue;
        expValue = baseExpValue;
    }

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
