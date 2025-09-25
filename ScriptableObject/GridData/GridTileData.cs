using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "GridData", menuName = "Scriptable Objects/GridData")]
public class GridTileData : ScriptableObject
{
    public GameObject GridPrefab;
    public GridType Type;
    public TileBase[] TilePalette;
}
