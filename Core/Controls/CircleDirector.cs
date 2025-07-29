using Core.Boundaries;
using Core.Entities;
using Core.Notifications;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using static Core.Entities.Arbiter;
using static Core.Entities.Artificer;
using static Core.Entities.Psijic;

namespace Core.Controls
{
    internal class CircleDirector : AbstractDirector, ICircleOperations
	{
		#region Initialisation

		public CircleDirector(CoreTerminal terminal) : base(terminal) { }

		#endregion

		#region Operations

		public async Task<GatheringShard> GetGatheringInformationAsync(long userId, long gatheringId)
        {
			var user = await GetUserAsync(userId);
			var targetGathering = await GetCircleAsync(gatheringId);

			// Verify user is allowed to view gathering
			Verify(await targetGathering.IsVisibleTo(user),
				new UserErrorException(CircleErrorCode.CANNOT_VIEW));

			return await targetGathering.ToIssueShard();
		}

		public async Task<List<ProfileIssueShard>> GetUpcomingGatheringsAsync(long userId)
        {
			var user = await GetUserAsync(userId);

			return (await user.UpcomingGatherings).ConvertAll(g => g.ToTwigShard());
		}

		public async Task<List<ProfileIssueShard>> GetOngoingGatheringsAsync(long userId)
        {
			var user = await GetUserAsync(userId);

			return (await user.OngoingGatherings).ConvertAll(g => g.ToTwigShard());
		}

		public async Task<List<ProfileIssueShard>> GetPastGatheringsAsync(long userId)
        {
			var user = await GetUserAsync(userId);

			return (await user.PastGatherings).ConvertAll(g => g.ToTwigShard());
		}

		public async Task<List<GatheringShard>> GetGatheringsInAreaAsync(long userId,
			double latitude, double longitude, double distance)
		{
			var user = await GetUserAsync(userId);
			var nearbyGatherings = await Circles.FindGatheringsAsync(latitude, longitude, distance);

			// Remove gatherings from list that the user cannot access
			var filteredGatherings = await RemoveInaccessibleGatheringsAsync(user, nearbyGatherings);

			// Ensure user's own and current gatherings show
			/*
			var upcomingGatherings = await Gatherings.FindUpcomingGatheringsForUserAsync(user.Id);
			upcomingGatherings.Add(await Gatherings.FindCurrentGatheringForUserAsync(user.Id));

			filteredGatherings = EnsureContains(filteredGatherings, upcomingGatherings);
			*/

			return filteredGatherings;
		}

		public async Task<List<GatheringShard>> GetPersonalisedGatheringsInAreaAsync(long userId,
			double latitude, double longitude, double distance)
		{
			var user = await GetUserAsync(userId);
			var nearbyGatherings = await Circles.FindGatheringsAsync(latitude, longitude, distance);

			// Remove inaccessible gatherings and gatherings with a large difference between gathering and user interest
			var filteredGatherings = await RemoveUnattractiveGatheringsAsync(user, nearbyGatherings, 1f);

            // Ensure user's own and current gatherings show
			/*
            var upcomingGatherings = await Gatherings.FindUpcomingGatheringsForUserAsync(user.Id);
            upcomingGatherings.Add(await Gatherings.FindCurrentGatheringForUserAsync(user.Id));

            filteredGatherings = EnsureContains(filteredGatherings, upcomingGatherings);
			*/

            return filteredGatherings;
		}

