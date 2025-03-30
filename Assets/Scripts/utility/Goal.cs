using System;
using Random = UnityEngine.Random;

public enum GoalType
{
    KILL_N_ENEMIES,
    GET_SCORE_N,
}

public class Goal
{
    public readonly GoalType Type;
    public readonly int N;

    public Goal(GoalType goalType, int goalN)
    {
        Type = goalType;
        N = goalN;
    }
    
    public Goal()
    {
        var values = Enum.GetValues(typeof(GoalType));
        
        Type = (GoalType)values.GetValue(Random.Range(0, values.Length));
    }
}