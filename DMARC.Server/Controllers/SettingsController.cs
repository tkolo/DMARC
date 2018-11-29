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
using DMARC.Server.Services.WritableOptions;
using DMARC.Shared.Model.Settings;
using Microsoft.AspNetCore.Mvc;

namespace DMARC.Server.Controllers
{
    public class SettingsController : ApiController
    {
        private readonly IWritableOptions<List<ImapClientOptions>> _options;

        public SettingsController(IWritableOptions<List<ImapClientOptions>> options)
        {
            _options = options;
        }

        [HttpGet]
        public Task<IActionResult> Get()
        {
            return Task.FromResult((IActionResult) Ok(_options.Value));
        }

        
        [HttpPost]
        public Task<IActionResult> Post([FromBody] List<ImapClientOptions> options)
        {
            foreach (var clientOptions in options)
            {
                if (string.IsNullOrEmpty(clientOptions.Id))
                {
                    clientOptions.Id = Guid.NewGuid().ToString();
                }

                if (string.IsNullOrEmpty(clientOptions.Server))
                    return Task.FromResult((IActionResult) BadRequest());
            }
            
            _options.Update(newOptions =>
            {
                newOptions.Clear();
                newOptions.AddRange(options);
            });

            return Task.FromResult((IActionResult) Ok());
        }
    }
}