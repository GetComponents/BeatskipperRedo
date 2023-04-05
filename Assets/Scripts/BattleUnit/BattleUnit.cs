using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public enum EnemyClass
//{
//    NONE,
//    MELEE1,
//    MELEE2,
//    RANGED
//}

public enum PlayerClass
{
    NONE,
    MELEE,
    RANGED,
    SUPPORT
}

/// <summary>
/// Was supposed to be used as parent of enemy and player unit scripts
/// </summary>
public class BattleUnit : MonoBehaviour
{    
    public bool isEnemy;
    public PlayerClass playerClass;
    //public EnemyClass enemyClass;
    public Vector2Int MyGridPosition;
    
    //Unused Method
    public virtual void Move()
    {

    }
}


