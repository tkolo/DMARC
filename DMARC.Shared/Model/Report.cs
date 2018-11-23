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
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace DMARC.Shared.Model
{
    public class Report : DbModel
    {
        public Report() {}
        
        public Report(XElement feedback)
        {
            if (!string.Equals(feedback.Name.LocalName, "feedback", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDmarcReportFormatException();

            
            // Report metadata
            var xReportMetadata = feedback.Element(@"report_metadata") ?? throw new InvalidDmarcReportFormatException();
            
            ReportId = xReportMetadata.Element(@"report_id")?.Value ?? throw new InvalidDmarcReportFormatException();
            OrganizationName = xReportMetadata.Element(@"org_name")?.Value ?? throw new InvalidDmarcReportFormatException();
            Email = xReportMetadata.Element(@"email")?.Value ?? throw new InvalidDmarcReportFormatException();
            ExtraContactInfo = xReportMetadata.Element(@"extra_contact_info")?.Value;
            var xDateRange = xReportMetadata.Element(@"date_range") ?? throw new InvalidDmarcReportFormatException();
            
            if (xDateRange == null)
                throw new InvalidDmarcReportFormatException();

            var sBegin = xDateRange.Element(@"begin")?.Value ?? throw new InvalidDmarcReportFormatException();
            if (long.TryParse(sBegin, out var lBegin) == false)
                throw new InvalidDmarcReportFormatException();

            Begin = DateTimeOffset.FromUnixTimeSeconds(lBegin);

            var sEnd = xDateRange.Element(@"end")?.Value ?? throw new InvalidDmarcReportFormatException();
            if (long.TryParse(sEnd, out var lEnd) == false)
                throw new InvalidDmarcReportFormatException();

            End = DateTimeOffset.FromUnixTimeSeconds(lEnd);
            
            Errors = string.Join(",", xReportMetadata.Elements(@"error").Select(x => x.Value));
            
            // Policy published
            var xPolicyPublished =
                feedback.Element(@"policy_published") ?? throw new InvalidDmarcReportFormatException();
            
            Domain = xPolicyPublished.Element(@"domain")?.Value ?? throw new InvalidDmarcReportFormatException();
            DkimAlignment = AlignmentParser.Parse(xPolicyPublished.Element(@"adkim") ?? throw new InvalidDmarcReportFormatException());
            SpfAlignment = AlignmentParser.Parse(xPolicyPublished.Element(@"aspf") ?? throw new InvalidDmarcReportFormatException());
            DomainPolicy = DispositionParser.Parse(xPolicyPublished.Element(@"p") ?? throw new InvalidDmarcReportFormatException());
            SubdomainPolicy = DispositionParser.Parse(xPolicyPublished.Element(@"sp") ?? throw new InvalidDmarcReportFormatException());
            Precent = int.TryParse(xPolicyPublished.Element(@"pct")?.Value ?? throw new InvalidDmarcReportFormatException(), out var iPercent) ? iPercent : throw new InvalidDmarcReportFormatException();
            
            // Records;
            Records = feedback.Elements(@"record").Select(x => new Record(x)).ToList();
            //TODO: dodać sprawdzenie czy jest conajmniej 1 rekord
        }

        // Report metadata
        public virtual string ReportId { get; set; }
        public virtual string OrganizationName { get; set; }
        public virtual string Email { get; set; }
        public virtual string ExtraContactInfo { get; set; }
        public virtual DateTimeOffset Begin { get; set; }
        public virtual DateTimeOffset End { get; set; }
        public virtual string Errors { get; set; }
        
        
        // Policy published
        public virtual string Domain { get; set; }
        public virtual Alignment DkimAlignment { get; set; }
        public virtual Alignment SpfAlignment { get; set; }
        public virtual Disposition DomainPolicy { get; set; }
        public virtual Disposition SubdomainPolicy { get; set; }
        public virtual int Precent { get; set; }
        
        public virtual IReadOnlyList<Record> Records { get; set; }
    }
}