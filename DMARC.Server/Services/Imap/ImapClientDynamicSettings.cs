#region License

// DMARC report aggregator
// Copyright (C) 2018 Tomasz Kolosowski
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

namespace DMARC.Server.Services.Imap
{
    public class ImapClientDynamicSettings
    {
        public string ClientId { get; set; }
        public uint LastReportedUid { get; set; }
    }
}