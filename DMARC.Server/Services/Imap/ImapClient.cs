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
using DMARC.Server.Services.Smtp;
using DMARC.Shared.Model.Report;
using DMARC.Shared.Model.Settings;
using MailKit;
using MailKit.Search;
using MailKit.Security;
using Microsoft.EntityFrameworkCore.Internal;

namespace DMARC.Server.Services.Imap
{
    public class ImapClient : IImapClient
    {
        private ServerOptions _options;
        private readonly IReportRepository _reportRepository;
        private readonly IImapClientDynamicSettingsRepository _settingsRepository;
        private readonly ISmtpService _smtpService;
        private MailKit.Net.Imap.ImapClient _client;
        private CancellationTokenSource _tokenSource;
        private Task _currentTask;

        public ImapClient(IReportRepository reportRepository,
            IImapClientDynamicSettingsRepository settingsRepository, 
            ISmtpService smtpService)
        {
            _reportRepository = reportRepository;
            _settingsRepository = settingsRepository;
            _smtpService = smtpService;
        }

        public void Start(ServerOptions options)
        {
            if (_currentTask != null)
                throw new InvalidOperationException("Client already started");

            _options = options;
            _tokenSource = new CancellationTokenSource();
            _currentTask = StartInternal();
        }

        private async Task StartInternal()
        {
            if (string.IsNullOrEmpty(_options.ImapOptions?.Server))
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
            var imapOptions = _options.ImapOptions;
            switch (imapOptions.Protocol)
            {
                case SslMode.Auto:
                    port = 0;
                    socketOptions = SecureSocketOptions.Auto;
                    break;
                case SslMode.NoSsl:
                    port = 143;
                    socketOptions = SecureSocketOptions.None;
                    break;
                case SslMode.Ssl:
                    port = 993;
                    socketOptions = SecureSocketOptions.SslOnConnect;
                    break;
                case SslMode.StartTls:
                    port = 143;
                    socketOptions = SecureSocketOptions.StartTls;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _client = new MailKit.Net.Imap.ImapClient();
            await _client.ConnectAsync(imapOptions.Server, port, socketOptions, cancellationToken);
            if (!string.IsNullOrEmpty(imapOptions.Username))
            {
                await _client.AuthenticateAsync(imapOptions.Username, imapOptions.Password, cancellationToken);
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


            var sendTasks = new List<Task>();
            foreach (var uid in uids)
            {
                var message = await _client.Inbox.GetMessageAsync(uid, cancellationToken);
                foreach (var report in MailParser.ParseMessage(message))
                {
                    report.ServerId = _options.Id;
                    report.Incoming = _options.ImapOptions.LocalDomains.Contains(report.Domain);
                    if (!await _reportRepository.CheckIfExistsAsync(report))
                        sendTasks.Add(CheckAndSendReport(report));
                    await _reportRepository.AddReportAsync(report);
                }
            }

            settings.LastReportedUid = uids.LastOrDefault().Id;
            await _settingsRepository.SaveAsync(settings);
            await Task.WhenAll(sendTasks);
        }

        private async Task CheckAndSendReport(Report report)
        {
            var smtpOptions = _options.SmtpOptions;
            if (smtpOptions != null)
            {
                switch (smtpOptions.SendVerbosity)
                {
                    case SendVerbosity.All:
                        await _smtpService.SendReport(report, smtpOptions);
                        break;
                    case SendVerbosity.PartialFailures:
                        if (report.Records.Any(x => x.Spf == DmarcResult.Fail || x.Dkim == DmarcResult.Fail))
                            await _smtpService.SendReport(report, smtpOptions);
                        break;
                    case SendVerbosity.Failures:
                        if (report.Records.Any(x => x.Spf == DmarcResult.Fail && x.Dkim == DmarcResult.Fail))
                            await _smtpService.SendReport(report, smtpOptions);
                        break;
                    case SendVerbosity.None:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
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