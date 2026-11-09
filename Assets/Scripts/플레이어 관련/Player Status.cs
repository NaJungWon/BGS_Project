using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    public PlayerSystem playerSystem;

    [Header("이름/레벨/직업/경험치")]
    public string characterName; // 플레이어 이름
    public int level; // 레벨
    public CharacterOccupation characterOccupation; //직업
    public float currentExperience; // 현재경험치
    public float needExperience; // 필요경험치
    public GameObject characterModel; //캐릭터 모델

    [Header("기본 스탯-체력/체젠/공/공속/방/이속/크확/크뎀")]
    public float maxHealth; // 최대 체력
    public float currentHealth; // 현재 체력
    public float healthRegeneration; // 체력 재생
    public float attack; // 공격력
    public float attackSpeed;
    public int defense; // 방어력
    public float speed; // 이동 속도
    public float critical; // 크리티컬 확률%
    public float criticalDamage; //크리티컬 데미지%

    [Header("추가 스탯-투사체/공범위/지속시간/획득범위/경험치증가")]
    public int projecTile; // 투사체 증가
    public float attackRange; // 공격 범위 증가%
    public float duration; // 지속시간 증가%
    public float acquisitionRange; // 획득 범위 증가%
    public float experienceGain; // 경험치 획득량 증가%

    [Header("생존 여부")]
    public bool isAlive;

    public bool isInvincible;

    [Header("무기 관련 변수")]
    public bool canShield; //방어 증강 먹었을때 전용

    [Header("패시브 스킬 관련")]
    public int monsterKillCount = 0; // 몬스터 처치 카운트
    public bool isPassiveActive = false; // 패시브 스킬 활성화 여부

    private float originalAttack;
    private float originalCritical;
    private float originalSpeed;
    private float originalAttackRange;

    private float healthRegenTimer = 0f;

    public enum CharacterOccupation
    {
        Warrior, Wizard
    }


    // Start is called before the first frame update
    void Start()
    {
        playerSystem = GetComponent<PlayerSystem>();
        originalAttack = attack;
        originalCritical = critical;
        originalSpeed = speed;
        originalAttackRange = attackRange;
    }

    void Update()
    {
        if(isAlive)
        {
            // 1초마다 체력 재생
            healthRegenTimer += Time.deltaTime;
            if (healthRegenTimer >= 1f)
            {
                if (currentHealth < maxHealth && healthRegeneration > 0)
                {
                currentHealth += healthRegeneration;
                //Debug.Log("체력 재생량 : " + healthRegeneration);
                playerSystem.playerUISetting.UpdateHealthUI(this);
                if (currentHealth > maxHealth)
                    currentHealth = maxHealth;
            }
            healthRegenTimer = 0f;
        }
        }
    }

    public void HpSynchronization()
    {
        currentHealth = maxHealth;
    }

    public void AddKillCount()
    {
        monsterKillCount++;
        if (monsterKillCount % 100 == 0) // 100마리 처치 시 패시브 발동
        {
            StartCoroutine(ActivatePassiveSkill());
        }
    }

    private IEnumerator ActivatePassiveSkill()
    {
        if (isPassiveActive) yield break; // 이미 패시브가 활성화된 경우 중복 실행 방지

        isPassiveActive = true;

        if (characterOccupation == CharacterOccupation.Warrior)
        {
            attack *= 1.5f; // 데미지 50% 증가
            critical += 20f; // 크리티컬 확률 20% 증가
        }
        else if (characterOccupation == CharacterOccupation.Wizard)
        {
            speed *= 1.5f; // 이동 속도 50% 증가
            attackRange *= 1.5f; // 공격 사거리 50% 증가
        }

        yield return new WaitForSeconds(5f); // 5초 동안 효과 유지

        // 원래 스탯으로 복구
        attack = originalAttack;
        critical = originalCritical;
        speed = originalSpeed;
        attackRange = originalAttackRange;

        isPassiveActive = false;
    }
}
