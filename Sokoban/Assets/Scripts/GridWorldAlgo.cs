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
    public GameObject arrowParent;

    float gamma = 0.5f;
    float deltaLimit = 0.0001f;
    bool policy = false;

    public int sizeX = 5, sizeY = 5;

    private int[,] grid = { { 0, 0, 0, 0, 0, 0, 0, 0},
                            { 0, 1, 1, 1, 0, 0, 1, 0},
                            { 0, 0, 0, 0, 0, 0, 0, 1},
                            { 0, 1, 0, 1, 0, 0, 1, 1},
                            { 1, 1, 0, 1, 0, 0, 1, 0},
                            { 2, 0, 0, 0, 1, 0, 0, 3} };

    public List<state> states = new List<state>();

    public MDP mdp;

    // Start is called before the first frame update
    void Start()
    {
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
        mdp = new MDP(this);
        //Value Iteration
        if (!policy)
        {
            mdp.allValueEvaluation();
            mdp.PolicyImprovement();
            drawArrows();
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Policy iteration
        if (policy)
        {
            deleteArrows();
            mdp.PolicyEvaluation();
            drawArrows();
            policy = !mdp.PolicyImprovement();
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
                    inst = Instantiate(arrow0, arrowParent.transform);
                    inst.transform.position = new Vector3(x-sizeX/2, y-sizeY/2, -1);
                    break;
                case 1:
                    inst = Instantiate(arrow1, arrowParent.transform);
                    inst.transform.position = new Vector3(x-sizeX/2, y-sizeY/2, -1);
                    break;
                case 2:
                    inst = Instantiate(arrow2, arrowParent.transform);
                    inst.transform.position = new Vector3(x-sizeX/2, y-sizeY/2, -1);
                    break;
                case 3:
                    inst = Instantiate(arrow3, arrowParent.transform);
                    inst.transform.position = new Vector3(x-sizeX/2, y-sizeY/2, -1);
                    break;
            }
        }
    }

    void deleteArrows()
    {
        for(int i = 0; i < arrowParent.transform.childCount; i++)
        {
            Destroy(arrowParent.transform.GetChild(i).gameObject);
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
}
