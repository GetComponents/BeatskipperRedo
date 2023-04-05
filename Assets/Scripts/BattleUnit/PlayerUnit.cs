using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;


public class PlayerUnit : BattleUnit, IHealth
{
    [SerializeField]
    UnitManager battleManagerRef;
    [HideInInspector]
    public UnitManager MyManager;
    public AIUnitManager AIManager;
    public int[,] PathfindingGrid;
    int[,] PathfindReset;
    List<Tile> tilesInRange;
    private bool hasSearchedForTiles;
    [SerializeField]
    GridManager myBattlefield => MyManager.MyBattlefield;
    public Vector3 StartPos;
    public List<Tile> MyPath;
    public ActionEvent MyActionEvent = new ActionEvent();

    public float Energy
    {
        get => m_energy;
        set
        {
            if (value >= MaxEnergy)
            {
                m_energy = MaxEnergy;
            }
            else if (value < 0)
            {
                m_energy = 0;
            }
            else
            {
                m_energy = value;
            }
        }
    }
    [SerializeField]
    private float m_energy;

    [SerializeField]
    public UnitStats myStats;

    public int MovementRange => myStats.MovementRange;
    public int SprintRange => myStats.SprintRange;
    public int AttackRange => myStats.AttackRange;

    public int movementCost => myStats.movementCost;
    public int sprintCost => myStats.sprintCost;
    public int attack1Cost => myStats.attack1Cost;
    public int attack2Cost => myStats.attack2Cost;
    public int attack3Cost => myStats.attack3Cost;
    public int defend1Cost => myStats.defend1Cost;
    public int defend2Cost => myStats.defend2Cost;
    public int defend3Cost => myStats.defend3Cost;

    public int attack1Dmg => myStats.attack1Dmg;
    public int attack2Dmg => myStats.attack2Dmg;
    public int attack3Dmg => myStats.attack3Dmg;
    public int defend1Armor => myStats.defend1Armor;
    public int defend2Armor => myStats.defend2Armor;
    public int defend3Armor => myStats.defend3Armor;

    public int MaxHP => myStats.MaxHP;
    public int MaxHealthPoints;
    public float MaxEnergy => myStats.maxEnergy;
    float movementSpeed => myStats.movementSpeed;

    public int buff1Strength => myStats.Buff1Strength;
    public int buff2Strength => myStats.Buff2Strength;
    public int buff3Strength => myStats.Buff3Strength;

    public float nerf1Strength => myStats.Nerf1Strength;
    public float nerf2Strength => myStats.Nerf2Strength;
    public float nerf3Strength => myStats.Nerf3Strength;


    public int currentArmor;

    //private StateEvent currentState;

    private Material lastMaterial;

    [SerializeField]
    bool trainingAI;

    public int SummarizedPos;

    public Coroutine CurrentCoroutine;

    public bool isPerformingAction;

    public int HealthPoints
    {
        get => healthPoints;
        set
        {
            healthPoints = value;
            OnHealthChanged?.Invoke();
            if (healthPoints <= 0)
            {
                Die();
            }
        }
    }

    public float MaxHealth => MaxHealthPoints;

    public float CurrentHealth => healthPoints;

    public UnityEvent OnHealthChange { get => OnHealthChanged; set { value = OnHealthChanged; } }

    [HideInInspector]
    public UnityEvent OnHealthChanged = new UnityEvent();

    [SerializeField]
    private int healthPoints;

    public enum EPathState
    {
        NONE,
        GoingToNextTile,
        GoingToCenterOfTile
    }
    public EPathState PathState;

    public enum EAction
    {
        IDLE,
        ATTACK,
        DEFEND,
        MOVEMENT,
        BUFFING,
        NERFING
    }
    private Vector2Int[] myPath;
    public int PathIndex = 0;

    public void GetSelected()
    {
        if (!hasSearchedForTiles)
        {
            PathfindingGrid = new int[myBattlefield.Gridsize.x, myBattlefield.Gridsize.y];
            PathfindReset = new int[myBattlefield.Gridsize.x, myBattlefield.Gridsize.y];
            for (int x = 0; x < myBattlefield.Gridsize.x; x++)
            {
                for (int y = 0; y < myBattlefield.Gridsize.y; y++)
                {
                    PathfindReset[x, y] = 100;
                }
            }
            hasSearchedForTiles = true;
        }
    }

