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

    public class PFLobbyConsts
    {
        public const uint LobbyMaxMemberCountLowerLimit = Interop.Methods.PFLobbyMaxMemberCountLowerLimit;
        public const uint LobbyMaxMemberCountUpperLimit = Interop.Methods.PFLobbyMaxMemberCountUpperLimit;
        public const uint LobbyMaxSearchPropertyCount = Interop.Methods.PFLobbyMaxSearchPropertyCount;
        public const uint LobbyMaxLobbyPropertyCount = Interop.Methods.PFLobbyMaxLobbyPropertyCount;
        public const uint LobbyMaxMemberPropertyCount = Interop.Methods.PFLobbyMaxMemberPropertyCount;
        public const uint LobbyClientRequestedSearchResultCountUpperLimit = Interop.Methods.PFLobbyClientRequestedSearchResultCountUpperLimit;
// There was a known bug with ReadOnlySpan<byte> which has been fixed since Unity 2021.2b6.
// https://forum.unity.com/threads/2021-2-0b6-and-system-memory-readonlyspan-under-net-4-8.1152104/#:~:text=These%20libraries%20should%20work.%20All%20of%20the%20APIs%20from%20System.Memory.dll%20should%20be%20available%20with%20Unity%20in%20version%202021.2b6%20and%20later.
#if UNITY_2021_3_OR_NEWER
        public static readonly string LobbyMemberCountSearchKey = ConvertReadOnlySpanToString(Interop.Methods.PFLobbyMemberCountSearchKey);
        public static readonly string LobbyMemberCountRemainingSearchKey = ConvertReadOnlySpanToString(Interop.Methods.PFLobbyMemberCountRemainingSearchKey);
        public static readonly string LobbyAmMemberSearchKey = ConvertReadOnlySpanToString(Interop.Methods.PFLobbyAmMemberSearchKey);
        public static readonly string LobbyAmOwnerSearchKey = ConvertReadOnlySpanToString(Interop.Methods.PFLobbyAmOwnerSearchKey);
        public static readonly string LobbyMembershipLockSearchKey = ConvertReadOnlySpanToString(Interop.Methods.PFLobbyMembershipLockSearchKey);
#else
        public static readonly string LobbyMemberCountSearchKey = "lobby/memberCount";
        public static readonly string LobbyMemberCountRemainingSearchKey = "lobby/memberCountRemaining";
        public static readonly string LobbyAmMemberSearchKey = "lobby/amMember";
        public static readonly string LobbyAmOwnerSearchKey = "lobby/amOwner";
        public static readonly string LobbyMembershipLockSearchKey = "lobby/membershipLock";
#endif

#if LOBBY_MOCK
        public const int S_MOCK_TEST_OK = 0x00001234;
        public const UInt64 MOCK_TEST_HANDLE = 0x1234567890123456;
        public const uint MOCK_TEST_UINT_PARAM1 = 0x12345678;
        public const uint MOCK_TEST_UINT_PARAM2 = 0x87654321;
        public const UInt64 MOCK_TEST_UINT64_PARAM1 = 0x1234567812345678;
        public const string MOCK_TEST_STRING_PARAM1 = "Test123";
        public const string MOCK_TEST_STRING_PARAM2 = "234Test";
        public const string MOCK_TEST_STRING_PARAM3 = "Test567";
        public const string MOCK_TEST_STRING_PARAM4 = "567Test";
        public const string MOCK_TEST_ENTITY_ID = "MockEntityId";
        public const string MOCK_TEST_ENTITY_TYPE = "MockEntityType";
#endif

#if UNITY_2021_3_OR_NEWER
        private static string ConvertReadOnlySpanToString(ReadOnlySpan<byte> span)
        {
            return Converters.ByteArrayToString(span.ToArray());
        }
#endif
    }
}
