using UnityEngine;

public class LobbyState : ScriptableObject
{
    public string StateName;
    public GameEvent OnStateEnter;
    public GameEvent OnStateExit;
    public GameEvent OnStateUpdate;
}
