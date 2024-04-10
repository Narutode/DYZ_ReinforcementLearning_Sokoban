using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MDP
{
    I_DPL game;
    List<state> states;
    List<int> actions;
    float gamma = 0.5f;
    float deltaLimit = 0.1f;

    public MDP(I_DPL g)
    {
        game = g;
        states = game.getStates();
        actions = game.getActions();
    }

    public void PolicyEvaluation()
    {
        float delta = 0;
        while (true)
        {
            delta = 0;
            //float[,] gridValueTemp = new float[sizeX, sizeY];
            for(int i = 0; i < states.Count; i++) {
                state st = states[i];
                float vPrime = 0;
                float prevValue = st.value;
                state nextSt = game.getNextState(st, st.policy);
                if(nextSt != null)
                {
                    vPrime = nextSt.value;
                }
                st.value = game.getReward(st) + gamma * vPrime;
                delta = Math.Max(delta, Math.Abs(prevValue - st.value));
            }
            Debug.Log(delta);
            if (delta < deltaLimit)
                break;
        }
    }

    public bool PolicyImprovement()
    {
        bool stable = true;
        for (int i = 0; i < states.Count; i++)
        {
            state st = states[i];
            int prevPolicy = st.policy;
            float valMax = 0;
            int newP = prevPolicy;
            foreach(int act in actions)
            {
                state nextSt = game.getNextState(st, act);
                if (nextSt != null)
                {
                    if (nextSt.value > valMax)
                    {
                        newP = act;
                        valMax = nextSt.value;
                    }
                }
            }
            if (prevPolicy != newP)
            {
                stable = false;
                st.policy = newP;
            }
        }
        return stable;
    }

    public void allValueEvaluation()
    {
        float delta = 0;
        while (true)
        {
            delta = 0;
            for (int i = 0; i < states.Count; i++)
            {
                state st = states[i];
                float vPrimeMax = 0;
                float prevValue = st.value;
                foreach(int act in actions)
                {
                    state nextSt = game.getNextState(st, act);
                    if (nextSt != null)
                    {
                        if (nextSt.value > vPrimeMax)
                            vPrimeMax = nextSt.value;
                    }
                }
                st.value = game.getReward(st) + gamma * vPrimeMax;
                delta = Math.Max(delta, Math.Abs(prevValue - st.value));
            }
            if (delta < deltaLimit)
                break;
        }
    }

}
