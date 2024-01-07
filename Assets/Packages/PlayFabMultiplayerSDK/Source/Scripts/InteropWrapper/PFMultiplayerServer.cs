
/*
 * PlayFab Unity SDK
 *
 * Copyright (c) Microsoft Corporation
 *
 * MIT License
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this
 * software and associated documentation files (the "Software"), to deal in the Software
 * without restriction, including without limitation the rights to use, copy, modify, merge,
 * publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
 * to whom the Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or
 * substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
 * PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
 * FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
 * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 */

namespace PlayFab.Multiplayer.InteropWrapper
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using PlayFab.Multiplayer.Interop;

    public class PFMultiplayerServer
    {
        public static int PFMultiplayerCreateAndClaimServerLobby(
            PFMultiplayerHandle handle,
            PFEntityKey server,
            PFLobbyCreateConfiguration createConfiguration,
            object asyncIdentifier,
            out PFLobbyHandle lobby)
        {
            using (DisposableCollection dc = new DisposableCollection())
            {
                unsafe
                {
                    IntPtr asyncId = IntPtr.Zero;
                    if (asyncIdentifier != null)
                    {
                        asyncId = GCHandle.ToIntPtr(GCHandle.Alloc(asyncIdentifier));
                    }

                    PFLobby* lobbyPtr = null;
                    void* asyncContext = asyncId.ToPointer();
                    int err = Methods.PFMultiplayerCreateAndClaimServerLobby(
                        handle.InteropHandle,
                        server.ToPointer(dc),
                        createConfiguration.ToPointer(dc),
                        asyncContext,
                        &lobbyPtr);

                    if (LobbyError.FAILED(err))
                    {
                        if (asyncId != IntPtr.Zero)
                        {
                            GCHandle asyncGcHandle = GCHandle.FromIntPtr(asyncId);
                            asyncGcHandle.Free();
                        }
                    }

                    lobby = new PFLobbyHandle(lobbyPtr);
                    return err;
                }
            }
        }

        public static int PFMultiplayerClaimServerLobby(
            PFMultiplayerHandle handle,
            PFEntityKey server,
            string lobbyId,
            object asyncIdentifier,
            out PFLobbyHandle lobby)
        {
            lobby = null;
            if (lobbyId == null)
            {
                return LobbyError.InvalidArg;
            }

            using (DisposableCollection dc = new DisposableCollection())
            {
                unsafe
                {
                    IntPtr asyncId = IntPtr.Zero;
                    if (asyncIdentifier != null)
                    {
                        asyncId = GCHandle.ToIntPtr(GCHandle.Alloc(asyncIdentifier));
                    }

                    UTF8StringPtr lobbyIdPtr = new UTF8StringPtr(lobbyId, dc);

                    PFLobby* lobbyPtr = null;
                    void* asyncContext = asyncId.ToPointer();
                    int err = Methods.PFMultiplayerClaimServerLobby(
                        handle.InteropHandle,
                        server.ToPointer(dc),
                        lobbyIdPtr.Pointer,
                        asyncContext,
                        &lobbyPtr);

                    if (LobbyError.FAILED(err))
                    {
                        if (asyncId != IntPtr.Zero)
                        {
                            GCHandle asyncGcHandle = GCHandle.FromIntPtr(asyncId);
                            asyncGcHandle.Free();
                        }
                    }

                    lobby = new PFLobbyHandle(lobbyPtr);
                    return err;
                }
            }
        }

        public static int PFLobbyServerPostUpdate(
            PFLobbyHandle lobby,
            PFLobbyDataUpdate lobbyUpdate,
            object asyncIdentifier)
        {
            if (lobby == null)
            {
                return LobbyError.InvalidArg;
            }

            var asyncId = IntPtr.Zero;
            if (asyncIdentifier != null)
            {
                asyncId = GCHandle.ToIntPtr(GCHandle.Alloc(asyncIdentifier));
            }

            using (DisposableCollection dc = new DisposableCollection())
            {
                unsafe
                {
                    Interop.PFLobbyDataUpdate* lobbyUpdateStructPtr = null;
                    if (lobbyUpdate != null)
                    {
                        lobbyUpdateStructPtr = lobbyUpdate.ToPointer(dc);
                    }

                    int err = Methods.PFLobbyServerPostUpdate(
                        lobby.InteropHandle,
                        lobbyUpdateStructPtr,
                        asyncId.ToPointer());

                    if (LobbyError.FAILED(err))
                    {
                        if (asyncId != IntPtr.Zero)
                        {
                            GCHandle asyncGcHandle = GCHandle.FromIntPtr(asyncId);
                            asyncGcHandle.Free();
                        }
                    }

                    return err;
                }
            }
        }

        public static int PFLobbyServerDeleteLobby(
            PFLobbyHandle lobby,
            object asyncIdentifier)
        {
            var asyncId = IntPtr.Zero;
            if (asyncIdentifier != null)
            {
                asyncId = GCHandle.ToIntPtr(GCHandle.Alloc(asyncIdentifier));
            }

            using (DisposableCollection dc = new DisposableCollection())
            {
                unsafe
                {
                    int err = Methods.PFLobbyServerDeleteLobby(
                        lobby.InteropHandle,
                        asyncId.ToPointer());

                    if (LobbyError.FAILED(err))
                    {
                        if (asyncId != IntPtr.Zero)
                        {
                            GCHandle asyncGcHandle = GCHandle.FromIntPtr(asyncId);
                            asyncGcHandle.Free();
                        }
                    }

                    return err;
                }
            }
        }

        public static int PFMultiplayerCreateServerBackfillTicket(
            PFMultiplayerHandle multiplayer,
            PFEntityKey server,
            PFMatchmakingServerBackfillTicketConfiguration configuration,
            object asyncIdentifier,
            out PFMatchmakingTicketHandle handle)
        {
            using (var disposableCollection = new DisposableCollection())
            {
                unsafe
                {
                    Interop.PFMatchmakingTicket* matchTicketHandle;

                    var asyncId = IntPtr.Zero;
                    if (asyncIdentifier != null)
                    {
                        asyncId = GCHandle.ToIntPtr(GCHandle.Alloc(asyncIdentifier));
                    }

                    int err = Methods.PFMultiplayerCreateServerBackfillTicket(
                        multiplayer.InteropHandle,
                        server.ToPointer(disposableCollection),
                        configuration.ToPointer(disposableCollection),
                        asyncId.ToPointer(),
                        &matchTicketHandle);

                    if (LobbyError.FAILED(err))
                    {
                        if (asyncId != IntPtr.Zero)
                        {
                            GCHandle asyncGcHandle = GCHandle.FromIntPtr(asyncId);
                            asyncGcHandle.Free();
                        }
                    }

                    return PFMatchmakingTicketHandle.WrapAndReturnError(err, matchTicketHandle, out handle);
                }
            }
        }
    }
}