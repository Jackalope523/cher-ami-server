using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Boundaries;
using Core.Entities;
using Core.Notifications;
using Microsoft.Extensions.Logging;

using static Core.Entities.Arbiter;

namespace Core.Controls
{
    internal class NestDirector : AbstractDirector, INestOperations
	{
		#region Initialisation

		public NestDirector(CoreTerminal terminal) : base(terminal) { }

		#endregion

		#region Operations

        public async Task<NestShard> GetNestAsync(long userId, long targetId)
        {
            var user = await GetUserAsync(userId);
            var targetUser = await GetUserAsync(targetId);

            // Fail if user is blocked
            FailIf(await user.IsBlockedBy(targetUser),
                new UserErrorException(UserErrorCode.CANNOT_VIEW));
            
            NestShard nest = new(new());

            // Check if user is themself
            if (user.Equals(targetUser))
            {
                var pastGatheringsSync = targetUser.PastGatherings;
                var ongoingGatheringsSync = targetUser.OngoingGatherings;
                var upcomingGatheringsSync = targetUser.UpcomingGatherings;

                // Get gatherings and snapshots
                var twigs = (await pastGatheringsSync)
                    .Concat(await ongoingGatheringsSync)
                    .Concat(await upcomingGatheringsSync)
                    .ToList()
                    .ConvertAll(e => e.ToTwigShard());

                nest = nest with
                {
                    Twigs = twigs
                };
            }
            // Check if users are companions
            else if (await targetUser.IsCompanionsWith(user))
            {
                var haveMutualSync = Nests.HaveMutualGathering(user.Id, targetUser.Id);

                var pastGatheringsSync = targetUser.PastGatherings;
                var ongoingGatheringsSync = targetUser.OngoingGatherings;
                var upcomingGatheringsSync = targetUser.UpcomingGatherings;

                // Get gatherings and snapshots
                var twigs = (await pastGatheringsSync)
                    .Concat(await ongoingGatheringsSync)
                    .Concat(await upcomingGatheringsSync)
                    .ToList()
                    .ConvertAll(e => e.ToTwigShard());

                if (await haveMutualSync)
                {
                    nest = new(twigs, (await Nests.GetFirstMutualGathering(user.Id, targetUser.Id)).Id);
                }
                else
                {
                    nest = new(twigs, default);
                }

                nest = await RemoveUnviewableNestTwigsAsync(user, nest);
            }
            // User is a stranger
            else
            {
                // Check if has a mutual gathering
                var haveMutualSync = Nests.HaveMutualGathering(user.Id, targetUser.Id);

                // Get public hosted gatherings
                var hostedGatherings = (await Gatherings.FindGatheringsByUserAsync(targetUser.Id))
                    .ConvertAll(e => new Gathering(e));

                var twigs = hostedGatherings
                    .ConvertAll(e => e.ToTwigShard());

                // Get common gatherings
                var commonGatherings = (await targetUser.PastGatherings)
                    .Except(hostedGatherings)
                    .Intersect(await user.PastGatherings)
                    .ToList()
                    .ConvertAll(e => e.ToTwigShard());

                twigs.AddRange(commonGatherings);

                if (await haveMutualSync)
                {
                    nest = new(twigs, (await Nests.GetLatestMutualGathering(user.Id, targetUser.Id)).Id);
                }
                else
                {
                    nest = new(twigs, default);
                }

                nest = await RemoveUnviewableNestTwigsAsync(user, nest);
            }

            return nest;
        }

        public async Task<AgendaShard> GetUserAgendaAsync(long userId)
        {
            var user = await GetUserAsync(userId);

            // Gather active and upcoming gatherings
            var upcomingAgenda = await RequestAgenda(user);

            return upcomingAgenda;
        }

