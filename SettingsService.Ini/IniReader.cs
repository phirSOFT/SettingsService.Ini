using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace phirSOFT.SettingsService.IniSettingsService
{
    sealed class IniReader : IDisposable, IEnumerable<(ReaderNodes node, string content)>, IEnumerator<(ReaderNodes node, string content)>
    {
        private readonly TextReader _reader;
        private string _valueCache;
        public IniReader(Stream stream) : this(stream, Encoding.Unicode, false)
        {

        }

        public IniReader(Stream stream, Encoding encoding, bool leaveOpen)
        {
            _reader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: true, bufferSize: 4096, leaveOpen: leaveOpen);
        }

        public void Dispose()
        {
            _reader.Dispose();
        }

        public IEnumerator<(ReaderNodes node, string content)> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        public bool MoveNext()
        {
            if (_valueCache != null)
            {
                Current = (ReaderNodes.Value ,_valueCache);
                _valueCache = null;
                return true;
            }

            var line = _reader.ReadLine();
            if (line == null)
                return false;

            if (string.IsNullOrWhiteSpace(line))
            {
                Current = (ReaderNodes.EmptyLine, line);
                return true;
            }

            if (line[0] == ';')
            {
                Current = (ReaderNodes.Comment, line.Substring(1));
                return true;
            }

            if (line[0] == '[')
            {
                int closingBracket = line.IndexOf(']');
                if (closingBracket > 0)
                {
                    Current = (ReaderNodes.Comment, line.Substring(1, closingBracket - 2));
                    return true;
                }
            }

            var eqsign = line.IndexOf('=');
            _valueCache = line.Substring(eqsign + 1);
            Current = (ReaderNodes.Key, line.Substring(0, eqsign - 1));
            return true;
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        public (ReaderNodes node, string content) Current { get; private set; }

        object IEnumerator.Current => Current;
    }
}
