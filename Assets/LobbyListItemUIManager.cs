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
    private TMP_Text _gamemodeText;

    [SerializeField]
    private TMP_Text _playerCountText;

    [SerializeField]
    private TMP_Text _maxPlayerCountText;

    [SerializeField]
    private TMP_Text _gamestateText;

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

        if (place % 2 == 0)
            _background.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        else
            _background.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
    }
}
