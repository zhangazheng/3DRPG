using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Attack", menuName = "Attack/Attack Data")]
public class AttackData_SO : ScriptableObject
{
    public float attackRange;
    public float skillRange;
    public float collDown;
    public int minDamage;
    public int maxDamage;
    public float criticalMultiplier;// ±©»÷¼Ó³É
    public float cirticalChance; // ±©»÷ÂÊ
}
