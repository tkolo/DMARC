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
using System.Threading.Tasks;
using DMARC.Shared.Model;

namespace DMARC.Server.Repositories.InMemoryRepositories
{
    public class ReportInMemoryRepository : IReportRepository
    {
        private static readonly List<Report> Reports = new List<Report>();

        public Task AddReportAsync(Report report)
        {
            lock (Reports)
            {
                report.Id = Reports.Count + 1;
                Reports.Add(report);
            }

            return Task.CompletedTask;
        }

        public Task<IEnumerable<Report>> GetAllReportsAsync()
        {
            List<Report> copy;
            lock (Reports)
            {
                copy = new List<Report>(Reports);
            }

            return Task.FromResult((IEnumerable<Report>) copy);
        }
    }
}