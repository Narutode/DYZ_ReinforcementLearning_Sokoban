using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridWorldAlgo : MonoBehaviour
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

    float gamma = 0.01f;
    float deltaLimit = 0.0001f;
    bool policy = true;

    public int sizeX = 5, sizeY = 5;

    private int[,] grid = {  { 0, 0, 0, 0, 0 , 0, 0,0 },
                            { 0, 1, 1, 1, 0, 0, 1, 0},
                            { 0, 0, 0, 0, 0, 0, 0, 1},
                            { 0, 1, 0, 1, 0, 0, 1, 1 },
                            { 1, 1, 0, 1, 0 , 0, 1, 0},
                            { 2, 0, 0, 0, 0 , 0, 0, 3}};

    private float[,] gridValue;

    private int[,] gridPolicy;

    // Start is called before the first frame update
    void Start()
    {
        gridValue = new float[sizeX, sizeY];
        gridPolicy = new int[sizeX, sizeY];
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                gridValue[x, y] = 0;
                gridPolicy[x, y] = Random.Range(0, 4);
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
            //Value Iteration
            if (!policy)
            {
                allValueEvaluation();
                PolicyImprovement();
                drawArrows();
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Policy iteration
        if (policy)
        {
            deleteArrows();
            PolicyEvaluation();
            //allValueEvaluation();
            drawArrows();
            policy = PolicyImprovement();
        }
    }

    void PolicyEvaluation()
    {
        float delta = 0;
        while (true)
        {
            delta = 0;
            //float[,] gridValueTemp = new float[sizeX, sizeY];
            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    if (grid[x, y] == 1)
                    {
                        gridValue[x, y] = 0;
                        continue;
                    }
                    float vPrime = 0;
                    float prevValue = gridValue[x, y];
                        switch (gridPolicy[x, y])
                        {
                            case 0:
                                if (y - 1 >= 0)
                                    if (grid[x, y - 1] != 1)
                                        vPrime = gridValue[x, y - 1];
                                break;
                            case 1:
                                if (x - 1 >= 0)
                                    if (grid[x - 1, y] != 1)
                                        vPrime = gridValue[x - 1, y];
                                break;
                            case 2:
                                if (y + 1 < sizeY)
                                    if (grid[x, y + 1] != 1)
                                        vPrime = gridValue[x, y + 1];
                                break;
                            case 3:
                                if (x + 1 < sizeX)
                                    if (grid[x + 1, y] != 1)
                                        vPrime = gridValue[x + 1, y];
                                break;
                        }
                    
                    gridValue[x, y] = reward(x, y) + gamma * vPrime;
                    delta = Mathf.Max(delta, Mathf.Abs(prevValue - gridValue[x, y]));
                }
            }
            //gridValue = gridValueTemp;
            if (delta < deltaLimit)
                break;
        }
    }

    void allValueEvaluation()
    {
        float delta = 0;
        while (true)
        {
            delta = 0;
            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    if (grid[x, y] == 1)
                    {
                        gridValue[x, y] = 0;
                        continue;
                    }
                    float vPrimeMax = 0;
                    float prevValue = gridValue[x, y];

                    if (y - 1 >= 0)
                        if (grid[x, y - 1] != 1)
                            if(gridValue[x, y - 1] > vPrimeMax)
                                vPrimeMax = gridValue[x, y - 1];
                    if (x - 1 >= 0)
                        if (grid[x - 1, y] != 1)
                            if (gridValue[x - 1, y] > vPrimeMax)
                                vPrimeMax = gridValue[x - 1, y];
                    if (y + 1 < sizeY)
                        if (grid[x, y + 1] != 1)
                            if (gridValue[x, y + 1] > vPrimeMax)
                                vPrimeMax = gridValue[x, y + 1];
                    if (x + 1 < sizeX)
                        if (grid[x + 1, y] != 1)
                            if (gridValue[x + 1, y] > vPrimeMax)
                                vPrimeMax = gridValue[x + 1, y];

                    gridValue[x, y] = reward(x, y) + gamma * vPrimeMax;
                    delta = Mathf.Max(delta, Mathf.Abs(prevValue - gridValue[x, y]));
                }
            }
            if (delta < deltaLimit)
                break;
        }
    }

    bool PolicyImprovement()
    {
        bool stable = true;
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                int temp = gridPolicy[x, y];
                float valMax = 0;
                int newP = temp;
                if (y - 1 >= 0)
                {
                    if(gridValue[x, y - 1] > valMax)
                    {
                        valMax = gridValue[x, y - 1];
                        newP = 0;
                    }
                }
                if (x - 1 >= 0)
                {
                    if (gridValue[x - 1, y] > valMax)
                    {
                        valMax = gridValue[x - 1, y];
                        newP = 1;
                    }
                }
                if (y + 1 < sizeY)
                {
                    if (gridValue[x, y + 1] > valMax)
                    {
                        valMax = gridValue[x, y + 1];
                        newP = 2;
                    }
                }
                if (x + 1 < sizeX)
                {
                    if (gridValue[x + 1, y] > valMax)
                    {
                        valMax = gridValue[x + 1, y];
                        newP = 3;
                    }
                }
                if (temp != newP)
                {
                    stable = false;
                    gridPolicy[x, y] = newP;
                }
            }
        }
        return stable;
    }

    float reward(int x, int y)
    {
        if (grid[x, y] == 3)
            return 1;
        else return 0;
    }

    void drawArrows()
    {
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                if (grid[x, y] != 0)
                    continue;
                GameObject inst;
                switch (gridPolicy[x, y])
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
    }

    void deleteArrows()
    {
        for(int i = 0; i < arrowParent.transform.childCount; i++)
        {
            Destroy(arrowParent.transform.GetChild(i).gameObject);
        }
    }
}
