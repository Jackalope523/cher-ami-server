using System;
using Core.Exceptions;

namespace Repository.Exceptions
{
    #region Repository

    [Serializable]
    public class UnsupportedHarborFlagException : HollowFailureException
    {
        public UnsupportedHarborFlagException()
        {
        }

        public UnsupportedHarborFlagException(string message)
            : base(message)
        {
        }

        public UnsupportedHarborFlagException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
    [Serializable]
    public class InvalidInputException : HollowFailureException
    {
        public InvalidInputException()
        {
        }

        public InvalidInputException(string message)
            : base(message)
        {
        }

        public InvalidInputException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    [Serializable]
    public class VaultIOException : HollowFailureException
    {
        private static readonly string defaultMessage = "An unexpected error occured while communicating with the key vault.";
        public VaultIOException()
        {
        }

        public VaultIOException(string message)
            : base(message)
        {
        }
        public VaultIOException(Exception inner)
            : base(defaultMessage, inner)
        {
        }
        public VaultIOException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
    #endregion
}
