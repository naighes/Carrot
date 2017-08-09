using System;
using System.Collections.Generic;
using Carrot.Configuration;

namespace Carrot
{
    public interface IBroker
    {
        IConnection Connect();

        Queue DeclareQueue(String name);

        Queue DeclareDurableQueue(String name);

        Exchange DeclareDirectExchange(String name);

        Exchange DeclareDurableDirectExchange(String name);

        Exchange DeclareFanoutExchange(String name);

        Exchange DeclareDurableFanoutExchange(String name);

        Exchange DeclareTopicExchange(String name);

        Exchange DeclareDurableTopicExchange(String name);

        Exchange DeclareHeadersExchange(String name);

        Exchange DeclareDurableHeadersExchange(String name);

        void DeclareExchangeBinding(Exchange exchange, Queue queue, String routingKey = "", IDictionary<String, Object> arguments = null);

        Boolean TryDeclareExchangeBinding(Exchange exchange, Queue queue, String routingKey = "", IDictionary<String, Object> arguments = null);

        void SubscribeByAtMostOnce(Queue queue, Action<ConsumingConfiguration> configure);

        void SubscribeByAtLeastOnce(Queue queue, Action<ConsumingConfiguration> configure);
    }
}