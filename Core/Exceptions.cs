using System;
using Core.Boundaries;

namespace Core.Exceptions
{
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
            return new(ErrorCode, Details);
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

        public UserErrorException(GatheringErrorCode code, object details = null)
        {
            ErrorCode = $"GATHERING.{code}";
            Details = details;
        }

        public UserErrorException(SnapshotErrorCode code, object details = null)
        {
            ErrorCode = $"SNAPSHOT.{code}";
            Details = details;
        }

        public UserErrorException(ConversationErrorCode code, object details = null)
        {
            ErrorCode = $"CONVERSATION.{code}";
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

        CANNOT_FOLLOW,
        CODE_NOT_FOUND,
        CANNOT_FOLLOW_SELF,

        CANNOT_BLOCK_SELF,
    }

    public enum GatheringErrorCode
    {
        CANNOT_VIEW,
        CANNOT_WATCH,
        CANNOT_UNWATCH,
        CANNOT_JOIN,
        CANNOT_JOIN_GUEST,

        CANNOT_LEAVE,

        CANNOT_INVITE_NEUTRAL,
        CANNOT_INVITE_INVALID_INVITEE,

        CANNOT_KICK_PERMISSION,
        CANNOT_KICK_ARCHIVED,
        CANNOT_KICK_SELF,

        USER_ATTENDING_ELSEWHERE,
        NO_IMMEDIATE,

        NOT_STARTED,
        NOT_GUEST,
        KICKED,

        // Hosting
        CANNOT_HOST,
        CANNOT_HOST_XYZ,

        LOCATION_DISABLED,
        INVALID_DETAILS,
        CONFLICT,

        CANNOT_EDIT_PERMISSION,
        CANNOT_EDIT_STARTED,
        CANNOT_EDIT_ENDED,

        NOT_HOST,
        CANNOT_START,
        CANNOT_END,

        CANNOT_CANCEL_STARTED,
        CANNOT_CANCEL_PERMISSION,

        SEALED,
        ENDED,
    }

    public enum SnapshotErrorCode
    {
        CANNOT_VIEW,
        CANNOT_DELETE,

        CANNOT_INTERACT,
        CANNOT_INTERACT_SELF,

        WINDOW_CLOSED,
    }

    public enum ConversationErrorCode
    {
        EMPTY_MESSAGE,

        NOT_MEMBER,
        NOT_GROUP_CHAT,

        CANNOT_INVITE_NEUTRAL,
        CANNOT_KICK_PERMISSION,

        INVALID_DETAILS,
        CANNOT_EDIT_PERMISSION,
        CANNOT_DELETE_PERMISSION,
    }

    public enum HollowErrorCode
    {
        UPLOAD_FAILED,
        DOWNLOAD_FAILED,
        UNKNOWN,
    }

    #endregion
}
