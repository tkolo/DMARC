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
using System.Threading.Tasks;
using DMARC.Shared.Dto;
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

        public async Task<Report> GetReportAsync(string reportId)
        {
            var path = DocumentPath<Report>.Id(reportId);

            return (await Client.GetAsync(path, x => x.Index("reports"))).Source;
        }

        public async Task<(IEnumerable<Report>, int)> GetAllReportsAsync(int pageNum,
            int pageSize,
            bool onlyFailed,
            Direction requestDirection)
        {
            var response =
                await Client.SearchAsync<Report>(x => BuildDescriptor(x, pageNum, pageSize, onlyFailed, requestDirection));

            return (response.Documents, (int) response.Total);
        }

        private static SearchDescriptor<Report> BuildDescriptor(SearchDescriptor<Report> descriptor,
            int pageNum,
            int pageSize,
            bool onlyFailed,
            Direction direction)
        {
            QueryContainer BuildQuery(QueryContainerDescriptor<Report> query)
            {
                var result = new List<QueryContainer>();
                if (onlyFailed)
                    result.Add(query.Bool(
                        b => b.Must(
                            m => m.Terms(t => t.Field(f => f.Records.First().Dkim).Terms(DmarcResult.Fail)),
                            m => m.Terms(t => t.Field(f => f.Records.First().Spf).Terms(DmarcResult.Fail))
                        )
                    ));

                if (direction != Direction.Both)
                    result.Add(query.Term(t => t.Field(f => f.Incoming).Value(direction == Direction.Incoming)));

                return result.Count == 1 ? result.Single() : query.Bool(b => b.Must(result.ToArray()));
            }
            
            if (onlyFailed || direction != Direction.Both)
            {
                descriptor.Query(BuildQuery);
            }

            descriptor.From((pageNum - 1) * pageSize).Size(pageSize).Sort(s => s.Descending(r => r.End));
            return descriptor;
        }
    }
}