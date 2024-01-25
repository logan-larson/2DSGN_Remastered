using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameUIManager : NetworkBehaviour
{

    [SerializeField]
    private GameObject _pauseCanvas;

    private InputManager _inputManager;

    #region Initialization

    private void Awake()
    {
        _pauseCanvas.SetActive(false);

        FirstObjectNotifier.OnFirstObjectSpawned += FirstObjectNotifier_OnFirstObjectSpawned;
    }

    private void OnDestroy()
    {
        FirstObjectNotifier.OnFirstObjectSpawned -= FirstObjectNotifier_OnFirstObjectSpawned;
    }

    private void FirstObjectNotifier_OnFirstObjectSpawned(Transform obj, GameObject go)
    {
        _inputManager = obj.GetComponent<InputManager>();

        _inputManager.TogglePause.AddListener(TogglePause);
    }

    #endregion

    public void TogglePause()
    {
        _pauseCanvas.SetActive(!_pauseCanvas.activeSelf);
    }

    public void OnLeaveGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void OnResumeGame()
    {
        _pauseCanvas.SetActive(false);
    }

}