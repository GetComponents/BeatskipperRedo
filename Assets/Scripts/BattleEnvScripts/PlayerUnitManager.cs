using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//[System.Serializable]
//public class UnitEvent : UnityEvent<PlayerUnit>{}

public class PlayerUnitManager : UnitManager
{

    bool planningCombat;
    bool hoveringUnit;
    Tile highlightedTile;
    public UnityEvent UnitSelected;
    [SerializeField]
    GameObject movemenSignifier;
    [SerializeField]
    Material movementMat, sprintMat;
    List<GameObject> PathSignifiers;
    public UnityEvent OnStartCombat;

    /// <summary>
    /// States: 0 = idle, 1 = move, 2 = run
    /// </summary>
    public Dictionary<PlayerUnit, int> MovementState = new Dictionary<PlayerUnit, int>();

    public Dictionary<PlayerUnit, int> EnergyUsedThisTurn = new Dictionary<PlayerUnit, int>();

    public PlayerUnit SelectedUnit
    {
        get => m_selectedUnit;
        set
        {
            m_selectedUnit = value;
            UnitSelected?.Invoke();
        }
    }
    PlayerUnit m_selectedUnit;


    private void Start()
    {
        PathSignifiers = new List<GameObject>();
        OnStartCombat.AddListener(EmptyUnitMoves);
        UnitSelected.AddListener(FindTilesInRange);
        UnitSelected.AddListener(SelectUnit);
    }

    private void Update()
    {
        if (planningCombat)
        {
            ReadInputs();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    #region PlayerInput

    /// <summary>
    /// Reads the mousposition of the player and if he pressed m0
    /// </summary>
    private void ReadInputs()
    {
        Tile hoveredTile = HoverOverTile();
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (hoveredTile == null)
                return;
            if ((hoveredTile.Type & ETileType.WITHUNIT) == ETileType.WITHUNIT)
            {
                RemovePathSignifiers(false);
                if (IdentifySelectedUnit(hoveredTile.PositionInGrid) == 1)
                {
                    if (SelectedUnit.MyPath == null)
                    {
                        SelectedUnit.MyPath.Clear();
                    }
                    ShowPathSignifiers();
                    hoveringUnit = true;
                }
            }
            else
            {
                if (SelectedUnit == null || SelectedUnit.MyPath == null)
                    return;
                if (SelectedUnit.MyPath.Contains(hoveredTile))
                {
                    hoveringUnit = true;
                    CutDownPath(hoveredTile);
                    return;
                }
                //MakePathToTile(hoveredTile);
            }
        }
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            hoveringUnit = false;
            //if (SelectedUnit != null && !SelectedUnit.isEnemy)
            //{
            //    DropOnTile();
            //}
        }
        if (hoveringUnit)
        {
            SelectPath(hoveredTile);
        }
    }

    /// <summary>
    /// Used to add tiles to the path
    /// </summary>
    /// <param name="_hoveredTile"></param>
    private void SelectPath(Tile _hoveredTile)
    {
        if (_hoveredTile == null)
            return;
        if (SelectedUnit.MyPath.Count != 0 && SelectedUnit.MyPath[SelectedUnit.MyPath.Count - 1] == _hoveredTile)
            return;
        if ((_hoveredTile.Type & ETileType.WALL) == ETileType.WALL
            || ((_hoveredTile.Type & ETileType.WITHUNIT) == ETileType.WITHUNIT && _hoveredTile.PositionInGrid != SelectedUnit.MyGridPosition))
        {
            return;
        }
        if (SelectedUnit.MyPath.Contains(_hoveredTile))
        {
            CutDownPath(_hoveredTile);
            return;
        }
        if (_hoveredTile.PositionInGrid == SelectedUnit.MyGridPosition && SelectedUnit.MyPath.Count == 1)
        {
            SetMovementState(0);
            CutDownPath(_hoveredTile);
            return;
        }
        if (!IsNextToPreviousTile(_hoveredTile))
        {
            return;
        }
        if (SelectedUnit.MyPath.Count < SelectedUnit.MovementRange &&
            (MovementState[SelectedUnit] == 1 || SelectedUnit.CostOfPerformingAction(PlayerUnit.EAction.MOVEMENT, 0) + EnergyUsedThisTurn[SelectedUnit] <= SelectedUnit.Energy))
        {
            SelectedUnit.MyPath.Add(_hoveredTile);
            SpawnMovementSignifier(new Vector3(_hoveredTile.PositionInGrid.x,
                SelectedUnit.transform.localPosition.y, _hoveredTile.PositionInGrid.y), movementMat);
            SetMovementState(1);
        }
        else if (SelectedUnit.MyPath.Count < SelectedUnit.SprintRange &&
            (MovementState[SelectedUnit] == 2 || SelectedUnit.CostOfPerformingAction(PlayerUnit.EAction.MOVEMENT, 1) + EnergyUsedThisTurn[SelectedUnit] <= SelectedUnit.Energy))
        {
            SelectedUnit.MyPath.Add(_hoveredTile);
            SpawnMovementSignifier(new Vector3(_hoveredTile.PositionInGrid.x,
                SelectedUnit.transform.localPosition.y, _hoveredTile.PositionInGrid.y), sprintMat);
            SetMovementState(2);
        }
        FindTilesInTempRange();
    }

