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

namespace PlayFab.Multiplayer
{
    /// <summary>
    /// Information for all the constants related to lobby.
    /// </summary>
    public class LobbyConstants
    {
        /// <summary>
        /// The minimum allowed value for <see cref="LobbyCreateConfiguration.MaxMemberCount" /> and
        /// <see cref="LobbyDataUpdate.MaxMemberCount" />.
        /// </summary>
        public const uint LobbyMaxMemberCountLowerLimit = InteropWrapper.PFLobbyConsts.LobbyMaxMemberCountLowerLimit;

        /// <summary>
        /// The maximum allowed value for <see cref="LobbyCreateConfiguration.MaxMemberCount" /> and
        /// <see cref="LobbyDataUpdate.MaxMemberCount" />.
        /// </summary>
        public const uint LobbyMaxMemberCountUpperLimit = InteropWrapper.PFLobbyConsts.LobbyMaxMemberCountUpperLimit;

        /// <summary>
        /// The maximum number of concurrent search properties which can be stored for the lobby.
        /// </summary>
        public const uint LobbyMaxSearchPropertyCount = InteropWrapper.PFLobbyConsts.LobbyMaxSearchPropertyCount;

        /// <summary>
        /// The maximum number of concurrent properties which can be stored for the lobby and which aren't owned by any specific
        /// member.
        /// </summary>
        public const uint LobbyMaxLobbyPropertyCount = InteropWrapper.PFLobbyConsts.LobbyMaxLobbyPropertyCount;

        /// <summary>
        /// The maximum number of concurrent properties allowed for each member in the lobby.
        /// </summary>
        public const uint LobbyMaxMemberPropertyCount = InteropWrapper.PFLobbyConsts.LobbyMaxMemberPropertyCount;

        /// <summary>
        /// The maximum number of search results that client-entity callers may request when performing a
        /// <see cref="PlayFabMultiplayer.FindLobbies" /> operation.
        /// </summary>
        public const uint LobbyClientRequestedSearchResultCountUpperLimit = InteropWrapper.PFLobbyConsts.LobbyClientRequestedSearchResultCountUpperLimit;
        
        /// <summary>
        /// A special, predefined search key, which can be used in the <see cref="LobbySearchConfiguration.FilterString" /> filtering and
        /// sorting strings to search for lobbies based on the current number of members in the lobby.
        /// </summary>
        /// <remarks>
        /// Example: "lobby/memberCount lt 5"
        /// </remarks>
        public static readonly string LobbyMemberCountSearchKey = InteropWrapper.PFLobbyConsts.LobbyMemberCountSearchKey;

        /// <summary>
        /// A special, predefined search key, which can be used in the <see cref="LobbySearchConfiguration.FilterString" /> filtering and
        /// sorting strings to search for lobbies based on the number of remaining members who can join the lobby.
        /// </summary>
        /// <remarks>
        /// Example: "lobby/memberCount lt 5"
        /// </remarks>
        public static readonly string LobbyMemberCountRemainingSearchKey = InteropWrapper.PFLobbyConsts.LobbyMemberCountRemainingSearchKey;
        
        /// <summary>
        /// A special, predefined search key, which can be used in the <see cref="LobbySearchConfiguration.FilterString" /> filtering
        /// string to search for lobbies that you' re currently a member of.
        /// </summary>
        /// <remarks>
        /// Example: "lobby/amMember eq true"
        /// </remarks>
        public static readonly string LobbyAmMemberSearchKey = InteropWrapper.PFLobbyConsts.LobbyAmMemberSearchKey;
        
        /// <summary>
        /// A special, predefined search key, which can be used in the <see cref="LobbySearchConfiguration.FilterString" /> filtering
        /// string to search for lobbies that you own.
        /// </summary>
        /// <remarks>
        /// Example: "lobby/amOwner eq true"
        /// </remarks>
        public static readonly string LobbyAmOwnerSearchKey = InteropWrapper.PFLobbyConsts.LobbyAmOwnerSearchKey;

        /// <summary>
        /// A special, predefined search key that can be used in the <see cref="LobbySearchConfiguration.FilterString" /> filtering
        /// string to search for lobbies with a specific lock state.
        /// </summary>
        /// <remarks>
        /// Example: "lobby/membershipLock eq 'Unlocked'"
        /// </remarks>
        static readonly string LobbyMembershipLockSearchKey = InteropWrapper.PFLobbyConsts.LobbyMembershipLockSearchKey;
    }
}
