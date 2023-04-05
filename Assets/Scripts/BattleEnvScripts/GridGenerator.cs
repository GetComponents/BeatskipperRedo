using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    public static GridGenerator Instance;

    [SerializeField]
    public Vector2Int GridSize;

    [SerializeField]
    GameObject gridMap;

    [SerializeField]
    GameObject[] Maps;

    public Dictionary<EMapSelection, Tile[]> AllMaps = new Dictionary<EMapSelection, Tile[]>();

    public enum EMapSelection
    {
        MAP1 = 0,
        MAP2,
        MAP3,
        MAP4,
        MAP5,
        RANDOM,
    }

    [SerializeField]
    EMapSelection selectedMap;

    [SerializeField]
    GameObject TileInGrid;

    [SerializeField]
    Material[] tileMaterials;

    [SerializeField]
    Transform Cameras;

    /// <summary>
    /// Mainly for seeing results after training
    /// </summary>
    public int EpisodesWon
    {
        get => episodesWon;
        set
        {
            episodesWon = value;
            Debug.Log("Episodes Won");
        }
    }
    private int episodesWon;

    /// <summary>
    /// Mainly for seeing results after training
    /// </summary>
    public int EpisodesLost
    {
        get => episodesLost;
        set
        {
            episodesLost = value;
            Debug.Log("Episodes Lost");
        }
    }
    private int episodesLost;

    [SerializeField]
    public Tile BaseGrid;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        LoadMaps(selectedMap);
    }

    void Start()
    {
        SpawnEmptyMaps();
    }

    /// <summary>
    /// Loads all the maps that are going to be used
    /// </summary>
    /// <param name="_maps"></param>
    private void LoadMaps(EMapSelection _maps)
    {
        if (_maps == EMapSelection.RANDOM)
        {
            AllMaps.Add(EMapSelection.MAP1, new Tile[GridSize.x * GridSize.y]);
            AllMaps.Add(EMapSelection.MAP2, new Tile[GridSize.x * GridSize.y]);
            AllMaps.Add(EMapSelection.MAP3, new Tile[GridSize.x * GridSize.y]);
            AllMaps.Add(EMapSelection.MAP4, new Tile[GridSize.x * GridSize.y]);
            AllMaps.Add(EMapSelection.MAP5, new Tile[GridSize.x * GridSize.y]);
            for (int i = 0; i < Maps.Length; i++)
            {
                int index = 0;
                foreach (Tile tile in Maps[i].GetComponentsInChildren<Tile>())
                {
                    switch (i)
                    {
                        case 0:
                            AllMaps[EMapSelection.MAP1][index] = tile;
                            break;
                        case 1:
                            AllMaps[EMapSelection.MAP2][index] = tile;
                            break;
                        case 2:
                            AllMaps[EMapSelection.MAP3][index] = tile;
                            break;
                        case 3:
                            AllMaps[EMapSelection.MAP4][index] = tile;
                            break;
                        case 4:
                            AllMaps[EMapSelection.MAP5][index] = tile;
                            break;
                        default:
                            break;
                    }
                    //Debug.Log(AllMaps[EMapSelection.MAP1][index].Type);
                    index++;
                }
            }
        }
        else
        {
            AllMaps.Add(_maps, new Tile[GridSize.x * GridSize.y]);
            int index = 0;
            foreach (Tile tile in Maps[(int)_maps].GetComponentsInChildren<Tile>())
            {
                switch ((int)_maps)
                {
                    case 0:
                        AllMaps[EMapSelection.MAP1][index] = tile;
                        break;
                    case 1:
                        AllMaps[EMapSelection.MAP2][index] = tile;
                        break;
                    case 2:
                        AllMaps[EMapSelection.MAP3][index] = tile;
                        break;
                    case 3:
                        AllMaps[EMapSelection.MAP4][index] = tile;
                        break;
                    case 4:
                        AllMaps[EMapSelection.MAP5][index] = tile;
                        break;
                    default:
                        break;
                }
                index++;
            }
        }
    }

    /// <summary>
    /// Initializes tiles
    /// </summary>
    private void SpawnEmptyMaps()
    {
        foreach (var gridManager in FindObjectsOfType<GridManager>())
        {
            gridManager.Grid = new Tile[GridSize.x, GridSize.y];
            for (int y = 0; y < GridSize.y; y++)
            {
                for (int x = 0; x < GridSize.x; x++)
                {
                    GameObject tmp = Instantiate(TileInGrid, gridManager.TileParent.transform);
                    tmp.transform.localPosition = new Vector3((x * (TileInGrid.transform.localScale.x) * 10f),
                    0, (y * (TileInGrid.transform.localScale.z) * 10f));


                    gridManager.Grid[x, y] = tmp.GetComponent<Tile>();
                    gridManager.TileMaterials = tileMaterials;
                    gridManager.standardType = ETileType.WALKABLE;
                    gridManager.Grid[x, y].PositionInGrid = new Vector2Int(x, y);
                }
            }
            SetManagerStats(gridManager);
        }
    }

    /// <summary>
    /// Sets specific stats of the gridmanager
    /// </summary>
    /// <param name="_manager"></param>
    private void SetManagerStats(GridManager _manager)
    {
        _manager.Gridsize = GridSize;
        _manager.UnitView = new ETileMask[GridSize.x, GridSize.y];
        _manager.myCamera.localPosition = new Vector3((GridSize.x * 0.5f) + -0.5f, Cameras.position.y, (GridSize.y * 0.5f) + -0.5f);
        _manager.InitializeTiles();
    }

    /// <summary>
    /// Resets the given grid with a preset map
    /// </summary>
    /// <param name="_grid"></param>
    public void ResetTypes(Tile[,] _grid)
    {
        EMapSelection tmp;

        if (selectedMap == EMapSelection.RANDOM)
        {
            tmp = (EMapSelection)Random.Range(0, 5);
        }
        else
        {
            tmp = selectedMap;
        }
        for (int y = 0; y < GridSize.y; y++)
        {
            for (int x = 0; x < GridSize.x; x++)
            {
                _grid[x, y].Type = AllMaps[tmp][x + GridSize.y * y].Type;
            }
        }
    }
}
