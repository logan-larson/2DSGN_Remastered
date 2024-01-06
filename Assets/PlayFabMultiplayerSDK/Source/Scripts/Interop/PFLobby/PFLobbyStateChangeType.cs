namespace PlayFab.Multiplayer.Interop
{
    [Interop.NativeTypeName("uint32_t")]
    public enum PFLobbyStateChangeType : uint
    {
        CreateAndJoinLobbyCompleted = 0,
        JoinLobbyCompleted = 1,
        MemberAdded = 2,
        AddMemberCompleted = 3,
        MemberRemoved = 4,
        ForceRemoveMemberCompleted = 5,
        LeaveLobbyCompleted = 6,
        Updated = 7,
        PostUpdateCompleted = 8,
        Disconnecting = 9,
        Disconnected = 10,
        JoinArrangedLobbyCompleted = 11,
        FindLobbiesCompleted = 12,
        InviteReceived = 13,
        InviteListenerStatusChanged = 14,
        SendInviteCompleted = 15,
        CreateAndClaimServerLobbyCompleted = 16,
        ClaimServerLobbyCompleted = 17,
        ServerPostUpdateCompleted = 18,
        ServerDeleteLobbyCompleted = 19,
    }
}
