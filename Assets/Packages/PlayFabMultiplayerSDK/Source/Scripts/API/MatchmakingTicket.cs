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
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Information for a matchmaking ticket.
    /// </summary>
    public class MatchmakingTicket
    {
        private static Dictionary<IntPtr, MatchmakingTicket> matchmakingTicketCache = new Dictionary<IntPtr, MatchmakingTicket>();

        internal MatchmakingTicket(InteropWrapper.PFMatchmakingTicketHandle handle)
        {
            this.Handle = handle;
        }

        /// <summary>
        /// The matchmaking ticket status.
        /// </summary>
        public MatchmakingTicketStatus Status
        {
            get
            {
                InteropWrapper.PFMatchmakingTicketStatus status;
                PlayFabMultiplayer.Succeeded(InteropWrapper.PFMultiplayer.PFMatchmakingTicketGetStatus(this.Handle, out status));
                return (MatchmakingTicketStatus)status;
            }
        }

        /// <summary>
        /// The ID of the matchmaking ticket
        /// </summary>
        public string TicketId
        {
            get
            {
                string ticketId;
                PlayFabMultiplayer.Succeeded(InteropWrapper.PFMultiplayer.PFMatchmakingTicketGetTicketId(this.Handle, out ticketId));
                return ticketId;
            }
        }

        public object Context
        {
            /// <summary>
            /// Retrieves the app's private, custom pointer-sized context value previously associated with this ticket object.
            /// </summary>
            /// <remarks>
            /// If no custom context has been set yet, the value pointed to by <paramref name="customContext" /> is set to nullptr.
            /// </remarks>
            /// <returns>
            /// <c>Custom Context</c> if retrieving the custom context succeeded or null.
            /// </returns>
            /// <seealso cref="PFMatchmakingTicketSetCustomContext" />
            get
            {
                object context;
                PlayFabMultiplayer.Succeeded(InteropWrapper.PFMultiplayer.PFMatchmakingTicketGetCustomContext(this.Handle, out context));
                if (context != null)
                {
                    return context;
                }
                else
                {
                    return null;
                }
            }

            /// <summary>
            /// Configures an optional, custom pointer-sized context value with this ticket object.
            /// </summary>
            /// <remarks>
            /// The custom context is typically used as a "shortcut" that simplifies accessing local, title-specific memory
            /// associated with the ticket without requiring a mapping lookup. The value is retrieved using the
            /// <see cref="PFMatchmakingTicketGetCustomContext()" /> method.
            /// <para>
            /// Any configured value is treated as opaque by the library, and is only valid on the local device; it is not
            /// transmitted over the network.
            /// </para>
            /// </remarks>
            set
            {
                PlayFabMultiplayer.Succeeded(InteropWrapper.PFMultiplayer.PFMatchmakingTicketSetCustomContext(this.Handle, value));
            }
        }

        internal InteropWrapper.PFMatchmakingTicketHandle Handle { get; set; }

        /// <summary>
        /// Cancels the ticket.
        /// </summary>
        /// <remarks>
        /// This method queues an asynchronous operation to cancel this matchmaking ticket. On success, a
        /// <see cref="PlayFabMultiplayer.OnMatchmakingTicketCompleted" /> will be provided indicating that the ticket has been
        /// canceled.
        /// <para>
        /// This method does not guarantee the ticket will be canceled. The ticket may complete before the cancellation can be
        /// processed, or the cancellation request may fail due to networking or service errors. If the cancellation attempt fails
        /// but is retrievable, the library will continue to retry the cancellation. Otherwise, a
        /// <see cref="PlayFabMultiplayer.OnMatchmakingTicketCompleted" /> will be provided that indicates the ticket failed.
        /// </para>
        /// </remarks>
        public void Cancel()
        {
            PlayFabMultiplayer.Succeeded(InteropWrapper.PFMultiplayer.PFMatchmakingTicketCancel(this.Handle));
        }

        /// <summary>
        /// Provides the match, if one has been found.
        /// </summary>
        public MatchmakingMatchDetails GetMatchDetails()
        {
            InteropWrapper.PFMatchmakingMatchDetails matchDetails;
            PlayFabMultiplayer.Succeeded(InteropWrapper.PFMultiplayer.PFMatchmakingTicketGetMatch(this.Handle, out matchDetails));
            if (matchDetails != null)
            {
                return new MatchmakingMatchDetails(matchDetails);
            }
            else
            {
                return null;
            }
        }

        internal static MatchmakingTicket GetMatchmakingTicketUsingCache(InteropWrapper.PFMatchmakingTicketHandle handle)
        {
            MatchmakingTicket ticket;
            bool found = matchmakingTicketCache.TryGetValue(handle.InteropHandleIntPtr, out ticket);
            if (found)
            {
                return ticket;
            }
            else
            {
                ticket = new MatchmakingTicket(handle);
                matchmakingTicketCache[handle.InteropHandleIntPtr] = ticket;
                return ticket;
            }
        }

        internal static void ClearMatchmakingTicketFromCache(InteropWrapper.PFMatchmakingTicketHandle handle)
        {
            if (matchmakingTicketCache.ContainsKey(handle.InteropHandleIntPtr))
            {
                matchmakingTicketCache.Remove(handle.InteropHandleIntPtr);
            }
        }
    }
}
