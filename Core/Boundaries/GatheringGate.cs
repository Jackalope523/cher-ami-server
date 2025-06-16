using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Core.Boundaries
{
    #region Schemas

    public enum GatheringState
	{ Alive, NOP_REMOVE, Cancelled, Ended }

    public enum GatheringVisibility
	{ Visible, Hidden, Sealed }

    public enum GatheringBond
    { NOP_REMOVE, Guest, Arrived, Left, Kicked }

    public record CoreGathering(long Id, long HostId, string Title, string Description,
		DateTimeOffset StartTime, double Latitude, double Longitude, string FriendlyLocation,
		DateTimeOffset? TimeEnded, GatheringState State, int GroupMinimum, int GroupMaximum, CharacterShard Character,
		double Radius, bool IsDynamic, bool IsPendingDeletion, int NumberOfGuests,
		int DegreeOfPrivacy, GatheringVisibility Visibility, DateTimeOffset TimeOfCreation, float Decay)
		: CoreOnlyData();

	public record GatheringShard(long Id, UserShard Host, string Title, string Description,
        DateTimeOffset StartTime, double Latitude, double Longitude, string FriendlyLocation,
		DateTimeOffset? TimeEnded, GatheringState State, int GroupMinimum, int GroupMaximum,
        double Radius, int DegreeOfPrivacy, int NumberOfGuests, float RelativeAngle,
		GatheringVisibility Visibility, float Decay);

	public record GuestListBondPair(UserShard User, GatheringBond Bond);

    #endregion

    #region Gates

    public interface IGatheringDatabase
	{
        Task<CoreGathering> FindGatheringAsync(long gatheringId);
		Task<List<CoreGathering>> FindGatheringsAsync(double latitude, double longitude, double distance);
		Task<List<CoreGathering>> FindPastGatheringsForUserAsync(long userId);
		Task<List<CoreGathering>> FindOngoingGatheringsForUserAsync(long userId, DateTimeOffset currentTime);
		Task<List<CoreGathering>> FindUpcomingGatheringsForUserAsync(long userId, DateTimeOffset currentTime);
		Task<List<CoreGathering>> FindGatheringsByUserAsync(long userId);

		Task<CoreGathering> CreateGatheringAsync(long hostId, string title, string description,
			DateTimeOffset startTime, double latitude, double longitude, string friendlyLocation,
			int groupMinimum, int groupMaximum, CharacterShard character,
			double Radius, bool isDynamic, int degreeOfPrivacy, DateTimeOffset timeOfCreation);
		Task UpdateGatheringAsync(long gatheringId, List<(string Property, object Value)> edits);
		Task TerminateGatheringAsync(long gatheringId, DateTimeOffset time);
		Task CancelGatheringAsync(long gatheringId);

		Task<GatheringBond?> GetUserStateAsync(long userId, long gatheringId);
		Task SetUserStateAsync(long userId, long gatheringId, GatheringBond userState, DateTimeOffset time);
		Task DeleteUserStateAsync(long userId, long gatheringId);

		Task<List<(long UserId, GatheringBond State)>> GetAllUsersAsync(long gatheringId);
		Task<List<(long UserId, DateTimeOffset Joined, DateTimeOffset? Left)>> GetGuestHistoryAsync(long gatheringId);

        Task<bool> UserIsAuthorizedGuest(long userId, long gatheringId);
        Task<List<long>> GetAuthorizedGuests(long gatheringId);
        Task AddGuestAuthorization(long gatheringId, long userId);

        Task SoftDeleteAsync(long gatheringId);
        Task HardDeleteAsync(long gatheringId);
    }

	public interface IGatheringOperations
	{
		Task<GatheringShard> GetGatheringInformationAsync(long userId, long gatheringId);
		Task<List<TwigShard>> GetUpcomingGatheringsAsync(long userId);
		Task<List<TwigShard>> GetOngoingGatheringsAsync(long userId);
		Task<List<TwigShard>> GetPastGatheringsAsync(long userId);

		Task<List<GatheringShard>> GetGatheringsInAreaAsync(long userId,
			double latitude, double longitude, double distance);
		Task<List<GatheringShard>> GetPersonalisedGatheringsInAreaAsync(long userId,
			double latitude, double longitude, double distance);

		Task<GatheringShard> CreateGatheringAsync(long userId, string gatheringTitle, string gatheringDescription,
			DateTimeOffset startTime, double latitude, double longitude, string friendlyLocation,
			double radius, bool isDynamic, int degreeOfPrivacy, int? groupMinimum, int? groupMaximum,
			MemoryStream heroImage);
		Task EditGatheringAsync(long userId, long gatheringId,
			string gatheringTitle = "", string gatheringDescription = "",
			DateTimeOffset? startTime = null, double? latitude = null, double? longitude = null, string friendlyLocation = "",
			double? radius = null, bool? isDynamic = null, int? degreeOfPrivacy = null,
			int? groupMinimum = null, int? groupMaximum = null, MemoryStream header = null);
		Task TerminateGatheringAsync(long userId, long gatheringId);
		Task CancelGatheringAsync(long userId, long gatheringId);

		Task ChangeGatheringVisibilityAsync(long userId, long gatheringId, bool hide);

		Task JoinGatheringAsync(long userId, long gatheringId);
		Task LeaveGatheringAsync(long userId, long gatheringId);

		Task<List<GuestListBondPair>> GetGuestListAsync(long userId, long gatheringId);
		Task<List<UserShard>> GetPotentialInviteesAsync(long userId, long gatheringId);
		Task InviteUserAsync(long inviterId, long inviteeId, long gatheringId);
		Task KickUserAsync(long hostId, long targetId, long gatheringId);

		Task<bool> AuthorisedToJoin(long userId, long gatheringId);
		Task<bool> AuthorisedToUpload(long userId, long gatheringId);
	}

	#endregion
}
