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
using Microsoft.AspNetCore.Blazor.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Net.Mime;
using DMARC.Server.Repositories;
using DMARC.Server.Repositories.ElasticsearchRepositories;
using DMARC.Server.Services.ImapClient;
using DMARC.Server.Services.WritableOptions;
using DMARC.Shared.Model.Settings;
using Microsoft.Extensions.Configuration;

namespace DMARC.Server
{
    public class Startup
    {
        private readonly IHostingEnvironment _environment;
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;
            _environment = environment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddResponseCompression(options =>
            {
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
                {
                    MediaTypeNames.Application.Octet,
                    WasmMediaTypeNames.Application.Wasm,
                });
            });

            // configurations
            services.ConfigureWritable<List<ServerOptions>>(Configuration.GetSection("Servers"),
                Program.WritableFileName);
            services.ConfigureWritable<ElasticsearchConfig>(Configuration.GetSection("ElasticsearchConfig"),
                Program.WritableFileName);

            //repositories
            services.AddScoped<IReportRepository, ElasticsearchReportRepository>();
            services
                .AddScoped<IImapClientDynamicSettingsRepository, ElasticsearchImapClientDynamicSettingsRepository>();

            // other services
            services.AddScoped<IImapClient, ImapClient>();
            services.AddSingleton<IImapClientManager, ImapClientManager>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc(routes => { routes.MapRoute(name: "default", template: "{controller}/{action}/{id?}"); });

            app.UseBlazor<Client.Startup>();
            
            serviceProvider.GetService<IImapClientManager>().Start();
        }
    }
}