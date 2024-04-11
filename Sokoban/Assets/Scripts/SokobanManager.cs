using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SokobanManager : MonoBehaviour, I_DPL
{
    public GameObject floor;
    public GameObject wall;
    public GameObject player;
    public GameObject finish;
    public GameObject crate;

    public Tilemap tilemap;
    public Vector3Int[] cratesPos;
    public Vector3Int playerPos;

    public GameObject parent;

    private GridLayout gridLayout;
    private GameObject[] crates;

    // Array to store integer representation of the tilemap
    //private int[,] integerTilemap;
    
    private int[,] grid = { { 0,0,2,0,0 },
                            { 4,3,4,4,0 },
                            { 0,0,0,4,2 },
                            { 0,0,0,4,2 },
                            { 2,0,0,2,0 } };
    /*
    private int[,] grid = { {0,0,1,1,1,1,1,0 },
                            {1,1,1,0,0,0,1,0 },
                            {1,2,3,4,0,0,1,0 },
                            {1,1,1,0,4,2,1,0 },
                            {1,2,1,1,4,0,1,0 },
                            {1,0,1,0,2,0,1,1 },
                            {1,4,4,2,4,4,2,1 },
                            {1,0,0,0,2,0,0,1 },
                            { 1,1,1,1,1,1,1,1 } };*/
    int sizeY = 5, sizeX = 5;
    List<state> states = new List<state>();
    public int nbCrates = 1;
    bool policy = true, useMcts = true;
    bool draw = false, end = false;
    MDP mdp;
    MCTS mcts;
    state currentState;
    int index = 0;

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
                if (y - 1 < 0 || grid[x, y - 1] == 1)
                    return null;
                else
                    y--;
                break;
            case 1:
                if (x - 1 < 0 || grid[x - 1, y] == 1)
                    return null;
                else
                    x--;
                break;
            case 2:
                if (y +  1 >= sizeX || grid[x, y + 1] == 1)
                    return null;
                else
                    y++;
                break;
            case 3:
                if (x + 1 >= sizeY || grid[x + 1, y] == 1)
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

            if (x == xc && y == yc)
            {
                switch (action)
                {
                    case 0:
                        if (yc - 1 < 0 || grid[xc, yc - 1] == 1)
                            return null;
                        else
                            yc--;
                        break;
                    case 1:
                        if (xc - 1 < 0 || grid[xc - 1, yc] == 1)
                            return null;
                        else
                            xc--;
                        break;
                    case 2:
                        if (yc + 1 >= sizeX || grid[xc, yc + 1] == 1)
                            return null;
                        else
                            yc++;
                        break;
                    case 3:
                        if (xc + 1 >= sizeY || grid[xc + 1, yc] == 1)
                            return null;
                        else
                            xc++;
                        break;
                }
                for (int j = 0; j < nbCrates; j++)
                {
                    int xc2 = st.key[(i + 1) * 2];
                    int yc2 = st.key[((i + 1) * 2) + 1];
                    if (j != i && xc == xc2 && yc == yc2)
                        return null;
                }
            }
            newKey.Add(xc);
            newKey.Add(yc);
        }
        return states.First(t => t.key.SequenceEqual(newKey));
    }

    public List<int> getNextStateMCTS(state st, int action)
    {
        int x = st.key[0];
        int y = st.key[1];
        switch (action)
        {
            case 0:
                if (y - 1 < 0 || grid[x, y - 1] == 1)
                    return null;
                else
                    y--;
                break;
            case 1:
                if (x - 1 < 0 || grid[x - 1, y] == 1)
                    return null;
                else
                    x--;
                break;
            case 2:
                if (y + 1 >= sizeX || grid[x, y + 1] == 1)
                    return null;
                else
                    y++;
                break;
            case 3:
                if (x + 1 >= sizeY || grid[x + 1, y] == 1)
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

            if (x == xc && y == yc)
            {
                switch (action)
                {
                    case 0:
                        if (yc - 1 < 0 || grid[xc, yc - 1] == 1)
                            return null;
                        else
                            yc--;
                        break;
                    case 1:
                        if (xc - 1 < 0 || grid[xc - 1, yc] == 1)
                            return null;
                        else
                            xc--;
                        break;
                    case 2:
                        if (yc + 1 >= sizeX || grid[xc, yc + 1] == 1)
                            return null;
                        else
                            yc++;
                        break;
                    case 3:
                        if (xc + 1 >= sizeY || grid[xc + 1, yc] == 1)
                            return null;
                        else
                            xc++;
                        break;
                }
                for (int j = 0; j < nbCrates; j++)
                {
                    int xc2 = st.key[(j + 1) * 2];
                    int yc2 = st.key[((j + 1) * 2) + 1];
                    if (j != i && xc == xc2 && yc == yc2)
                        return null;
                }
            }
            newKey.Add(xc);
            newKey.Add(yc);
        }
        return newKey;
    }

    public float getReward(state st)
    {
        if (st == null)
            return nbCrates;
        float reward = 0;
        try
        {
            for (int i = 0; i < nbCrates; i++)
            {
                int x = st.key[(i + 1) * 2];
                int y = st.key[((i + 1) * 2) + 1];

                if (grid[x, y] == 2)
                {
                    reward += 1 / (float)nbCrates;
                }
            }
        }
        catch(System.Exception e)
        {
            int a = 0;
        }
        return reward;
    }

    public List<state> getStates()
    {
        return states;
    }

    public state getFirstState()
    {
        return currentState;
    }

    void Start()
    {
        GameObject inst;
        int[] playerPos = new int[2];
        List<int[]> cratePos = new List<int[]>();

        for (int x = 0; x < sizeY; x++)
        {
            for (int y = 0; y < sizeX; y++)
            {
                if (!useMcts)
                {
                    List<int> xy = new List<int>() { x, y };
                    List<List<int>> newList = addCratesToState(nbCrates - 1, xy);
                    foreach (var key in newList)
                    {
                        state newState = new state();
                        newState.key = key;
                        newState.value = 0;
                        newState.policy = Random.Range(0, 4);
                        states.Add(newState);
                    }
                }

                switch(grid[x,y]) {
                    case 0:
                        inst = Instantiate(floor);
                        inst.transform.position = new Vector3(x - sizeY / 2, y - sizeX / 2, 0);
                        break;
                    case 1:
                        inst = Instantiate(wall);
                        inst.transform.position = new Vector3(x - sizeY / 2, y - sizeX / 2, 0);
                        break;
                    case 2:
                        inst = Instantiate(finish);
                        inst.transform.position = new Vector3(x - sizeY / 2, y - sizeX / 2, 0);
                        break;
                    case 3:
                        inst = Instantiate(floor);
                        inst.transform.position = new Vector3(x - sizeY / 2, y - sizeX / 2, 0);
                        playerPos[0] = x;
                        playerPos[1] = y;
                        break;
                    case 4:
                        inst = Instantiate(floor);
                        inst.transform.position = new Vector3(x - sizeY / 2, y - sizeX / 2, 0);
                        cratePos.Add(new int[] { x, y });
                        break;
                }
            }
        }
        List<int> startKey = new List<int>();
        startKey.Add(playerPos[0]);
        startKey.Add(playerPos[1]);
        for(int i = 0;  i < cratePos.Count(); i++)
        {
            startKey.Add(cratePos[i][0]);
            startKey.Add(cratePos[i][1]);
        }
        if (useMcts)
        {
            currentState = new state();
            currentState.key = startKey;
            currentState.value = 0;
            currentState.policy = Random.Range(0, 4);
            mcts = new MCTS(this);
        }
        else
        {
            currentState = states.First(t => t.key.SequenceEqual(startKey));
            mdp = new MDP(this);
        }
        //Value Iteration
        if (!useMcts && !policy)
        {
            mdp.allValueEvaluation();
            mdp.PolicyImprovement();
            draw = true;
        }
    }

    List<List<int>> addCratesToState(int n, List<int> prev)
    {
        List<List<int>> listKeys = new List<List<int>>();
        for (int x = 0; x < sizeY; x++)
        {
            for (int y = 0; y < sizeX; y++)
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
        if (draw && !end)
        {
            drawState();
            currentState = getNextState(currentState, currentState.policy);
            if (getReward(currentState) >= nbCrates)
                end = true;
        }
        else if (end && !useMcts)
        {
            drawState();
        }
        else if(end)
        {
            if (index < states.Count() - 1)
            {
                currentState = states[index];
                drawState();
                index++;
            }
            else
                index = 0;
        }
        else if (useMcts)
        {
            while(!mcts.selection())
            {
                mcts.expension();
                mcts.simulation();
                mcts.propagation();
            }
            mcts.expension();
            mcts.simulation();
            mcts.propagation();
            end = true;
            states = mcts.getStates();
        }
        else if (policy)
        {
            mdp.PolicyEvaluation();
            draw = mdp.PolicyImprovement();
        }
    }

    void drawState()
    {
        if (currentState != null)
        {
            CleanObjs();

            SpawnPlayer(new Vector3Int(currentState.key[0] - sizeY / 2, currentState.key[1] - sizeX / 2, -1));

            for (int i = 2; i < nbCrates * 2 + 2; i += 2)
            {
                SpawnCrate(new Vector3Int(currentState.key[i] - sizeY / 2, currentState.key[i + 1] - sizeX / 2, -1));
            }
        }
    }

    void CleanObjs()
    {
        for(int i = 0; i < parent.transform.childCount; i++) 
        {
            Destroy(parent.transform.GetChild(i).gameObject);
        }
    }

    void SpawnCrate(Vector3Int gridPos)
    {
        Instantiate(crate, gridPos, Quaternion.Euler(0,0,0), parent.transform);
    }

    void SpawnPlayer(Vector3Int gridPos)
    {
        Instantiate(player, gridPos, Quaternion.Euler(0, 0, 0), parent.transform);
    }
}
