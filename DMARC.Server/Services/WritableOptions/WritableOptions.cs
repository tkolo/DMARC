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
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DMARC.Server.Services.WritableOptions
{
    public class WritableOptions<T> : IWritableOptions<T> where T : class, new()
    {
        private readonly IHostingEnvironment _environment;
        private readonly IOptionsMonitor<T> _options;
        private readonly string _section;
        private readonly string _file;

        public WritableOptions(
            IHostingEnvironment environment,
            IOptionsMonitor<T> options,
            string section,
            string file)
        {
            _environment = environment;
            _options = options;
            _section = section;
            _file = file;
        }

        public T Value => _options.CurrentValue;
        public T Get(string name) => _options.Get(name);

        public void Update(Action<T> applyChanges)
        {
            var fileProvider = _environment.ContentRootFileProvider;
            var fileInfo = fileProvider.GetFileInfo(_file);
            var physicalPath = fileInfo.PhysicalPath;

            T sectionObject;
            JObject jObject;
            if (File.Exists(physicalPath))
            {
                jObject = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(physicalPath));
                sectionObject = jObject.TryGetValue(_section, out JToken section)
                    ? JsonConvert.DeserializeObject<T>(section.ToString())
                    : (Value ?? new T());
            }
            else
            {
                jObject = new JObject();
                sectionObject = new T();
            }

            applyChanges(sectionObject);

            jObject[_section] = JToken.Parse(JsonConvert.SerializeObject(sectionObject));
            File.WriteAllText(physicalPath, JsonConvert.SerializeObject(jObject, Formatting.Indented));
        }
    }
}