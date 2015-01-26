
namespace OVHApi.Http
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

#if ASYNC
    using System.Net.Http;

    public class JsonContent : StringContent
    {
        public JsonContent(string str)
            : base(str)
        {
            base.Headers.ContentType.MediaType = "application/json";
        }
    }
#else
    using System.Net;

    public class JsonContent : StringContent
    {
        public JsonContent(string content)
            : this(content, Encoding.UTF8)
        {
        }

        public JsonContent(string content, Encoding encoding)
            : base(content, encoding, "application/json; charset=" + encoding.WebName)
        {
        }
    }
#endif
}
