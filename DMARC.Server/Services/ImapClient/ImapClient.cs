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
using System.Threading;
using System.Threading.Tasks;
using DMARC.Server.Repositories;
using MailKit;
using MailKit.Search;
using MailKit.Security;
using Microsoft.Extensions.Options;

namespace DMARC.Server.Services.ImapClient
{
    public class ImapClient : IImapClient
    {
        private readonly IOptionsMonitor<ImapClientOptions> _options;
        private readonly IReportRepository _reportRepository;
        private readonly DynamicSettings.DynamicSettings _dynamicSettings;
        private readonly IDisposable _reloader;
        private MailKit.Net.Imap.ImapClient _client;
        private CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private Task _currentTask;

        public ImapClient(IOptionsMonitor<ImapClientOptions> options,
            IReportRepository reportRepository,
            DynamicSettings.DynamicSettings dynamicSettings)
        {
            _options = options;
            _reportRepository = reportRepository;
            _dynamicSettings = dynamicSettings;
            _reloader = _options.OnChange(OnOptionsChange);
        }

        public void Start()
        {
            _currentTask = StartInternal();
        }

        public async Task StartInternal()
        {
            int port;
            SecureSocketOptions socketOptions;
            var options = _options.CurrentValue;

            if (string.IsNullOrEmpty(options.Server))
            {
                _currentTask = null;
                return;
            }

            switch (options.Protocol)
            {
                case ImapProtocol.Auto:
                    port = 0;
                    socketOptions = SecureSocketOptions.Auto;
                    break;
                case ImapProtocol.Imap:
                    port = 143;
                    socketOptions = SecureSocketOptions.None;
                    break;
                case ImapProtocol.Imaps:
                    port = 993;
                    socketOptions = SecureSocketOptions.SslOnConnect;
                    break;
                case ImapProtocol.ImapStartTls:
                    port = 143;
                    socketOptions = SecureSocketOptions.StartTls;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var cancellationToken = _tokenSource.Token;

            _client = new MailKit.Net.Imap.ImapClient();
            await _client.ConnectAsync(options.Server, port, socketOptions, cancellationToken);
            if (!string.IsNullOrEmpty(options.Username))
            {
                await _client.AuthenticateAsync(options.Username, options.Password, cancellationToken);
            }

            await _client.Inbox.OpenAsync(FolderAccess.ReadOnly, cancellationToken);

            IList<UniqueId> uids;
            if (_dynamicSettings.LastIndexedReportUid != 0)
            {
                var uid = new UniqueId(_dynamicSettings.LastIndexedReportUid + 1);
                var uidRange = new UniqueIdRange(uid, UniqueId.MaxValue);
                uids = await _client.Inbox.SearchAsync(uidRange, SearchQuery.All, cancellationToken);
            }
            else
            {
                uids = await _client.Inbox.SearchAsync(SearchQuery.All, cancellationToken);
            }
            

            foreach (var uid in uids)
            {
                var message = await _client.Inbox.GetMessageAsync(uid, cancellationToken);
                foreach (var report in MailParser.ParseMessage(message))
                {
                    await _reportRepository.AddReportAsync(report);
                }
            }
            _dynamicSettings.LastIndexedReportUid = uids.LastOrDefault().Id;
        }

        private void OnOptionsChange(ImapClientOptions newOptions, string arg2)
        {
            _tokenSource.Cancel();
            _currentTask?.Wait();
            _tokenSource = new CancellationTokenSource();
            _client?.Disconnect(true);
            _client?.Dispose();
            _client = null;
            if (_currentTask != null)
            {
                _currentTask = StartInternal();
            }
        }

        public void Dispose()
        {
            _reloader?.Dispose();
        }
    }
}