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
    public enum PolicyOverride
    {
        Forwarded,
        SampledOut,
        TrustedForwarder,
        MailingList,
        LocalPolicy,
        Other
    }

    public static class PolicyOverrideParser
    {
        public static PolicyOverride Parse(XElement xPolicyOverride)
        {
            if (xPolicyOverride == null)
                throw new InvalidDmarcReportFormatException();

            if (string.IsNullOrWhiteSpace(xPolicyOverride.Value))
                return PolicyOverride.Other;

            return PolicyStringToEnum(xPolicyOverride.Value);
        }

        private static PolicyOverride PolicyStringToEnum(string policyString)
        {
            switch (policyString.ToLowerInvariant())
            {
                case "forwarded":
                    return PolicyOverride.Forwarded;
                case "sampled_out":
                    return PolicyOverride.SampledOut;
                case "trusted_forwarder":
                    return PolicyOverride.TrustedForwarder;
                case "mailing_list":
                    return PolicyOverride.MailingList;
                case "local_policy":
                    return PolicyOverride.LocalPolicy;
                default:
                    throw new ArgumentException($"Invalid policy string {policyString}", nameof(policyString));
            }
        }
    }
}