    public void MyUpdate()
    {
    }

    private void Update()
    {
        MoveUnit();
    }

    private void MoveUnit()
    {
        switch (PathState)
        {
            case EPathState.NONE:
                break;
            case EPathState.GoingToNextTile:
                GoingToNextTile();
                break;
            case EPathState.GoingToCenterOfTile:
                GoingToCenterOfTile();
                break;
            default:
                break;
        }
    }

    private void GoingToCenterOfTile()
    {
        Vector3 direction = Vector3.Normalize(myBattlefield.Grid[MyGridPosition.x, MyGridPosition.y].transform.localPosition - transform.localPosition);
        transform.localPosition += new Vector3(direction.x, 0, direction.z) * movementSpeed * Time.deltaTime;

        //Center of Tile
        if (transform.localPosition.x >= myBattlefield.Grid[MyGridPosition.x, MyGridPosition.y].transform.localPosition.x - (movementSpeed * Time.deltaTime) &&
            transform.localPosition.x <= myBattlefield.Grid[MyGridPosition.x, MyGridPosition.y].transform.localPosition.x + (movementSpeed * Time.deltaTime) &&
            transform.localPosition.z >= myBattlefield.Grid[MyGridPosition.x, MyGridPosition.y].transform.localPosition.z - (movementSpeed * Time.deltaTime) &&
            transform.localPosition.z <= myBattlefield.Grid[MyGridPosition.x, MyGridPosition.y].transform.localPosition.z + (movementSpeed * Time.deltaTime))
        {
            transform.localPosition = myBattlefield.Grid[MyGridPosition.x, MyGridPosition.y].transform.localPosition;
            PathIndex++;
            if (myPath.Length > PathIndex)
            {
                PathState = EPathState.GoingToNextTile;
            }
            else
            {
                //isPerformingAction = false;
                MyManager.FindTilesInRange();
                if (myPath.Length > MovementRange)
                {
                    Energy -= sprintCost;
                }
                else
                {
                    Energy -= movementCost;
                }
                MyManager.ReduceBattleManagerWait();
                PathState = EPathState.NONE;
                PathIndex = 0;
                myPath = null;
            }
        }
    }

    public void ActionIdle()
    {
        MyManager.MyBattleManager.UnitsGettingEnergy[this] += 1;
        //Energy += 1;
        MyActionEvent?.Invoke(EAction.IDLE, 0);
    }

    public void MovementIdle()
    {
        MyManager.MyBattleManager.UnitsGettingEnergy[this] += 1;
        MyManager.ReduceBattleManagerWait();
    }

    private void ReduceCooldown(float _amount)
    {
        //Energy -= _amount;
    }

    private bool CheckForValidCooldown(int _amount)
    {
        return _amount <= Energy;
    }

    public void MoveToPoint(Tile tile)
    {
        Vector2Int[] tmp = NewGeneratePath(tile.PositionInGrid);
        if (tmp.Length > 0)
        {
            myPath = tmp;
            PathState = EPathState.GoingToNextTile;
        }
        else
        {
            MovementIdle();
        }
        //CurrentCoroutine = StartCoroutine(Walk(tmp));
    }

    public void MoveToPoint(Vector2Int[] _path)
    {
        if (_path.Length > 0)
        {
            myPath = _path;
            PathState = EPathState.GoingToNextTile;
        }
        else
        {
            MovementIdle();
        }
        //CurrentCoroutine = StartCoroutine(Walk(_path));
    }

    private PlayerUnit GetUnitOnTile(Tile selectedTile)
    {
        PlayerUnit tmp = null;
        if (isEnemy)
        {
            foreach (PlayerUnit _unit in MyManager.EnemyUnits)
            {
                if (_unit.MyGridPosition == selectedTile.PositionInGrid)
                {
                    tmp = _unit;
                    break;
                }
            }

        }
        else
        {
            foreach (PlayerUnit _unit in MyManager.MyUnits)
            {
                if (_unit.MyGridPosition == selectedTile.PositionInGrid)
                {
                    tmp = _unit;
                    break;
                }
            }
        }
        return tmp;
    }

