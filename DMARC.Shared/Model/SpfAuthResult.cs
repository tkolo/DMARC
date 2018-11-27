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
    public class SpfAuthResult
    {
        public SpfAuthResult() { }
        
        public SpfAuthResult(XElement xSpfAuthResult)
        {
            Domain = xSpfAuthResult.Element(@"domain")?.Value ?? throw new InvalidDmarcReportFormatException();
            Result = SpfResultParser.Parse(xSpfAuthResult.Element(@"result") ?? throw new InvalidDmarcReportFormatException());
        }

        public virtual string Domain { get; set; }
        public virtual SpfResult Result { get; set; }

        protected bool Equals(SpfAuthResult other)
        {
            return string.Equals(Domain, other.Domain) && Result == other.Result;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SpfAuthResult) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Domain != null ? Domain.GetHashCode() : 0) * 397) ^ (int) Result;
            }
        }
    }
}