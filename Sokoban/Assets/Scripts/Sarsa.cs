using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Sarsa
{
    Dictionary<Tuple<state, int>, float> Qvalue = new Dictionary<Tuple<state, int>, float>();
    state firstState;
    float alpha = 0.1f;
    float gamma = 0.9f;
    float epsilon = 0.1f;
    I_DPL game;

    public Sarsa(I_DPL g)
    {
        game = g;
        firstState = game.getFirstState();
        List<state> states = game.getStates();

        foreach(var st in states)
        {
            for(int i = 0; i < 4; i++)
            {
                Qvalue.Add(new Tuple<state, int>(st, i), 0);
            }
        }
    }

    public void Train(int episodes)
    {
        for (int episode = 0; episode < episodes; episode++)
        {
            state state = firstState;
            int action = EpsilonGreedy(state, 4);
            float reward = game.getReward(state);
            while (reward < 1)
            {
                state nextState = game.getNextState(state, action);
                reward = game.getReward(nextState);
                var valState = Qvalue[new Tuple<state, int>(state, action)];
                var nextAction = EpsilonGreedy(state, 4);
                var valNextState = Qvalue[new Tuple<state, int>(nextState, nextAction)];
                valState += alpha * (reward + gamma * valNextState - valState);
                Qvalue[new Tuple<state, int>(state, action)] = valState;
            }
        }
    }

    private int EpsilonGreedy(state state, int numActions)
    {
        // Politique epsilon-greedy
        if (UnityEngine.Random.Range(0f, 1f) < epsilon)
        {
            return UnityEngine.Random.Range(0, numActions); // Exploration : action aléatoire
        }
        else
        {
            // Exploitation : action avec la plus grande valeur Q
            float[] qValuesForState = new float[numActions];
            for (int a = 0; a < numActions; a++)
            {
                qValuesForState[a] = Qvalue.GetValueOrDefault(new Tuple<state, int>(state, a));
            }
            return Array.IndexOf(qValuesForState, qValuesForState.Max());
        }
    }
}
