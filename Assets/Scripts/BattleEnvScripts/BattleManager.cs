using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{

    public UnitManager playerManager, enemyManager;

    [SerializeField]
    bool trainingAI;

    [SerializeField]
    public int maxUnits;

    public List<GameObject> DeadUnits;

    [SerializeField]
    private GridManager gridManager;

    [SerializeField]
    RoundManager roundManager;

    public Dictionary<PlayerUnit, int> UnitsGettingDamaged;
    public Dictionary<PlayerUnit, int> UnitsGettingEnergy;
    public Dictionary<PlayerUnit, float> UnitsGettingNerfed;

    public int MovementsToResolve
    {
        get => movementsToResolve;
        set
        {
            //Debug.Log(value);
            movementsToResolve = value;
            if (movementsToResolve < 0)
            {
                Debug.LogError("There are less movements to resolve than 0");
            }
        }
    }

    private int movementsToResolve;

    public int MovementCount;

    public int PlayerIndex, EnemyIndex;

    [SerializeField]
    private bool difficultyEnabled;

    public DifficultyManager difficultyManager;

    bool plannedCombat = false, resolvedMovements;
    //private int playerHaveDecided;

    private void Awake()
    {
        if (difficultyEnabled)
        {
            difficultyManager.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        ResolveActions();
    }

    /// <summary>
    /// Ends the Round (a player has no units left)
    /// </summary>
    /// <param name="_whoWon"></param>
    public void EndRound(AIUnitManager.EWonLastEpisode _whoWon)
    {
        if (trainingAI)
        {
            ResetBattlefield();
        }
        else
        {
            roundManager.EndRound(_whoWon);
        }
    }

    /// <summary>
    /// Resets the battlefield
    /// </summary>
    public void ResetBattlefield()
    {
        if (difficultyEnabled && !difficultyManager.MadeDecision)
        {
            //Debug.Log("difficulty is being adjusted");
            difficultyManager.EndRound();
            return;
        }
        gridManager.ResetGrid();
        MovementsToResolve = 0;
        PlayerIndex = 0;
        EnemyIndex = 0;

        enemyManager.OnGameStart?.Invoke();

        ResetTempUnitInfo();
        PlanCombat(playerManager);
        PlanCombat(enemyManager);
        difficultyManager.MadeDecision = false;
        //For training
        Debug.Log("EpisodeCount");
    }

    public void PlanCombat(UnitManager _manager)
    {
        _manager.PlanCombat();
    }

    /// <summary>
    /// Checks whether both players are ready
    /// </summary>
    public void ManagerReadyCheck()
    {
        if (playerManager.TookDecision && enemyManager.TookDecision)
        {
            ResetTempUnitInfo();
            playerManager.TookDecision = false;
            enemyManager.TookDecision = false;
            plannedCombat = true;
        }
    }

    /// <summary>
    /// If the combat is planned it gets resolved here
    /// </summary>
    public void ResolveActions()
    {
        if (plannedCombat == true)
        {
            if (MovementsToResolve > 0)
            {
                return;
            }
            else if (resolvedMovements == false)
            {
                resolvedMovements = true;
                ResolveMovements();
            }
            else
            {
                plannedCombat = false;
                resolvedMovements = false;
                ResolveCombat();
            }
        }
    }

    private void ResolveMovements()
    {
        enemyManager.PerformMovements();
        playerManager.PerformMovements();
    }

    /// <summary>
    /// Resolves the combat by performing actions, receiving energy, blockung damage and planning combat anew
    /// </summary>
    private void ResolveCombat()
    {
        enemyManager.PerformAction();
        playerManager.PerformAction();
        ReceiveEnergy();
        BlockDamage(playerManager);
        BlockDamage(enemyManager);
        if (playerManager.MyUnits.Count == 0 || enemyManager.MyUnits.Count == 0)
        {
            return;
        }

        PlanCombat(playerManager);
        PlanCombat(enemyManager);
    }

    /// <summary>
    /// Units that took damage will get it dealt here
    /// </summary>
    /// <param name="_defendingManager"></param>
    private void BlockDamage(UnitManager _defendingManager)
    {
        //Create a new list in case a unit gets removed from the list while it is iterated upon
        List<PlayerUnit> tmpList = new List<PlayerUnit>();
        for (int i = 0; i < _defendingManager.MyUnits.Count; i++)
        {
            tmpList.Add(_defendingManager.MyUnits[i]);
        }
        foreach (PlayerUnit playerUnit in tmpList)
        {
            if (UnitsGettingDamaged[playerUnit] > 0)
            {
                if (_defendingManager.isAI == false && playerUnit.currentArmor > 0)
                {
                    _defendingManager.RewardForBlocking(Mathf.Clamp(UnitsGettingDamaged[playerUnit], 0, playerUnit.currentArmor), playerUnit);
                }
                int damageTaken = Mathf.Clamp(
                    Mathf.FloorToInt(UnitsGettingDamaged[playerUnit] * UnitsGettingNerfed[playerUnit]) - playerUnit.currentArmor,
                    0, 999);
                difficultyManager.DifficultyEventTrigger(EDifficultyEvent.HP_LOST, !playerUnit.isEnemy, damageTaken);
                playerUnit.HealthPoints -= damageTaken;
                //Debug.Log($"{playerUnit.name} has taken {damageTaken} damage");
            }
            playerUnit.currentArmor = 0;
            UnitsGettingDamaged[playerUnit] = 0;
        }
    }

    /// <summary>
    /// Adds the units the energy that they gained
    /// </summary>
    private void ReceiveEnergy()
    {
        foreach (PlayerUnit unit in playerManager.MyUnits)
        {
            unit.Energy += UnitsGettingEnergy[unit];
        }
        foreach (PlayerUnit unit in enemyManager.MyUnits)
        {
            unit.Energy += UnitsGettingEnergy[unit];
        }
    }

    /// <summary>
    /// Resets the dictionaries
    /// </summary>
    public void ResetTempUnitInfo()
    {
        UnitsGettingDamaged = new Dictionary<PlayerUnit, int>();
        UnitsGettingNerfed = new Dictionary<PlayerUnit, float>();
        UnitsGettingEnergy = new Dictionary<PlayerUnit, int>();
        for (int i = 0; i < maxUnits; i++)
        {
            if (i < playerManager.EnemyUnits.Count)
            {
                UnitsGettingDamaged.Add(playerManager.EnemyUnits[i], 0);
                UnitsGettingNerfed.Add(playerManager.EnemyUnits[i], 1);
                UnitsGettingEnergy.Add(enemyManager.MyUnits[i], 0);
            }

            if (i < enemyManager.EnemyUnits.Count)
            {
                UnitsGettingDamaged.Add(enemyManager.EnemyUnits[i], 0);
                UnitsGettingNerfed.Add(enemyManager.EnemyUnits[i], 1);
                UnitsGettingEnergy.Add(playerManager.MyUnits[i], 0);
            }
        }
    }

    /// <summary>
    /// Gets triggered when a unit dies. It gets removed from manager references and deactivated for object pooling
    /// </summary>
    /// <param name="_unit"></param>
    public void DestroyUnit(PlayerUnit _unit)
    {
        if (_unit.isEnemy)
        {
            if (enemyManager.MyUnits.Contains(_unit))
            {
                enemyManager.MyUnits.Remove(_unit);
            }
            if (playerManager.EnemyUnits.Contains(_unit))
            {
                playerManager.EnemyUnits.Remove(_unit);
            }
            //playerManager.EnemyUnitKilled();
            enemyManager.MyUnitKilled();
        }
        else
        {
            if (playerManager.MyUnits.Contains(_unit))
            {
                playerManager.MyUnits.Remove(_unit);
            }
            if (enemyManager.EnemyUnits.Contains(_unit))
            {
                enemyManager.EnemyUnits.Remove(_unit);
            }
            enemyManager.EnemyUnitKilled();
            //playerManager.MyUnitKilled();
        }
        DeadUnits.Add(_unit.gameObject);
        _unit.gameObject.SetActive(false);
    }
}
