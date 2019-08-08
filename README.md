# Carrot

Carrot is a .NET lightweight library that provides a couple of facilities over RabbitMQ.

[![install from nuget](https://img.shields.io/nuget/v/Carrot.svg?style=flat-square)](https://www.nuget.org/packages/Carrot)[![downloads](http://img.shields.io/nuget/dt/Carrot.svg?style=flat-square)](https://www.nuget.org/packages/Carrot)

## What is it? ##

Prompted by the need for an easy-to-use RabbitMQ access component not requiring lots of boilerplate code in order to accomplish basic operations with RabbitMQ broker.
Inspired by [MassTransit](https://github.com/MassTransit/MassTransit "MassTransit").

## Getting started ##

Just mark your POCO message contracts with `MessageBinding` attribute:

```csharp
[MessageBinding("urn:message:foo")]
public class Foo
{
    public Int32 Bar { get; set; }
}
```

Define your message consumer:

```csharp
class FooConsumer : Consumer<Foo>
{
    public override Task ConsumeAsync(ConsumingContext<Foo> context)
    {
        return Task.Factory.StartNew(() =>
                                     {
                                         Console.WriteLine("received '{0}'",
                                                           context.Message.Headers.MessageId);
                                     });
    }
}
```

Create an instance of `Broker` providing the RabbitMQ host as input.

```csharp
var broker = Broker.New(_ =>
                        {
                            _.Endpoint(new Uri("amqp://guest:guest@localhost:5672/", UriKind.Absolute));
                            _.ResolveMessageTypeBy(new MessageBindingResolver(typeof(Foo).GetTypeInfo().Assembly));
                        });
```

Declare your AMQP entities as the following:

```csharp
var exchange = broker.DeclareDirectExchange("source_exchange");
var queue = broker.DeclareQueue("my_test_queue");
```

Bind entities, subscribe your queue and call `IBroker.Connect`:

```csharp
broker.DeclareExchangeBinding(exchange, queue, "routing_key");
broker.SubscribeByAtLeastOnce(queue, _ => _.Consumes(new FooConsumer()));
var connection = broker.Connect();
```

You're up 'n running!
Do not forget to call `IConnection.Dispose()` when your application exits.

Please note that exchanges, queues and messages are not durable by default.
You can create durable entities by calling the proper builder methods.

```csharp
var durableExchange = broker.DeclareDurableDirectExchange("source_exchange");
var durableQueue = broker.DeclareDurableQueue("my_test_queue");
```

You can publish messages as the following:

```csharp
connection.PublishAsync(new OutboundMessage<Foo>(new Foo { Bar = 2 }),
                        exchange);
```

Please note that messages are not durable by default.
If you need durable messaging, make use of `DurableOutboundMessage<T>`:

```csharp
connection.PublishAsync(new DurableOutboundMessage<Foo>(new Foo { Bar = 2 }),
                        exchange);
```

To activate publisher confirms, configure a reliable outbound channel when creating the broker:

```csharp
var broker = Broker.New(_ =>
                        {
                            _.Endpoint(new Uri("amqp://guest:guest@localhost:5672/", UriKind.Absolute));
                            _.ResolveMessageTypeBy(new MessageBindingResolver(typeof(Foo).GetTypeInfo().Assembly));
                            _.PublishBy(OutboundChannel.Reliable());
                        });
```

## Building from Source ##

Clone the source down to your machine.

```sh
git clone https://github.com/naighes/Carrot.git
```

Run `.\build.ps1`

## Resources ##

- Carrot can be installed from [NuGet](https://www.nuget.org/packages/Carrot "Carrot")
- Find more information in the [wiki](https://github.com/naighes/Carrot/wiki "Carrot wiki")
- Follow [@nicolabaldi on Twitter](https://twitter.com/nicolabaldi "@nicolabaldi") for updates
