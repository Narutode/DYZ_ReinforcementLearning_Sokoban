using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SokobanManager : MonoBehaviour, I_DPL
{
    public Tilemap tilemap;

    // Array to store integer representation of the tilemap
    private int[,] integerTilemap;
    List<state> states = new List<state>();
    public int nbCrates = 1;
    bool policy = false;
    MDP mdp;

    public List<int> getActions()
    {
        return new List<int>() { 0, 1, 2, 3 };
    }

    public state getNextState(state st, int action)
    {
        int x = st.key[0];
        int y = st.key[1];
        switch (action)
        {
            case 0:
                if (y - 1 <= 0 || integerTilemap[x, y - 1] == 1)
                    return null;
                else
                    y--;
                break;
            case 1:
                if (x - 1 <= 0 || integerTilemap[x - 1, y] == 1)
                    return null;
                else
                    x--;
                break;
            case 2:
                if (y+  1 <= 0 || integerTilemap[x, y + 1] == 1)
                    return null;
                else
                    y++;
                break;
            case 3:
                if (x + 1 <= 0 || integerTilemap[x + 1, y] == 1)
                    return null;
                else
                    x++;
                break;
        }
        List<int> newKey = new List<int>();
        newKey.Add(x);
        newKey.Add(y);

        for (int i = 0; i < nbCrates; i++)
        {
            int xc = st.key[(i + 1) * 2];
            int yc = st.key[((i + 1) * 2) + 1];

            if(x == xc && y == yc)
            {
                switch (action)
                {
                    case 0:
                        if (yc - 1 <= 0 || integerTilemap[xc, yc - 1] == 1)
                            return null;
                        else
                            yc--;
                        break;
                    case 1:
                        if (xc - 1 <= 0 || integerTilemap[xc - 1, yc] == 1)
                            return null;
                        else
                            xc--;
                        break;
                    case 2:
                        if (yc + 1 <= 0 || integerTilemap[xc, yc + 1] == 1)
                            return null;
                        else
                            yc++;
                        break;
                    case 3:
                        if (xc + 1 <= 0 || integerTilemap[xc + 1, yc] == 1)
                            return null;
                        else
                            xc++;
                        break;
                }
            }
            newKey.Add(xc);
            newKey.Add(yc);
        }
        return states.First(t => t.key.SequenceEqual(newKey));
    }

    public float getReward(state st)
    {
        float reward = 0;
        for(int i = 0; i < nbCrates; i++)
        {
            int x = st.key[(i + 1) * 2];
            int y = st.key[((i + 1) * 2) + 1];

            if (integerTilemap[x,y] == 2)
            {
                reward++;
            }
        }
        return reward;
    }

    public List<state> getStates()
    {
        return states;
    }

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
                List<int> xy = new List<int>() { x, y };
                List<List<int>> newList = addCratesToState(nbCrates-1, xy);
                foreach(var key in newList)
                {
                    state newState = new state();
                    newState.key = key;
                    newState.value = 0;
                    newState.policy = Random.Range(0, 4);
                    states.Add(newState);
                }
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
        mdp = new MDP(this);
        //Value Iteration
        if (!policy)
        {
            mdp.allValueEvaluation();
            mdp.PolicyImprovement();
        }
    }

    List<List<int>> addCratesToState(int n, List<int> prev)
    {
        List<List<int>> listKeys = new List<List<int>>();
        BoundsInt bounds = tilemap.cellBounds;
        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                List<int> newKey = new List<int>();
                newKey.AddRange(prev);
                newKey.Add(x);
                newKey.Add(y);
                if (n > 0)
                    listKeys.AddRange(addCratesToState(n - 1, newKey));
                else
                    listKeys.Add(newKey);
            }
        }
        return listKeys;
    }

    void FixedUpdate()
    {
        //Policy iteration
        if (policy)
        {
            mdp.PolicyEvaluation();
            policy = !mdp.PolicyImprovement();
        }
    }
}
