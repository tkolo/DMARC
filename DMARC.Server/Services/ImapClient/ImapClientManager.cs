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
using DMARC.Shared.Model.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DMARC.Server.Services.ImapClient
{
    public class ImapClientManager : IImapClientManager
    {
        private readonly List<ImapClientOptions> _options = new List<ImapClientOptions>();
        private readonly List<IImapClient> _clients = new List<IImapClient>();
        private readonly IDisposable _optionsChange;
        private readonly IServiceScope _scope;

        public ImapClientManager(IOptionsMonitor<List<ImapClientOptions>> optionsMonitor,
            IServiceScopeFactory scopeFactory)
        {
            _optionsChange = optionsMonitor.OnChange(ReloadClients);
            _options.AddRange(optionsMonitor.CurrentValue);
            _scope = scopeFactory.CreateScope();
        }

        public async void Start()
        {
            await LoadAndConnectClients();
        }

        private async void ReloadClients(List<ImapClientOptions> options)
        {
            _options.Clear();
            _options.AddRange(options);
            await LoadAndConnectClients();
        }

        private async Task DisconnectAllClients()
        {
            var tasks = new List<Task>();
            foreach (var client in _clients)
            {
                tasks.Add(client.Stop());
            }

            await Task.WhenAll(tasks);

            foreach (var client in _clients)
            {
                client.Dispose();
            }

            _clients.Clear();
        }

        private async Task LoadAndConnectClients()
        {
            await DisconnectAllClients();
            foreach (var clientOptions in _options)
            {
                var client = _scope.ServiceProvider.GetService<IImapClient>();
                client.Start(clientOptions);
                _clients.Add(client);
            }
        }

        public void Dispose()
        {
            _optionsChange?.Dispose();
            _scope?.Dispose();
        }
    }
}