using Beamable;
using Beamable.Player;
using Hathora.Core.Scripts.Runtime.Client;
using HathoraCloud;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AuthenticationUIManager : MonoBehaviour
{

    [SerializeField]
    private TMP_InputField _username;

    [SerializeField]
    private TMP_Text _usernameError;

    [SerializeField]
    private TMP_Text _loginStatus;

    [SerializeField]
    private UserInfo _userInfo;

    private BeamContext _beamContext;

    private PlayerAccount _playerAccount;

    [SerializeField]
    private GameEvent _playMusicEvent;

    private async void Start()
    {
        _playMusicEvent.Raise();

        _usernameError.gameObject.SetActive(false);

        _beamContext = BeamContext.Default;

        _loginStatus.text = "Connecting to authentication service...";

        // Login to Hathora, sets the AuthToken in HathoraClientMgr
        //await HathoraClientMgr.Singleton.AuthLoginAsync();

        await _beamContext.OnReady;

        _loginStatus.text = "Fetching user details...";
        await _beamContext.Accounts.OnReady;

        _loginStatus.text = "User details fetched!";

        _playerAccount = _beamContext.Accounts.Current;

        _username.text = _playerAccount.Alias;

        Debug.Log($"User Id: {_beamContext.PlayerId}");
    }

    public void Continue()
    {
        if (_username.text.Length == 0)
        {
            _usernameError.gameObject.SetActive(true);
            return;
        }

        _usernameError.gameObject.SetActive(false);

        if (_playerAccount.Alias != _username.text)
        {
            _loginStatus.text = "Updating user details...";

            var accountPromise = _beamContext.Accounts.Current.SetAlias(_username.text);

            accountPromise.Then((account) =>
            {
                _loginStatus.text = "User details updated!";
                _userInfo.Username = _username.text;

                // Load the main menu scene
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            });
        }
        else
        {
            _userInfo.Username = _username.text;

            // Load the main menu scene
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
}