		public async Task<GatheringShard> CreateGatheringAsync(long userId,
			string gatheringName, string gatheringDescription, DateTimeOffset startTime,
			double latitude, double longitude, string friendlyLocation,
			double radius, bool isDynamic, int degreeOfPrivacy,
			int? groupMinimum, int? groupMaximum,
			MemoryStream heroImage)
		{
			var user = await GetUserAsync(userId);

			// Verify user can host
			Verify(user.CanHost,
				new UserErrorException(CircleErrorCode.CANNOT_HOST, new { user.AccountStatus }));

			// Verify user has position enabled
			Verify((await user.LastKnownLocation).Exists,
				new UserErrorException(CircleErrorCode.LOCATION_DISABLED));

			// Create gathering
			Issue gatheringStub = new()
			{
				Title = gatheringName,
				Description = gatheringDescription,
				StartDate = startTime,
				Location = new() { Latitude = latitude, Longitude = longitude },
				FriendlyLocation = friendlyLocation,
				GroupMinimum = groupMinimum ?? 0,
				GroupMaximum = groupMaximum ?? 0,
				Radius = new() { Kilometres = Math.Clamp(radius, 0.1, radius) },
				IsDynamic = isDynamic,
				DegreeOfPrivacy = degreeOfPrivacy,
			};

			// Validate gathering
			Verify(gatheringStub.ValidateAndNormalise(out string issues),
				new UserErrorException(CircleErrorCode.INVALID_DETAILS, new { issues }));

			// Verify user has no conflict
			var conflict = (await user.UpcomingGatherings).Find(e => IsWithin(e.StartTime - gatheringStub.StartDate, HalfHour));
			if (conflict != null)
			{ throw new UserErrorException(CircleErrorCode.CONFLICT, new { conflict.Id }); }

			// Try to create a gathering
			Issue newGathering = new(await Circles.CreateGatheringAsync(user.Id,
				gatheringStub.Title, gatheringStub.Description, gatheringStub.StartDate,
				gatheringStub.Location.Latitude, gatheringStub.Location.Longitude, gatheringStub.FriendlyLocation,
				gatheringStub.GroupMinimum, gatheringStub.GroupMaximum, user.Character.ToCharacter(),
				gatheringStub.Radius.Kilometres, gatheringStub.IsDynamic, gatheringStub.DegreeOfPrivacy,
				Time));

			try
			{
				// Upload hero
				await Terminal.MediaDirector.UploadGatheringHeaderAsync(newGathering.Id, heroImage);
			}
			catch (Exception ex)
			{
				// If failed, remove gathering
				await Circles.HardDeleteAsync(newGathering.Id);
				throw new UnexpectedFailureException($"Failed to upload hero image for gathering by {user.Id}.", ex, HollowErrorCode.UPLOAD_FAILED);
            }

            // If now
			if (HasAlready(newGathering.StartDate))
			{
				await Circles.UpdateGatheringAsync(newGathering.Id, new() { (nameof(CoreGathering.StartTime), Time) });
				newGathering = await GetCircleAsync(newGathering.Id);
			}
			else
			{
				// Schedule notifications
				_ = ScheduleNotifications(newGathering);
			}

            // Notify companions of gathering
            _ = user.NotifyCompanions(CardinalNotification.CompanionGatheringCreated(user.ToUserShard(), await newGathering.ToIssueShard()));
			
			return await newGathering.ToIssueShard();
		}