    /// <summary>
    /// Attacks all Surrounding Enemies and returns: 0 = Enemies Hit, 1 = Enemies that did not block
    /// </summary>
    /// <param name="_weight"></param>
    /// <returns></returns>
    public int AttackSurrondingUnits(int _weight, float _multiplier)
    {
        //GeneratePathfindingNumbers(AttackRange);
        int hitEnemies = 0;
        for (int i = 0; i < MyManager.EnemyUnits.Count; i++)
        {
            if (PathfindingGrid[MyManager.EnemyUnits[i].MyGridPosition.x, MyManager.EnemyUnits[i].MyGridPosition.y] <= AttackRange)
            {
                hitEnemies++;
                switch (_weight)
                {
                    case 0:
                        MyManager.MyBattleManager.UnitsGettingDamaged[MyManager.EnemyUnits[i]] += Mathf.FloorToInt(attack1Dmg * _multiplier);
                        break;
                    case 1:
                        MyManager.MyBattleManager.UnitsGettingDamaged[MyManager.EnemyUnits[i]] += Mathf.FloorToInt(attack2Dmg * _multiplier);
                        break;
                    case 2:
                        MyManager.MyBattleManager.UnitsGettingDamaged[MyManager.EnemyUnits[i]] += Mathf.FloorToInt(attack3Dmg * _multiplier);
                        break;
                    default:
                        break;
                }
            }
        }
        ReduceEnergy(EAction.ATTACK, _weight);
        MyActionEvent?.Invoke(EAction.ATTACK, _weight);
        //Debug.Log($"{gameObject.name} hit {hitEnemies} units with weight of {_weight}");
        return hitEnemies;
    }

    /// <summary>
    /// Gives hit allies Energy back (TODO: doesnt quite seem to work right now)
    /// </summary>
    /// <param name="_weight"></param>
    /// <returns></returns>
    public int EnergySupport(int _weight)
    {
        int hitAllies = 0;
        for (int i = 0; i < MyManager.MyUnits.Count; i++)
        {
            if (PathfindingGrid[MyManager.MyUnits[i].MyGridPosition.x, MyManager.MyUnits[i].MyGridPosition.y] > AttackRange)
            {
                continue;
            }
            hitAllies++;
            switch (_weight)
            {
                case 0:
                    MyManager.MyBattleManager.UnitsGettingEnergy[MyManager.MyUnits[i]] += buff1Strength;
                    break;
                case 1:
                    MyManager.MyBattleManager.UnitsGettingEnergy[MyManager.MyUnits[i]] += buff2Strength;
                    break;
                case 2:
                    MyManager.MyBattleManager.UnitsGettingEnergy[MyManager.MyUnits[i]] += buff3Strength;
                    break;
                default:
                    break;
            }

        }
        ReduceEnergy(EAction.DEFEND, _weight);
        MyActionEvent?.Invoke(EAction.DEFEND, _weight);
        return hitAllies;
    }

    /// <summary>
    /// Debuffs hit enemies to take % more damage
    /// </summary>
    /// <param name="_weight"></param>
    public void StatSupport(int _weight)
    {
        for (int i = 0; i < MyManager.EnemyUnits.Count; i++)
        {
            if (PathfindingGrid[MyManager.EnemyUnits[i].MyGridPosition.x, MyManager.EnemyUnits[i].MyGridPosition.y] > AttackRange)
            {
                continue;
            }
            switch (_weight)
            {
                case 0:
                    MyManager.MyBattleManager.UnitsGettingNerfed[MyManager.EnemyUnits[i]] *= nerf1Strength;
                    break;
                case 1:
                    MyManager.MyBattleManager.UnitsGettingNerfed[MyManager.EnemyUnits[i]] *= nerf2Strength;
                    break;
                case 2:
                    MyManager.MyBattleManager.UnitsGettingNerfed[MyManager.EnemyUnits[i]] *= nerf3Strength;
                    break;
                default:
                    break;
            }

        }
        //Debug.Log($"{gameObject.name} hit {hitEnemies} units with weight of {_weight}");
    }

