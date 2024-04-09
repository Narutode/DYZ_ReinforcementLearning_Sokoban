using System.Collections.Generic;

public class state
{
    public List<int> key;
    public float value;
    public int policy;
    public bool exist;
}

public interface I_DPL
{
    List<state> getStates();
    List<int> getActions();
    state getNextState(state st, int action);
    float getReward(state st);
}
