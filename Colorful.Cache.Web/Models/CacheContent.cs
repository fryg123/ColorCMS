using System;
using System.Collections.Generic;
using System.Text;

namespace Colorful.Models
{
    public class CacheContent
    {
        public byte[] Content { get; set; }
        public string ContentType { get; set; }
        public Dictionary<string, string> Headers { get; set; }
    }
}
