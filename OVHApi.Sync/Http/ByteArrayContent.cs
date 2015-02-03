
namespace OVHApi.Http
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class ByteArrayContent : HttpContent
    {
        public ByteArrayContent(byte[] content)
            : base(ByteArrayContent.CreateStream(content))
        {
        }

        private static MemoryStream CreateStream(byte[] content)
        {
            var stream = new MemoryStream(content);
            stream.Seek(0L, SeekOrigin.Begin);
            return stream;
        }
    }
}
