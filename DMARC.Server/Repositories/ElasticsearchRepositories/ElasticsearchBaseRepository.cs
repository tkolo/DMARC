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

using DMARC.Server.Services.Imap;
using DMARC.Shared.Model;
using DMARC.Shared.Model.Report;
using Microsoft.Extensions.Options;
using Nest;

namespace DMARC.Server.Repositories.ElasticsearchRepositories
{
    public abstract class ElasticsearchBaseRepository
    {
        protected IElasticClient Client { get; }

        protected ElasticsearchBaseRepository(IOptions<ElasticsearchConfig> config)
        {
            var connectionConfiguration = new ConnectionSettings(config.Value.Url)
                .DefaultMappingFor<Report>(x => x.IdProperty(r => r.ReportId).IndexName("reports"))
                .ThrowExceptions();

            Client = new ElasticClient(connectionConfiguration);
            
            InitializeIndexes();
        }

        private void InitializeIndexes()
        {
            if (!Client.IndexExists("reports").Exists)
            {
                Client.CreateIndex("reports", c => c
                    .Mappings(ms => ms                        
                        .Map<Report>(m => m
                            .AutoMap<DkimAuthResult>()
                            .AutoMap<PolicyOverrideReason>()
                            .AutoMap<Record>())));
            }

            if (!Client.IndexExists("imapsettings").Exists)
            {
                Client.CreateIndex("imapsettings", c => c
                    .Mappings(ms => ms
                        .Map<ImapClientDynamicSettings>(m => m.AutoMap())));
            }
        }
    }
}