		public async Task EditGatheringAsync(long userId, long gatheringId,
			string gatheringName = "", string gatheringDescription = "",
			DateTimeOffset? startTime = null,
			double? latitude = null, double? longitude = null, string friendlyLocation = "",
			double? radius = null, bool? isDynamic = null, int? degreeOfPrivacy = null,
			int? groupMinimum = null, int? groupMaximum = null,
			MemoryStream header = null)
		{
			var user = await GetUserAsync(userId);
			var originalGathering = await GetCircleAsync(gatheringId);

			// Verify user is gathering host
			Verify(originalGathering.IsModifiableBy(user),
				new UserErrorException(CircleErrorCode.CANNOT_EDIT_PERMISSION));

			// Ensure gathering is editable
			FailIf(originalGathering.IsTerminated,
				new UserErrorException(CircleErrorCode.CANNOT_EDIT_ENDED));

			// Fail if edits may not be done during the gathering
			FailIf(originalGathering.IsOngoing &&
				(!string.IsNullOrEmpty(gatheringName) ||
				!string.IsNullOrEmpty(gatheringDescription) ||
				IsNotNull(startTime) ||
				AreNotNull(latitude, longitude) ||
                !string.IsNullOrEmpty(friendlyLocation) ||
                IsNotNull(radius) || IsNotNull(isDynamic)),
				new UserErrorException(CircleErrorCode.CANNOT_EDIT_STARTED));

			Issue editedGathering = new(originalGathering.ToCoreIssue())
			{
                Title = string.IsNullOrEmpty(gatheringName) ? originalGathering.Title : gatheringName,
                Description = string.IsNullOrEmpty(gatheringDescription) ? originalGathering.Description : gatheringDescription,
				StartDate = startTime ?? originalGathering.StartDate,
				Location = AreNull(latitude, longitude) ? originalGathering.Location : new() { Latitude = latitude.Value, Longitude = longitude.Value },
				FriendlyLocation = string.IsNullOrEmpty(friendlyLocation) ? originalGathering.FriendlyLocation : friendlyLocation,
				Radius = IsNull(radius) ? originalGathering.Radius : new() { Kilometres = Math.Clamp(radius.Value, 0.1, radius.Value) },
				IsDynamic = isDynamic ?? originalGathering.IsDynamic,
				DegreeOfPrivacy = degreeOfPrivacy ?? originalGathering.DegreeOfPrivacy,
				GroupMinimum = groupMinimum ?? originalGathering.GroupMinimum,
				GroupMaximum = groupMaximum ?? originalGathering.GroupMaximum,
			};

			// Validate gathering
			Verify(editedGathering.ValidateAndNormalise(out string issues),
				new UserErrorException(CircleErrorCode.INVALID_DETAILS, new { issues }));

			List<(string Property, object Value)> edits = new();
            List<ActivityMessageShard> editMessages = new();

            // Gather individual edits
            if (!string.IsNullOrEmpty(gatheringName))
			{
				edits.Add((nameof(CoreGathering.Title), editedGathering.Title));
                editMessages.Add(new(ActivityMessageType.Edited, ActorId: user.Id, Info: "title"));
            }
			if (!string.IsNullOrEmpty(gatheringDescription))
			{
				edits.Add((nameof(CoreGathering.Description), editedGathering.Description));
                editMessages.Add(new(ActivityMessageType.Edited, ActorId: user.Id, Info: "description"));
			}
			if (IsNotNull(startTime))
			{
				edits.Add((nameof(CoreGathering.StartTime), editedGathering.StartDate));
                editMessages.Add(new(ActivityMessageType.Edited, ActorId: user.Id, Info: "time"));
			}
			if (IsNotNull(latitude) && IsNotNull(longitude))
			{
				edits.Add(("Location", (editedGathering.Location.Latitude, editedGathering.Location.Longitude)));
                editMessages.Add(new(ActivityMessageType.Edited, ActorId: user.Id, Info: "location"));
			}
			if (!string.IsNullOrEmpty(friendlyLocation))
			{
				edits.Add((nameof(CoreGathering.FriendlyLocation), editedGathering.FriendlyLocation));
			}
			if (IsNotNull(radius))
			{
				edits.Add((nameof(CoreGathering.Radius), editedGathering.Radius.Kilometres));
			}
			if (IsNotNull(isDynamic))
			{
				edits.Add((nameof(CoreGathering.IsDynamic), editedGathering.IsDynamic));
			}
			if (IsNotNull(degreeOfPrivacy))
			{
				edits.Add((nameof(CoreGathering.DegreeOfPrivacy), editedGathering.DegreeOfPrivacy));
                editMessages.Add(new(ActivityMessageType.Edited, ActorId: user.Id, Info: "visibility"));
			}
			if (IsNotNull(groupMinimum))
			{
				edits.Add((nameof(CoreGathering.GroupMinimum), editedGathering.GroupMinimum));
			}
			if (IsNotNull(groupMaximum))
			{
				edits.Add((nameof(CoreGathering.GroupMaximum), editedGathering.GroupMaximum));
            }

            if (header != null && header.Length > 0)
            {
                await Terminal.MediaDirector.UploadGatheringHeaderAsync(originalGathering.Id, header);
                editMessages.Add(new(ActivityMessageType.Edited, ActorId: user.Id, Info: "header"));
            }

            if (edits.Any())
			{
				// Push update
				await Circles.UpdateGatheringAsync(originalGathering.Id, edits);

				_ = originalGathering.NotifyGuests(CardinalNotification.GatheringEdited(await originalGathering.ToIssueShard()), notifyHost: false);

				// Reschedule notifications if required
				if (IsNotNull(startTime))
				{
					_ = RescheduleSchedule(editedGathering);
				}
			}

            if (editMessages.Any() && await Messages.CircleChatExists(originalGathering.Id))
            {
				Chat conversation = new(await Messages.GetOrCreateCircleChat(originalGathering.Id, Time));

                foreach (var value in editMessages)
                {
                    var message = await Messages.AddMessageAsync(conversation.Id, User.Hollow.Id, Time, MessageType.Activity, value);
                    _ = conversation.MessageOthersAsync(User.Hollow, message);
                }
            }
        }

