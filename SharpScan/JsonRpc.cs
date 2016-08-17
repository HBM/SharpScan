// <copyright file="JsonRpc.cs" company="Hottinger Baldwin Messtechnik GmbH">
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

namespace Hbm.Devices.Scan
{
    using System.Globalization;
    using System.Runtime.Serialization;

    [DataContractAttribute]
    public abstract class JsonRpc
    {
        protected JsonRpc(string method)
        {
            this.Version = "2.0";
            this.Method = method;
        }

        [DataMember(Name = "jsonrpc")]
        public string Version { get; private set; }

        [DataMember(Name = "method")]
        public string Method { get; private set; }

        public string JsonString { get; internal set; }

        public bool Equals(JsonRpc other)
        {
            if (object.ReferenceEquals(other, null))
            {
                return false;
            }

            return string.Compare(this.JsonString, other.JsonString, CultureInfo.InvariantCulture, CompareOptions.Ordinal) == 0;
        }
    }
}
