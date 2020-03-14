using System;
using System.IO;

namespace Ctlg.UnitTests.TestDoubles
{
    public class VirtualFileContent
    {
        public VirtualFileContent()
        {
        }

        public Stream GetReadStream()
        {
            return new MemoryStream(Content);
        }

        public Stream GetWriteStream()
        {
            _content = null;
            WriteStream = new MemoryStream();
            return WriteStream;
        }

        public long GetSize()
        {
            return Content.LongLength;
        }

        private MemoryStream WriteStream;
        private byte[] Content
        {
            get
            {
                if (_content == null)
                {
                    _content = WriteStream.ToArray();
                    WriteStream = null;
                }
                return _content;
            }
        }
        private byte[] _content;
    }
}
