// <copyright file="FamilyTypeMatcher.cs" company="Hottinger Baldwin Messtechnik GmbH">
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

namespace Hbm.Devices.Scan.Announcing.Filter
{
    public class FamilyTypeMatcher : IMatcher
    {
        public const string QuantumX = "QuantumX";
        public const string PMX = "PMX";

        private readonly string[] familyTypes;

        public FamilyTypeMatcher(string[] familyTypes)
        {
            if (familyTypes == null)
            {
                throw new ArgumentNullException("familyTypes");
            }
            this.familyTypes = (string[])familyTypes.Clone();
        }

        public bool Match(Announce announce)
        {
            if (announce == null)
            {
                return false;
            }
            foreach (string familyType in familyTypes)
            {
                if (string.Compare(announce.Parameters.Device.FamilyType, familyType, StringComparison.Ordinal) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        public string[] GetFilterStrings()
        {
            return (string[])familyTypes.Clone();
        }
    }
}
