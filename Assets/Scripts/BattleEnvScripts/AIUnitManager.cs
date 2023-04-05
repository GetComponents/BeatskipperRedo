using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.Events;
using Unity.MLAgents.Policies;
using System.Linq;
using Unity.Barracuda;

public class AIUnitManager : UnitManager
{

    [Header("AI Stuff")]
    [SerializeField]
    int enemiesKilledForEpisodeEnd;
    int currentKilledEnemies;
    public bool isUsingHeuristics;

    public UnityEvent OnEpisodeStart;
    [SerializeField]
    float HPObservationMultiplier;
    public int[] DistanceToOtherUnits;
    [SerializeField]
    float smallPunishment, mediumPunishment, largePunishment, smallReward, mediumReward, largeReward;
    [SerializeField]
    private bool isTrainingAgent;

    public BehaviorParameters AIBrain;

    [Space]
    public bool PressedTab;
    public bool PressedSpace, PressedMouse1;

    private Vector2Int currentTilePos = new Vector2Int();

    public int CombatState
    {
        get => combatState;
        set
        {
            if (value != combatState)
            {
                if (value > 2)
                {
                    combatState = 0;
                }
                else
                {
                    combatState = value;
                }
                FindTilesInRange();
                if (isUsingHeuristics)
                {
                    currentTilePos.x = MyUnits[CurrentIndex].MyGridPosition.x;
                    currentTilePos.y = MyUnits[CurrentIndex].MyGridPosition.y;
                }
            }
        }
    }

    [SerializeField]
    private int combatState;

    public enum EWonLastEpisode
    {
        NONE,
        PLAYER_WON,
        TIE,
        ENEMY_WON
    }

    private EWonLastEpisode wonLastEpisode = new EWonLastEpisode();

    private void Awake()
    {
        AIBrain = GetComponent<BehaviorParameters>();
        DistanceToOtherUnits = new int[maxUnitSize * 2];
    }

    private void Update()
    {
        //Only undo if Maxsteps is not 0
        //AddReward((-Time.deltaTime / MaxStep) * largePunishment);
    }

    public override void OnEpisodeBegin()
    {
        currentKilledEnemies = 0;
        //Only gets to go on if its an enemy manager
        if (isPlayerManager)
        {
            return;
        }

        switch (wonLastEpisode)
        {
            case EWonLastEpisode.NONE:
                break;
            case EWonLastEpisode.PLAYER_WON:
                MyBattlefield.DefaultMat = MyBattlefield.WinMat;
                break;
            case EWonLastEpisode.TIE:
                MyBattlefield.DefaultMat = MyBattlefield.TimeOutMat;
                break;
            case EWonLastEpisode.ENEMY_WON:
                MyBattlefield.DefaultMat = MyBattlefield.LoseMat;
                break;
            default:
                break;
        }
        MyBattleManager.EndRound(wonLastEpisode);
        wonLastEpisode = EWonLastEpisode.TIE;
    }

    public override void PlanCombat()
    {
        CurrentIndex++;
        RequestDecision();
    }

