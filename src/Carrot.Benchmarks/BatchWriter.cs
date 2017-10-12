using System;
using System.IO;
using System.Text;

namespace Carrot.Benchmarks
{
    internal class BatchWriter : TextWriter
    {
        private readonly TextWriter[] _writers;
        private readonly UTF8Encoding _encoding;

        public BatchWriter(params TextWriter[] writers)
        {
            _writers = writers;
            _encoding = new UTF8Encoding(true);
        }

        public override Encoding Encoding => _encoding;

        public override void WriteLine(String format, Object arg0)
        {
            foreach (var writer in _writers)
                writer.WriteLine(format, arg0);
        }

        public override void WriteLine(String format, Object arg0, Object arg1)
        {
            foreach (var writer in _writers)
                writer.WriteLine(format, arg0, arg1);
        }

        public override void WriteLine(String value)
        {
            foreach (var writer in _writers)
                writer.WriteLine(value);
        }

        public override void WriteLine()
        {
            foreach (var writer in _writers)
                writer.WriteLine();
        }

        public override void WriteLine(String format, params Object[] arg)
        {
            foreach (var writer in _writers)
                writer.WriteLine(format, arg);
        }

        protected override void Dispose(Boolean disposing)
        {
            base.Dispose(disposing);

            foreach (var writer in _writers)
                writer.Dispose();
        }

        public override void Write(Char value)
        {
            foreach (var writer in _writers)
                writer.Write(value);
        }
    }
}