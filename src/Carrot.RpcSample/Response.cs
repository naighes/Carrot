using System;
using Carrot.Configuration;

namespace Carrot.RpcSample
{
    [MessageBinding("urn:message:response")]
    public class Response
    {
        public Int32 BarBar { get; set; }
    }
}