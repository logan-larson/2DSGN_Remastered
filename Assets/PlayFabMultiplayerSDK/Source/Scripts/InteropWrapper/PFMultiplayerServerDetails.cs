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

    public class PFMultiplayerServerDetails
    {
        public PFMultiplayerServerDetails(string fqdn, string ipv4Address, PFMultiplayerPort[] ports, string region, uint portCount)
        {
            this.Fqdn = fqdn;
            this.Ipv4Address = ipv4Address;
            this.Ports = ports;
            this.Region = region;
            this.PortCount = portCount;
        }

        internal unsafe PFMultiplayerServerDetails(Interop.PFMultiplayerServerDetails* interopStruct)
        {
            this.Region = Converters.PtrToStringUTF8((IntPtr)interopStruct->region);
            this.Fqdn = Converters.PtrToStringUTF8((IntPtr)interopStruct->fqdn);
            this.Ipv4Address = Converters.PtrToStringUTF8((IntPtr)interopStruct->ipv4Address);
            this.Ports = new PFMultiplayerPort[interopStruct->portCount];
            for (int i = 0; i < interopStruct->portCount; i++)
            {
                this.Ports[i] = new PFMultiplayerPort(&interopStruct->ports[i]);
            }

            this.PortCount = interopStruct->portCount;
        }

        internal unsafe Interop.PFMultiplayerServerDetails* ToPointer(DisposableCollection disposableCollection)
        {
            Interop.PFMultiplayerServerDetails serverDetailsPtr = new Interop.PFMultiplayerServerDetails();
            UTF8StringPtr fqdnPtr = new UTF8StringPtr(this.Fqdn, disposableCollection);
            serverDetailsPtr.fqdn = fqdnPtr.Pointer;

            UTF8StringPtr ipv4AddressPtr = new UTF8StringPtr(this.Ipv4Address, disposableCollection);
            serverDetailsPtr.ipv4Address = ipv4AddressPtr.Pointer;

            if (this.PortCount > 0)
            {
                Interop.PFMultiplayerPort[] ports = new Interop.PFMultiplayerPort[this.PortCount];
                for (int i = 0; i < this.PortCount; i++)
                {
                    ports[i] = *this.Ports[i].ToPointer(disposableCollection);
                }

                fixed (Interop.PFMultiplayerPort* portsArray = &ports[0])
                {
                    serverDetailsPtr.ports = portsArray;
                }
            }
            else
            {
                serverDetailsPtr.ports = null;
            }

            serverDetailsPtr.portCount = this.PortCount;

            UTF8StringPtr regionPtr = new UTF8StringPtr(this.Region, disposableCollection);
            serverDetailsPtr.region = regionPtr.Pointer;

            return (Interop.PFMultiplayerServerDetails*)Converters.StructToPtr<Interop.PFMultiplayerServerDetails>(serverDetailsPtr, disposableCollection);
        }

        public string Fqdn { get; set; }

        public string Ipv4Address { get; set; }

        public PFMultiplayerPort[] Ports { get; set; }

        public string Region { get; set; }

        public uint PortCount { get; set; }
    }
}