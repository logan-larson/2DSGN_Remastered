using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUIManager : MonoBehaviour
{
    public GameObject BaseMenu;
    public GameObject HostMenu;
    public GameObject JoinMenu;

    public TMP_Text UsernameText;

    [SerializeField]
    private BuildInfo _buildInfo;

    [SerializeField]
    private UserInfo _userInfo;

    private void Start()
    {
        BaseMenu.SetActive(true);
        HostMenu.SetActive(false);
        JoinMenu.SetActive(false);

        _buildInfo.IsFreeplay = false;

        UsernameText.text = _userInfo.Username;
    }

    public void OpenHostMenu()
    {
        HostMenu.SetActive(true);
    }

    public void CloseHostMenu()
    {
        HostMenu.SetActive(false);
    }

    public void OpenJoinMenu()
    {
        JoinMenu.SetActive(true);
    }

    public void CloseJoinMenu()
    {
        JoinMenu.SetActive(false);
    }

    public void OpenLobbyShell()
    {
        // Load the lobby scene
        SceneManager.LoadScene("LobbyShell");
    }

    public void OpenFreeplayGame()
    {
        // Set the server info to localhost and port 7770
        _buildInfo.IsFreeplay = true;

        // Load the freeplay scene
        SceneManager.LoadScene("PreGameLobby");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
