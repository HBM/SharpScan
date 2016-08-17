// <copyright file="DeviceMonitor.cs" company="Hottinger Baldwin Messtechnik GmbH">
//
// CS Scan, a library for scanning and configuring HBM devices.
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

using System;
using System.Collections.Generic;
using System.Timers;

namespace Hbm.Devices.Scan.Announcing
{
    public class DeviceMonitor
    {
        public event EventHandler<NewDeviceEventArgs> HandleNewDevice;
        public event EventHandler<UpdateDeviceEventArgs> HandleUpdateDevice;
        public event EventHandler<RemoveDeviceEventArgs> HandleRemoveDevice;

        private bool stopped;
        private readonly IDictionary<string, AnnounceTimer> deviceMap;

        public DeviceMonitor()
        {
            stopped = false;
            deviceMap = new Dictionary<string, AnnounceTimer>();
        }

        public void Close()
        {
            lock (deviceMap)
            {
                stopped = true;
                var keysToRemove = new List<string>();
                foreach (KeyValuePair<string, AnnounceTimer> entry in deviceMap)
                {
                    AnnounceTimer timer = deviceMap[entry.Key];
                    timer.Stop();
                    timer.Close();
                    keysToRemove.Add(entry.Key);
                }
                foreach (var key in keysToRemove)
                {
                    deviceMap.Remove(key);
                }
            }
        }

        public bool IsClosed()
        {
            lock (deviceMap)
            {
                return stopped;
            }
        }

        public void HandleEvent(object sender, AnnounceEventArgs args)
        {
            if ((sender != null) && (args != null))
            {
                Announce announce = args.Announce;
                ArmTimer(announce);
            }
        }

        private void ArmTimer(Announce announce)
        {
            string path = announce.Path;
            int expriationMs = GetExpirationMilliSeconds(announce);
            lock (deviceMap)
            {
                if (!stopped)
                {
                    if (deviceMap.ContainsKey(path))
                    {
                        AnnounceTimer timer = deviceMap[path];
                        timer.Stop();
                        Announce oldAnnounce = timer.announce;
                        if (!oldAnnounce.Equals(announce))
                        {
                            timer.announce = announce;
                            if (HandleUpdateDevice != null)
                            {
                                UpdateDeviceEventArgs updateDeviceEvent = new UpdateDeviceEventArgs();
                                updateDeviceEvent.NewAnnounce = announce;
                                updateDeviceEvent.OldAnnounce = oldAnnounce;
                                HandleUpdateDevice(this, updateDeviceEvent);
                            }
                        }
                        timer.Interval = expriationMs;
                        timer.Start();
                    }
                    else
                    {
                        AnnounceTimer timer = new AnnounceTimer(expriationMs, announce);
                        timer.AutoReset = false;
                        timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                        deviceMap.Add(path, timer);
                        timer.Start();
                        if (HandleNewDevice != null)
                        {
                            NewDeviceEventArgs newDeviceEvent = new NewDeviceEventArgs();
                            newDeviceEvent.Announce = announce;
                            HandleNewDevice(this, newDeviceEvent);
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
                string path = timer.announce.Path;
                lock (deviceMap)
                {
                    deviceMap.Remove(path);
                }
                if (HandleRemoveDevice != null)
                {
                    RemoveDeviceEventArgs removeDeviceEvent = new RemoveDeviceEventArgs();
                    removeDeviceEvent.Announce = timer.announce;
                    HandleRemoveDevice(this, removeDeviceEvent);
                }
            }
        }

        private class AnnounceTimer : System.Timers.Timer
        {
            internal Announce announce;

            internal AnnounceTimer(double expire, Announce announce)
                : base(expire)
            {
                this.announce = announce;
            }
        }
    }

    public class NewDeviceEventArgs : System.EventArgs
    {
        public Announce Announce { get; internal set; }
    }

    public class UpdateDeviceEventArgs : System.EventArgs
    {
        public Announce NewAnnounce { get; internal set; }
        public Announce OldAnnounce { get; internal set; }
    }

    public class RemoveDeviceEventArgs : System.EventArgs
    {
        public Announce Announce { get; internal set; }
    }
}
