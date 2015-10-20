using System;

namespace Carrot
{
    public interface INewId
    {
        String Next();
    }

    internal class NewGuid : INewId
    {
        public String Next()
        {
            return Guid.NewGuid().ToString();
        }
    }
}