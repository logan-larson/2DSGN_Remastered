using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildInfo", menuName = "SGN/BuildInfo", order = 1)]
public class BuildInfo : ScriptableObject
{
    public bool IsServer;
}
