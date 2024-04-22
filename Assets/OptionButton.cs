using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OptionButton : MonoBehaviour
{
    public TMP_Text VoteCountText;

    public int OptionIndex;

    [SerializeField]
    private PreGameLobbyUIManager _preGameLobbyUIManager;


    #region Scriptable Object Method

    public IntVariable VoteOptionIndex;
    public GameEvent OnVoteCast;

    #endregion

    public void OnOptionClicked()
    {
        _preGameLobbyUIManager.OnOptionClicked(OptionIndex);

        VoteOptionIndex.Value = OptionIndex;
        OnVoteCast.Raise();
    }
}