    /// <summary>
    /// Gives unit armor
    /// </summary>
    /// <param name="_weight"></param>
    /// <param name="_multiplier"></param>
    public void Defend(int _weight, float _multiplier)
    {
        //Debug.Log($"{gameObject.name} is defending");
        switch (_weight)
        {
            case 0:
                currentArmor = Mathf.FloorToInt(defend1Armor * _multiplier);
                break;
            case 1:
                currentArmor = Mathf.FloorToInt(defend2Armor * _multiplier);
                break;
            case 2:
                currentArmor = Mathf.FloorToInt(defend3Armor * _multiplier);
                break;
            default:
                break;
        }
        ReduceEnergy(EAction.DEFEND, _weight);
        MyActionEvent?.Invoke(EAction.DEFEND, _weight);
    }

    /// <summary>
    /// Checks whether this unit has enough energy to perform specific action
    /// </summary>
    /// <param name="_action"></param>
    /// <param name="_weight"></param>
    /// <returns></returns>
    public bool HaveEnoughEnergyToPerformAction(EAction _action, int _weight)
    {
        switch (_action)
        {
            //Movement
            case EAction.MOVEMENT:
                switch (_weight)
                {
                    case 0:
                        return movementCost <= Energy;
                    case 1:
                        return sprintCost <= Energy;
                    default:
                        break;
                }
                break;
            //Attack
            case EAction.ATTACK:
                switch (_weight)
                {
                    case 0:
                        return attack1Cost <= Energy;
                    case 1:
                        return attack2Cost <= Energy;
                    case 2:
                        return attack3Cost <= Energy;
                    default:
                        break;
                }
                break;
            //Defend
            case EAction.DEFEND:
                switch (_weight)
                {
                    case 0:
                        return defend1Cost <= Energy;
                    case 1:
                        return defend2Cost <= Energy;
                    case 2:
                        return defend3Cost <= Energy;
                    default:
                        break;
                }
                break;
            default:
                break;
        }
        return false;
    }

    /// <summary>
    /// Returns the cost of performing a specific action
    /// </summary>
    /// <param name="_action"></param>
    /// <param name="_weight"></param>
    /// <returns></returns>
    public int CostOfPerformingAction(EAction _action, int _weight)
    {
        int cost = 0;
        switch (_action)
        {
            case EAction.MOVEMENT:
                switch (_weight)
                {
                    case 0:
                        cost += movementCost;
                        break;
                    case 1:
                        cost += sprintCost;
                        break;
                }
                break;
            case EAction.ATTACK:
                switch (_weight)
                {
                    case 0:
                        cost += attack1Cost;
                        break;
                    case 1:
                        cost += attack2Cost;
                        break;
                    case 2:
                        cost += attack3Cost;
                        break;
                    default:
                        break;
                }
                break;
            case EAction.DEFEND:
                switch (_weight)
                {
                    case 0:
                        cost += defend1Cost;
                        break;
                    case 1:
                        cost += defend2Cost;
                        break;
                    case 2:
                        cost += defend3Cost;
                        break;
                }
                break;
            default:
                break;
        }
        return cost;
    }

    /// <summary>
    /// Checks whether this unit has enough energy to perform action and movement
    /// </summary>
    /// <param name="_action"></param>
    /// <param name="_weight"></param>
    /// <param name="_movement"></param>
    /// <returns></returns>
    public bool HaveEnoughEnergyToPerformAction(EAction _action, int _weight, int _movement)
    {
        int cost = 0;
        switch (_action)
        {
            case EAction.MOVEMENT:
                break;
            case EAction.ATTACK:
                switch (_weight)
                {
                    case 0:
                        cost += attack1Cost;
                        break;
                    case 1:
                        cost += attack2Cost;
                        break;
                    case 2:
                        cost += attack3Cost;
                        break;
                    default:
                        break;
                }
                break;
            case EAction.DEFEND:
                switch (_weight)
                {
                    case 0:
                        cost += defend1Cost;
                        break;
                    case 1:
                        cost += defend2Cost;
                        break;
                    case 2:
                        cost += defend3Cost;
                        break;
                }
                break;
            default:
                break;
        }
        switch (_movement)
        {
            case 0:
                break;
            case 1:
                cost += movementCost;
                break;
            case 2:
                cost += sprintCost;
                break;
            default:
                break;
        }
        return cost <= Energy;
    }

