using System;
using System.Collections.Generic;
using Carrot.Configuration;

namespace Carrot
{
    public interface IBroker
    {
        IConnection Connect();

        Queue DeclareQueue(String name,
                           IDictionary<String, Object> arguments = null);

        Queue DeclareDurableQueue(String name,
                                  IDictionary<String, Object> arguments = null);

        Exchange DeclareDirectExchange(String name,
                                       IDictionary<String, Object> arguments = null);

        Exchange DeclareDurableDirectExchange(String name,
                                              IDictionary<String, Object> arguments = null);

        Exchange DeclareFanoutExchange(String name,
                                       IDictionary<String, Object> arguments = null);

        Exchange DeclareDurableFanoutExchange(String name,
                                              IDictionary<String, Object> arguments = null);

        Exchange DeclareTopicExchange(String name,
                                      IDictionary<String, Object> arguments = null);

        Exchange DeclareDurableTopicExchange(String name,
                                             IDictionary<String, Object> arguments = null);

        Exchange DeclareHeadersExchange(String name,
                                        IDictionary<String, Object> arguments = null);

        Exchange DeclareDurableHeadersExchange(String name,
                                               IDictionary<String, Object> arguments = null);

        void DeclareExchangeBinding(Exchange exchange,
                                    Queue queue,
                                    String routingKey = "",
                                    IDictionary<String, Object> arguments = null);

        Boolean TryDeclareExchangeBinding(Exchange exchange,
                                          Queue queue,
                                          String routingKey = "",
                                          IDictionary<String, Object> arguments = null);

        void SubscribeByAtMostOnce(Queue queue, Action<ConsumingConfiguration> configure);

        void SubscribeByAtLeastOnce(Queue queue, Action<ConsumingConfiguration> configure);
    }
}