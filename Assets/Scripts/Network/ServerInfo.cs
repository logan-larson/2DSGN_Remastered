using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ServerInfo", menuName = "SGN/ServerInfo", order = 3)]
public class ServerInfo : ScriptableObject
{
    public string Address;
    public ushort Port;

    public bool IsFreeplay;
}
