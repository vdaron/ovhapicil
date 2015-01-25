
namespace OVHApi.Http
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;

#if ASYNC
    using System.Net.Http;
#endif

    public class JsonContent : StringContent
    {
        public JsonContent(string content)
            : this(content, Encoding.UTF8)
        {
        }

        public JsonContent(string content, Encoding encoding)
            : base(content, encoding, "application/json; charset=" + encoding.WebName)
        {
#if ASYNC
            base.Headers.ContentType.MediaType = "application/json";
#endif
        }
    }
}
