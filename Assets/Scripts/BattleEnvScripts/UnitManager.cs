using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.Events;
using System.Linq;

/// <summary>
/// Parent of AIUnitManager and PlayerUnitManager so that training and playing with player is managable
/// </summary>
public class UnitManager : Agent
{
    public bool isPlayerManager;
    public bool isAI;

    /// <summary>
    /// int[] = 0: Target Index, 1: Movement, 2: Action, 3: ActionWeight
    /// </summary>
    public Dictionary<PlayerUnit, int[]> myUnitMoves;

    public List<PlayerUnit> MyUnits;
    public List<PlayerUnit> EnemyUnits;
    public UnityEvent OnGameStart;

    [SerializeField]
    public GridManager MyBattlefield;

    public BattleManager MyBattleManager;

    public int maxUnitSize;

    public float AttackMultiplier = 1;
    public float DefenceMultiplier = 1;
    public float HealthMultiplier = 1;

    public int CurrentIndex
    {
        get => m_currentIndex;
        set
        {
            //if (value != m_currentIndex)
            //{
            if (value < MyUnits.Count && value > 0)
            {
                m_currentIndex = value;
            }
            else
            {
                m_currentIndex = 0;
            }
            if (MyUnits.Count == 0)
            {
                return;
            }
            MyUnits[m_currentIndex].GetSelected();
            FindTilesInRange();
            //}
        }
    }
    [SerializeField]
    private int m_currentIndex;

    public bool TookDecision;


    public override void OnEpisodeBegin()
    {
        //MyBattleManager.ResetBattlefield();
    }

    public virtual void PerformMovements()
    {

    }

    public virtual void ReduceBattleManagerWait()
    {
        MyBattleManager.MovementsToResolve--;
    }

    public virtual void FindTilesInRange()
    {
    }

    public virtual void EnemyUnitKilled()
    {
        //AI Thing
    }

    public virtual void MyUnitKilled()
    {
        //AI Thing
    }

    public virtual void PlanCombat()
    {

    }

    public virtual void PerformAction()
    {

    }

    public virtual void RewardForBlocking(int _amountBlocked, PlayerUnit _blockingUnit)
    {

    }

    public virtual void GetDistanceToOtherUnits(PlayerUnit _unit)
    {

    }
}