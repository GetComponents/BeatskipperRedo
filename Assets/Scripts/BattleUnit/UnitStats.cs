using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CLASSNAME", menuName = "ScriptableObjects/ClassStats", order = 1)]
public class UnitStats : ScriptableObject
{
    public int MovementRange, SprintRange, AttackRange, movementCost, sprintCost, attack1Cost, attack2Cost, attack3Cost,
        defend1Cost, defend2Cost, defend3Cost, attack1Dmg, attack2Dmg, attack3Dmg, defend1Armor, defend2Armor, defend3Armor, MaxHP, maxEnergy, movementSpeed;
    [Space]
    public int Buff1Strength;
    public int Buff2Strength, Buff3Strength;
    public float Nerf1Strength, Nerf2Strength, Nerf3Strength;
}
