// <copyright file="AnnounceDeserializer.cs" company="Hottinger Baldwin Messtechnik GmbH">
//
// SharpScan, a library for scanning and configuring HBM devices.
//
// The MIT License (MIT)
//
// Copyright (C) Hottinger Baldwin Messtechnik GmbH
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS
// BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN
// ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//
// </copyright>

namespace Hbm.Devices.Scan.Announcing
{
    using System;
    using System.IO;
    using System.Net.NetworkInformation;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.Text;

    public class AnnounceDeserializer
    {
        private readonly DataContractJsonSerializer deserializer;
        private readonly AnnounceCache cache;
        private readonly AnnounceEventArgs eventArgs;

        public AnnounceDeserializer(int cacheSize)
        {
            this.deserializer = new DataContractJsonSerializer(typeof(Announce));
            this.cache = new AnnounceCache(cacheSize);
            this.eventArgs = new AnnounceEventArgs();
        }

        public AnnounceDeserializer() : this(AnnounceCache.DefaultCacheSize)
        {
        }

        public event EventHandler<AnnounceEventArgs> HandleMessage;

        public void HandleEvent(object sender, MulticastMessageEventArgs args)
        {
            if ((sender != null) && (this.HandleMessage != null) && (args != null))
            {
                string jsonString = args.AnnounceJson;
                NetworkInterface incomingIF = args.IncomingInterface;

                if (!string.IsNullOrEmpty(jsonString))
                {
                    Announce announce = this.cache.Get(jsonString);
                    if (announce == null)
                    {
                        using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString)))
                        {
                            try
                            {
                                announce = (Announce)this.deserializer.ReadObject(ms);
                                if (MandatoryKeysPresent(announce))
                                {
                                    announce.JsonString = jsonString;
                                    announce.IdentifyCommunicationPath(incomingIF);
                                    this.cache.Add(jsonString, announce);
                                    this.eventArgs.Announce = announce;
                                    this.HandleMessage(this, this.eventArgs);
                                }
                            }
                            catch (SerializationException)
                            {
                                return;
                            }
                        }
                    }
                    else
                    {
                        this.eventArgs.Announce = announce;
                        this.HandleMessage(this, this.eventArgs);
                    }
                }
            }
        }

        internal AnnounceCache GetCache()
        {
            return this.cache;
        }

        private static bool MandatoryKeysPresent(Announce announce)
        {
            if (announce == null)
            {
                return false;
            }

            return KeysInAnnounceParamsPresent(announce.Parameters);
        }

        private static bool KeysInAnnounceParamsPresent(AnnounceParameters parameters)
        {
            if ((parameters == null) ||
                string.IsNullOrEmpty(parameters.ApiVersion) ||
                !parameters.ApiVersion.Equals("1.0"))
            {
                return false;
            }

            if ((parameters.Router != null) && string.IsNullOrEmpty(parameters.Router.Uuid))
            {
                return false;
            }

            return KeysInDevicePresent(parameters.Device) && KeysInNetSettingsPresent(parameters.NetSettings);
        }

        private static bool KeysInNetSettingsPresent(NetSettings netSettings)
        {
            if ((netSettings == null) ||
                (netSettings.Interface == null) ||
                string.IsNullOrEmpty(netSettings.Interface.Name))
            {
                return false;
            }

            return true;
        }

        private static bool KeysInDevicePresent(AnnouncedDevice device)
        {
            if ((device == null) ||
                string.IsNullOrEmpty(device.Uuid))
            {
                return false;
            }

            return true;
        }
    }
}
