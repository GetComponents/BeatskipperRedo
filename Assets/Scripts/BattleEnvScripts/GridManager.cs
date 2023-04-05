using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GridManager : MonoBehaviour
{
    public Tile[,] Grid;
    public Material[] TileMaterials;
    public ETileType standardType;
    public ETileMask[,] UnitView;
    public Transform myCamera;
    public Vector2Int Gridsize;
    public Transform TileParent;
    public Material DefaultMat, AIDefaultMat, InRangeMat, EnemyMat, PlayerMat, PlayerInRangeMat, HighlightMat;
    public Material WinMat, LoseMat, TimeOutMat;

    /// <summary>
    /// Initializes the tiles of the grid
    /// </summary>
    public void InitializeTiles()
    {
        for (int x = 0; x < Gridsize.x; x++)
        {
            for (int y = 0; y < Gridsize.y; y++)
            {
                Grid[x, y].Materials = TileMaterials;
                Grid[x, y].PositionInGrid = new Vector2Int(x, y);
                Grid[x, y].myBattlefield = this;
            }
        }
        ResetGrid();
    }

    /// <summary>
    /// Resets the Grid
    /// </summary>
    public void ResetGrid()
    {
        //Grid = GridBlueprint;
        GridGenerator.Instance.ResetTypes(Grid);
    }

    /// <summary>
    /// Checks if a Vector2Int is in the bounds of the battlefield
    /// </summary>
    /// <param name="_vectorInQuestion"></param>
    /// <returns></returns>
    public bool VectorIsInBounds(Vector2Int _vectorInQuestion)
    {
        if (_vectorInQuestion.x < Gridsize.x && _vectorInQuestion.x >= 0 && _vectorInQuestion.y < Gridsize.y && _vectorInQuestion.y >= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}

/// <summary>
/// Bitsmask for tiles. This enum is for the representation of the tiles
/// </summary>
public enum ETileMask
{
    NONE = 0,
    REACHABLE = 1,
    ENVIRONMENT = 2,
    PLAYER = 4,
    ENEMY = 8,
}
