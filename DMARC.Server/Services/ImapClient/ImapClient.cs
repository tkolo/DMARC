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
using DMARC.Shared.Model.Settings;
using MailKit;
using MailKit.Search;
using MailKit.Security;
using Microsoft.Extensions.Options;

namespace DMARC.Server.Services.ImapClient
{
    public class ImapClient : IImapClient
    {
        private ImapClientOptions _options;
        private readonly IReportRepository _reportRepository;
        private readonly IImapClientDynamicSettingsRepository _settingsRepository;
        private MailKit.Net.Imap.ImapClient _client;
        private CancellationTokenSource _tokenSource;
        private Task _currentTask;

        public ImapClient(IReportRepository reportRepository,
            IImapClientDynamicSettingsRepository settingsRepository)
        {
            _reportRepository = reportRepository;
            _settingsRepository = settingsRepository;
        }

        public void Start(ImapClientOptions options)
        {
            if (_currentTask != null)
                throw new InvalidOperationException("Client already started");

            _options = options;
            _tokenSource = new CancellationTokenSource();
            _currentTask = StartInternal();
        }

        private async Task StartInternal()
        {
            if (string.IsNullOrEmpty(_options.Server))
            {
                _currentTask = null;
                return;
            }

            var cancellationToken = _tokenSource.Token;
            try
            {
                await Connect(cancellationToken);
                await FetchNewReports(cancellationToken);
                await ListenForNewReports(cancellationToken);
            }
            finally
            {
                await Disconnect();
            }
        }

        private async Task Connect(CancellationToken cancellationToken)
        {
            int port;
            SecureSocketOptions socketOptions;
            switch (_options.Protocol)
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

            _client = new MailKit.Net.Imap.ImapClient();
            await _client.ConnectAsync(_options.Server, port, socketOptions, cancellationToken);
            if (!string.IsNullOrEmpty(_options.Username))
            {
                await _client.AuthenticateAsync(_options.Username, _options.Password, cancellationToken);
            }

            await _client.Inbox.OpenAsync(FolderAccess.ReadOnly, cancellationToken);
        }
        
        private async Task FetchNewReports(CancellationToken cancellationToken)
        {
            var settings = await _settingsRepository.GetAsync(_options.Id);
            IList<UniqueId> uids;
            if (settings.LastReportedUid != 0)
            {
                var uid = new UniqueId(settings.LastReportedUid + 1);
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
                    report.ServerId = _options.Id;
                    report.Incoming = _options.LocalDomains.Contains(report.Domain);
                    await _reportRepository.AddReportAsync(report);
                }
            }

            settings.LastReportedUid = uids.LastOrDefault().Id;
            await _settingsRepository.SaveAsync(settings);
        }
        
        private async Task ListenForNewReports(CancellationToken cancellationToken)
        {
            while (true)
            {
                await _client.IdleAsync(cancellationToken, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                await FetchNewReports(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
        
        private async Task Disconnect()
        {
            await _client.DisconnectAsync(true);
            _client = null;
        }

        public async Task Stop()
        {
            if (_currentTask != null)
            {
                try
                {
                    _tokenSource.Cancel();
                }
                catch (AggregateException) {}

                try
                {
                    await _currentTask;
                }
                catch (OperationCanceledException) {}
                _currentTask = null;
                _tokenSource = null;
            }
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}