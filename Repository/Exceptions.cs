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
    public class InsufficientRadiusException : HollowException
    {
        public InsufficientRadiusException()
        {
        }

        public InsufficientRadiusException(string message)
            : base(message)
        {
        }

        public InsufficientRadiusException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
    [Serializable]
    public class ExcessiveRadiusException : HollowException
    {
        public ExcessiveRadiusException()
        {
        }

        public ExcessiveRadiusException(string message)
            : base(message)
        {
        }

        public ExcessiveRadiusException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
    [Serializable]
    public class UserNotFoundException : HollowFailureException
    {
        public UserNotFoundException()
        {
        }

        public UserNotFoundException(string message)
            : base(message)
        {
        }

        public UserNotFoundException(string message, Exception inner)
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
    public class DatabaseWriteException : HollowFailureException
    {
        private static readonly string defaultMessage = "An unexpected error occured while writing to the database:";

        public DatabaseWriteException()
        {
        }
        public DatabaseWriteException(string message)
            : base(message)
        {
        }
        public DatabaseWriteException(Exception inner)
           : base(defaultMessage + " " + inner.Message, inner)
        {
        }
        public DatabaseWriteException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
    [Serializable]
    public class DatabaseReadException : HollowFailureException
    {
        private static readonly string defaultMessage = "An unexpected error occured while reading from the database.";
        public DatabaseReadException()
        {
        }

        public DatabaseReadException(string message)
            : base(message)
        {
        }
        public DatabaseReadException(Exception inner)
            : base(defaultMessage + " " + inner.Message, inner)
        {
        }
        public DatabaseReadException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
    [Serializable]
    public class BlobIOException : HollowFailureException
    {
        private static readonly string defaultMessage = "An unexpected error occured while communicating with the blob storage.";
        public BlobIOException()
        {
        }

        public BlobIOException(string message)
            : base(message)
        {
        }
        public BlobIOException(Exception inner)
            : base(defaultMessage + " " + inner.Message, inner)
        {
        }
        public BlobIOException(string message, Exception inner)
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

    [Serializable]
    public class DatabaseCorruptionException : HollowFailureException
    {
        private static readonly string defaultMessage = "Data found in impossible configuration.";
        public DatabaseCorruptionException()
        {
        }

        public DatabaseCorruptionException(string message)
            : base(message)
        {
        }
        public DatabaseCorruptionException(Exception inner)
            : base(defaultMessage, inner)
        {
        }
        public DatabaseCorruptionException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
    #endregion
}