    /// <summary>
    /// Reduced energy of unit
    /// </summary>
    /// <param name="_action"></param>
    /// <param name="_weight"></param>
    public void ReduceEnergy(EAction _action, int _weight)
    {
        switch (_action)
        {
            case EAction.MOVEMENT:
                switch (_weight)
                {
                    case 0:
                        Energy -= movementCost;
                        return;
                    case 1:
                        Energy -= sprintCost;
                        return;
                    default:
                        break;
                }
                break;
            case EAction.ATTACK:
                switch (_weight)
                {
                    case 0:
                        Energy -= attack1Cost;
                        return;
                    case 1:
                        Energy -= attack2Cost;
                        return;
                    case 2:
                        Energy -= attack3Cost;
                        return;
                    default:
                        break;
                }
                break;
            case EAction.DEFEND:
                switch (_weight)
                {
                    case 0:
                        Energy -= defend1Cost;
                        return;
                    case 1:
                        Energy -= defend2Cost;
                        return;
                    case 2:
                        Energy -= defend3Cost;
                        return;
                    default:
                        break;
                }
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Changes the position of the unit on the grid
    /// </summary>
    /// <param name="_nextPosition"></param>
    public void MoveUnitToPosition(Vector2Int _nextPosition)
    {
        if ((myBattlefield.Grid[MyGridPosition.x, MyGridPosition.y].Type & ETileType.WITHUNIT) == ETileType.WITHUNIT)
        {
            myBattlefield.Grid[MyGridPosition.x, MyGridPosition.y].Type &= ~ETileType.WITHUNIT;
        }
        MyGridPosition = _nextPosition;
        myBattlefield.Grid[MyGridPosition.x, MyGridPosition.y].Type |= ETileType.WITHUNIT;
    }

    #region Pathfinding

    /// <summary>
    /// Resets the numbers used for pathfinding and marks the attackrange of the unit
    /// </summary>
    /// <param name="range"></param>
    public void FindTilesInRange()
    {
        GeneratePathfindingNumbers();
        MyManager.GetDistanceToOtherUnits(this);
        for (int i = 0; i < myBattlefield.Gridsize.x; i++)
        {
            for (int j = 0; j < myBattlefield.Gridsize.y; j++)
            {
                if ((myBattlefield.UnitView[i, j] & ETileMask.REACHABLE) == ETileMask.REACHABLE && (myBattlefield.Grid[i, j].Type & ETileType.WALKABLE) == ETileType.WALKABLE)
                {
                    myBattlefield.Grid[i, j].ChangeTileMat(ETileMat.INRANGE);
                }
                else if ((myBattlefield.UnitView[i, j] & ETileMask.NONE) == ETileMask.NONE && (myBattlefield.Grid[i, j].Type & ETileType.WALKABLE) == ETileType.WALKABLE)
                {
                    myBattlefield.Grid[i, j].ChangeTileMat(ETileMat.DEFAULT);
                }
                myBattlefield.UnitView[i, j] |= ETileMask.ENVIRONMENT;
            }
        }
        foreach (var unit in MyManager.MyUnits)
        {
            myBattlefield.UnitView[(int)unit.MyGridPosition.x, (int)unit.MyGridPosition.y] ^= ETileMask.ENVIRONMENT;
            myBattlefield.UnitView[(int)unit.MyGridPosition.x, (int)unit.MyGridPosition.y] |= ETileMask.ENEMY;
        }
        foreach (var unit in MyManager.EnemyUnits)
        {
            myBattlefield.UnitView[(int)unit.MyGridPosition.x, (int)unit.MyGridPosition.y] ^= ETileMask.ENVIRONMENT;
            myBattlefield.UnitView[(int)unit.MyGridPosition.x, (int)unit.MyGridPosition.y] |= ETileMask.PLAYER;
        }
    }

    /// <summary>
    /// Resets the numbers used for pathfinding
    /// </summary>
    /// <param name="range"></param>
    private void GeneratePathfindingNumbers()
    {
        if (PathfindingGrid == null)
        {
            GetSelected();
        }
        for (int x = 0; x < myBattlefield.Gridsize.x; x++)
        {
            for (int y = 0; y < myBattlefield.Gridsize.y; y++)
            {
                PathfindingGrid[x, y] = PathfindReset[x, y];
            }
        }

        PathfindingGrid[MyGridPosition.x, MyGridPosition.y] = 0;
        List<Tile> unreviewedTiles = new List<Tile>();
        List<Tile> reviewedTiles = new List<Tile>();

        unreviewedTiles.Add(myBattlefield.Grid[MyGridPosition.x, MyGridPosition.y]);
        while (unreviewedTiles.Count > 0)
        {
            int x = unreviewedTiles[0].PositionInGrid.x;
            int y = unreviewedTiles[0].PositionInGrid.y;

            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    Vector2Int direction = new Vector2Int(i, j);
                    //Skips diagonal and the main block example: (1/1), (0/0), (-1/1) 
                    if (Mathf.Abs(i) == Mathf.Abs(j))
                    {
                        continue;
                    }
                    //Skips things outside of bounds
                    else if (!myBattlefield.VectorIsInBounds(unreviewedTiles[0].PositionInGrid + direction))
                    {
                        continue;
                    }
                    //Skip if it's already contained in any list or not walkable
                    else if ((myBattlefield.Grid[x + direction.x, y + direction.y].Type & ETileType.WALKABLE) != ETileType.WALKABLE
                        || reviewedTiles.Contains(myBattlefield.Grid[x + direction.x, y + direction.y])
                        || unreviewedTiles.Contains(myBattlefield.Grid[x + direction.x, y + direction.y]))
                    {
                        continue;
                    }
                    //Skip if its a wall
                    else if ((myBattlefield.Grid[x + direction.x, y + direction.y].Type & ETileType.WALL) == ETileType.WALL)
                    {
                        continue;
                    }
                    if (PathfindingGrid[x + direction.x, y + direction.y] > PathfindingGrid[x, y])
                    {
                        PathfindingGrid[x + direction.x, y + direction.y] = PathfindingGrid[x, y] + 1;
                        unreviewedTiles.Add(myBattlefield.Grid[x + direction.x, y + direction.y]);
                        if (PathfindingGrid[x + direction.x, y + direction.y] <= AttackRange)
                        {
                            myBattlefield.UnitView[x + direction.x, y + direction.y] = ETileMask.REACHABLE;
                        }
                        else
                        {
                            myBattlefield.UnitView[x + direction.x, y + direction.y] = ETileMask.NONE;

                        }
                    }

                }
            }
            reviewedTiles.Add(unreviewedTiles[0]);
            unreviewedTiles.RemoveAt(0);
        }
    }

