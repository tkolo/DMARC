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

using System.Xml.Linq;

namespace DMARC.Shared.Model
{
    public class DkimAuthResult : DbModel
    {
        public DkimAuthResult() {}
        
        public DkimAuthResult(XElement xDkimAuthResult)
        {
            Domain = xDkimAuthResult.Element(@"domain")?.Value ?? throw new InvalidDmarcReportFormatException();
            Result = DkimResultParser.Parse(xDkimAuthResult.Element(@"result") ?? throw new InvalidDmarcReportFormatException());
            HumanResult = xDkimAuthResult.Element(@"human_result")?.Value;
        }

        public virtual string Domain { get; set; }
        public virtual DkimResult Result { get; set; }
        public virtual string HumanResult { get; set; }
    }
}