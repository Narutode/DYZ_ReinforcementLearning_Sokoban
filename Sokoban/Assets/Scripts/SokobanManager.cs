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

    public Vector3Int[] cratesPos;
    public Vector3Int playerPos;

    public GameObject parent;

    private noeud firstNoeud;
    private noeud curNoeud;
    
    private int[,] grid = { { 0,0,2,0,0 },
                            { 4,3,4,4,0 },
                            { 0,0,0,4,2 },
                            { 0,0,0,0,0 },
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
    public bool policy = true, useMcts = true;
    bool draw = false, end = false;
    bool mctsOnPolicy = false;
    int episodes = 20;
    MDP mdp;
    MCTS mcts;
    state currentState;

public List<int> getActions()
{
   return new List<int>() { 0, 1, 2, 3 };
}

    public state getNextState(state st, int action)
    {
        List<int> newKey = getNextStateKey(st, action);
        if (newKey != null)
            return states.First(t => t.key.SequenceEqual(newKey));
        else return null;
    }

    public List<int> getNextStateKey(state st, int action)
    {
        int y = st.key[0];
        int x = st.key[1];
        //On v�rifie si la position du joueur n'est pas en dehors de la carte ou sur un mur
        switch (action)
        {
            case 0:
                if (x - 1 < 0 || grid[y, x - 1] == 1)
                    return null;
                else
                    x--;
                break;
            case 1:
                if (y - 1 < 0 || grid[y - 1, x] == 1)
                    return null;
                else
                    y--;
                break;
            case 2:
                if (x + 1 >= sizeX || grid[y, x + 1] == 1)
                    return null;
                else
                    x++;
                break;
            case 3:
                if (y + 1 >= sizeY || grid[y + 1, x] == 1)
                    return null;
                else
                    y++;
                break;
        }
        List<int> newKey = new List<int>();
        newKey.Add(y);
        newKey.Add(x);

        //Si l'action est valide on v�rifie si la position du joueur est sur une caisse
        for (int i = 0; i < nbCrates; i++)
        {
            int xc = st.key[(i + 1) * 2];
            int yc = st.key[((i + 1) * 2) + 1];

            //Si c'est le cas on d�place la caisse dans la m�me direction
            //En v�rifiant si elle n'est pas en dehors de la carte ou sur un mur
            if (y == xc && x == yc)
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
                //On v�rifie en suite que la caisse n'est pas au m�me endroit qu'une autre caisse
                //Sinon on annule le d�placement
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
            return 0;
        float reward = 0;
        for (int i = 0; i < nbCrates; i++)
        {
            int x = st.key[(i + 1) * 2];
            int y = st.key[((i + 1) * 2) + 1];

            if (grid[x, y] == 2)
            {
                reward += 1 / (float)nbCrates;
            }
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
                //Si on utilise pas le mcts on initialise tous les �tats possible
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

        //on initialise l'�tat de d�part
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
            if(curNoeud.childs[currentState.policy] != null)
            {
                curNoeud = curNoeud.childs[currentState.policy];
                currentState = curNoeud.state;
                drawState();
            }
            else
            {
                curNoeud = firstNoeud.childs[firstNoeud.state.policy];
                currentState = curNoeud.state;
                drawState();
            }
        }
        else if (useMcts)
        {
            //MCTS
            while(!mcts.selection())
            {
                mcts.expension();
                if (mctsOnPolicy)
                {
                    for(int i = 0; i < episodes; i++)
                    {
                        mcts.simulation(1);
                        mcts.propagation();
                    }
                } 
                else
                {
                    mcts.simulation(episodes);
                    mcts.propagation();
                }
            }
            mcts.expension();
            mcts.simulation(1);
            mcts.propagation();
            end = true;
            firstNoeud = mcts.getStates();
            curNoeud = firstNoeud;
            currentState = curNoeud.state;
            drawState();
        }
        else if (policy)
        {
            //Policy evaluation
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
