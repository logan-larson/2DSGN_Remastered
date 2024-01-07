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
    /// Details about a Playfab Multiplayer Server.
    /// </summary>
    public class MultiplayerServerDetails
    {
        /// <summary>
        /// Details about a Playfab Multiplayer Server.
        /// </summary>
        /// <param name="fqdn">
        /// The fully qualified domain name of the virtual machine that is hosting this multiplayer server.
        /// </param>
        /// <param name="ipv4Address">
        /// The IPv4 address of the virtual machine that is hosting this multiplayer server.
        /// </param>
        /// <param name="ports">
        /// The ports the multiplayer server uses.
        /// </param>
        /// <param name="region">
        /// The server's region.
        /// </param>
        public MultiplayerServerDetails(string fqdn, string ipv4Address, IList<MultiplayerPort> ports, string region)
        {
            InteropWrapper.PFMultiplayerPort[] interopPorts = new InteropWrapper.PFMultiplayerPort[ports.Count];
            for (int i = 0; i < ports.Count; i++)
            {
                interopPorts[i] = new InteropWrapper.PFMultiplayerPort(
                    ports[i].Name,
                    ports[i].Num,
                    (InteropWrapper.PFMultiplayerProtocolType)ports[i].Protocol);
            }

            this.multiplayerServerDetails = new InteropWrapper.PFMultiplayerServerDetails(
                fqdn,
                ipv4Address,
                interopPorts,
                region,
                (uint)ports.Count);
        }

        internal MultiplayerServerDetails(InteropWrapper.PFMultiplayerServerDetails serverDetails)
        {
            this.multiplayerServerDetails = serverDetails;
        }

        internal InteropWrapper.PFMultiplayerServerDetails PFMultiplayerServerDetails
        {
            get
            {
                return this.multiplayerServerDetails;
            }
        }

        /// <summary>
        /// The fully qualified domain name of the virtual machine that is hosting this multiplayer server.
        /// </summary>
        public string Fqdn
        {
            get
            {
                return this.multiplayerServerDetails.Fqdn;
            }
            set
            {
                this.multiplayerServerDetails.Fqdn = value;
            }
        }

        /// <summary>
        /// The IPv4 address of the virtual machine that is hosting this multiplayer server.
        /// </summary>
        public string Ipv4Address
        {
            get
            {
                return this.multiplayerServerDetails.Ipv4Address;
            }
            set
            {
                this.multiplayerServerDetails.Ipv4Address = value;
            }
        }

        /// <summary>
        /// The ports the multiplayer server uses.
        /// </summary>
        public IList<MultiplayerPort> Ports
        {
            get
            {
                IList<MultiplayerPort> ports = new List<MultiplayerPort>();
                for (int i = 0; i < this.multiplayerServerDetails.PortCount; i++)
                {
                    MultiplayerPort port = new MultiplayerPort(this.multiplayerServerDetails.Ports[i].Name,
                                                               this.multiplayerServerDetails.Ports[i].Num,
                                                               (MultiplayerProtocolType)this.multiplayerServerDetails.Ports[i].Protocol);
                    ports.Add(port);
                }

                return ports;
            }
            set
            {
                this.multiplayerServerDetails.Ports = new InteropWrapper.PFMultiplayerPort[value.Count];
                for (int i = 0; i < value.Count; i++)
                {
                    this.multiplayerServerDetails.Ports[i] = new InteropWrapper.PFMultiplayerPort(
                        value[i].Name,
                        value[i].Num,
                        (InteropWrapper.PFMultiplayerProtocolType)value[i].Protocol);
                }
            }
        }
        

        /// <summary>
        /// The server's region.
        /// </summary>
        public string Region
        {
            get
            {
                return this.multiplayerServerDetails.Region;
            }
            set
            {
                this.multiplayerServerDetails.Region = value;
            }
        }

        internal InteropWrapper.PFMultiplayerServerDetails multiplayerServerDetails;
    }
}
