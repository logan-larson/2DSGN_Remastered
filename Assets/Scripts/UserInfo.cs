using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UserInfo", menuName = "SGN/UserInfo", order = 1)]
public class UserInfo : ScriptableObject
{
    public string Username;
    public bool IsAuthenticated;
}
