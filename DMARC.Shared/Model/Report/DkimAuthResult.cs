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

namespace DMARC.Shared.Model.Report
{
    public class DkimAuthResult
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

        protected bool Equals(DkimAuthResult other)
        {
            return string.Equals(Domain, other.Domain) && Result == other.Result && string.Equals(HumanResult, other.HumanResult);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DkimAuthResult) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Domain != null ? Domain.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) Result;
                hashCode = (hashCode * 397) ^ (HumanResult != null ? HumanResult.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}