using Core.Boundaries;
using System;
using System.Net;

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

    #region Exceptions

    [Serializable]
    public abstract class HollowException : Exception
    {
        public static readonly HollowException Default = new UnexpectedFailureException("Default Hollow exception thrown.");

        public string ErrorCode { get; set; }

        public object Details { get; set; }

        public HollowException()
        { }

        public HollowException(string message)
            : base(message) { }

        public HollowException(string message, Exception innerException)
            : base(message, innerException) { }

        public ErrorShard ToErrorShard()
        {
            return new(HttpStatusCode.InternalServerError, "Fix in Exceptions.cs.");
        }
    }

    [Serializable]
    public abstract class HollowFailureException : HollowException
    {
        public HollowFailureException()
        { }

        public HollowFailureException(string message)
            : base(message) { }

        public HollowFailureException(string message, Exception inner)
            : base(message, inner) { }
    }

    [Serializable]
    public class UndefinedBehaviourException : HollowException
    {
        public UndefinedBehaviourException()
        { }

        public UndefinedBehaviourException(string message)
            : base(message) { }

        public UndefinedBehaviourException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    [Serializable]
    public class UnexpectedFailureException : HollowFailureException
    {
        public UnexpectedFailureException(string internalMessage, Exception innerException = null,
            HollowErrorCode code = HollowErrorCode.UNKNOWN, object details = null)
            : base(internalMessage, innerException)
        {
            ErrorCode = $"HOLLOW.{code}";
            Details = details;
        }
    }

    [Serializable]
    public class UserErrorException : HollowException
    {

        public UserErrorException(AccountErrorCode code, object details = null)
        {
            ErrorCode = $"ACCOUNT.{code}";
            Details = details;
        }

        public UserErrorException(UserErrorCode code, object details = null)
        {
            ErrorCode = $"USER.{code}";
            Details = details;
        }

        public UserErrorException(CircleErrorCode code, object details = null)
        {
            ErrorCode = $"CIRCLE.{code}";
            Details = details;
        }

        public UserErrorException(IssueErrorCode code, object details = null)
        {
            ErrorCode = $"ISSUE.{code}";
            Details = details;
        }

        public UserErrorException(ChatErrorCode code, object details = null)
        {
            ErrorCode = $"CHAT.{code}";
            Details = details;
        }
    }

    #endregion

    #region Error Codes

    public enum AccountErrorCode
    {
        NOT_FOUND,
        UNVERIFIED,
        LOCKED,
        LOCKED_OUT,
        DELETED,
        INCORRECT_CODE,
        INVALID_PHONE_NUMBER,
        PHONE_NUMBER_EXISTS,
        EMAIL_EXISTS,
        INVALID_DETAILS,
        INVALID_DETAILS_XYZ,
    }

    public enum UserErrorCode
    {
        CANNOT_REPORT_COOLDOWN,
        CANNOT_REPORT_DUPLICATE,
        CANNOT_VIEW,

        CANNOT_BLOCK_SELF,
    }

    public enum CircleErrorCode
    {
        CODE_NOT_FOUND,

        CANNOT_VIEW,

        CANNOT_JOIN,
        CANNOT_JOIN_GUEST,

        CANNOT_LEAVE,

        CANNOT_INVITE_NEUTRAL,
        CANNOT_INVITE_INVALID_INVITEE,

        CANNOT_KICK_PERMISSION,
        CANNOT_KICK_ARCHIVED,
        CANNOT_KICK_SELF,

        NOT_GUEST,
        KICKED,

        INVALID_DETAILS,

        CANNOT_EDIT_PERMISSION,

        NOT_HOST,

        CANNOT_DELETE_PERMISSION,

        SEALED,
    }

    public enum IssueErrorCode
    {
        CANNOT_VIEW,
        CANNOT_DELETE,

        CANNOT_INTERACT,
        CANNOT_INTERACT_SELF,

        WINDOW_CLOSED,
    }

    public enum ChatErrorCode
    {
        EMPTY_MESSAGE,

        NOT_MEMBER,
    }

    public enum HollowErrorCode
    {
        UPLOAD_FAILED,
        DOWNLOAD_FAILED,

        UNKNOWN,
    }

    #endregion
}
