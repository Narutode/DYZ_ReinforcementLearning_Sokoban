using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
public class Sarsa
{
    float alpha = 0.1f;
    float gamma = 0.9f;
    float epsilon = 0.1f;
    public void Train(int episodes)
    {
        for (int episode = 0; episode < episodes; episode++)
        {
            // Début d'un épisode
            int state = environment.Reset(); // État initial
            int action = EpsilonGreedy(state, environment.GetNumActions());

            while (!environment.IsTerminal(state))
            {
                // Étape de l'épisode
                Tuple<int, float> nextStateReward = environment.Step(action);
                int nextState = nextStateReward.Item1;
                float reward = nextStateReward.Item2;

                int nextAction = EpsilonGreedy(nextState, environment.GetNumActions());

                // Mise à jour de la valeur Q
                QValues[state, action] += alpha * (reward + gamma * QValues[nextState, nextAction] - QValues[state, action]);

                state = nextState;
                action = nextAction;
            }
        }
    }

    private int EpsilonGreedy(int state, int numActions)
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
                qValuesForState[a] = QValues[state, a];
            }
            return Array.IndexOf(qValuesForState, qValuesForState.Max());
        }
    }
}
*/