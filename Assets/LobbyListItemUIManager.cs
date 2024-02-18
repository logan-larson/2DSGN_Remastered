using Beamable.Experimental.Api.Lobbies;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListItemUIManager : MonoBehaviour
{
    [SerializeField]
    private Image _background;

    [SerializeField]
    private TMP_Text _nameText;

    [SerializeField]
    private TMP_Text _mapText;

    [SerializeField]
    private TMP_Text _gamemodeText;

    [SerializeField]
    private TMP_Text _gamestateText;

    [SerializeField]
    private TMP_Text _playerCountText;

    [SerializeField]
    private Button _joinButton;

    [SerializeField]
    private MainMenuUIManager _mainMenuUIManager;

    private Lobby _lobby;

    private void Start()
    {
        _mainMenuUIManager = FindObjectOfType<MainMenuUIManager>();

        _joinButton.onClick.AddListener(OnJoinButtonClicked);
    }

    private void OnJoinButtonClicked()
    {
        _mainMenuUIManager.JoinLobbyAsync(_lobby.lobbyId);
    }

    public void SetLobby(Lobby lobby, int place)
    {
        _lobby = lobby;

        if (_nameText != null)
            _nameText.text = lobby.name;

        var details = ParseDescription(lobby.description);

        if (_mapText != null)
            _mapText.text = details.Map;

        if (_gamemodeText != null)
            _gamemodeText.text = details.Gamemode;

        if (_gamestateText != null)
            _gamestateText.text = details.Gamestate;

        if (_playerCountText != null)
            _playerCountText.text = $"{lobby.players.Count}/{lobby.maxPlayers}";

        if (place % 2 == 0)
            _background.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        else
            _background.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
    }

    private LobbyListItemDetails ParseDescription(string description)
    {

        Debug.Log(description);

        // The description is a semicolon delimited list of strings
        // server_address:port;map;gamemode;gamestate;player_count;max_players
        var details = new LobbyListItemDetails();

        var parts = description.Split(';');

        details.Map = parts[1];
        details.Gamemode = parts[2];
        details.Gamestate = parts[3];
        //details.PlayerCount = parts[4];
        //details.MaxPlayers = parts[5];

        return details;
    }
}

public class LobbyListItemDetails
{
    public string Map;
    public string Gamemode; 
    public string Gamestate;

    // Deprecated
    public string PlayerCount;
    public string MaxPlayers;
}
