using System;
using System.Threading.Tasks;
using Carrot.Configuration;
using Carrot.Messages;

namespace Carrot.RpcSample
{
    class Program
    {
        static void Main(string[] args)
        {
            const String routingKey = "routing_key";
            const String endpointUrl = "amqp://guest:guest@localhost:5672/";
            IMessageTypeResolver resolver = new MessageBindingResolver(typeof(Foo).Assembly);

            var broker = Broker.New(_ =>
            {
                _.Endpoint(new Uri(endpointUrl, UriKind.Absolute));
                _.ResolveMessageTypeBy(resolver);
            });

            var exchange = broker.DeclareDirectExchange("source_exchange");
            var queue = broker.DeclareQueue("my_test_queue");
            broker.DeclareExchangeBinding(exchange, queue, routingKey);
            broker.SubscribeByAtLeastOnce(queue, _ =>  _.Consumes(new FooConsumer1()));

            var replyToQueue = "reply_to_queue";
            var replyQueue = broker.DeclareQueue(replyToQueue);
            broker.SubscribeByAtLeastOnce(replyQueue, _ => _.Consumes(new FooConsumer2()));

            var connection = broker.Connect();


            var message = new OutboundMessage<Foo>(new Foo { Bar = 42 });
            message.SetCorrelationId(Guid.NewGuid().ToString());
            message.SetReplyTo(replyToQueue);
            connection.PublishAsync(message, exchange, routingKey);

            Console.ReadLine();
            connection.Dispose();
        }
    }

    internal class FooConsumer1 : Consumer<Foo>
    {
        public FooConsumer1()
        {
            
        }

        public override Task ConsumeAsync(ConsumedMessage<Foo> message)
        {
            return Task.Factory.StartNew(() =>
            {
                Console.WriteLine("[{0}]received '{1}' by '{2}'",
                                  message.ConsumerTag,
                                  message.Headers.MessageId,
                                  GetType().Name);


            });
        }
    }

    internal class FooConsumer2 : Consumer<Foo>
    {
        public override Task ConsumeAsync(ConsumedMessage<Foo> message)
        {
            return Task.Factory.StartNew(() =>
            {
                Console.WriteLine("[{0}]received '{1}' by '{2}'",
                                  message.ConsumerTag,
                                  message.Headers.MessageId,
                                  GetType().Name);
            });
        }
    }
}
