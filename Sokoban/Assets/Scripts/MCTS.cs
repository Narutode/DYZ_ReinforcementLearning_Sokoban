using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class noeud
{
    public state state;
    public noeud parent = null;
    public noeud[] childs = new noeud[4];
    public bool end = true;
    public float trys = 0;
    public float score = 0;
}

public class MCTS
{
    I_DPL game;
    List<noeud> treeStates;
    List<int> actions;
    noeud firstNode;
    noeud currentNode;
    float probaExplo = 0.5f;

    public MCTS(I_DPL g)
    {
        game = g;
        actions = game.getActions();
        firstNode = new noeud();
        firstNode.state = game.getFirstState();
    }

    public bool selection()
    {
        currentNode = firstNode;
        while(!currentNode.end)
        {
            if(Random.Range(0f,1f) > probaExplo)
            {
                noeud newNode = currentNode.childs[Random.Range(0, 4)];
                while(newNode == null)
                    newNode = currentNode.childs[Random.Range(0, 4)];
                currentNode = newNode;
            }
            else
            {
                noeud newNode = currentNode.childs[currentNode.state.policy];
                while (newNode == null)
                    newNode = currentNode.childs[Random.Range(0, 4)];
                currentNode = newNode;          
            }
        }
        return (game.getReward(currentNode.state) >= 1);
    }

    public void expension()
    {
        currentNode.end = false;
        for (int i = 0; i < 4; i++)
        {
            List<int> nextStateKey = game.getNextStateMCTS(currentNode.state, i);
            if(nextStateKey == null)
            {
                currentNode.childs[i] = null;
            }
            else
            {
                state nextState = new state();
                nextState.key = nextStateKey;
                nextState.policy = Random.Range(0, 4);
                nextState.value = 0;
                noeud nextNode = new noeud();
                nextNode.state = nextState;
                nextNode.parent = currentNode;
                currentNode.childs[i] = nextNode;
            }
        }
        noeud newNode = currentNode.childs[currentNode.state.policy];
        while (newNode == null)
            newNode = currentNode.childs[Random.Range(0, 4)];
        currentNode = newNode;
    }

    public void simulation()
    {
        float res = 0;
        for (int i = 0; i < 20; i++)
        {
            float indexMax = 100;
            state curState = currentNode.state;
            while (indexMax > 1 && game.getReward(curState) < 1)
            {
                indexMax--;
                List<int> nextStateKey = null;
                while(nextStateKey == null)
                    nextStateKey = game.getNextStateMCTS(curState, Random.Range(0, 4));
                state nextState = new state();
                nextState.key = nextStateKey;
                nextState.policy = Random.Range(0, 4);
                nextState.value = 0;
                curState = nextState;
            }
            res += game.getReward(curState);
        }
        currentNode.trys += 20;
        currentNode.score += res;
    }

    public void propagation()
    {
        float curScore = currentNode.score;
        while(currentNode.parent != null)
        {
            currentNode = currentNode.parent;
            currentNode.score += curScore;
            currentNode.trys += 20;
            float valMax = 0;
            int iMax = currentNode.state.policy;
            for(int i = 0; i < 4; i++)
            {
                noeud child = currentNode.childs[i];
                if(child != null)
                {
                    if (child.trys > 0)
                    {
                        if ((child.score / child.trys) > valMax)
                        {
                            valMax = (child.score / child.trys);
                            iMax = i;
                        }
                    }
                }
            }
            currentNode.state.policy = iMax;
        }
    }

    public List<state> getStates()
    {
        List<state> listeStates = new List<state>();
        while(!firstNode.end)
        {
            listeStates.Add(firstNode.state);
            firstNode = firstNode.childs[firstNode.state.policy];
            if (firstNode == null)
                break;
        }
        listeStates.Add(currentNode.state);
        return listeStates;
    }
}
