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

    public unsafe class PFMultiplayerHandle
    {
#if LOBBY_MOCK
        public PFMultiplayerHandle(UInt64 handle)
        {
            this.InteropHandle = (Interop.PFMultiplayer*)handle;
        }
#endif

        internal PFMultiplayerHandle(Interop.PFMultiplayer* interopHandle)
        {
            this.InteropHandle = interopHandle;
        }

#if LOBBY_MOCK
        public Interop.PFMultiplayer* InteropHandle { get; set; }
#else
        internal Interop.PFMultiplayer* InteropHandle { get; set; }
#endif

        internal static int WrapAndReturnError(int error, Interop.PFMultiplayer* interopHandle, out PFMultiplayerHandle handle)
        {
            if (LobbyError.SUCCEEDED(error))
            {
                handle = new PFMultiplayerHandle(interopHandle);
            }
            else
            {
                handle = default(PFMultiplayerHandle);
            }

            return error;
        }
    }
}
