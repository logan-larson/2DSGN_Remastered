namespace PlayFab.Multiplayer.Interop
{
    [NativeTypeName("struct PFLobbyClaimServerLobbyCompletedStateChange : PFLobbyStateChange")]
    public unsafe partial struct PFLobbyClaimServerLobbyCompletedStateChange
    {
        public PFLobbyStateChange __AnonymousBase_1;

        public int result;

        public void* asyncContext;

        [NativeTypeName("const char *")]
        public sbyte* lobbyId;

        [NativeTypeName("PFLobbyHandle")]
        public PFLobby* lobby;
    }
}
