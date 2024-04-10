using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class noeud
{
    public state state;
    public noeud parent;
    public noeud[] childs = new noeud[4];
    public bool end = true;
}

public class MCTS
{
    I_DPL game;
    List<noeud> treeStates;
    List<int> actions;
    noeud firstNode;
    noeud currentNode;

    public MCTS(I_DPL g)
    {
        game = g;
        actions = game.getActions();
        firstNode = new noeud();
        firstNode.state = game.getFirstState();
    }

    void selection()
    {
        currentNode = firstNode;
        while(!currentNode.end)
        {
            currentNode = currentNode.childs[currentNode.state.policy];
        }
        currentNode.end = false;
    }

    void expension()
    {
        for (int i = 0; i < 4; i++)
        {
            List<int> nextStateKey = game.getNextStateMCTS(currentNode.state, currentNode.state.policy);
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

    void simulation()
    {
        int indexMax = 20;
        state curState = currentNode.state;
        while (indexMax > 0 && game.getReward(curState) <= 1)
        {
            indexMax--;
            List<int> nextStateKey = game.getNextStateMCTS(currentNode.state, currentNode.state.policy);
            state nextState = new state();
            nextState.key = nextStateKey;
            nextState.policy = Random.Range(0, 4);
            nextState.value = 0;
            curState = nextState;
        }
    }

    void propagation()
    {

    }
}
