#region License
// DMARC report aggregator
// Copyright (C) 2018 Tomasz Kołosowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DMARC.Shared.Model.Settings
{
    public class AuthOptions
    {
        public string Server { get; set; }
        public SslMode Protocol { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class ImapOptions : AuthOptions
    {
        public List<string> LocalDomains { get; set; } = new List<string>();
    }

    public class SmtpOptions : AuthOptions
    {
        public List<string> SendTo { get; set; } = new List<string>();
        public SendVerbosity SendVerbosity { get; set; }
    }

    public enum SslMode
    {
        Auto,
        NoSsl,
        Ssl,
        StartTls
    }

    public enum SendVerbosity
    {
        All,
        PartialFailures,
        Failures,
        None
    }
}