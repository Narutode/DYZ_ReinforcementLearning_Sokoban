using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridWorldAlgo : MonoBehaviour, I_DPL
{
    public GameObject floor;
    public GameObject wall;
    public GameObject player;
    public GameObject finish;
    public GameObject arrow0;
    public GameObject arrow1;
    public GameObject arrow2;
    public GameObject arrow3;
    public GameObject parent;

    float gamma = 0.5f;
    float deltaLimit = 0.0001f;
    public bool policy = false, useMcts = false;

    public int sizeX = 5, sizeY = 5;

    private int[,] grid = { { 0, 0, 0, 0, 0, 0, 0, 0},
                            { 0, 1, 1, 1, 0, 0, 1, 0},
                            { 0, 0, 0, 0, 0, 0, 0, 1},
                            { 0, 1, 0, 1, 0, 0, 1, 1},
                            { 1, 1, 0, 1, 0, 0, 1, 0},
                            { 2, 0, 0, 0, 1, 0, 0, 3} };

    List<state> states = new List<state>();
    state firstState;

    MDP mdp;
    MCTS mcts;
    private bool end = false;
    private noeud firstNoeud;
    private noeud curNoeud;
    private state currentState;

    void Start()
    {
        int playerX = 0, playerY = 0;
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                state newState = new state();
                newState.key = new List<int> { x, y };
                newState.value = 0;
                newState.policy = Random.Range(0, 4);
                states.Add(newState);
                GameObject inst = null;
                switch (grid[x, y])
                {
                    case 0:
                        inst = Instantiate(floor);
                        inst.transform.position = new Vector3(x - sizeX / 2, y - sizeY / 2, 0);
                        break;
                    case 1:
                        inst = Instantiate(wall);
                        inst.transform.position = new Vector3(x - sizeX / 2, y - sizeY / 2, 0);
                        break;
                    case 2:
                        inst = Instantiate(player);
                        inst.transform.position = new Vector3(x - sizeX / 2, y - sizeY / 2, 0);
                        inst = Instantiate(floor);
                        inst.transform.position = new Vector3(x - sizeX / 2, y - sizeY / 2, 1);
                        playerX = x;
                        playerY = y;
                        break;
                    case 3:
                        inst = Instantiate(finish);
                        inst.transform.position = new Vector3(x - sizeX / 2, y - sizeY / 2, 0);
                        inst = Instantiate(floor);
                        inst.transform.position = new Vector3(x - sizeX / 2, y - sizeY / 2, 1);
                        break;
                }
            }
        }
        if(useMcts)
        {
            firstState = new state();
            firstState.key = new List<int>() { playerX, playerY };
            firstState.value = 0;
            firstState.policy = Random.Range(0, 4);
            mcts = new MCTS(this);
        }
        else
            mdp = new MDP(this);
        if (!policy && !useMcts)
        {
            //Value Iteration
            mdp.allValueEvaluation();
            mdp.PolicyImprovement();
            drawArrows();
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (end)
        {
            if (curNoeud.childs[currentState.policy] != null)
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
        else if(useMcts)
        {
            //MCTS
            while (!mcts.selection())
            {
                mcts.expension();
                mcts.simulation();
                mcts.propagation();
            }
            mcts.expension();
            mcts.simulation();
            mcts.propagation();
            end = true;
            firstNoeud = mcts.getStates();
            curNoeud = firstNoeud;
            currentState = curNoeud.state;
            drawState();
        }
        else if (policy)
        {
            //Policy iteration
            deleteObjects();
            mdp.PolicyEvaluation();
            drawArrows();
            policy = !mdp.PolicyImprovement();
        }
    }

    private void drawState()
    {
        if(currentState != null)
        {
            deleteObjects();
            Vector3 pos = new Vector3(currentState.key[0] - sizeX / 2, currentState.key[1] - sizeY / 2, -1);
            Instantiate(player, pos, Quaternion.Euler(0, 0, 0), parent.transform);
        }
    }

    void drawArrows()
    {
        foreach(var st in states) {
            int x = st.key[0];
            int y = st.key[1];

            if (grid[x, y] != 0)
                    continue;
            GameObject inst;
            switch (st.policy)
            {
                case 0:
                    inst = Instantiate(arrow0, parent.transform);
                    inst.transform.position = new Vector3(x-sizeX/2, y-sizeY/2, -1);
                    break;
                case 1:
                    inst = Instantiate(arrow1, parent.transform);
                    inst.transform.position = new Vector3(x-sizeX/2, y-sizeY/2, -1);
                    break;
                case 2:
                    inst = Instantiate(arrow2, parent.transform);
                    inst.transform.position = new Vector3(x-sizeX/2, y-sizeY/2, -1);
                    break;
                case 3:
                    inst = Instantiate(arrow3, parent.transform);
                    inst.transform.position = new Vector3(x-sizeX/2, y-sizeY/2, -1);
                    break;
            }
        }
    }

    void deleteObjects()
    {
        for(int i = 0; i < parent.transform.childCount; i++)
        {
            Destroy(parent.transform.GetChild(i).gameObject);
        }
    }

    public List<state> getStates()
    {
        return states;
    }

    public List<int> getActions()
    {
        return new List<int> { 0, 1, 2, 3 };
    }

    public state getNextState(state st, int action)
    {
        int x = st.key[0];
        int y = st.key[1];
        switch (action)
        {
            case 0:
                if (y - 1 >= 0)
                    if (grid[x, y - 1] != 1)
                        return states.First(t => t.key[0] == x && t.key[1] == y-1);
                break;
            case 1:
                if (x - 1 >= 0)
                    if (grid[x - 1, y] != 1)
                        return states.First(t => t.key[0] == x-1 && t.key[1] == y);
                break;
            case 2:
                if (y + 1 < sizeY)
                    if (grid[x, y + 1] != 1)
                        return states.First(t => t.key[0] == x && t.key[1] == y+1);
                break;
            case 3:
                if (x + 1 < sizeX)
                    if (grid[x + 1, y] != 1)
                        return states.First(t => t.key[0] == x+1 && t.key[1] == y);
                break;
        }
        return null;
    }

    public float getReward(state st)
    {
        if (grid[st.key[0], st.key[1]] == 3)
            return 1;
        else return 0;
    }

    public state getFirstState()
    {
        return firstState;
    }

    public List<int> getNextStateKey(state st, int action)
    {
        int x = st.key[0];
        int y = st.key[1];
        switch (action)
        {
            case 0:
                if (y - 1 >= 0)
                    if (grid[x, y - 1] != 1)
                        return new List<int>() { x, y - 1 } ;
                break;
            case 1:
                if (x - 1 >= 0)
                    if (grid[x - 1, y] != 1)
                        return new List<int>() { x - 1, y };
                break;
            case 2:
                if (y + 1 < sizeY)
                    if (grid[x, y + 1] != 1)
                        return new List<int>() { x, y + 1 };
                break;
            case 3:
                if (x + 1 < sizeX)
                    if (grid[x + 1, y] != 1)
                        return new List<int>() { x + 1, y };
                break;
        }
        return null;
    }
}
