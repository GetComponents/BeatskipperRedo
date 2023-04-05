using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITrainingSpawner : MonoBehaviour
{
    [SerializeField]
    GameObject /*meleePlayer, rangedPlayer, supportPlayer, meleeEnemy, rangedEnemy, supportEnemy, */UnitPlayer, UnitEnemy;

    [SerializeField]
    Material enemyDefaultMat, playerDefaultMat;

    [SerializeField]
    int wantedMeleeUnits, wantedRangedUnits, wantedSupportUnits;

    [SerializeField]
    Transform objectParent;

    [SerializeField]
    int trainsScenario;

    [SerializeField]
    BattleManager battleManager;

    [SerializeField]
    GridManager gridManager;

    [SerializeField]
    bool spawnRandomUnits;

    [SerializeField]
    List<UnitStats> possibleStats;

    private void Start()
    {
        battleManager.enemyManager.OnGameStart.AddListener(Scenario1);
    }

    public void Scenario1()
    {
        Scenario1(battleManager.playerManager);
        Scenario1(battleManager.enemyManager);
        battleManager.ResetTempUnitInfo();
    }

    private void InstantiateUnit()
    {
        GameObject tmp = Instantiate(UnitEnemy, objectParent);
        battleManager.DeadUnits.Add(tmp);
        tmp.SetActive(false);
    }

    /// <summary>
    /// Refills ressources of units that are alive and spawns new units for those that are not
    /// </summary>
    /// <param name="_manager"></param>
    private void Scenario1(UnitManager _manager)
    {
        _manager.myUnitMoves = new Dictionary<PlayerUnit, int[]>();
        for (int i = 0; i < battleManager.maxUnits; i++)
        {
            Vector2Int tmpPos;
            //Search for a valid position to place the unit
            do
            {
                tmpPos = new Vector2Int(Random.Range(0, gridManager.Gridsize.x), Random.Range(0, gridManager.Gridsize.y));
            } while ((gridManager.Grid[tmpPos.x, tmpPos.y].Type & ETileType.WALL) == ETileType.WALL || (gridManager.Grid[tmpPos.x, tmpPos.y].Type & ETileType.WITHUNIT) == ETileType.WITHUNIT);
            //If there are not enough units on the board a new one will be spawned
            if (_manager.MyUnits.Count <= i)
            {
                if (spawnRandomUnits)
                {
                    ActivateUnitAtPosition(_manager, tmpPos);
                    gridManager.Grid[tmpPos.x, tmpPos.y].Type |= ETileType.WITHUNIT;
                    continue;
                }
            }
            //Else its resources will be replenished and values reset
            else
            {
                PlayerUnit tmp = _manager.MyUnits[i];
                tmp.transform.localPosition = new Vector3(tmpPos.x, 0, tmpPos.y);
                tmp.MyGridPosition = new Vector2Int(Mathf.FloorToInt(tmp.transform.localPosition.x), Mathf.FloorToInt(tmp.transform.localPosition.z));


                tmp.GetSelected();
                tmp.PathState = PlayerUnit.EPathState.NONE;

                tmp.MaxHealthPoints = Mathf.FloorToInt(tmp.MaxHP * tmp.MyManager.HealthMultiplier);
                tmp.HealthPoints = tmp.MaxHealthPoints;
                tmp.Energy = tmp.MaxEnergy;
                tmp.PathIndex = 0;
                tmp.FindTilesInRange();
                _manager.myUnitMoves.Add(tmp, new int[4]);
                gridManager.Grid[tmpPos.x, tmpPos.y].Type |= ETileType.WITHUNIT;
            }
            //Debug.Log("Tile " + tmpPos + " / " + gridManager.Grid[tmpPos.x, tmpPos.y].Type);
        }
    }

    /// <summary>
    /// Activates a unit at a position. The units type is selected randomly
    /// </summary>
    /// <param name="_manager"></param>
    /// <param name="tmpPos"></param>
    private void ActivateUnitAtPosition(UnitManager _manager, Vector2Int tmpPos)
    {
        if (battleManager.DeadUnits.Count == 0)
        {
            InstantiateUnit();
        }
        int unitType = Random.Range(1, 4);
        if (_manager.isPlayerManager)
        {
            switch (unitType)
            {
                case 1:
                    ActivateUnit("Melee Player", tmpPos, (PlayerClass)unitType, false);
                    break;
                case 2:
                    ActivateUnit("Ranged Player", tmpPos, (PlayerClass)unitType, false);
                    break;
                case 3:
                    ActivateUnit("Support Player", tmpPos, (PlayerClass)unitType, false);
                    break;
            }
        }
        else
        {
            switch (unitType)
            {
                case 1:
                    ActivateUnit("Melee Enemy", tmpPos, (PlayerClass)unitType, true);
                    break;
                case 2:
                    ActivateUnit("Ranged Enemy", tmpPos, (PlayerClass)unitType, true);
                    break;
                case 3:
                    ActivateUnit("Support Enemy", tmpPos, (PlayerClass)unitType, true);
                    break;
            }
        }
    }

    /// <summary>
    /// Activates a unit
    /// </summary>
    /// <param name="_name"></param>
    /// <param name="_spawnPos"></param>
    /// <param name="_class"></param>
    /// <param name="_isEnemy"></param>
    private void ActivateUnit(string _name, Vector2Int _spawnPos, PlayerClass _class, bool _isEnemy)
    {
        GameObject tmp = battleManager.DeadUnits[0];
        battleManager.DeadUnits.Remove(tmp);
        tmp.SetActive(true);
        tmp.name = _name;
        tmp.transform.localPosition = new Vector3(_spawnPos.x, 0, _spawnPos.y);
        PlayerUnit unit = tmp.GetComponent<PlayerUnit>();
        unit.MyGridPosition = _spawnPos;
        unit.playerClass = _class;

        switch (_class)
        {
            case PlayerClass.NONE:
                break;
            case PlayerClass.MELEE:
                tmp.GetComponent<PlayerUnit>().myStats = possibleStats[0];
                break;
            case PlayerClass.RANGED:
                tmp.GetComponent<PlayerUnit>().myStats = possibleStats[1];
                break;
            case PlayerClass.SUPPORT:
                tmp.GetComponent<PlayerUnit>().myStats = possibleStats[2];
                break;
            default:
                break;
        }
        unit.Energy = unit.MaxEnergy;
        unit.PathIndex = 0;

        tmp.GetComponent<PlayerUnit>().isEnemy = _isEnemy;
        if (!_isEnemy)
        {
            tmp.GetComponent<PlayerUnit>().MyManager = battleManager.playerManager;
            tmp.GetComponent<MeshRenderer>().material = playerDefaultMat;
            battleManager.enemyManager.EnemyUnits.Add(tmp.GetComponent<PlayerUnit>());
            battleManager.playerManager.MyUnits.Add(tmp.GetComponent<PlayerUnit>());
            if (!battleManager.playerManager.myUnitMoves.ContainsKey(tmp.GetComponent<PlayerUnit>()))
                battleManager.playerManager.myUnitMoves.Add(tmp.GetComponent<PlayerUnit>(), new int[4]);
        }
        else
        {
            tmp.GetComponent<PlayerUnit>().MyManager = battleManager.enemyManager;
            tmp.GetComponent<MeshRenderer>().material = enemyDefaultMat;
            battleManager.enemyManager.MyUnits.Add(tmp.GetComponent<PlayerUnit>());
            battleManager.playerManager.EnemyUnits.Add(tmp.GetComponent<PlayerUnit>());
            if (!battleManager.enemyManager.myUnitMoves.ContainsKey(tmp.GetComponent<PlayerUnit>()))
                battleManager.enemyManager.myUnitMoves.Add(tmp.GetComponent<PlayerUnit>(), new int[4]);
        }
        unit.MaxHealthPoints = Mathf.FloorToInt(unit.MaxHP * unit.MyManager.HealthMultiplier);
        unit.HealthPoints = unit.MaxHealthPoints;
        tmp.GetComponent<PlayerUnit>().FindTilesInRange();
    }

    private void Scenario2()
    {

    }
}
