using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapInfo", menuName = "SGN/MapInfo", order = 5)]
public class MapInfo : ScriptableObject
{
    public string Name;
    public string PrefabPath;
    public string ThumbnailPath;
}
