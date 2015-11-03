using System;

namespace Carrot
{
    public interface INewId
    {
        String Next();
    }

    internal class NewGuid : INewId
    {
        internal NewGuid() { }

        public String Next()
        {
            return Guid.NewGuid().ToString();
        }
    }
}