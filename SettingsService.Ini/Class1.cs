using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nito.AsyncEx;
using phirSOFT.SettingsService;
using phirSOFT.SettingsService.IniSettingsService;

namespace SettingsService.Ini
{
    public class IniSettingsService : CachedSettingsService
    {
        private readonly string _iniFile;
        private readonly Dictionary<string, string> _values;
        private readonly Dictionary<Type, IIniAdapter> _converters;

        public IniSettingsService(string filename)
        {
            _iniFile = filename;
            LoadIniFile();
        }

        private void LoadIniFile()
        {
            string currentSection = null;
            _values.Clear();
            string key = null;
            foreach ((ReaderNodes node, string content) in new IniReader(new FileStream(_iniFile, FileMode.Open)))
            {
                switch (node)
                {
                    case ReaderNodes.EmptyLine:
                    case ReaderNodes.Comment:
                        continue;
                    case ReaderNodes.SectionHeading:
                        currentSection = content;
                        break;
                    case ReaderNodes.Key:
                        key = currentSection != null ? $"{currentSection}.{content}" : content;
                        break;
                    case ReaderNodes.Value:
                        _values[key] = content;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        protected override Task<object> GetSettingInternalAsync(string key, Type type)
        {
            return Task.FromResult(_converters[type].DeserializeString(_values[key], type));
        }

        protected override async Task StoreInternalAsync()
        {
            using (var writer = new IniWriter(new FileStream(_iniFile, FileMode.Create)))
            {
                string currentSection = null;
                foreach (string fullqualifiedKey in _values.Keys.OrderBy(v => v, IniComparer.Instance))
                {
                    (string section, string key) = SplitFullQualifiedKey(fullqualifiedKey);
                    if (section != currentSection)
                    {
                        currentSection = section;
                        await writer.WriteSectionAsync(currentSection).ConfigureAwait(continueOnCapturedContext: false);
                    }

                    await writer.WriteKeyAsync(key, _values[fullqualifiedKey]).ConfigureAwait(continueOnCapturedContext: false);
                }
            }
        }

        private (string section, string key) SplitFullQualifiedKey(string key)
        {
            int index = key.LastIndexOf(value: '.');
            return index < 0 ? (null, key) : (key.Substring(startIndex: 0, length: index - 1), key.Substring(index + 1));
        }

        protected override Task RegisterSettingInternalAsync(string key, object defaultValue, object initialValue, Type type)
        {
            _values.Add(key, _converters[type].SerializeObject(initialValue, type));
            return Task.CompletedTask;
        }

        protected override Task UnregisterSettingInternalAsync(string key)
        {
            _values.Remove(key);
            return Task.CompletedTask;
        }

        protected override Task<bool> IsRegisterdInternalAsync(string key)
        {
            return Task.FromResult(_values.ContainsKey(key));
        }

        protected override Task SetSettingInternalAsync(string key, object value, Type type)
        {
            _values.Add(key, _converters[type].SerializeObject(value, type));
            return Task.CompletedTask;
        }

        protected override bool SupportConcurrentUnregister { get; } = true;
        protected override bool SupportConcurrentUpdate { get; } = true;
        protected override bool SupportConcurrentRegister { get; } = true;
    }

    public class IniComparer : IComparer<string>
    {
        private static readonly Lazy<IniComparer> _instance = new Lazy<IniComparer>();
        public static IniComparer Instance => _instance.Value;

        private IniComparer()
        {

        }

        public int Compare(string x, string y)
        {
            Debug.Assert(x != null, nameof(x) + " != null");
            Debug.Assert(y != null, nameof(y) + " != null");
            return x.IndexOf(value: '.') - y.IndexOf(value: '.');
        }
    }
}