		public async Task TerminateGatheringAsync(long userId, long gatheringId)
		{
			var user = await GetUserAsync(userId);
			var gathering = await GetCircleAsync(gatheringId);

			// Verify user is able to end the gathering
			Verify(gathering.IsModifiableBy(user),
				new UserErrorException(CircleErrorCode.NOT_HOST));

			// Verify gathering is able to be terminated
            Verify(gathering.IsTerminable(),
                new UserErrorException(CircleErrorCode.CANNOT_END));

            // Try to end gathering
            await Circles.TerminateGatheringAsync(gathering.Id, Time);

			// Reshow if hidden
			if (gathering.Visibility == GatheringVisibility.Hidden)
			{
				await Circles.UpdateGatheringAsync(gathering.Id, new() { (nameof(CoreGathering.Visibility), GatheringVisibility.Visible) });
			}

			var participants = await gathering.Ended();

			// Update all participants' vectors
			_ = Terminal.AccountDirector.UpdateAllAsync(participants, user => new() { (nameof(CoreUser.Character), user.Character) });

			// Schedule photo reminder for attendees
			_ = User.NotifyAll(CardinalNotification.GatheringUploadClosing(await gathering.ToIssueShard()), notifyAt: Time + OneDay * 0.7, users: (await gathering.Left).ToArray());
        }

		public async Task CancelGatheringAsync(long userId, long gatheringId)
		{
            var user = await GetUserAsync(userId);
            var gathering = await GetCircleAsync(gatheringId);

			// Verify gathering has not yet started
			Verify(gathering.IsCancelable(),
                new UserErrorException(CircleErrorCode.CANNOT_CANCEL_STARTED));

            // Verify user is able to cancel the gathering
            Verify(gathering.IsModifiableBy(user),
                new UserErrorException(CircleErrorCode.CANNOT_CANCEL_PERMISSION));

            // Try to cancel gathering
            await Circles.CancelGatheringAsync(gathering.Id);

            _ = gathering.NotifyGuests(CardinalNotification.GatheringCancelled(await gathering.ToIssueShard()), notifyHost: false);

			// Cancel scheduled notifications
			_ = CancelScheduledNotifications(gathering);
        }

        public async Task ChangeGatheringVisibilityAsync(long userId, long gatheringId, bool hide)
		{
			var user = await GetUserAsync(userId);
			var gathering = await GetCircleAsync(gatheringId);

            // Verify user is gathering host
            Verify(gathering.IsModifiableBy(user),
                new UserErrorException(CircleErrorCode.CANNOT_EDIT_PERMISSION));

            // Ensure gathering is editable
            Verify(gathering.IsOngoing,
                new UserErrorException(CircleErrorCode.NOT_STARTED));

            // Ensure gathering is not sealed
            FailIf(gathering.Visibility == GatheringVisibility.Sealed,
                new UserErrorException(CircleErrorCode.SEALED));

            var visibility = hide ? GatheringVisibility.Hidden : GatheringVisibility.Visible;

			await Circles.UpdateGatheringAsync(gathering.Id, new() { (nameof(CoreGathering.Visibility), visibility) });
        }

