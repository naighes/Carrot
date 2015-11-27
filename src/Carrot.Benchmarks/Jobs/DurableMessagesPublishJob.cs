using System;
using Carrot.Messages;

namespace Carrot.Benchmarks.Jobs
{
    [JobName("publish-durable-messages")]
    public class DurableMessagesPublishJob : PublishJob
    {
        public DurableMessagesPublishJob(IBroker broker, Exchange exchange, String routingKey)
            : base(broker, exchange, routingKey)
        {
        }

        protected override OutboundMessage<Foo> BuildMessage(Int32 i)
        {
            return new DurableOutboundMessage<Foo>(new Foo { Bar = i });
        }
    }
}