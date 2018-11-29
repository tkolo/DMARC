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
using System.Threading.Tasks;
using DMARC.Shared.Model;
using DMARC.Shared.Model.Report;
using Microsoft.Extensions.Options;
using Nest;

namespace DMARC.Server.Repositories.ElasticsearchRepositories
{
    public class ElasticsearchReportRepository : ElasticsearchBaseRepository, IReportRepository
    {
        public ElasticsearchReportRepository(IOptions<ElasticsearchConfig> config) : base(config)
        {
        }

        public async Task AddReportAsync(Report report)
        {
            var documentPath = DocumentPath<Report>.Id(report);
            
            if ((await Client.DocumentExistsAsync(documentPath)).Exists)
            {
                var oldReport = (await Client.GetAsync(documentPath)).Source;
                if (!report.Equals(oldReport))
                    throw new InvalidOperationException($"Report collision found for report {report.ReportId}");
            }
            else
            {
                await Client.IndexAsync(report, x => x.Index("reports").Id(report.ReportId));
            }
        }

        public async Task<IEnumerable<Report>> GetAllReportsAsync()
        {
            return (await Client.SearchAsync<Report>(x => x.Size(1000))).Documents;
        }
    }
}