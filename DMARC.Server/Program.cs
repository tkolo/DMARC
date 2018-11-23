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


using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;

namespace DMARC.Server
{
    public class Program
    {
        private const string HelpTemplate = "-h|--help";

        public static int Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.Name = "DMARC";
            app.Description = ".NET core DMARC report parser and aggregator";
            app.HelpOption(HelpTemplate);

            app.Command("server", c =>
            {
                c.Description = "Starts the web server";
                c.HelpOption(HelpTemplate);
                c.ExtendedHelpText =
                    "This command starts the kestrel web server. " +
                    "Usual kestrel command line and environmental variable configurations should work here.";
                c.OnExecute(() => StartWebServer(args));
            });
            app.Command("parse", c =>
            {
                c.Description = "Parses and aggregates DMARC report";
                c.HelpOption(HelpTemplate);
                var pathArguments = c.Argument("path", "Path to file containing DMARC report", true);
                c.OnExecute(() => ParseDmarcReports(pathArguments));
            });


            app.OnExecute(() =>
            {
                app.ShowHelp();
                return 0;
            });

            return app.Execute(args);
        }

        private static async Task<int> ParseDmarcReports(CommandArgument pathArguments)
        {
            throw new System.NotImplementedException();
        }

        private static async Task<int> StartWebServer(string[] args)
        {
            await BuildWebHost(args).RunAsync();
            return 0;
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", false, true)
                        .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json",
                            true,
                            true)
                        .AddUserSecrets<Startup>()
                        .AddEnvironmentVariables();

                    if (args != null)
                        config.AddCommandLine(args);
                })
                .UseStartup<Startup>()
                .Build();
    }
}