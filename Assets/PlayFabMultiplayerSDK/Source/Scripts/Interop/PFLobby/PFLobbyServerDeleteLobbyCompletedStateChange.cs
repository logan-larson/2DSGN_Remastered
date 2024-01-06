namespace PlayFab.Multiplayer.Interop
{
    [NativeTypeName("struct PFLobbyServerDeleteLobbyCompletedStateChange : PFLobbyStateChange")]
    public unsafe partial struct PFLobbyServerDeleteLobbyCompletedStateChange
    {
        public PFLobbyStateChange __AnonymousBase_1;

        [NativeTypeName("PFLobbyHandle")]
        public PFLobby* lobby;

        public void* asyncContext;
    }
}
