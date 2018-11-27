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
using System.IO;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace DMARC.Server.Services.DynamicSettings
{
    public class DynamicSettings : IDisposable
    {
        [JsonIgnore]
        private readonly DynamicSettingsOptions _options;
        [JsonIgnore]
        private bool _supressWrites = false;
        [JsonIgnore]
        private FileSystemWatcher _watcher;
        [JsonIgnore]
        private readonly IDisposable _monitor;
        
        public DynamicSettings(IOptionsMonitor<DynamicSettingsOptions> options)
        {
            _options = options.CurrentValue;
            _monitor = options.OnChange((x, y) => ResetWatcher());
            ResetWatcher();
        }

        private void ResetWatcher()
        {
            var absolute = Path.GetFullPath(_options.Path);
            if (!File.Exists(absolute))
                return;

            var basedir = Path.GetDirectoryName(absolute);
            var filename = Path.GetFileName(absolute);
            
            _watcher?.Dispose();
            _watcher = new FileSystemWatcher(basedir, filename);
            _watcher.NotifyFilter = NotifyFilters.LastWrite;
            _watcher.Changed += (sender, e) => OnFileChanged();
            _watcher.EnableRaisingEvents = true;
            OnFileChanged();
        }

        private void OnFileChanged()
        {
            using (var file = new FileStream(_options.Path, FileMode.OpenOrCreate))
            using (var streamReader = new StreamReader(file))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                try
                {
                    _supressWrites = true;
                    var serializer = new JsonSerializer();
                    serializer.Populate(jsonReader, this);
                }
                finally
                {
                    _supressWrites = false;
                }
            }
        }

        private uint _lastIndexedReportUid;
        public uint LastIndexedReportUid
        {
            get => _lastIndexedReportUid;
            set
            {
                _lastIndexedReportUid = value;
                Save();
            }
        }

        private void Save()
        {
            if (_supressWrites)
                return;
            
            using (var file = new FileStream(_options.Path, FileMode.OpenOrCreate))
            using (var streamWriter = new StreamWriter(file))
            using (var jsonWriter = new JsonTextWriter(streamWriter))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(jsonWriter, this);
            }
        }

        public void Dispose()
        {
            _watcher?.Dispose();
            _monitor?.Dispose();
        }
    }
}