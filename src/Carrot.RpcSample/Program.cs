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
            IMessageTypeResolver resolver = new MessageBindingResolver(typeof(Request).Assembly);

            var broker = Broker.New(_ =>
            {
                _.Endpoint(new Uri(endpointUrl, UriKind.Absolute));
                _.ResolveMessageTypeBy(resolver);
            });

            var fooConsumer1 = new FooConsumer1(endpointUrl);

            var exchange = broker.DeclareDirectExchange("source_exchange");
            var queue = broker.DeclareQueue("request_queue");
            broker.DeclareExchangeBinding(exchange, queue, routingKey);
            broker.SubscribeByAtLeastOnce(queue, _ => _.Consumes(fooConsumer1));

            var replyToQueue = "reply_to_queue";
            var replyQueue = broker.DeclareQueue(replyToQueue);
            var replyExchange = broker.DeclareDirectExchange("reply_exchange");
            broker.DeclareExchangeBinding(replyExchange, replyQueue, replyToQueue); //?????
            broker.SubscribeByAtLeastOnce(replyQueue, _ => _.Consumes(new FooConsumer2()));
            
            var connection = broker.Connect();

            var message = new OutboundMessage<Request>(new Request { Bar = 42 });
            message.SetCorrelationId(Guid.NewGuid().ToString());
            message.SetReplyTo($"//direct://{replyExchange.Name}/{replyQueue.Name}"); //exchangeType://exchangeName/routingKey
            connection.PublishAsync(message, exchange, routingKey);

            Console.ReadLine();
            fooConsumer1.Dispose();
            connection.Dispose();
        }
    }

    internal class FooConsumer1 : Consumer<Request>, IDisposable
    {
        private readonly IConnection _connection;
        private IBroker _broker;

        public FooConsumer1(string endpointUrl)
        {
            _broker = Broker.New(_ =>
            {
                _.Endpoint(new Uri(endpointUrl, UriKind.Absolute));
            });

            _connection = _broker.Connect();
        }

        public override Task ConsumeAsync(ConsumedMessage<Request> message)
        {
            return Task.Factory.StartNew(() =>
            {
                Console.WriteLine("[{0}]received '{1}' by '{2}'",
                                  message.ConsumerTag,
                                  message.Headers.MessageId,
                                  GetType().Name);

                var exchange = _broker.DeclareDirectExchange(message.Headers.ReplyTo);

                var outboundMessage = new OutboundMessage<Response>(new Response {BarBar = message.Content.Bar*2});
                outboundMessage.SetCorrelationId(message.Headers.CorrelationId);
                _connection.PublishAsync(outboundMessage, exchange, message.Headers.ReplyTo);
            });
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }

    internal class FooConsumer2 : Consumer<Response>
    {
        public override Task ConsumeAsync(ConsumedMessage<Response> message)
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
