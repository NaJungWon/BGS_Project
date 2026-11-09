using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitdecision : MonoBehaviour
{
    public PlayerHitSetting playerHitSetting;
    public PlayerSystem playerSystem;
    //public PlayerUISetting playerUISetting;

    // Start is called before the first frame update
    void Start()
    {
        playerSystem = GetComponent<PlayerSystem>();
        playerHitSetting = this.transform.GetChild(0).GetComponent<PlayerHitSetting>();
    }

    // 데미지 적용 메서드
    public void ApplyDamage(PlayerStatus playerStatus, float damage)
    {
        if(playerStatus.isInvincible)
        {
            return;
        }
        else if(playerStatus.canShield)//방어 증강 먹었을때 설정
        {
            playerStatus.canShield = false;
            return;
        }
        else
        {
            playerHitSetting.hitPlayer();
            float damageMultiplier = 1f;
            if (playerStatus.defense >= 0)
            {
                damageMultiplier = 100f / (100f + playerStatus.defense);
            }
            
            // 데미지 계산 후 소수점 제거
            int finalDamage = Mathf.FloorToInt(damage * damageMultiplier);
            playerStatus.currentHealth -= finalDamage;
            
            // 체력이 0 미만이 되지 않도록 제한
            playerStatus.currentHealth = Mathf.Max(0, playerStatus.currentHealth);
            
            // 체력이 0이 되면 isAlive를 false로 설정
            if (playerStatus.currentHealth <= 0)
            {
                playerStatus.isAlive = false;
            }
            
            // 체력이 최대 체력을 초과하지 않도록 제한
            playerStatus.currentHealth = Mathf.Min(playerStatus.currentHealth, playerStatus.maxHealth);
            
            //Debug.Log($"Player took {finalDamage} damage! Current Health: {playerStatus.currentHealth}");
        
            // 체력 UI 업데이트
            playerSystem.playerUISetting.UpdateHealthUI(playerStatus);
        }
    }
}
