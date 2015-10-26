namespace Carrot.Configuration
{
    public class EmptyMessageBinding : MessageBinding
    {
        public static readonly MessageBinding Instance = new EmptyMessageBinding();

        private EmptyMessageBinding()
            : base(null, null)
        {
        }
    }
}