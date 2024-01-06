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
    using System.Linq;

    public class PFMatchmakingMatchMember
    {
        internal PFMatchmakingMatchMember()
        {
        }

        internal unsafe PFMatchmakingMatchMember(Interop.PFMatchmakingMatchMember* interopStruct)
        {
            this.EntityKey = new PFEntityKey(&interopStruct->entityKey);
            this.TeamId = Converters.PtrToStringUTF8((IntPtr)interopStruct->teamId);
            this.Attributes = Converters.PtrToStringUTF8((IntPtr)interopStruct->attributes);
        }

        internal unsafe Interop.PFMatchmakingMatchMember* ToPointer(DisposableCollection disposableCollection)
        {
            Interop.PFMatchmakingMatchMember memberPtr = new Interop.PFMatchmakingMatchMember();

            UTF8StringPtr idPtr = new UTF8StringPtr(this.EntityKey.Id, disposableCollection);
            UTF8StringPtr typePtr = new UTF8StringPtr(this.EntityKey.Type, disposableCollection);
            memberPtr.entityKey.id = idPtr.Pointer;
            memberPtr.entityKey.type = typePtr.Pointer;

            UTF8StringPtr teamIdPtr = new UTF8StringPtr(this.TeamId, disposableCollection);
            memberPtr.teamId = teamIdPtr.Pointer;

            UTF8StringPtr attributesPtr = new UTF8StringPtr(this.Attributes, disposableCollection);
            memberPtr.attributes = attributesPtr.Pointer;

            return (Interop.PFMatchmakingMatchMember*)Converters.StructToPtr<Interop.PFMatchmakingMatchMember>(memberPtr, disposableCollection);
        }

        public PFEntityKey EntityKey { get; set; }

        public string TeamId { get; set; }

        public string Attributes { get; set; }
    }
}
