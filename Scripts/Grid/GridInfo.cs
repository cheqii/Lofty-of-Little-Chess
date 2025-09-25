using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum GridType
{
    Walkable = 0,
    Fluid = 1,
    Ability = 2,
}

[Serializable]
public class GridInfo
{
    // public GameObject GridPrefab;
    public GridTileData GridData;
    public float Height;
    [Range(0f, 100f)]
    public float SpawnRate;
    // public GridType Type;

    // public TileBase[] TilePalette;
}
