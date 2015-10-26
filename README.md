# Carrot

Carrot is a .NET lightweight library that provides a couple of facilities over RabbitMQ.

[![install from nuget](https://img.shields.io/nuget/v/Carrot.svg?style=flat-square)](https://www.nuget.org/packages/Carrot)[![downloads](http://img.shields.io/nuget/dt/Carrot.svg?style=flat-square)](https://www.nuget.org/packages/Carrot)

Getting started
====

Just mark your POCO message contracts with `MessageBinding` attribute: 

    [MessageBinding("urn:message:foo")]
    public class Foo
    {
        public Int32 Bar { get; set; }
    }

Define your message consumer:

    class FooConsumer : Consumer<Foo>
    {
        public override Task ConsumeAsync(ConsumedMessage<Foo> message)
        {
            return Task.Factory.StartNew(() =>
                                         {
                                             Console.WriteLine("received '{0}'",
                                                               message.Headers.MessageId);
                                         });
        }
    }

Create an instance of `AmqpChannel` providing the RabbitMQ host as input.

	var channel = AmqpChannel.New("amqp://guest:guest@localhost:5672/",
                                  new MessageBindingResolver(typeof(Foo).Assembly));
    var exchange = Exchange.Direct("source_exchange");

    channel.Bind("my_test_queue", exchange)
           .SubscribeByAtLeastOnce(_ => { _.Consumes(new FooConsumer()); });

You're up 'n running! Do not forget to call `AmqpChannel.Dispose()` when your application exits.

Please note that exchanges are not durable by default.
You can rely on proper methods in case:

    var exchange = Exchange.DurableDirect("source_exchange");

You can publish messages as the following:

    channel.PublishAsync(new OutboundMessage<Foo>(new Foo { Bar = 2 }),
                         exchange);

Please note that messages are not durable by default.
If you need durable messaging, make use of `DurableOutboundMessage<T>`:

    channel.PublishAsync(new DurableOutboundMessage<Foo>(new Foo { Bar = 2 }),
                         exchange);