    /// <summary>
    /// Checks whether a tile is next to the previous one or to the player
    /// </summary>
    /// <param name="_tile"></param>
    /// <returns></returns>
    private bool IsNextToPreviousTile(Tile _tile)
    {
        if (SelectedUnit.MyPath.Count == 0)
        {
            if ((Mathf.Abs(SelectedUnit.MyGridPosition.x - _tile.PositionInGrid.x)
        + Mathf.Abs(SelectedUnit.MyGridPosition.y - _tile.PositionInGrid.y) == 1))
            {
                return true;
            }
            return false;
        }
        if (_tile.PositionInGrid == SelectedUnit.MyGridPosition || (Mathf.Abs(SelectedUnit.MyPath[SelectedUnit.MyPath.Count - 1].PositionInGrid.x - _tile.PositionInGrid.x)
        + Mathf.Abs(SelectedUnit.MyPath[SelectedUnit.MyPath.Count - 1].PositionInGrid.y - _tile.PositionInGrid.y) == 1))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Cuts down the path to the position of the hovered tile
    /// </summary>
    /// <param name="_hoveredTile"></param>
    private void CutDownPath(Tile _hoveredTile)
    {
        bool cutDown = false;
        List<Tile> tmpList = new List<Tile>();
        List<GameObject> sigToDestroy = new List<GameObject>();
        if (_hoveredTile.PositionInGrid == SelectedUnit.MyGridPosition)
        {
            cutDown = true;
        }
        for (int i = 0; i < SelectedUnit.MyPath.Count; i++)
        {
            tmpList.Add(SelectedUnit.MyPath[i]);
        }
        for (int i = 0; i < SelectedUnit.MyPath.Count; i++)
        {
            if (cutDown == true)
            {
                sigToDestroy.Add(PathSignifiers[i]);
                SelectedUnit.MyPath.Remove(tmpList[i]);
            }
            if (tmpList[i] == _hoveredTile)
            {
                cutDown = true;
            }
        }
        int amountToDelete = tmpList.Count - SelectedUnit.MyPath.Count;
        for (int i = 0; i < amountToDelete; i++)
        {
            Destroy(PathSignifiers[SelectedUnit.MyPath.Count + i]);
        }
        foreach (GameObject go in sigToDestroy)
        {
            PathSignifiers.Remove(go);
            Destroy(go);
        }

        if (PathSignifiers.Count == 0)
        {
            SetMovementState(0);
        }
        else if (PathSignifiers.Count <= SelectedUnit.MovementRange)
        {
            SetMovementState(1);
        }
        else
        {
            SetMovementState(2);
        }
        FindTilesInTempRange();
    }

    private void SpawnMovementSignifier(Vector3 _position, Material _color)
    {
        GameObject tmp = Instantiate(movemenSignifier, _position, Quaternion.identity);
        tmp.GetComponent<MeshRenderer>().material = _color;
        PathSignifiers.Add(tmp);
    }

    /// <summary>
    /// Reveals path signifiers
    /// </summary>
    private void ShowPathSignifiers()
    {
        foreach (Tile _tile in SelectedUnit.MyPath)
        {
            //Debug.Log($"Count: {PathSignifiers.Count} Range: {SelectedUnit.MovementRange}");
            if (PathSignifiers.Count < SelectedUnit.MovementRange)
            {
                SpawnMovementSignifier(new Vector3(_tile.PositionInGrid.x,
                        SelectedUnit.transform.localPosition.y, _tile.PositionInGrid.y), movementMat);
            }
            else
            {
                SpawnMovementSignifier(new Vector3(_tile.PositionInGrid.x,
                    SelectedUnit.transform.localPosition.y, _tile.PositionInGrid.y), sprintMat);
            }
        }
    }

    /// <summary>
    /// Highlights and returns the tile that is hovered by the mouse
    /// </summary>
    /// <returns></returns>
    private Tile HoverOverTile()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            Tile _foundTile;
            if (hit.transform.TryGetComponent<Tile>(out _foundTile))
            {
                if (highlightedTile == null)
                {
                    highlightedTile = _foundTile;
                    highlightedTile.ChangeTileMat(ETileMat.HIGHLIGHT);
                }
                else if ((_foundTile.Type & ETileType.WALL) == ETileType.WALL)
                {
                    return null;
                }
                else if (highlightedTile != _foundTile)
                {
                    highlightedTile.ChangeTileMat(ETileMat.UNHHIGHLIGHT);
                    highlightedTile = _foundTile;
                    highlightedTile.ChangeTileMat(ETileMat.HIGHLIGHT);
                }
                return _foundTile;
            }
        }
        return null;
    }

    /// <summary>
    /// Returns the type of unit that is in a position
    /// </summary>
    /// <param name="_position"></param>
    /// <returns>0: nothing, 1: Player Unit, 2: Enemy Units</returns>
    private int IdentifySelectedUnit(Vector2Int _position)
    {
        foreach (PlayerUnit unit in MyUnits)
        {
            if (unit.MyGridPosition == _position)
            {
                SelectedUnit = unit;
                return 1;
            }
        }
        foreach (PlayerUnit unit in EnemyUnits)
        {
            if (unit.MyGridPosition == _position)
            {
                SelectedUnit = unit;
                return 2;
            }
        }
        return 0;
    }
    #endregion PlayerInput

    /// <summary>
    /// makes sure that both Movementstate and EnergyUsedThisTurn contain all playable units
    /// </summary>
    private void SelectUnit()
    {
        if (!EnergyUsedThisTurn.ContainsKey(SelectedUnit))
        {
            EnergyUsedThisTurn.Add(SelectedUnit, 0);
        }
        if (!MovementState.ContainsKey(SelectedUnit))
        {
            MovementState.Add(SelectedUnit, 0);
        }
    }

    /// <summary>
    /// Readies up the player, as that the combat can begin
    /// </summary>
    public void ReadyUp()
    {
        TookDecision = true;
        MyBattleManager.ManagerReadyCheck();
        EnergyUsedThisTurn = new Dictionary<PlayerUnit, int>();
        MovementState = new Dictionary<PlayerUnit, int>();
    }

    /// <summary>
    /// Reduced the "wait" of the battlemanager, which means that a unit stopped moving
    /// </summary>
    public override void ReduceBattleManagerWait()
    {
        MyBattleManager.MovementsToResolve--;
    }

    /// <summary>
    /// Units perform their movement
    /// </summary>
    public override void PerformMovements()
    {
        RemovePathSignifiers(false);
        for (int h = 0; h < MyUnits.Count; h++)
        {
            MoveToPoint(MyUnits[h]);
        }
    }

    /// <summary>
    /// makes it so that the players unit moves according to their path
    /// </summary>
    /// <param name="_unit"></param>
    private void MoveToPoint(PlayerUnit _unit)
    {
        MyBattleManager.MovementsToResolve++;
        if (_unit.MyPath.Count == 0)
        {
            _unit.MovementIdle();
            return;
        }
        Vector2Int[] tmpPath = new Vector2Int[_unit.MyPath.Count];
        for (int i = 0; i < tmpPath.Length; i++)
        {

            tmpPath[i] = _unit.MyPath[i].PositionInGrid;
        }
        _unit.MoveToPoint(tmpPath);
        _unit.MyPath.Clear();
    }

    /// <summary>
    /// Removes the pathsignifiers and optionally also the paths themselves
    /// </summary>
    /// <param name="_alsoDeleteUnitPaths"></param>
    public void RemovePathSignifiers(bool _alsoDeleteUnitPaths)
    {
        foreach (GameObject go in PathSignifiers)
        {
            Destroy(go);
        }
        PathSignifiers.Clear();
        if (_alsoDeleteUnitPaths)
        {
            for (int i = 0; i < MyUnits.Count; i++)
            {
                MyUnits[i].MyPath.Clear();
            }
        }
    }

    /// <summary>
    /// Rebuilds the Pathfinding and showcases which tiles are in attack range
    /// </summary>
    public override void FindTilesInRange()
    {
        if (SelectedUnit != null)
        {
            SelectedUnit.FindTilesInRange();
        }
        FindTilesInTempRange();
    }

    /// <summary>
    /// Shows what the range looks like when moved (does not 100% display it correctly, since walls and other units are ignored)
    /// </summary>
    public void FindTilesInTempRange()
    {
        if (SelectedUnit.MyPath == null || SelectedUnit.MyPath.Count == 0)
        {
            return;
        }
        Tile lastTile = SelectedUnit.MyPath[SelectedUnit.MyPath.Count - 1];

        for (int y = 0; y < MyBattlefield.Grid.GetLength(1); y++)
        {
            for (int x = 0; x < MyBattlefield.Grid.GetLength(0); x++)
            {
                if (Mathf.Abs(lastTile.PositionInGrid.x - x) + Mathf.Abs(lastTile.PositionInGrid.y - y) <= SelectedUnit.AttackRange
                    && (MyBattlefield.Grid[x, y].Type & ETileType.WALKABLE) == ETileType.WALKABLE)
                {
                    MyBattlefield.Grid[x, y].ChangeTileMat(ETileMat.INRANGE);
                }
                else if ((MyBattlefield.Grid[x, y].Type & ETileType.WALKABLE) == ETileType.WALKABLE)
                {
                    MyBattlefield.Grid[x, y].ChangeTileMat(ETileMat.DEFAULT);
                }
            }
        }
    }

    /// <summary>
    /// Starts the combats planning phase
    /// </summary>
    public override void PlanCombat()
    {
        planningCombat = true;
        RemovePathSignifiers(true);

        OnStartCombat?.Invoke();
    }

    /// <summary>
    /// Empties the myUnityMoves array
    /// </summary>
    private void EmptyUnitMoves()
    {
        foreach (PlayerUnit _unit in MyUnits)
        {
            myUnitMoves[_unit] = new int[4];
        }
    }

    /// <summary>
    /// Makes units perform their planned action
    /// </summary>
    public override void PerformAction()
    {
        EnergyUsedThisTurn = new Dictionary<PlayerUnit, int>();
        for (int h = 0; h < MyUnits.Count; h++)
        {
            switch (myUnitMoves[MyUnits[h]][2])
            {
                case 0:
                    //Idle
                    MyUnits[h].ActionIdle();
                    break;
                case 1:
                    //Attack
                    if (MyUnits[h].playerClass == PlayerClass.SUPPORT)
                    {
                        MyUnits[h].StatSupport(myUnitMoves[MyUnits[h]][3]);
                    }
                    MyUnits[h].AttackSurrondingUnits(myUnitMoves[MyUnits[h]][3], 1);
                    break;
                case 2:
                    //Defend
                    if (MyUnits[h].playerClass == PlayerClass.SUPPORT)
                    {
                        MyUnits[h].EnergySupport(myUnitMoves[MyUnits[h]][3]);
                        return;
                    }
                    MyUnits[h].Defend(myUnitMoves[MyUnits[h]][3], 1);
                    break;
                default:
                    break;
            }
        }
    }


    /// <summary>
    /// Used to set a movement state (used to calculate how much energy is used in a turn)
    /// </summary>
    /// <param name="_state"></param>
    private void SetMovementState(int _state)
    {
        if (MovementState[SelectedUnit] != _state)
        {
            int oldCost = 0, newCost = 0;
            switch (_state)
            {
                case 0:
                    newCost = 0;
                    break;
                case 1:
                    newCost = SelectedUnit.movementCost;
                    break;
                case 2:
                    newCost = SelectedUnit.sprintCost;
                    break;
                default:
                    break;
            }
            switch (MovementState[SelectedUnit])
            {
                case 0:
                    oldCost = 0;
                    break;
                case 1:
                    oldCost = SelectedUnit.movementCost;
                    break;
                case 2:
                    oldCost = SelectedUnit.sprintCost;
                    break;
                default:
                    break;
            }
            MovementState[SelectedUnit] = _state;
            EnergyUsedThisTurn[SelectedUnit] += newCost - oldCost;
            UnitSelected?.Invoke();
        }
    }

    #region AIStuff
    public override void EnemyUnitKilled()
    {
        //AI Thing
    }

    public override void MyUnitKilled()
    {
        //AI Thing
    }

    public override void RewardForBlocking(int _amountBlocked, PlayerUnit _blockingUnit)
    {
        //AI Thing
    }

    public override void GetDistanceToOtherUnits(PlayerUnit _unit)
    {
        //AI Thing
    }
    #endregion AIStuff
}
