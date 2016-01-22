using System;
using Carrot.Configuration;

namespace Carrot.RpcSample
{
    [MessageBinding("urn:message:foo")]
    public class Foo
    {
        public Int32 Bar { get; set; }
    }
}