using System;

namespace CherAmiAPI.Exceptions
{
    public class LockedOutException : Exception
    {
        public LockedOutException()
        {
        }

        public LockedOutException(string message)
            : base(message)
        {
        }

        public LockedOutException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

}
