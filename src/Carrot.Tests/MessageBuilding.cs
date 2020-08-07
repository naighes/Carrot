using System;
using System.Collections.Generic;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Xunit;

namespace Carrot.Tests
{
    public class MessageBuilding
    {
        [Fact]
        public void ProperContentType()
        {
            var content = new Foo();
            var message = new FakeConsumedMessage(content, FakeBasicDeliverEventArgs());
            var actual = message.To<Foo>();
            Assert.Equal(content, actual.Content);
        }

        [Fact]
        public void WrongContentType()
        {
            var content = new Foo();
            var message = new FakeConsumedMessage(content, FakeBasicDeliverEventArgs());
            Assert.Throws<InvalidCastException>(() => message.To<Bar>());
        }

        [Fact]
        public void HeaderMapping()
        {
            var content = new Foo();
            const String messageId = "one-id";
            const Int64 timestamp = 123456789L;
            const String replyExchangeType = "direct";
            const String replyExchangeName = "exchange-name";
            const String replyRoutingKey = "routing-key";
            const String correlationId = "one-correlation-id";
            var replyTo = $"{replyExchangeType}://{replyExchangeName}/{replyRoutingKey}";


            var message = new FakeConsumedMessage(content, new BasicDeliverEventArgs
            {
                BasicProperties = BasicPropertiesStubber.Stub(_ =>
                {
                    _.MessageId = messageId;
                    _.Timestamp = new AmqpTimestamp(timestamp);
                    _.CorrelationId = correlationId;
                    _.ReplyTo = replyTo;
                })
            });
            var actual = message.To<Foo>();
            Assert.Equal(messageId, actual.Headers.MessageId);
            Assert.Equal(timestamp, actual.Headers.Timestamp);
            Assert.Equal(correlationId, actual.Headers.CorrelationId);
            Assert.Equal(replyTo, actual.Headers.ReplyConfiguration.ToString());
        }

        [Fact]
        public void ConsumerTag()
        {
            var content = new Foo();
            const String consumerTag = "one-tag";
            var message = new FakeConsumedMessage(content, new BasicDeliverEventArgs
            {
                ConsumerTag = consumerTag,
                BasicProperties = BasicPropertiesStubber.Stub()
            });
            var actual = message.To<Foo>();
            Assert.Equal(consumerTag, actual.ConsumerTag);
        }

        [Fact]
        public void CustomHeader()
        {
            var key = "a";
            var value = "b";
            var content = new Foo();

            var message = new FakeConsumedMessage(content, new BasicDeliverEventArgs
            {
                BasicProperties = BasicPropertiesStubber.Stub(_ =>
                {
                    _.Headers = new Dictionary<String, Object>
                    {
                        {key, value}
                    };
                })
            });

            var actual = message.To<Foo>();

            Assert.True(actual.ContainsHeader(key));
            Assert.Equal(value, actual.Headers[key]);
        }

        private static BasicDeliverEventArgs FakeBasicDeliverEventArgs()
        {
            return new BasicDeliverEventArgs
            {
                BasicProperties = BasicPropertiesStubber.Stub()
            };
        }
    }
}