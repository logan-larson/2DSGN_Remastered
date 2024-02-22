using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Connection;

public class Player
{
    public GameObject GameObject { get; set; }
    public NetworkObject Nob { get; set; }
    /// <summary>
    /// This is the connection that owns this player. Also serves as a unique identifier.
    /// </summary>
    public NetworkConnection Connection { get; set; }
    public string Username { get; set; } = "user123";
    public float Health { get; set; } = 100f;

    public PlayerStatus Status { get; set; } = PlayerStatus.Alive;
    public bool IsDead { get; set; } = false;

    public Transform RespawnPoint { get; set; } = null;

    // Players Manager things
    public int Kills { get; set; } = 0;
    public int Deaths { get; set; } = 0;

    public int Assists { get; set; } = 0;
    public int Score { get; set; } = 0;

    public int OptionVoteIndex { get; set; } = -1;

}

public enum PlayerStatus
{
    Alive,
    Dead,
    Respawning,
    Disabled,
    Spectating
}
