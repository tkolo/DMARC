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

using System;
using System.Xml.Linq;

namespace DMARC.Shared.Model.Report
{
    public enum SpfResult
    {
        None,
        Neutral,
        Pass,
        Fail,
        SoftFail,
        TempError,
        Unknown = TempError,
        PermError,
        Error = PermError
    }

    public static class SpfResultParser
    {
        public static SpfResult Parse(XElement xSpfResult)
        {
            if (xSpfResult == null)
                throw new InvalidDmarcReportFormatException();

            if (string.IsNullOrWhiteSpace(xSpfResult.Value))
                return SpfResult.None;

            if (Enum.TryParse(xSpfResult.Value, true, out SpfResult spfResult))
                return spfResult;

            throw new InvalidDmarcReportFormatException();
        }
    }
}