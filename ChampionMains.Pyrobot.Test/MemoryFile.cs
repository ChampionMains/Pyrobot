using System;
using System.IO;
using System.Web;

namespace ChampionMains.Pyrobot.Test
{
    public class MemoryFile : HttpPostedFileBase
    {
        readonly Stream _inputStream;

        public MemoryFile(Stream inputStream, string contentType, string fileName)
        {
            _inputStream = inputStream;
            ContentType = contentType;
            FileName = fileName;
        }

        public override string ContentType { get; }

        public override string FileName { get; }
        
        public override int ContentLength => (int) _inputStream.Length;

        public override Stream InputStream => _inputStream;

        public override void SaveAs(string filename)
        {
            throw new InvalidOperationException();
        }
    }
}
