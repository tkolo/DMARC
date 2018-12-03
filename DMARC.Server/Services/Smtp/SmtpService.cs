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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMARC.Server.Services.RazorRenderer;
using DMARC.Shared.Model.Report;
using DMARC.Shared.Model.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace DMARC.Server.Services.Smtp
{
    public class SmtpService : ISmtpService
    {
        private readonly IRazorViewToStringRenderer _renderer;

        public SmtpService(IRazorViewToStringRenderer renderer)
        {
            _renderer = renderer;
        }

        public async Task SendReport(Report report, SmtpOptions smtpOptions)
        {
            var html = await _renderer.RenderViewToStringAsync("Views/Report/Report.cshtml", report);
            var preMailer = new PreMailer.Net.PreMailer(html);
            var result = preMailer.MoveCssInline(true, removeComments: true);

            using (var client = new SmtpClient())
            {
                int port;
                SecureSocketOptions socketOptions;
                switch (smtpOptions.Protocol)
                {
                    case SslMode.Auto:
                        port = 0;
                        socketOptions = SecureSocketOptions.Auto;
                        break;
                    case SslMode.NoSsl:
                        port = 25;
                        socketOptions = SecureSocketOptions.None;
                        break;
                    case SslMode.Ssl:
                        port = 465;
                        socketOptions = SecureSocketOptions.SslOnConnect;
                        break;
                    case SslMode.StartTls:
                        port = 587;
                        socketOptions = SecureSocketOptions.StartTls;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                await client.ConnectAsync(smtpOptions.Server, port, socketOptions);
                if (!string.IsNullOrEmpty(smtpOptions.Username))
                {
                    await client.AuthenticateAsync(smtpOptions.Username, smtpOptions.Password);
                }

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(smtpOptions.From));
                foreach (var address in smtpOptions.SendTo)
                    message.To.Add(new MailboxAddress(address));
                message.Subject = $"DMARC Report {report.ReportId} for domain {report.Domain}";

                var bodyBuilder = new BodyBuilder {HtmlBody = result.Html};

                message.Body = bodyBuilder.ToMessageBody();
                await client.SendAsync(message);
            }
        }
    }
}