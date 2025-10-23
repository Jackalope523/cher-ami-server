using System;

namespace CrazyLizard.Exceptions
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