		public async Task JoinGatheringAsync(long userId, long gatheringId)
		{
			var user = await GetUserAsync(userId);
			var gathering = await GetCircleAsync(gatheringId);
			_ = user.LastKnownLocation.Sync();

			// Verify user is allowed to join gathering
			Verify(await gathering.IsJoinableBy(user),
                new UserErrorException(CircleErrorCode.CANNOT_JOIN, new { user.AccountStatus }));

            GatheringBond? previousUserState = null;

            try
            {
                previousUserState = await Circles.GetUserStateAsync(userId, gatheringId);
            }
            catch { }

            // Check that user was not kicked
            FailIf(previousUserState.HasValue &&
                previousUserState.Value.Equals(GatheringBond.Kicked),
                new UserErrorException(CircleErrorCode.KICKED));

            // Check if user is already guest or arrived
            if (previousUserState.HasValue &&
				(previousUserState.Value.Equals(GatheringBond.Guest) ||
                previousUserState.Value.Equals(GatheringBond.Arrived)))
			{
                throw new UserErrorException(CircleErrorCode.CANNOT_JOIN_GUEST);
            }

			// Check if gathering is active and user is already there
			if (HasAlready(gathering.StartDate) &&
				await gathering.IsInRange(user))
			{
				// Try to add user to the gathering
				await Circles.SetUserStateAsync(user.Id, gathering.Id, GatheringBond.Guest, Time);
				await Circles.SetUserStateAsync(user.Id, gathering.Id, GatheringBond.Arrived, Time);
                await Circles.UpdateGatheringAsync(gathering.Id, new() { (nameof(CoreGathering.Decay), Issue.InitialDecay) });
            }
			else
			{
				// Try to add user to the gathering
				await Circles.SetUserStateAsync(user.Id, gathering.Id, GatheringBond.Guest, Time);

				// Schedule notifications as required
				_ = ScheduleNotificationsForGuest(gathering, user);

				// Notify any companions at gathering
				var activeGuests = (await gathering.Guests).Concat(await gathering.Arrived);
				var userCompanions = await user.Companions;

				var activeCompanions = activeGuests.Intersect(userCompanions);

				_ = User.NotifyAll(CardinalNotification.CompanionJoined(user.ToUserShard(), await gathering.ToIssueShard()), users: activeCompanions.ToArray());
            }

			// Add member to chat
			if (await Messages.CircleChatExists(gathering.Id))
			{
				Chat conversation = new(await Messages.GetOrCreateCircleChat(gathering.Id, Time));

				await Messages.AddUsersToChatAsync(conversation.Id, user.Id);

                ActivityMessageShard activityMessage = new(ActivityMessageType.Joined, ActorId: user.Id);
                var message = await Messages.AddMessageAsync(conversation.Id, User.Hollow.Id, Time, MessageType.Activity, activityMessage);

                _ = conversation.MessageOthersAsync(User.Hollow, message);
            }
		}

		public async Task LeaveGatheringAsync(long userId, long gatheringId)
		{
			var user = await GetUserAsync(userId);
			var gathering = await GetCircleAsync(gatheringId);

            // Get the user's current status
            var userIntention = await Circles.GetUserStateAsync(userId, gatheringId);

			// Check that user was associated
			Verify(userIntention.HasValue,
				new UserErrorException(CircleErrorCode.NOT_GUEST));

            // Check that user was not kicked
            FailIf(userIntention.HasValue &&
                userIntention.Value.Equals(GatheringBond.Kicked),
                new UserErrorException(CircleErrorCode.KICKED));

			// Try to remove user from gathering
			await Circles.DeleteUserStateAsync(user.Id, gathering.Id);

            // Delete any snapshots
            foreach (PostShard snapshot in await gathering.Posts)
            {
                if (user.Owns(snapshot))
                { _ = Issues.SoftDeleteAsync(snapshot.Id); }
            }

            // Cancel scheduled notifications
            _ = CancelScheduledNotificationsForGuest(gathering, user);

            // Remove member from chat
            if (await Messages.CircleChatExists(gathering.Id))
            {
                Chat conversation = new(await Messages.GetOrCreateCircleChat(gathering.Id, Time));

                await Messages.RemoveUserFromChatAsync(conversation.Id, user.Id);

                ActivityMessageShard activityMessage = new(ActivityMessageType.Left, ActorId: user.Id);
                var message = await Messages.AddMessageAsync(conversation.Id, User.Hollow.Id, Time, MessageType.Activity, activityMessage);

                _ = conversation.MessageOthersAsync(User.Hollow, message);
            }
        }

