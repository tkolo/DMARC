#region License

// DMARC report aggregator
// Copyright (C) 2018 Tomasz Kolosowski
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
using System.Net;

namespace DMARC.Client.Helpers
{
    public static class UriHelpers
    {   
        public static string ToQueryString(this object obj)
        {
            if (obj == null)
                return "";

            var valueParis = obj
                .GetType().GetProperties()
                .Where(x => x.CanRead)
                .Select(propertyInfo =>
                    $"{propertyInfo.Name}={WebUtility.UrlEncode(propertyInfo.GetValue(obj).ToString())}");

            return string.Join("&", valueParis);
        }
    }
}