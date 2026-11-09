using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldItem : MonoBehaviour
{
    public int goldValue = 50; // 골드 값

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player")) // 플레이어와 충돌 시
        {
            // PlayerSystem 컴포넌트를 통해 GameManager의 골드 증가
            PlayerSystem playerSystem = other.GetComponent<PlayerSystem>();
            if (playerSystem != null && playerSystem.gameManager != null)
            {
                // UpgradeInGame 컴포넌트에서 goldGain 수치 가져오기
                UpgradeInGame upgradeInGame = other.GetComponent<UpgradeInGame>();
                if (upgradeInGame != null)
                {
                    // goldGain 수치만큼 골드 증가 (소수점 제외)
                    int finalGoldValue = Mathf.FloorToInt(goldValue * (1 + upgradeInGame.upgradeAbilityValues.goldGain / 100f));
                    playerSystem.gameManager.gold += finalGoldValue;
                    //Debug.Log($"골드 획득: {finalGoldValue}, 현재 골드: {playerSystem.gameManager.gold}");
                }
            }
            EnemyManager.Instance?.ReturnGold(gameObject);
        }
    }
}
