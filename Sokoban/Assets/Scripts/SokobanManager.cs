using UnityEngine;
using UnityEngine.Tilemaps;

public class SokobanManager : MonoBehaviour
{
    public Tilemap tilemap;

    // Array to store integer representation of the tilemap
    private int[,] integerTilemap;

    void Start()
    {
        // Get the bounds of the tilemap
        BoundsInt bounds = tilemap.cellBounds;

        // Initialize the integerTilemap array
        integerTilemap = new int[bounds.size.x, bounds.size.y];



        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                Vector3Int tilePosition = new Vector3Int(bounds.xMin + x, bounds.yMin + y, 0);
                TileBase tile = tilemap.GetTile(tilePosition);

                // Check if the tile is null (no tile)
                if (tile == null)
                {
                    integerTilemap[x, y] = -1; // Represent no tile as -1
                }
                else if (tile.name.Contains("2_87"))
                {
                    integerTilemap[x, y] = 0; // GROUND
                }
                else if (tile.name.Contains("2_83"))
                {
                    integerTilemap[x, y] = 1; // WALL
                }
                else if (tile.name.Contains("2_99"))
                {
                    integerTilemap[x, y] = 2; // OBJECTIVE
                }
                Debug.Log("Next");
            }
        }

        // use WorldToCell to know where are player and crates on the grid
    }
}
