using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Game Mode", menuName = "SGN/Game Mode")]
public class GameModeInfo : ScriptableObject
{
    public string Name;
    public string Description;
    public int MaxPlayers;
}