    /// <summary>
    /// Summarizes the position of a unit. In this case in one of 9 squares
    /// </summary>
    /// <param name="_units"></param>
    private void SummarizeUnitsPositions(PlayerUnit[] _units)
    {

        for (int i = 0; i < _units.Length; i++)
        {
            _units[i].SummarizedPos = 0;
            if (_units[i].MyGridPosition.x > Mathf.FloorToInt(MyBattlefield.Gridsize.x * (2f / 3f)))
            {
                _units[i].SummarizedPos += 2;
            }
            else if (_units[i].MyGridPosition.x > Mathf.FloorToInt(MyBattlefield.Gridsize.x * (1f / 3f)))
            {
                _units[i].SummarizedPos += 1;
            }

            if (_units[i].MyGridPosition.y > Mathf.FloorToInt(MyBattlefield.Gridsize.y * (2f / 3f)))
            {
                _units[i].SummarizedPos += 6;
            }
            else if (_units[i].MyGridPosition.y > Mathf.FloorToInt(MyBattlefield.Gridsize.y * (1f / 3f)))
            {
                _units[i].SummarizedPos += 3;
            }
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        SummarizeUnitsPositions(MyUnits.ToArray());
        SummarizeUnitsPositions(EnemyUnits.ToArray());
        List<int> usedNumbers = new List<int>();

        for (int i = 0; i < maxUnitSize; i++)
        {
            //There to signify that this is a player
            sensor.AddObservation(true);
            //If a unit is dead the stats get nulled
            if (MyUnits.Count <= i)
            {
                sensor.AddObservation(0);
                sensor.AddObservation(0);
                sensor.AddObservation(0);
                sensor.AddObservation(0);
                //sensor.AddObservation(0);
            }
            else
            {
                sensor.AddObservation((int)MyUnits[i].playerClass);
                sensor.AddObservation((float)Mathf.Ceil((MyUnits[i].HealthPoints / MyUnits[i].MaxHealthPoints) * 4));
                sensor.AddObservation(DistanceToOtherUnits[maxUnitSize + i]);
                sensor.AddObservation(MyUnits[i].SummarizedPos);
                //sensor.AddObservation(MyUnits[i].Energy);
            }
        }
        usedNumbers = new List<int>();
        for (int i = 0; i < maxUnitSize; i++)
        {
            //There to signify that this is an enemy
            sensor.AddObservation(false);
            //If a unit is dead the stats get nulled
            if (EnemyUnits.Count <= i)
            {
                sensor.AddObservation(0);
                sensor.AddObservation(0);
                sensor.AddObservation(0);
                sensor.AddObservation(0);
                //sensor.AddObservation(0);
            }
            else
            {
                sensor.AddObservation((int)EnemyUnits[i].playerClass);
                sensor.AddObservation((float)Mathf.Ceil((EnemyUnits[i].HealthPoints / EnemyUnits[i].MaxHealthPoints) * 4));
                sensor.AddObservation(DistanceToOtherUnits[i]);
                sensor.AddObservation(EnemyUnits[i].SummarizedPos);
                //sensor.AddObservation(MyUnits[i].Energy);
            }
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (MyUnits.Count <= CurrentIndex)
        {
            Debug.Log("Fewer Units than currently indexed");
        }
        for (int i = 0; i < 4; i++)
        {
            myUnitMoves[MyUnits[CurrentIndex]][i] = actions.DiscreteActions[i];
        }
        //If all decisions have been made for all units, the ai does the ready check
        if (CurrentIndex == MyUnits.Count - 1)
        {
            TookDecision = true;
            MyBattleManager.ManagerReadyCheck();
        }
        else
        {
            MyBattleManager.PlanCombat(this);
        }
    }

    /// <summary>
    /// The AI performs the movements for its units
    /// </summary>
    public override void PerformMovements()
    {
        for (int h = 0; h < MyUnits.Count; h++)
        {
            MyUnits[h].GetSelected();
            if (!CheckIfIndexValid(myUnitMoves[MyUnits[h]][0]))
                continue;
            MyBattleManager.MovementsToResolve++;
            FindTilesInRange();
            switch (myUnitMoves[MyUnits[h]][1])
            {
                case 0:
                    MoveTowards(MyUnits[h], myUnitMoves[MyUnits[h]][0], 0);
                    //SmartAIGoAway(MyUnits[h], 0, 1);
                    break;
                case 1:
                    MoveTowards(MyUnits[h], myUnitMoves[MyUnits[h]][0], 1);
                    //SmartAIGoAway(MyUnits[h], 0, 1);
                    break;
                case 2:
                    MyUnits[h].MovementIdle();
                    //AIGoAway(AllEnemyUnits[h], enemyMoves[h][0], 1);
                    break;
                case 3:
                    SmartAIGoAway(MyUnits[h], myUnitMoves[MyUnits[h]][0], 0);
                    break;
                case 4:
                    SmartAIGoAway(MyUnits[h], myUnitMoves[MyUnits[h]][0], 1);
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// Checks if the index for myUnitMoves is valid
    /// </summary>
    /// <param name="_index"></param>
    /// <returns></returns>
    private bool CheckIfIndexValid(int _index)
    {
        if (_index < maxUnitSize)
        {
            if (_index >= EnemyUnits.Count)
            {
                return false;
            }
        }
        else
        {
            if (_index - maxUnitSize >= MyUnits.Count)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Moves a unit towards a certain point, depending of the weight. If the weight is too high, it gets adjusted
    /// </summary>
    /// <param name="_movingUnit"></param>
    /// <param name="_targetIndex"></param>
    /// <param name="_weight"></param>
    private void MoveTowards(PlayerUnit _movingUnit, int _targetIndex, int _weight)
    {
        PlayerUnit tmp;
        if (_targetIndex > EnemyUnits.Count)
        {
            _movingUnit.MovementIdle();
            return;
        }

        if (_targetIndex < maxUnitSize)
        {
            tmp = EnemyUnits[_targetIndex];
        }
        else
        {
            tmp = MyUnits[_targetIndex - maxUnitSize];
        }

        for (int i = _weight; i >= -1; i--)
        {
            if (i == -1)
            {
                _movingUnit.MovementIdle();
                break;
            }
            if (_movingUnit.HaveEnoughEnergyToPerformAction(PlayerUnit.EAction.MOVEMENT, i))
            {
                switch (i)
                {
                    case 0:
                        GoTowardsUnit(_movingUnit, tmp.MyGridPosition, tmp.MovementRange);
                        return;
                    case 1:
                        GoTowardsUnit(_movingUnit, tmp.MyGridPosition, tmp.SprintRange);
                        return;
                    default:
                        break;
                }
            }
        }
    }

    /// <summary>
    /// The AI Performs the actions of its units
    /// </summary>
    public override void PerformAction()
    {
        for (int h = 0; h < MyUnits.Count; h++)
        {
            int _action = myUnitMoves[MyUnits[h]][2];
            int _weight = myUnitMoves[MyUnits[h]][3];
            //Debug.Log("Action: " + _action);
            if (_action == 0 || !CheckIfIndexValid(myUnitMoves[MyUnits[h]][0]))
            {
                MyUnits[h].ActionIdle();
                continue;
            }
            for (int i = _weight; i >= -1; i--)
            {
                if (i == -1)
                {
                    MyUnits[h].ActionIdle();
                    break;
                }
                if (!MyUnits[h].HaveEnoughEnergyToPerformAction((PlayerUnit.EAction)_action, i))
                {
                    continue;
                    //AddReward(smallPunishment);
                }
                if (_action == 1)
                {
                    int hitUnits;
                    if (MyUnits[h].playerClass == PlayerClass.SUPPORT)
                    {
                        MyUnits[h].StatSupport(i);
                    }
                    hitUnits = MyUnits[h].AttackSurrondingUnits(i, AttackMultiplier);
                    MyBattleManager.difficultyManager.DifficultyEventTrigger(EDifficultyEvent.PLAYER_ATTACK, true, hitUnits);
                    if (hitUnits == 0)
                    {
                        //Debug.Log("Enemies hit = " + hitEnemies);
                        //AddReward(smallPunishment * (i + 1));
                    }
                    else
                    {
                        //AddReward(mediumReward * hitUnits * (i + 1));
                        //AddReward(largeReward * hitUnits * (i + 1));
                    }
                    break;
                }
                else if (_action == 2)
                {
                    if (MyUnits[h].playerClass == PlayerClass.SUPPORT)
                    {
                        int hitUnits = MyUnits[h].EnergySupport(i);
                        //AddReward(mediumReward * hitUnits * (i + 1));
                        break;
                    }
                    MyUnits[h].Defend(i, DefenceMultiplier);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// The AI goes away from a certain unit. for performance sake it will choose a square that is a certain distance away and try to go there
    /// </summary>
    /// <param name="_movingUnit"></param>
    /// <param name="_unitIndex"></param>
    /// <param name="_weight"></param>
    private void SmartAIGoAway(PlayerUnit _movingUnit, int _unitIndex, int _weight)
    {
        PlayerUnit tmp;
        if (_unitIndex < maxUnitSize)
        {
            tmp = EnemyUnits[_unitIndex];
        }
        else
        {
            tmp = MyUnits[_unitIndex - maxUnitSize];
        }
        int range = 0;
        //int j = 1;
        for (int j = _weight; j >= -1; j--)
        {
            if (j == -1)
            {
                _movingUnit.MovementIdle();
                return;
            }
            if (_movingUnit.HaveEnoughEnergyToPerformAction(PlayerUnit.EAction.MOVEMENT, j))
            {
                switch (j)
                {
                    case 0:
                        range = _movingUnit.MovementRange;
                        j = -2;
                        break;
                    case 1:
                        range = _movingUnit.SprintRange;
                        j = -2;
                        break;
                    default:
                        break;
                }
            }
        }
        if (range == 0)
        {
            _movingUnit.MovementIdle();
            return;
        }
        List<Tile> tiles = new List<Tile>();
        for (int y = 0; y < _movingUnit.PathfindingGrid.GetLength(1); y++)
        {
            for (int x = 0; x < _movingUnit.PathfindingGrid.GetLength(0); x++)
            {
                if (_movingUnit.PathfindingGrid[x, y] == range)
                {
                    tiles.Add(MyBattlefield.Grid[x, y]);
                }
            }
        }
        Tile bestTile = FindDistantTile(_movingUnit.MyGridPosition, tmp, tiles.ToArray());
        //If the AI doesnt find a tile, it will just walk away vectorwise
        if (bestTile == null)
        {
            GoTowardsWall(_movingUnit, tmp, range);
        }
        else
        {
            //Debug.Log($"Range: {range}, myPos: {_movingUnit.MyGridPosition}, GoalPos: {bestTile.PositionInGrid}");
            //bestTile.ChangeTileMat(ETileMat.TEST);
            GoTowardsUnit(_movingUnit, bestTile.PositionInGrid, range);
        }
    }

    /// <summary>
    /// The AI tries to find a square that is the furthest away from its target. Its not optimized but it generally works
    /// </summary>
    /// <param name="_startPos"></param>
    /// <param name="_unit"></param>
    /// <param name="_tiles"></param>
    /// <returns></returns>
    private Tile FindDistantTile(Vector2Int _startPos, PlayerUnit _unit, Tile[] _tiles)
    {
        int[,] pathGrid = _unit.PathfindingGrid;
        int biggestDistance = pathGrid[_startPos.x, _startPos.y];
        Tile bestTile = null;
        for (int i = 0; i < _tiles.Length; i++)
        {
            if (pathGrid[_tiles[i].PositionInGrid.x, _tiles[i].PositionInGrid.x] != 100 && pathGrid[_tiles[i].PositionInGrid.x, _tiles[i].PositionInGrid.x] > biggestDistance)
            {
                biggestDistance = pathGrid[_tiles[i].PositionInGrid.x, _tiles[i].PositionInGrid.x];
                bestTile = _tiles[i];
            }
        }
        return bestTile;
    }

    /// <summary>
    /// Walks away from a unit using vectors to know which way "away" is
    /// </summary>
    /// <param name="_movingUnit"></param>
    /// <param name="_antiTargetUnit"></param>
    /// <param name="_range"></param>
    private void GoTowardsWall(PlayerUnit _movingUnit, PlayerUnit _antiTargetUnit, int _range)
    {
        Vector3 direction = Vector3.Normalize(new Vector3(_movingUnit.MyGridPosition.x - _antiTargetUnit.MyGridPosition.x, 0,
            _movingUnit.MyGridPosition.y - _antiTargetUnit.MyGridPosition.y));
        Vector2Int newSquarePos;
        //After finding the "away" vector the line will be drawn and decreased if the square that is selected can't be walked towards (wall, outside range, oob)
        while (_range > 0)
        {
            newSquarePos = new Vector2Int(Mathf.RoundToInt(direction.x * _range), Mathf.RoundToInt(direction.z * _range)) + _movingUnit.MyGridPosition;
            if (MyBattlefield.Gridsize.x <= newSquarePos.x || newSquarePos.x < 0 || MyBattlefield.Gridsize.y <= newSquarePos.y || newSquarePos.y < 0)
            {
                _range--;
                continue;
            }
            if ((MyBattlefield.Grid[newSquarePos.x, newSquarePos.y].Type & ETileType.WALKABLE) == ETileType.WALKABLE)
            {
                GoTowardsUnit(_movingUnit, newSquarePos, _range);
                break;
            }
            else
            {
                _range--;
            }
        }

        if (_range == 0)
        {
            _movingUnit.MovementIdle();
            //AddReward(largePunishment);
        }
    }

    /// <summary>
    /// Go Towards a unit using the units pathfinding
    /// </summary>
    /// <param name="_movingUnit"></param>
    /// <param name="_targetPosition"></param>
    /// <param name="_range"></param>
    /// <returns></returns>
    private int GoTowardsUnit(PlayerUnit _movingUnit, Vector2Int _targetPosition, int _range)
    {
        Vector2Int[] tmp = _movingUnit.NewGeneratePath(_targetPosition);
        Vector2Int[] tmp2;
        if (tmp.Length == 0)
        {
            tmp2 = new Vector2Int[0];
        }
        if (_range >= tmp.Length)
        {
            tmp2 = new Vector2Int[tmp.Length];
        }
        else
        {
            //tmp2 = new Vector2Int[tmp.Length];
            tmp2 = new Vector2Int[_range];
        }
        for (int i = 0; i < tmp2.Length; i++)
        {
            tmp2[i] = (tmp[tmp.Length - i - 1]);
            //MyBattlefield.Grid[tmp2Pos.x, tmp2Pos.y].ChangeTileMat(ETileMat.TEST2);
        }
        //MyBattlefield.Grid[tmpPos.x, tmpPos.y].ChangeTileMat(ETileMat.TEST);
        _movingUnit.MoveToPoint(tmp2);
        return tmp2.Length;
    }

    public override void FindTilesInRange()
    {
        if (CurrentIndex < MyUnits.Count)
            MyUnits[CurrentIndex].FindTilesInRange();
    }

    /// <summary>
    /// Rewards a unit for blocking damage
    /// </summary>
    /// <param name="_amountBlocked"></param>
    /// <param name="_blockingUnit"></param>
    public override void RewardForBlocking(int _amountBlocked, PlayerUnit _blockingUnit)
    {
        if (MyBattleManager.playerManager == this)
        {
            MyBattleManager.difficultyManager.DifficultyEventTrigger(EDifficultyEvent.PLAYER_DEFEND, true, _amountBlocked);
        }
        if (_blockingUnit.playerClass == PlayerClass.SUPPORT)
        {
            Debug.LogWarning("ERROR");
        }
        if (_amountBlocked <= _blockingUnit.defend1Armor)
        {
            //AddReward(smallReward);
            //AddReward(smallReward * 0.5f);
            return;
        }
        if (_amountBlocked <= _blockingUnit.defend2Armor)
        {
            //AddReward(mediumReward);
            //AddReward(smallReward);
            return;
        }
        if (_amountBlocked <= _blockingUnit.defend3Armor)
        {
            //AddReward(largeReward);
            //AddReward(mediumReward);
            return;
        }
    }

    /// <summary>
    /// Gets the distance to other units.
    /// </summary>
    /// <param name="_unit"></param>
    public override void GetDistanceToOtherUnits(PlayerUnit _unit)
    {
        for (int i = 0; i < maxUnitSize; i++)
        {
            DistanceToOtherUnits[i] = 4;
            //If the Unit is on the board
            if (EnemyUnits.Count > i)
            {
                if (_unit.PathfindingGrid[EnemyUnits[i].MyGridPosition.x, EnemyUnits[i].MyGridPosition.y] == 0)
                {
                    DistanceToOtherUnits[i] = 0;
                    continue;
                }
                if (_unit.PathfindingGrid[EnemyUnits[i].MyGridPosition.x, EnemyUnits[i].MyGridPosition.y] <= _unit.SprintRange)
                {
                    DistanceToOtherUnits[i] -= 1;
                }
                if (_unit.PathfindingGrid[EnemyUnits[i].MyGridPosition.x, EnemyUnits[i].MyGridPosition.y] <= _unit.AttackRange)
                {
                    DistanceToOtherUnits[i] -= 1;
                }
                if (_unit.PathfindingGrid[EnemyUnits[i].MyGridPosition.x, EnemyUnits[i].MyGridPosition.y] <= _unit.MovementRange)
                {
                    DistanceToOtherUnits[i] -= 1;
                }
            }
        }
        for (int i = 0; i < maxUnitSize; i++)
        {
            DistanceToOtherUnits[i] = 4;
            //If the Unit is on the board
            if (i < MyUnits.Count)
            {
                if (_unit.PathfindingGrid[MyUnits[i].MyGridPosition.x, MyUnits[i].MyGridPosition.y] == 0)
                {
                    DistanceToOtherUnits[i + maxUnitSize] = 0;
                    continue;
                }
                if (_unit.PathfindingGrid[MyUnits[i].MyGridPosition.x, MyUnits[i].MyGridPosition.y] <= _unit.SprintRange)
                {
                    DistanceToOtherUnits[i + maxUnitSize] -= 1;
                }
                if (_unit.PathfindingGrid[MyUnits[i].MyGridPosition.x, MyUnits[i].MyGridPosition.y] <= _unit.AttackRange)
                {
                    DistanceToOtherUnits[i + maxUnitSize] -= 1;
                }
                if (_unit.PathfindingGrid[MyUnits[i].MyGridPosition.x, MyUnits[i].MyGridPosition.y] <= _unit.MovementRange)
                {
                    DistanceToOtherUnits[i + maxUnitSize] -= 1;
                }
            }
        }
    }

    /// <summary>
    /// Gets triggered if an enemy unit was killed
    /// </summary>
    public override void EnemyUnitKilled()
    {
        //AddReward(largeReward);
        if (MyBattleManager.enemyManager == this)
        {
            MyBattleManager.difficultyManager.DifficultyEventTrigger(EDifficultyEvent.UNIT_DEATH, false, 1);
        }
        currentKilledEnemies++;
        if (enemiesKilledForEpisodeEnd <= currentKilledEnemies)
        {
            AddReward(largeReward);
            GridGenerator.Instance.EpisodesWon++;
            wonLastEpisode = EWonLastEpisode.ENEMY_WON;
            if (MyBattleManager.enemyManager == this)
            {
                MyBattleManager.difficultyManager.DifficultyEventTrigger(EDifficultyEvent.WIN, false, 1);
            }
            RewardEndOfRound();
            EndEpisode();
        }
    }

    /// <summary>
    /// Gets triggered if a player unit was killed
    /// </summary>
    public override void MyUnitKilled()
    {
        //AddReward(largePunishment);
        if (MyBattleManager.enemyManager == this)
        {
            MyBattleManager.difficultyManager.DifficultyEventTrigger(EDifficultyEvent.UNIT_DEATH, true, 1);
        }
        if (MyUnits.Count == 0)
        {
            AddReward(largePunishment);
            GridGenerator.Instance.EpisodesLost++;
            wonLastEpisode = EWonLastEpisode.PLAYER_WON;
            if (MyBattleManager.enemyManager == this)
            {
                MyBattleManager.difficultyManager.DifficultyEventTrigger(EDifficultyEvent.WIN, true, 1);
            }
            RewardEndOfRound();
            EndEpisode();
        }
    }

    /// <summary>
    /// Rewads the ai for a close battle (low hp and low amounts of units in game)
    /// </summary>
    private void RewardEndOfRound()
    {
        for (int i = 0; i < maxUnitSize; i++)
        {
            if (i >= MyUnits.Count)
            {
                AddReward(mediumReward);
            }
            else
            {
                AddReward(mediumReward * (1 - (MyUnits[i].HealthPoints / MyUnits[i].MaxHealthPoints)));
            }
            if (i >= EnemyUnits.Count)
            {
                AddReward(mediumReward);
            }
            else
            {
                AddReward(mediumReward * (1 - (EnemyUnits[i].HealthPoints / EnemyUnits[i].MaxHealthPoints)));
            }
        }
    }
}
