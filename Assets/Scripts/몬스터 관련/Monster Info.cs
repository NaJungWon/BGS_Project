using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MonsterInfo")]

public class MonsterInfo : ScriptableObject
{
    [Header("몬스터 이름/종류/공격타입")]
    public string monsterName;
    public MonsterType monsterType;
    public AttackType attackType;
    public GameObject monstermodel;

    [Header("기본 스탯- 체력/공/사정거리/공속/이속")]
    public float hp;
    public int damage;
    public float attackRange;
    public float attackSpeed;
    public float moveSpeed;

    [Header("생존 여부")]
    public bool monsterAlive;

    public enum MonsterType
    {
        Normale, Elite, Middleboss, Boss
    }

    public enum AttackType
    {
        Melee, Ranged, Special
    }
}
