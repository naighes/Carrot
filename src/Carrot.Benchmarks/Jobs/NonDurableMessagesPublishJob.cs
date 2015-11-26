using System;
using Carrot.Messages;

namespace Carrot.Benchmarks.Jobs
{
    [JobName("publish-nondurable-messages-with-confirms")]
    public class NonDurableMessagesPublishJob : PublishJob
    {
        public NonDurableMessagesPublishJob(IChannel channel, Exchange exchange, String routingKey)
            : base(channel, exchange, routingKey)
        {
        }

        protected override OutboundMessage<Foo> BuildMessage(Int32 i)
        {
            return new OutboundMessage<Foo>(new Foo { Bar = i });
        }
    }
}