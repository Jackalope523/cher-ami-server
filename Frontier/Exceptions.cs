using System;

namespace Frontier.Exceptions
{
    [Serializable]
    public class InvalidEnvironmentException : HollowFailureException
    {
        public InvalidEnvironmentException()
            : base() { }
        public InvalidEnvironmentException(string message)
            : base(message) { }
        public InvalidEnvironmentException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    [Serializable]
    public class MissingInformationException : HollowException
    {
        public MissingInformationException()
            : base()
        {
            ErrorCode = "HOLLOW.MISSING_INFORMATION";
        }
    }
}
