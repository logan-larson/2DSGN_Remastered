using FishNet;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SoloDeathmatchScoreboardUIManager : NetworkBehaviour
{
    [SerializeField]
    private GameObject _playerList;

    [SerializeField]
    private PodiumUIManager _podium;

    [SerializeField]
    private GameObject _playerListItemPrefab;

    private SessionManager _sessionManager;

    public override void OnStartClient()
    {
        base.OnStartClient();

        var networkManager = InstanceFinder.NetworkManager;

        if (networkManager == null)
        {
            Debug.LogError("NetworkManager not found.");
            return;
        }

        // Find the session manager and listen for player list updates
        _sessionManager = networkManager.GetComponent<SessionManager>();

        _sessionManager.OnPlayerListUpdate.AddListener(UpdatePlayerList);
    }

    private void UpdatePlayerList(PlayerListUpdateBroadcast broadcast)
    {
        // Clear the player list but leave the headers
        for (int i = 1; i < _playerList.transform.childCount; i++)
        {
            Destroy(_playerList.transform.GetChild(i).gameObject);
        }

        // Add players to a list and sort them by kills

        List<Player> players = new List<Player>(broadcast.Players.Values.ToList());

        players.Sort((x, y) => y.Kills.CompareTo(x.Kills));

        int place = 1;
        foreach (var player in players)
        {
            GameObject playerUI = Instantiate(_playerListItemPrefab, _playerList.transform);

            PlayerListItemUIManager playerUIManager = playerUI.GetComponent<PlayerListItemUIManager>();

            playerUIManager.SetPlayer(player, place);

            place++;
        }


    }
}
