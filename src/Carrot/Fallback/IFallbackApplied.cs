using System;

namespace Carrot.Fallback
{
    public interface IFallbackApplied
    {
        bool Success { get; }
    }

    internal class FallbackAppliedSuccessful : IFallbackApplied
    {
        public bool Success => true;
    }

    internal class FallbackAppliedFailure : IFallbackApplied
    {
        public FallbackAppliedFailure(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
        public bool Success => false;
    }
}