// <copyright file="Sender.cs" company="Hottinger Baldwin Messtechnik GmbH">
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

using System;
using System.Threading;
using Hbm.Devices.Scan;
using Hbm.Devices.Scan.Configure;

public class Sender : IConfigurationCallback
{
    private ConfigurationService service;

    private Sender()
    {
        ConfigurationMessageReceiver receiver = new ConfigurationMessageReceiver();
        ResponseDeserializer parser = new ResponseDeserializer();
        receiver.HandleMessage += parser.HandleEvent;
        this.service = new ConfigurationService(new ScanInterfaces().NetworkInterfaces, parser);
    }

    public static void Main(string[] args)
    {
        Sender sender = new Sender();
        ConfigurationDevice device = new ConfigurationDevice("0009E5001231");
        ConfigurationNetSettings settings = new ConfigurationNetSettings(new ConfigurationInterface("eth0", ConfigurationInterface.Method.Dhcp));

        // ConfigurationNetSettings settings = new ConfigurationNetSettings(new ConfigurationInterface("eth0", "10.1.2.3", "255.0.0.0", ConfigurationInterface.Method.Manual));
        ConfigurationParams parameters = new ConfigurationParams(device, settings);
        sender.service.SendConfiguration(parameters, sender, 1000);
        Thread.Sleep(500000);
        sender.service.Close();
    }

    public void OnError(JsonRpcResponse response)
    {
        Console.WriteLine("Error");
    }

    public void OnSuccess(JsonRpcResponse response)
    {
        Console.WriteLine("Success");
    }

    public void OnTimeout()
    {
        Console.WriteLine("Timeout");
    }
}