        public async Task<IDictionary<long, AgendaShard>> GetCompanionAgendasAsync(long userId)
        {
            var user = await GetUserAsync(userId);

            Dictionary<long, AgendaShard> companionGatherings = new();

            // Gather visible agenda of each companion
            foreach (var companion in await user.Companions)
            {
                var companionAgenda = await RequestAgenda(companion);
                companionAgenda = await RemoveUnviewableAgendaCardsAsync(user, companionAgenda);
                companionGatherings.TryAdd(companion.Id, companionAgenda);
            };

            return companionGatherings;
        }

        public async Task<List<UserShard>> GetCompanionsAsync(long userId)
        {
            var user = await GetUserAsync(userId);

            return (await user.Companions)
                .ConvertAll(u => u.ToUserShard());
        }

        public async Task<List<CompanionshipRequestShard>> GetIncomingCompanionshipRequestsAsync(long userId)
        {
            var user = await GetUserAsync(userId);

            var requests = await Nests.GetIncomingRequestsAsync(user.Id);

            return requests;
        }

        public async Task<List<CompanionshipRequestShard>> GetOutgoingCompanionshipRequestsAsync(long userId)
        {
            var user = await GetUserAsync(userId);

            var requests = await Nests.GetOutgoingRequestsAsync(user.Id);

            return requests;
        }

        public async Task<List<UserShard>> GetRecentlyMetAsync(long userId)
        {
            var user = await GetUserAsync(userId);

            var recentlyMet = await Nests.GetRecentlyMetAsync(user.Id);

            return recentlyMet
                .ConvertAll(u => new User(u).ToUserShard());
        }

        public async Task<List<BlockedUserShard>> GetBlockedUsersAsync(long userId)
        {
            return await Nests.GetBlockedUsersAsync(userId);
        }

        public async Task AcceptOrRequestCompanionshipAsync(long userId, long targetId)
        {
            var user = await GetUserAsync(userId);
            var targetUser = await GetUserAsync(targetId);

            FailIf(user.Equals(targetUser),
                new UserErrorException(UserErrorCode.CANNOT_FOLLOW_SELF));

            Verify(await user.CanFollow(targetUser),
                new UserErrorException(UserErrorCode.CANNOT_FOLLOW));

            await Nests.FollowUserAsync(user.Id, targetUser.Id, Psijic.Time);

            CanaryNotification targetNotification = CanaryNotification.CompanionshipRequest(user.ToUserShard());

            // Should always hit
            if (await Nests.HaveMutualGathering(user.Id, targetUser.Id))
            {
                var lastMutualGathering = await Nests.GetLatestMutualGathering(user.Id, targetUser.Id);

                targetNotification = CanaryNotification.CompanionshipRequest(user.ToUserShard(), lastMutualGathering.Title);
            }

            // Check if this forges companionship
            if (await targetUser.IsFollowing(user))
            {
                targetNotification = CanaryNotification.CompanionshipForged(user.ToUserShard());
            }
            else
            {
                // todo user followed message?
            }

            _ = targetUser.Notify(targetNotification);
        }

        public async Task RequestCompanionshipAsync(long userId, string code)
        {
            var user = await GetUserAsync(userId);

            CoreUser potentialUser;

            // Check user code
            try
            {
                potentialUser = await Accounts.FindUserByCodeAsync(code.ToLower());
            }
            catch
            { throw new UserErrorException(UserErrorCode.CODE_NOT_FOUND); }

            User targetUser = new(potentialUser);

            FailIf(user.Equals(targetUser),
                new UserErrorException(UserErrorCode.CANNOT_FOLLOW_SELF));

            Verify(await user.CanFollow(targetUser, hasCode: true),
                new UserErrorException(UserErrorCode.CANNOT_FOLLOW));

            await Nests.FollowUserAsync(user.Id, targetUser.Id, Psijic.Time);

            CanaryNotification targetNotification = CanaryNotification.CompanionshipRequest(user.ToUserShard());

            // Check if this forges companionship
            if (await targetUser.IsFollowing(user))
            {
                targetNotification = CanaryNotification.CompanionshipForged(user.ToUserShard());
            }
            else
            {
                // todo user followed message?
            }

            _ = targetUser.Notify(targetNotification);
        }

