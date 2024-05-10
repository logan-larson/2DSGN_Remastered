using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionButton : MonoBehaviour
{
    public TMP_Text VoteCountText;

    public Image ButtonImage; 

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

    public void OnVoteCasted()
    {
        if (VoteOptionIndex.Value == OptionIndex)
        {
            ButtonImage.color = Color.green;
        }
        else
        {
            ButtonImage.color = Color.white;
        }
    }
}
