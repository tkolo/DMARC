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
    public class PolicyOverrideReason
    {
        public PolicyOverrideReason() {}
        
        public PolicyOverrideReason(XElement xPolicyOverrideReason)
        {
            Type = PolicyOverrideParser.Parse(xPolicyOverrideReason.Element(@"type") ?? throw new InvalidDmarcReportFormatException());
            Comment = xPolicyOverrideReason.Element(@"comment")?.Value;
        }

        public PolicyOverride Type { get; set; }
        public string Comment { get; set; }

        protected bool Equals(PolicyOverrideReason other)
        {
            return Type == other.Type && string.Equals(Comment, other.Comment);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PolicyOverrideReason) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) Type * 397) ^ (Comment != null ? Comment.GetHashCode() : 0);
            }
        }
    }
}