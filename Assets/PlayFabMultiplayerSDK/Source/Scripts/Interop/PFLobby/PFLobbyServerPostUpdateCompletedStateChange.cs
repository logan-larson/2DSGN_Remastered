namespace PlayFab.Multiplayer.Interop
{
    [NativeTypeName("struct PFLobbyServerPostUpdateCompletedStateChange : PFLobbyStateChange")]
    public unsafe partial struct PFLobbyServerPostUpdateCompletedStateChange
    {
        public PFLobbyStateChange __AnonymousBase_1;

        public int result;

        [NativeTypeName("PFLobbyHandle")]
        public PFLobby* lobby;

        public void* asyncContext;
    }
}