    #endregion Pathfinding

    #region NewPathfinding

    /// <summary>
    /// Generates a path from a point to this unit
    /// </summary>
    /// <param name="startingPosition"></param>
    /// <returns>the created path</returns>
    public Vector2Int[] NewGeneratePath(Vector2Int startingPosition)
    {
        bool foundSmallerNumber = false;
        Vector2Int currentPos = startingPosition;
        List<Vector2Int> path = new List<Vector2Int>();
        int currentSmallestNumber;
        currentSmallestNumber = 100;
        if (PathfindingGrid == null)
        {
            Debug.Log("h");
        }
        do
        {
            currentSmallestNumber = 100;
            foundSmallerNumber = false;
            Vector2Int currentBestDirection = new Vector2Int();
            //Looks at the directly connecting blocks
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    Vector2Int direction = new Vector2Int(i, j);
                    //Skips diagonal and the main block example: (1/1), (0/0), (-1/1) 
                    if (Mathf.Abs(i) == Mathf.Abs(j))
                    {
                        continue;
                    }
                    //Skips things outside of bounds
                    else if (!myBattlefield.VectorIsInBounds(currentPos + direction))
                    {
                        continue;
                    }
                    //Skips if it is not a better number
                    else if (PathfindingGrid[currentPos.x + direction.x, currentPos.y + direction.y] >= currentSmallestNumber)
                    {
                        continue;
                    }
                    //Skips if theres a unit on a block
                    //else if ((myBattlefield.Grid[currentPos.x + direction.x, currentPos.y + direction.y].Type & ETileType.WITHUNIT) == ETileType.WITHUNIT
                    //        && PathfindingGrid[currentPos.x + direction.x, currentPos.y + direction.y] > 0)
                    //{
                    //    continue;
                    //}
                    //Skips if it's a wall
                    else if ((myBattlefield.Grid[currentPos.x + direction.x, currentPos.y + direction.y].Type & ETileType.WALL) == ETileType.WALL)
                    {
                        continue;
                    }
                    currentSmallestNumber = PathfindingGrid[currentPos.x + direction.x, currentPos.y + direction.y];
                    currentBestDirection = direction;
                    foundSmallerNumber = true;
                }
            }
            //To prevent it from going back and fourth
            if (path.Count > 0 && currentBestDirection + path[path.Count - 1] == Vector2Int.zero)
            {
                break;
            }
            //To prevent infinite loops
            if (path.Count > 40)
            {
                break;
            }
            if (currentSmallestNumber == 0)
            {
                break;
            }
            if (foundSmallerNumber)
            {
                currentPos = new Vector2Int(currentPos.x + currentBestDirection.x, currentPos.y + currentBestDirection.y);
                path.Add(currentPos);
            }
        } while (foundSmallerNumber);
        string debugText = "";
        for (int i = 0; i < path.Count; i++)
        {
            debugText += $"{path[i]}, ";
        }
        //Debug.Log(debugText);
        return path.ToArray();
    }

    /// <summary>
    /// Brings the unit to the next tile in the path
    /// </summary>
    private void GoingToNextTile()
    {
        Vector2Int tmp = myBattlefield.Grid[myPath[PathIndex].x, myPath[PathIndex].y].PositionInGrid;
        //transform.localPosition += Vector3.Normalize(new Vector3(tmp.x - transform.localPosition.x, 0, tmp.y - transform.position.z)) * movementSpeed * Time.deltaTime;
        //transform.localPosition += Vector3.Normalize(new Vector3(tmp.x - transform.localPosition.x, 0, tmp.y - transform.position.z));
        transform.localPosition = new Vector3(tmp.x, transform.localPosition.y, tmp.y);
        Vector2Int currentPosition = new Vector2Int(Mathf.FloorToInt(transform.localPosition.x), Mathf.FloorToInt(transform.localPosition.z));
        if (currentPosition != MyGridPosition)
        {
            if (!myBattlefield.VectorIsInBounds(currentPosition))
            {
                myBattlefield.Grid[MyGridPosition.x, MyGridPosition.y].Type |= ETileType.WITHUNIT;
                myPath = new Vector2Int[0];
                PathState = EPathState.GoingToCenterOfTile;
                Debug.Log($"{HealthPoints} I was out of BOUNDS!");
                return;
            }
            if ((myBattlefield.Grid[currentPosition.x, currentPosition.y].Type & ETileType.WITHUNIT) == ETileType.WITHUNIT)
            {
                PathState = EPathState.GoingToCenterOfTile;
                //Debug.Log($"{HealthPoints}I walked into a PLAYER!");
                myPath = new Vector2Int[0];
                return;
            }
            MoveUnitToPosition(currentPosition);
            PathState = EPathState.GoingToCenterOfTile;
            //Debug.Log($"{HealthPoints}I Reached the end of my path");
        }
    }

    #endregion NewPathfinding

    /// <summary>
    /// Triggered when unit dies
    /// </summary>
    private void Die()
    {
        if ((myBattlefield.Grid[MyGridPosition.x, MyGridPosition.y].Type & ETileType.WITHUNIT) == ETileType.WITHUNIT)
        {
            myBattlefield.Grid[MyGridPosition.x, MyGridPosition.y].Type &= ~ETileType.WITHUNIT;
        }
        MyManager.MyBattleManager.DestroyUnit(this);
    }
}
