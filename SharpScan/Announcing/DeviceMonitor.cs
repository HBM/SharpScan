// <copyright file="DeviceMonitor.cs" company="Hottinger Baldwin Messtechnik GmbH">
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
    using System.Collections.Generic;
    using System.Timers;

    public class DeviceMonitor
    {
        private readonly IDictionary<string, AnnounceTimer> deviceMap;

        private bool stopped;

        public DeviceMonitor()
        {
            this.stopped = false;
            this.deviceMap = new Dictionary<string, AnnounceTimer>();
        }

        public event EventHandler<NewDeviceEventArgs> HandleNewDevice;

        public event EventHandler<UpdateDeviceEventArgs> HandleUpdateDevice;

        public event EventHandler<RemoveDeviceEventArgs> HandleRemoveDevice;

        public void Close()
        {
            lock (this.deviceMap)
            {
                this.stopped = true;
                var keysToRemove = new List<string>();
                foreach (KeyValuePair<string, AnnounceTimer> entry in this.deviceMap)
                {
                    AnnounceTimer timer = this.deviceMap[entry.Key];
                    timer.Stop();
                    timer.Close();
                    keysToRemove.Add(entry.Key);
                }

                foreach (var key in keysToRemove)
                {
                    this.deviceMap.Remove(key);
                }
            }
        }

        public bool IsClosed()
        {
            lock (this.deviceMap)
            {
                return this.stopped;
            }
        }

        public void HandleEvent(object sender, AnnounceEventArgs args)
        {
            if ((sender != null) && (args != null))
            {
                Announce announce = args.Announce;
                this.ArmTimer(announce);
            }
        }

        private void ArmTimer(Announce announce)
        {
            string path = announce.Path;
            int expriationMs = this.GetExpirationMilliSeconds(announce);
            lock (this.deviceMap)
            {
                if (!this.stopped)
                {
                    if (this.deviceMap.ContainsKey(path))
                    {
                        AnnounceTimer timer = this.deviceMap[path];
                        timer.Stop();
                        Announce oldAnnounce = timer.Announce;
                        if (!oldAnnounce.Equals(announce))
                        {
                            timer.Announce = announce;
                            if (this.HandleUpdateDevice != null)
                            {
                                UpdateDeviceEventArgs updateDeviceEvent = new UpdateDeviceEventArgs();
                                updateDeviceEvent.NewAnnounce = announce;
                                updateDeviceEvent.OldAnnounce = oldAnnounce;
                                this.HandleUpdateDevice(this, updateDeviceEvent);
                            }
                        }

                        timer.Interval = expriationMs;
                        timer.Start();
                    }
                    else
                    {
                        AnnounceTimer timer = new AnnounceTimer(expriationMs, announce);
                        timer.AutoReset = false;
                        timer.Elapsed += new ElapsedEventHandler(this.OnTimedEvent);
                        this.deviceMap.Add(path, timer);
                        timer.Start();
                        if (this.HandleNewDevice != null)
                        {
                            NewDeviceEventArgs newDeviceEvent = new NewDeviceEventArgs();
                            newDeviceEvent.Announce = announce;
                            this.HandleNewDevice(this, newDeviceEvent);
                        }
                    }
                }
            }
        }

        private int GetExpirationMilliSeconds(Announce announce)
        {
            int expiration = announce.Parameters.Expiration;
            if (expiration == 0)
            {
                expiration = 6;
            }

            return expiration * 1000;
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            if (e != null)
            {
                AnnounceTimer timer = (AnnounceTimer)source;
                timer.Stop();
                string path = timer.Announce.Path;
                lock (this.deviceMap)
                {
                    this.deviceMap.Remove(path);
                }

                if (this.HandleRemoveDevice != null)
                {
                    RemoveDeviceEventArgs removeDeviceEvent = new RemoveDeviceEventArgs();
                    removeDeviceEvent.Announce = timer.Announce;
                    this.HandleRemoveDevice(this, removeDeviceEvent);
                }
            }
        }

        private class AnnounceTimer : System.Timers.Timer
        {
            internal AnnounceTimer(double expire, Announce announce)
                : base(expire)
            {
                this.Announce = announce;
            }

            internal Announce Announce { get; set; }
        }
    }
}
