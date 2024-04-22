using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Map", menuName = "SGN/Map")]
public class MapInfo : ScriptableObject
{
    public string MapName;
    public Sprite MapThumbnail;
    public GameModeInfo GameMode;


}