        public async Task DenyOrRemoveUserAsync(long userId, long targetId)
        {
            await Nests.UnfollowUserAsync(targetId, userId);
            await Nests.UnfollowUserAsync(userId, targetId);
        }

        public async Task BlockUserAsync(long userId, long targetId)
        {
            var user = await GetUserAsync(userId);
            var targetUser = await GetUserAsync(targetId);

			FailIf(user.Equals(targetUser),
				new UserErrorException(UserErrorCode.CANNOT_BLOCK_SELF));

			await Nests.BlockUserAsync(userId, targetId, Psijic.Time);

            // Ensure removal from upcoming hosted gatherings
            foreach (var gathering in await user.UpcomingGatherings)
            {
                // Check if user is host
                if (gathering.HostId.Equals(user.Id))
                {
                    try
                    {
                        // Blind-remove target
                        await Gatherings.DeleteUserStateAsync(targetUser.Id, gathering.Id);
                    }
                    catch { }
                }
            }
        }

        public async Task UnblockUserAsync(long userId, long targetId)
        {
            await Nests.UnblockUserAsync(userId, targetId);
        }

        public async Task<bool> AuthorisedToFollow(long userId, long targetId)
        {
            var user = await GetUserAsync(userId);
            var targetUser = await GetUserAsync(targetId);

            return await user.CanFollow(targetUser);
        }

		#endregion

		#region Favours

        internal async Task<List<User>> RequestCompanionsAsync(User user)
        {
            return (await Nests.GetCompanionsAsync(user.Id))
                .ConvertAll(user => new User(user));
		}

        internal async Task<List<User>> RequestFollowedUsersAsync(User user)
        {
            return (await Nests.GetFollowedUsersAsync(user.Id))
                .ConvertAll(user => new User(user));
		}

        internal async Task<List<User>> RequestFollowersAsync(User user)
        {
            return (await Nests.GetUserFollowersAsync(user.Id))
                .ConvertAll(user => new User(user));
		}

		internal async Task<List<User>> RequestBlockedUsersAsync(User user)
        {
            var users = await Nests.GetBlockedUsersAsync(user.Id);

            return (await Psijic.Once(users.Select(u => User.GetUserAsync(u.Id)).ToArray()))
                .ToList();
        }

		internal async Task<List<User>> RequestUsersBlockingAsync(User user)
        {
            return (await Nests.GetUsersBlockingAsync(user.Id))
				.ConvertAll(user => new User(user));
        }

        internal async Task<bool> RequestAttendedMutualGatheringAsync(User user, User target)
        {
            return await Nests.HaveMutualGathering(user.Id, target.Id);
        }

        #endregion

        #region Tools

        private async Task<AgendaShard> RequestAgenda(User user)
        {
            _ = user.OngoingGatherings.Sync();
            _ = user.UpcomingGatherings.Sync();

            // Gather all user gathering data
            AgendaShard agenda = new((await user.OngoingGatherings)
                .Concat(await user.UpcomingGatherings)
                .ToList()
                .ConvertAll(gathering => new CardShard(gathering.Id, gathering.StartTime, GatheringBond.Guest)));

            return agenda;
        }

        private async Task<AgendaShard>
            RemoveUnviewableAgendaCardsAsync(User user, AgendaShard agenda)
        {
            AgendaShard viewableGatherings = new(new());

            foreach (var card in agenda.Cards)
            {
                Gathering gathering = await GetGatheringAsync(card.GatheringId);

                if (await user.CanView(gathering))
                { viewableGatherings.Cards.Add(card); }
            }

            return viewableGatherings;
        }

        private async Task<NestShard>
            RemoveUnviewableNestTwigsAsync(User user, NestShard nest)
        {
            NestShard visibleNest = new(new(), nest.RelativeGatheringId);

            foreach (var card in nest.Twigs)
            {
                Gathering gathering = await GetGatheringAsync(card.GatheringId);

                if (await user.CanView(gathering))
                { visibleNest.Twigs.Add(card); }
            }

            return visibleNest;
        }

        #endregion
    }
}

