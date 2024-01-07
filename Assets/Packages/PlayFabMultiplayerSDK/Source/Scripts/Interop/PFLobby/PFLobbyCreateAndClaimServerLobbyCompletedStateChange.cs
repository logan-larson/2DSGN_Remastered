namespace PlayFab.Multiplayer.Interop
{
    [NativeTypeName("struct PFLobbyCreateAndClaimServerLobbyCompletedStateChange : PFLobbyStateChange")]
    public unsafe partial struct PFLobbyCreateAndClaimServerLobbyCompletedStateChange
    {
        public PFLobbyStateChange __AnonymousBase_1;

        public int result;

        public void* asyncContext;

        [NativeTypeName("PFLobbyHandle")]
        public PFLobby* lobby;
    }
}