		public async Task<List<GuestListBondPair>>
			GetGuestListAsync(long userId, long gatheringId)
		{
			var user = await GetUserAsync(userId);
			var gathering = await GetCircleAsync(gatheringId);

			// Gather
			var allGuests = SelectAsBonds(await gathering.AllUsers,
				user => user.State != GatheringBond.Kicked);

			// Sort
			allGuests.Sort((bond1, bond2) =>
            {
                int bondComparison = GetBondPriority(bond1.Bond).CompareTo(GetBondPriority(bond2.Bond));
                if (bondComparison != 0) return bondComparison;

				return bond1.User.Name.CompareTo(bond2.User.Name);
            });

			// Hide

			// Check if user is host or attendee
			if (gathering.IsModifiableBy(user) || await gathering.HasOnGuestList(user))
            {
				// Check if upcoming
				if (gathering.IsUpcoming)
				{
					// Hide strangers
					var strangers = await Profiles.ReturnStrangerDangerAsync(user.Id, allGuests.ConvertAll(bond => bond.User.Id).ToArray());

                    for (int i = 0; i < allGuests.Count; i++)
                    {
                        (User guest, GatheringBond bond) = allGuests[i];

                        bool isHost = gathering.IsModifiableBy(guest);
                        bool isSelf = user.Equals(guest);

                        // Check if incoming guest is not a companion, host, or self
                        if (strangers.Contains(guest.Id) && !(isHost || isSelf))
                        {
                            allGuests[i] = AsHiddenBondPair(bond);
                        }
                        // Else, guest is arrived or left (visible)
                    }
                }
				// Else, everyone is visible
			}
			// Check if user can view gathering
			else if (await gathering.IsVisibleTo(user))
			{
                // Hide strangers
                var strangers = await Profiles.ReturnStrangerDangerAsync(user.Id, allGuests.ConvertAll(bond => bond.User.Id).ToArray());

                for (int i = 0; i < allGuests.Count; i++)
                {
                    (User guest, GatheringBond bond) = allGuests[i];

                    bool isHost = gathering.IsModifiableBy(guest);
                    bool isSelf = user.Equals(guest);

                    // Check if incoming guest is not a companion, host, or self
                    if (strangers.Contains(guest.Id) && !(isHost || isSelf))
                    {
                        allGuests[i] = AsHiddenBondPair(bond);
                    }
                    // Else, guest is arrived or left (visible)
                }
            }
			// User cannot receive information about gathering
			else
			{ throw new UserErrorException(CircleErrorCode.CANNOT_VIEW); }

			List<GuestListBondPair> allGuestShards = allGuests
				.ConvertAll(userDetails => new GuestListBondPair(userDetails.User.ToUserShard(), userDetails.Bond));

            return allGuestShards;
		}

		public async Task<List<UserShard>> GetPotentialInviteesAsync(long userId, long gatheringId)
		{
			var user = await GetUserAsync(userId);
			var gathering = await GetCircleAsync(gatheringId);

			List<User> potentialUsers = new();

			// Check companions
			foreach (var companion in await user.Companions)
			{
				// Verify they can join and are not already on the guest list
				if (await gathering.IsJoinableBy(companion) &&
					!await gathering.HasOnGuestList(companion))
				{ potentialUsers.Add(companion); }
			}

			return potentialUsers
				.ConvertAll(u => u.ToUserShard());
		}

