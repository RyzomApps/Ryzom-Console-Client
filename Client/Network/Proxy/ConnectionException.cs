///////////////////////////////////////////////////////////////////
// This file contains modified code from 'L2 .NET'
// https://github.com/devmvalvm/L2Net
// which is released under GNU General Public License v2.0.
// http://www.gnu.org/licenses/
// Copyright 2018 devmvalvm
///////////////////////////////////////////////////////////////////

using System;

namespace Client.Network.Proxy
{
    public class ConnectionException : ApplicationException
    {
        public ConnectionException(string message) : base(message) { }
    }
}