using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class noeud
{
    public state state;
    public noeud parent = null;
    public noeud[] childs = new noeud[4];
    public bool end = true;
    public float essais = 0;
    public float score = 0;
}

public class MCTS
{
    I_DPL game;
    //List<state> allStates;
    List<noeud> allNoeud;
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
        allNoeud = new List<noeud>();
        allNoeud.Add(firstNode);
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
            List<int> nextStateKey = game.getNextStateKey(currentNode.state, i);
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
                allNoeud.Add(nextNode);
                currentNode.childs[i] = nextNode;
            }
        }
        noeud newNode = currentNode.childs[currentNode.state.policy];
        while (newNode == null)
            newNode = currentNode.childs[Random.Range(0, 4)];
        currentNode = newNode;
    }

    public void simulation(int n)
    {
        float res = 0;
        for (int i = 0; i < n; i++)
        {
            float indexMax = 1;
            state curState = currentNode.state;
            while (indexMax < 100 && game.getReward(curState) < 1)
            {
                indexMax++;
                List<int> nextStateKey = game.getNextStateKey(curState, curState.policy);
                while(nextStateKey == null)
                    nextStateKey = game.getNextStateKey(curState, Random.Range(0, 4));
                noeud nextNode = allNoeud.FirstOrDefault(t => t.state.key.SequenceEqual(nextStateKey));
                state nextState;
                if (nextNode == null)
                {
                    nextState = new state();
                    nextState.key = nextStateKey;
                    nextState.policy = Random.Range(0, 4);
                    nextState.value = 0;
                }
                else
                    nextState = nextNode.state;
                curState = nextState;
            }
            res += game.getReward(curState) / indexMax;
        }
        currentNode.essais += n;
        currentNode.score += res;
    }

    public void propagation()
    {
        float curScore = currentNode.score;
        while(currentNode.parent != null)
        {
            currentNode = currentNode.parent;
            currentNode.score += curScore;
            currentNode.essais += 20;
            float valMax = 0;
            int iMax = currentNode.state.policy;
            for(int i = 0; i < 4; i++)
            {
                noeud child = currentNode.childs[i];
                if(child != null)
                {
                    if (child.essais > 0)
                    {
                        if ((child.score / child.essais) > valMax)
                        {
                            valMax = (child.score / child.essais);
                            iMax = i;
                        }
                    }
                }
            }
            currentNode.state.policy = iMax;
        }
    }

    public noeud getStates()
    {
        return firstNode;
    }
}
