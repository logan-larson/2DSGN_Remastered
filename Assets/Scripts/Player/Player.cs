using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Connection;

public class Player
{
    public GameObject GameObject { get; set; }
    public NetworkObject Nob { get; set; }
    public NetworkConnection Connection { get; set; }
    public string Username { get; set; } = "user123";
    public int Health { get; set; } = 100;
    public bool IsDead { get; set; } = false;
}
