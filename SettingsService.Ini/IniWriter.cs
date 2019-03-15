using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace phirSOFT.SettingsService.IniSettingsService
{
    sealed class IniWriter : IDisposable
    {
        private readonly TextWriter _writer;

        public IniWriter(Stream stream) : this(stream, Encoding.Unicode, false)
        {

        }

        public IniWriter(Stream stream, Encoding encoding, bool leaveOpen)
        {
            _writer = new StreamWriter(stream, encoding, bufferSize: 4096, leaveOpen: leaveOpen);
        }
        public void WriteSection(string section)
        {
            _writer.WriteLine($"[{section}]");
        }

        public Task WriteSectionAsync(string section)
        {
            return _writer.WriteLineAsync($"[{section}]");
        }

        public void WriteKey(string key, string value)
        {
            _writer.WriteLine($"{key}={value}");
        }

        public Task WriteKeyAsync(string key, string value)
        {
            return _writer.WriteLineAsync($"{key}={value}");
        }

        public void WriteComment(string comment)
        {
            _writer.WriteLine($";{comment}");
        }

        public Task WriteCommentAsync(string comment)
        {
            return _writer.WriteLineAsync($";{comment}");
        }

        public void Dispose()
        {
            _writer.Dispose();
        }
    }
}
