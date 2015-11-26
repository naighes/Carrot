using System;
using Carrot.Messages;

namespace Carrot.Benchmarks.Jobs
{
    [JobName("publish-durable-messages-with-confirms")]
    public class DurableMessagesPublishJob : PublishJob
    {
        public DurableMessagesPublishJob(IChannel channel, Exchange exchange, String routingKey)
            : base(channel, exchange, routingKey)
        {
        }

        protected override OutboundMessage<Foo> BuildMessage(Int32 i)
        {
            return new DurableOutboundMessage<Foo>(new Foo { Bar = i });
        }
    }
}