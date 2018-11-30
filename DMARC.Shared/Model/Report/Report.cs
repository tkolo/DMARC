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

namespace DMARC.Shared.Model.Report
{
    public class Report
    {
        public Report()
        {
        }

        public Report(XElement feedback)
        {
            if (!string.Equals(feedback.Name.LocalName, "feedback", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDmarcReportFormatException();


            // Report metadata
            var xReportMetadata = feedback.Element(@"report_metadata") ?? throw new InvalidDmarcReportFormatException();

            ReportId = xReportMetadata.Element(@"report_id")?.Value ?? throw new InvalidDmarcReportFormatException();
            OrganizationName = xReportMetadata.Element(@"org_name")?.Value ??
                               throw new InvalidDmarcReportFormatException();
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

            Errors = xReportMetadata.Elements(@"error").Select(x => x.Value).ToList();

            // Policy published
            var xPolicyPublished =
                feedback.Element(@"policy_published") ?? throw new InvalidDmarcReportFormatException();

            Domain = xPolicyPublished.Element(@"domain")?.Value ?? throw new InvalidDmarcReportFormatException();
            DkimAlignment =
                AlignmentParser.Parse(xPolicyPublished.Element(@"adkim") ??
                                      throw new InvalidDmarcReportFormatException());
            SpfAlignment =
                AlignmentParser.Parse(
                    xPolicyPublished.Element(@"aspf") ?? throw new InvalidDmarcReportFormatException());
            DomainPolicy =
                DispositionParser.Parse(xPolicyPublished.Element(@"p") ??
                                        throw new InvalidDmarcReportFormatException());
            SubdomainPolicy =
                DispositionParser.Parse(
                    xPolicyPublished.Element(@"sp") ?? throw new InvalidDmarcReportFormatException());
            Precent = int.TryParse(
                xPolicyPublished.Element(@"pct")?.Value ?? throw new InvalidDmarcReportFormatException(),
                out var iPercent)
                ? iPercent
                : throw new InvalidDmarcReportFormatException();

            // Records;
            Records = feedback.Elements(@"record").Select(x => new Record(x)).ToList();
            //TODO: dodać sprawdzenie czy jest conajmniej 1 rekord
        }

        // Report metadata
        public string ReportId { get; set; }
        public string OrganizationName { get; set; }
        public string Email { get; set; }
        public string ExtraContactInfo { get; set; }
        public DateTimeOffset Begin { get; set; }
        public DateTimeOffset End { get; set; }
        public ICollection<string> Errors { get; set; }


        // Policy published
        public string Domain { get; set; }
        public Alignment DkimAlignment { get; set; }
        public Alignment SpfAlignment { get; set; }
        public Disposition DomainPolicy { get; set; }
        public Disposition SubdomainPolicy { get; set; }
        public int Precent { get; set; }
        public string ServerId { get; set; }
        public bool Incoming { get; set; }

        public ICollection<Record> Records { get; set; }

        protected bool Equals(Report other)
        {
            return string.Equals(ReportId, other.ReportId) && string.Equals(OrganizationName, other.OrganizationName) &&
                   string.Equals(Email, other.Email) && string.Equals(ExtraContactInfo, other.ExtraContactInfo) &&
                   Begin.Equals(other.Begin) && End.Equals(other.End) && Errors.SequenceEqual(other.Errors) &&
                   string.Equals(Domain, other.Domain) && DkimAlignment == other.DkimAlignment &&
                   SpfAlignment == other.SpfAlignment && DomainPolicy == other.DomainPolicy &&
                   SubdomainPolicy == other.SubdomainPolicy && Precent == other.Precent &&
                   Records.SequenceEqual(other.Records);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Report) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (ReportId != null ? ReportId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (OrganizationName != null ? OrganizationName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Email != null ? Email.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ExtraContactInfo != null ? ExtraContactInfo.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Begin.GetHashCode();
                hashCode = (hashCode * 397) ^ End.GetHashCode();
                if (Errors != null)
                    hashCode = Errors.Aggregate(hashCode, (current, error) => (current * 397) ^ error.GetHashCode());
                hashCode = (hashCode * 397) ^ (Domain != null ? Domain.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) DkimAlignment;
                hashCode = (hashCode * 397) ^ (int) SpfAlignment;
                hashCode = (hashCode * 397) ^ (int) DomainPolicy;
                hashCode = (hashCode * 397) ^ (int) SubdomainPolicy;
                hashCode = (hashCode * 397) ^ Precent;
                if (Records != null)
                    hashCode = Records.Aggregate(hashCode, (current, record) => (current * 397) ^ record.GetHashCode());
                return hashCode;
            }
        }
    }
}