using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AuthenticationManager : MonoBehaviour
{

    [SerializeField]
    private TMP_InputField _username;

    [SerializeField]
    private TMP_Text _usernameError;

    [SerializeField]
    private UserInfo _userInfo;

    private void Start()
    {
        _usernameError.gameObject.SetActive(false);
    }

    public void Continue()
    {
        if (_username.text.Length == 0)
        {
            _usernameError.gameObject.SetActive(true);
            return;
        }

        _usernameError.gameObject.SetActive(false);

        _userInfo.Username = _username.text;

        // Load the main menu scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
