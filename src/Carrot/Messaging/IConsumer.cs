namespace Carrot.Messaging
{
    using System;
    using System.Threading.Tasks;

    public interface IConsumer
    {
        Task Consume(Object message);
    }
}