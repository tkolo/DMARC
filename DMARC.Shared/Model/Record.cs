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

namespace DMARC.Shared.Model
{
    public class Record : DbModel
    {
        public Record() { }
        
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
        public virtual string SourceIp { get; set; }
        public virtual int Count { get; set; }

        // Policy evaluated        
        public virtual Disposition Disposition { get; set; }
        public virtual DmarcResult Dkim { get; set; }
        public virtual DmarcResult Spf { get; set; }
        public virtual IReadOnlyList<PolicyOverrideReason> Reasons { get; set; }

        // Identifier
        public virtual string EnvelopeTo { get; set; }
        public virtual string HeaderFrom { get; set; }

        // Auth result
        public virtual IReadOnlyList<DkimAuthResult> Dkims { get; set; }
        public virtual IReadOnlyList<SpfAuthResult> Spfs { get; set; }
    }
}