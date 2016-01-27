using System;
using System.Threading.Tasks;
using Carrot.Configuration;
using Carrot.Messages;
using Carrot.Messages.Replies;

namespace Carrot.RpcSample
{
    class Program
    {
        static void Main(string[] args)
        {
            const String routingKey = "request_routing_key";
            const String endpointUrl = "amqp://guest:guest@localhost:5672/";
            const String replyQueueName = "reply_to_queue";
            const String replyExchangeName = "reply_exchange";

            IMessageTypeResolver resolver = new MessageBindingResolver(typeof(Response).Assembly);

            var broker = Broker.New(_ =>
            {
                _.Endpoint(new Uri(endpointUrl, UriKind.Absolute));
                _.ResolveMessageTypeBy(resolver);
            });

            var requestConsumer = new RequestConsumer(endpointUrl);

            var exchange = broker.DeclareDirectExchange("request_exchange");
            var queue = broker.DeclareQueue("request_queue");
            broker.DeclareExchangeBinding(exchange, queue, routingKey);
            broker.SubscribeByAtLeastOnce(queue, _ => _.Consumes(requestConsumer));

            var replyExchange = broker.DeclareDirectExchange(replyExchangeName);
            var replyQueue = broker.DeclareQueue(replyQueueName);
            broker.DeclareExchangeBinding(replyExchange, replyQueue, replyQueueName);
            broker.SubscribeByAtLeastOnce(replyQueue, _ => _.Consumes(new ResponseConsumer()));

            var connection = broker.Connect();

            var message = new OutboundMessage<Request>(new Request { Bar = 42 });
            message.SetCorrelationId(Guid.NewGuid().ToString());
            message.SetReply(new DirectReplyConfiguration(replyExchangeName, replyQueueName));
            connection.PublishAsync(message, exchange, routingKey);

            Console.ReadLine();
            requestConsumer.Dispose();
            connection.Dispose();
        }
    }

    internal class RequestConsumer : Consumer<Request>, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IBroker _broker;

        private readonly IMessageTypeResolver _resolver = new MessageBindingResolver(typeof(Response).Assembly);

        public RequestConsumer(string endpointUrl)
        {
            _broker = Broker.New(_ =>
            {
                _.Endpoint(new Uri(endpointUrl, UriKind.Absolute));
                _.ResolveMessageTypeBy(_resolver);
            });

            _connection = _broker.Connect();
        }

        public override Task ConsumeAsync(ConsumedMessage<Request> message)
        {
            return Task.Factory.StartNew(() =>
            {
                Console.WriteLine("[{0}]received '{1}' by '{2}' with correlation id {3}",
                                  message.ConsumerTag,
                                  message.Headers.MessageId,
                                  GetType().Name,
                                  message.Headers.CorrelationId);

                var exchange = _broker.DeclareDirectExchange(message.Headers.ReplyConfiguration.ExchangeName);
                var queue = _broker.DeclareQueue(message.Headers.ReplyConfiguration.RoutingKey);
                _broker.DeclareExchangeBinding(exchange, queue, message.Headers.ReplyConfiguration.RoutingKey);

                var outboundMessage = new OutboundMessage<Response>(new Response { BarBar = message.Content.Bar * 2 });
                outboundMessage.SetCorrelationId(message.Headers.CorrelationId);
                _connection.PublishAsync(outboundMessage, exchange, message.Headers.ReplyConfiguration.RoutingKey);
            });
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }

    internal class ResponseConsumer : Consumer<Response>
    {
        public override Task ConsumeAsync(ConsumedMessage<Response> message)
        {
            return Task.Factory.StartNew(() =>
            {
                Console.WriteLine("[{0}]received '{1}' by '{2}' with correlation id {3}",
                                  message.ConsumerTag,
                                  message.Headers.MessageId,
                                  GetType().Name,
                                  message.Headers.CorrelationId);
            });
        }
    }
}
