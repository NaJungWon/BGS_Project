using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CharacterInfo")]

public class CharacterInfo : ScriptableObject
{
    [Header("이름/레벨/직업/경험치")]
    public string characterName; // 플레이어 이름
    public int level; // 레벨
    public CharacterOccupation characterOccupation;
    public float needExperience; // 필요 경험치
    public GameObject characterModel; //캐릭터 모델
    public CardData dfWeaponCard; //기본 무기(증강 카드)
    public Sprite characterImage; //플레이어 이미지
    public Sprite characterIcon; //플레이어 아이콘 이미지
    public GameObject chracterSkill; //캐릭터 기본 스킬
    public string eskillName; //E 스킬 이름
    public string qskillName; //Q 스킬 이름
    public string eskillDescription; //E 스킬 설명
    public string qskillDescription; //Q 스킬 설명

    [Header("기본 스탯-체력/체젠/공/공속/방/이속/크확/크뎀")]
    public int health; // 최대 체력
    public float healthRegeneration; // 체력 재생
    public int attack; // 공격력
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
    public bool isAllive;

    public enum CharacterOccupation
    {
        Warrior, Wizard
    }
}
