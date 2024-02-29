using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionButton : MonoBehaviour
{
    public TMP_Text VoteCountText;
    public TMP_Text MapName;
    public Image MapThumbnail;

    public int OptionIndex;

    [SerializeField]
    private PreGameLobbyUIManager _preGameLobbyUIManager;

    public void OnOptionClicked()
    {
        _preGameLobbyUIManager.OnOptionClicked(OptionIndex);
    }

    public void SetMapInfo(MapInfo mapInfo)
    {
        var mapName = mapInfo.Name;
        var mapThumbnailPath = mapInfo.ThumbnailPath;

        var mapThumbnailSprite = Resources.Load<Sprite>(mapThumbnailPath);

        MapName.text = mapName;
        MapThumbnail.sprite = mapThumbnailSprite;
    }

    public void SetImageColor(Color32 color)
    {
        MapThumbnail.color = color;
    }
}
