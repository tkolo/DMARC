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

namespace DMARC.Shared.Model
{
    public enum DkimResult
    {
        None,
        Pass,
        Fail,
        Policy,
        Neutral,
        TempError,
        PermError
    }

    public static class DkimResultParser
    {
        public static DkimResult Parse(XElement xDkimResult)
        {
            if (xDkimResult == null)
                throw new InvalidDmarcReportFormatException();

            if (string.IsNullOrWhiteSpace(xDkimResult.Value))
                return DkimResult.None;

            if (Enum.TryParse(xDkimResult.Value, true, out DkimResult dkimResult))
                return dkimResult;

            throw new InvalidDmarcReportFormatException();
        }
    }
}