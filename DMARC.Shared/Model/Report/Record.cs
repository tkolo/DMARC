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
using System.Linq;
using System.Xml.Linq;

namespace DMARC.Shared.Model.Report
{
    public class Record
    {
        public Record()
        {
            
        }

        public Record(XElement xRecord)
        {
            // Row
            var xRow = xRecord.Element(@"row") ?? throw new InvalidDmarcReportFormatException();

            SourceIp = xRow.Element(@"source_ip")?.Value ?? throw new InvalidDmarcReportFormatException();
            Count = int.TryParse(xRow.Element(@"count")?.Value ?? throw new InvalidDmarcReportFormatException(),
                out var iCount)
                ? iCount
                : throw new InvalidDmarcReportFormatException();
            var xPolicyEvaluated = xRow.Element(@"policy_evaluated");
            if (xPolicyEvaluated != null)
            {
                Disposition = DispositionParser.Parse(xPolicyEvaluated.Element(@"disposition") ??
                                                      throw new InvalidDmarcReportFormatException());
                Dkim = DmarcResultParser.Parse(xPolicyEvaluated.Element(@"dkim") ??
                                               throw new InvalidDmarcReportFormatException());
                Spf = DmarcResultParser.Parse(xPolicyEvaluated.Element(@"spf") ??
                                              throw new InvalidDmarcReportFormatException());
                Reasons = xPolicyEvaluated.Elements(@"reason").Select(x => new PolicyOverrideReason(x)).ToList();
            }

            // Identifier
            var xIdentifier = xRecord.Element(@"identifiers") ?? throw new InvalidDmarcReportFormatException();

            EnvelopeTo = xIdentifier.Element(@"envelope_to")?.Value;
            HeaderFrom = xIdentifier.Element(@"header_from")?.Value ?? throw new InvalidDmarcReportFormatException();

            // Auth result
            var xAuthResult = xRecord.Element(@"auth_results") ?? throw new InvalidDmarcReportFormatException();

            Dkims = xAuthResult.Elements(@"dkim").Select(x => new DkimAuthResult(x)).ToList();
            Spfs = xAuthResult.Elements(@"spf").Select(x => new SpfAuthResult(x)).ToList();
            //TODO: dodać sprawdzenie czy jest conajmniej 1 spfs
        }

        // Row
        public string SourceIp { get; set; }
        public int Count { get; set; }

        // Policy evaluated        
        public Disposition Disposition { get; set; }
        public DmarcResult Dkim { get; set; }
        public DmarcResult Spf { get; set; }
        public ICollection<PolicyOverrideReason> Reasons { get; set; }

        // Identifier
        public string EnvelopeTo { get; set; }
        public string HeaderFrom { get; set; }

        // Auth result
        public ICollection<DkimAuthResult> Dkims { get; set; }
        public ICollection<SpfAuthResult> Spfs { get; set; }

        protected bool Equals(Record other)
        {
            return string.Equals(SourceIp, other.SourceIp) && Count == other.Count &&
                   Disposition == other.Disposition && Dkim == other.Dkim && Spf == other.Spf &&
                   Reasons.SequenceEqual(other.Reasons) && string.Equals(EnvelopeTo, other.EnvelopeTo) &&
                   string.Equals(HeaderFrom, other.HeaderFrom) && Dkims.SequenceEqual(other.Dkims) &&
                   Spfs.SequenceEqual(other.Spfs);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Record) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (SourceIp != null ? SourceIp.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Count;
                hashCode = (hashCode * 397) ^ (int) Disposition;
                hashCode = (hashCode * 397) ^ (int) Dkim;
                hashCode = (hashCode * 397) ^ (int) Spf;
                if (Reasons != null)
                    hashCode = Reasons.Aggregate(hashCode, (current, reason) => (current * 397) ^ reason.GetHashCode());
                hashCode = (hashCode * 397) ^ (EnvelopeTo != null ? EnvelopeTo.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (HeaderFrom != null ? HeaderFrom.GetHashCode() : 0);
                if (Dkims != null)
                    hashCode = Dkims.Aggregate(hashCode, (current, result) => (current * 397) ^ result.GetHashCode());
                if (Spfs != null)
                    hashCode = Spfs.Aggregate(hashCode, (current, result) => (current * 397) ^ result.GetHashCode());
                return hashCode;
            }
        }
    }
}