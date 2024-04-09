using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class SokobanManager : MonoBehaviour
{
    // Array to store placed tiles
    private Dictionary<TileBase, List<Vector3Int>> placedTilesByPalette;

    void Start()
    {
        // Initialize the dictionary to store tiles by palette
        placedTilesByPalette = new Dictionary<TileBase, List<Vector3Int>>();

        // Get all tilemaps in the scene
        Tilemap[] tilemaps = FindObjectsOfType<Tilemap>();

        // Iterate through each tilemap
        foreach (Tilemap tilemap in tilemaps)
        {
            // Get the bounds of the tilemap
            BoundsInt bounds = tilemap.cellBounds;

            // Loop through the bounds to get each tile
            for (int x = 0; x < bounds.size.x; x++)
            {
                for (int y = 0; y < bounds.size.y; y++)
                {
                    // Get the tile at the current position
                    Vector3Int tilePosition = new Vector3Int(bounds.xMin + x, bounds.yMin + y, 0);
                    TileBase tile = tilemap.GetTile(tilePosition);

                    // Skip if the tile is null
                    if (tile == null)
                        continue;

                    // Check if the tile palette is already in the dictionary
                    if (!placedTilesByPalette.ContainsKey(tile))
                    {
                        // If not, add it to the dictionary
                        placedTilesByPalette[tile] = new List<Vector3Int>();
                    }

                    // Add the tile position to the corresponding palette's list
                    placedTilesByPalette[tile].Add(tilePosition);
                    string dispName = "null";

                    if (tile.name.Contains("2_83")) dispName = "WALL";
                    else if (tile.name.Contains("2_87")) dispName = "GROUND";
                    else if (tile.name.Contains("2_62")) dispName = "OBJECTIVE";

                    Debug.Log($"{dispName} -- {tilePosition}");
                }
            }
        }

        // Now you have all the placed tiles stored in 'placedTilesByPalette' dictionary
        // The key is the tile base and the value is a list of positions where that tile is placed
    }
}
