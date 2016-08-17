// <copyright file="JsonRpc.cs" company="Hottinger Baldwin Messtechnik GmbH">
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

using System.Globalization;
using System.Runtime.Serialization;

namespace Hbm.Devices.Scan
{
    [DataContractAttribute]
    public abstract class JsonRpc
    {
        [DataMember(Name = "jsonrpc")]
        public string Version { get; private set; }

        [DataMember(Name = "method")]
        public string Method { get; private set; }

        public string JsonString { get; internal set; }

        protected JsonRpc(string method)
        {
            Version = "2.0";
            Method = method;
        }

        public bool Equals(JsonRpc other)
        {
            if (object.ReferenceEquals(other, null))
            {
                return false;
            }
            return string.Compare(JsonString, other.JsonString, CultureInfo.InvariantCulture, CompareOptions.Ordinal) == 0;
        }
    }
}
