using System;
using Carrot.Configuration;

namespace Carrot.RpcSample
{
    [MessageBinding("urn:message:request")]
    public class Request
    {
        public Int32 Bar { get; set; }
    }
}