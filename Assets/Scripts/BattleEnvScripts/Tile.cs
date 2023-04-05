using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Bitmask for the gridtiles
/// </summary>
public enum ETileType
{
    NONE,
    WALKABLE = 1,
    WALL = 2,
    WITHUNIT = 4,
}

/// <summary>
/// Enum used to change Material(color) of tiles
/// </summary>
public enum ETileMat
{
    NONE,
    DEFAULT,
    INRANGE,
    HIGHLIGHT,
    UNHHIGHLIGHT,
    TEST,
    TEST2
}

/// <summary>
/// Tiles that make up the battlefield
/// </summary>
[System.Serializable]
public class Tile : MonoBehaviour
{
    public Vector2Int PositionInGrid;
    public Material[] Materials;
    public Material[] AIMaterials;
    public GridManager myBattlefield;
    public MeshRenderer mr;
    public Material DefaultMat => myBattlefield.DefaultMat;
    public Material InRangeMat => myBattlefield.InRangeMat;
    public Material HighlightMat => myBattlefield.HighlightMat;
    public Color unhilightedColor = new Color();

    public ETileType Type
    {
        get => type;
        set
        {
            type = value;
            typeInt = (int)type;
            if (mr == null)
            {
                mr = GetComponent<MeshRenderer>();
            }
            switch (type)
            {
                case ETileType.NONE:
                    //mr.material = Materials[0];
                    break;
                case ETileType.WALKABLE:
                    DefaultMat.color = Color.white;
                    //mr.material = Materials[1];
                    break;
                case ETileType.WALL:
                    //DefaultMat.color = Color.black;
                    mr.material.color = Color.black;
                    //mr.material = Materials[2];
                    break;
                case ETileType.WITHUNIT:
                    //mr.material = HighlightMat;
                    break;
                default:
                    break;
            }
        }
    }
    [SerializeField]
    ETileType type;
    [SerializeField]
    int typeInt;

    /// <summary>
    /// Used to change material- but now changes color of tiles. (memory issues)
    /// </summary>
    /// <param name="_material"></param>
    public void ChangeTileMat(ETileMat _material)
    {
        switch (_material)
        {
            case ETileMat.NONE:
                break;
            case ETileMat.DEFAULT:
                mr.material.color = DefaultMat.color;
                unhilightedColor = mr.material.color;
                break;
            case ETileMat.INRANGE:
                mr.material.color = InRangeMat.color;
                unhilightedColor = mr.material.color;
                break;
            case ETileMat.HIGHLIGHT:
                mr.material.color = HighlightMat.color;
                break;
            case ETileMat.UNHHIGHLIGHT:
                mr.material.color = unhilightedColor;
                break;
            case ETileMat.TEST:
                mr.material.color = Color.green;
                StartCoroutine(ChangeColorBack());
                break;
            //case ETileMat.TEST2:
            //    mr.material.color = Color.red;
            //    StartCoroutine(ChnageColor());
            //    break;
            default:
                break;
        }
    }

    private IEnumerator ChangeColorBack()
    {
        yield return new WaitForSeconds(2);
        ChangeTileMat(ETileMat.DEFAULT);
    }
}