		public async Task AddMemberAsync(long inviterId, long inviteeId, long gatheringId)
		{
			var inviter = await GetUserAsync(inviterId);
			var invitee = await GetUserAsync(inviteeId);
			var gathering = await GetCircleAsync(gatheringId);

			// Verify inviter has relationship with gathering
			Verify(await gathering.HasUserRelationship(inviter),
				new UserErrorException(CircleErrorCode.NOT_GUEST));

			// Verify that the invitee can join the gathering
			Verify(await gathering.IsJoinableBy(invitee),
				new UserErrorException(CircleErrorCode.CANNOT_INVITE_INVALID_INVITEE));

			// Verify that inviter is companions with the invitee
			Verify(await inviter.IsCompanionsWith(invitee),
				new UserErrorException(CircleErrorCode.CANNOT_INVITE_NEUTRAL));

			Chat conversation = new(await Messages.GetOrCreateIndividualChatBetween(inviter.Id, invitee.Id, Time));
            var message = await Messages.AddMessageAsync(conversation.Id, inviter.Id, Time, MessageType.GatheringInvite, gathering.Id);

            _ = conversation.MessageOrNotifyOthersAsync(inviter, message);
        }

        public async Task KickUserAsync(long hostId, long targetId, long gatheringId)
		{
			var host = await GetUserAsync(hostId);
			var target = await GetUserAsync(targetId);
			var gathering = await GetCircleAsync(gatheringId);

			// Verify kicking user is the host
			Verify(gathering.IsHostedBy(host),
				new UserErrorException(CircleErrorCode.CANNOT_KICK_PERMISSION));

			// Verify host is not kicking themself
			FailIf(host.Equals(target),
				new UserErrorException(CircleErrorCode.CANNOT_KICK_SELF));

			// Kick target user from gathering
			await Circles.SetUserStateAsync(target.Id, gathering.Id, GatheringBond.Kicked, Time);

			// Remove target user's snapshots from gathering
			foreach (PostShard snapshot in await gathering.Posts)
			{
				if (target.Owns(snapshot))
				{ _ = Issues.SoftDeleteAsync(snapshot.Id); }
			}

            // Cancel any scheduled notifications
            _ = CancelScheduledNotificationsForGuest(gathering, target);

            // Remove member from chat
            if (await Messages.CircleChatExists(gathering.Id))
            {
                var conversation = await Messages.GetOrCreateCircleChat(gathering.Id, Time);

                await Messages.RemoveUserFromChatAsync(conversation.Id, target.Id);
            }
        }

		public async Task<bool> AuthorisedToJoin(long userId, long gatheringId)
        {
            var user = await GetUserAsync(userId);
            var gathering = await GetCircleAsync(gatheringId);

			return await user.CanJoin(gathering);
        }

		public async Task<bool> AuthorisedToUpload(long userId, long gatheringId)
        {
            var user = await GetUserAsync(userId);
            var gathering = await GetCircleAsync(gatheringId);

			return await gathering.HasOnGuestList(user);
        }

		#endregion

		#region Favours
		
		internal async Task<List<Circle>> RequestCirclesForUserAsync(User user)
		{
			var circles = await Circles.GetCirclesForUserAsync(user.Id);

			return circles.ConvertAll(c => new Circle(c));
		}

		internal async Task<List<CircleMember>> RequestCircleMembersAsync(Circle circle)
		{
			var users = await Circles.GetCircleMembersAsync(circle.Id);

			return (await Psijic.Once(users.Select(async userDetails => await CircleMember.FromMembershipAsync(userDetails))))
				.ToList();
		}
		
		internal async Task<List<CoreRecipient>> RequestCircleRecipientsAsync(Circle circle)
			=> await Circles.GetRecipientsForCircleAsync(circle.Id);

		#endregion

		#region Tools

		private List<(User User, GatheringBond Bond)>
			SelectAsBonds(List<(User User, GatheringBond State)> users, Func<(User User, GatheringBond State), bool> predicate)
		{
			return users.Where(predicate).ToList().ConvertAll(userDetails => (userDetails.User, userDetails.State));
		}

		private (User User, GatheringBond Bond) AsHiddenBondPair(GatheringBond bond)
		{
			return new(User.Hidden, bond);
		}

        private int GetBondPriority(GatheringBond bond)
        {
            return bond switch
            {
                GatheringBond.Arrived => 0, // sorted first
                GatheringBond.Guest => 1,   // sorted next
                GatheringBond.Left => 2,    // sorted last
                _ => 3
            };
        }

        #endregion
    }
}
