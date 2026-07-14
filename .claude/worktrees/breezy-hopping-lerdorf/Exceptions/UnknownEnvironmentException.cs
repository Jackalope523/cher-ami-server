using System;

namespace CherAmiAPI.Exceptions
{
    public class UnknownEnvironmentException : Exception
    {
        public UnknownEnvironmentException()
        {
        }

        public UnknownEnvironmentException(string message)
            : base(message)
        {
        }

        public UnknownEnvironmentException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

}
