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

using System.Collections.Generic;
using System.Threading.Tasks;
using DMARC.Server.Services.Imap;
using Microsoft.Extensions.Options;
using Nest;

namespace DMARC.Server.Repositories.ElasticsearchRepositories
{
    public class ElasticsearchImapClientDynamicSettingsRepository : ElasticsearchBaseRepository,
        IImapClientDynamicSettingsRepository
    {
        public ElasticsearchImapClientDynamicSettingsRepository(IOptions<ElasticsearchConfig> config) : base(config)
        {
        }

        public async Task<IEnumerable<ImapClientDynamicSettings>> GetAllAsync()
        {
            return (await Client
                .SearchAsync<ImapClientDynamicSettings>(x => x.Index("imapsettings").Size(int.MaxValue))).Documents;
        }

        public async Task<ImapClientDynamicSettings> GetAsync(string clientId)
        {
            var documentPath = DocumentPath<ImapClientDynamicSettings>.Id(clientId);

            if ((await Client.DocumentExistsAsync(documentPath, x => x.Index("imapsettings"))).Exists)
            {
                return (await Client.GetAsync(documentPath, x => x.Index("imapsettings"))).Source;
            }

            return new ImapClientDynamicSettings
            {
                ClientId = clientId
            };
        }

        public async Task SaveAsync(ImapClientDynamicSettings settings)
        {   
            var documentPath = DocumentPath<ImapClientDynamicSettings>.Id(settings.ClientId);
            
            if ((await Client.DocumentExistsAsync(documentPath, x => x.Index("imapsettings"))).Exists)
            {
                await Client.UpdateAsync(documentPath, descriptor => descriptor.Index("imapsettings").Doc(settings));
            }
            else
            {
                await Client.IndexAsync(settings, x => x.Index("imapsettings").Id(settings.ClientId));
            }
        }
    }
}