using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Repository
{
    public class EFCoreGatheringStore : QueryStore, IGatheringDatabase
    {
        public EFCoreGatheringStore(Harbor.Flag flag) : base(flag)
        {

        }

        internal void PropagateClearance(long userId, long gatheringId, int degree, List<long> exclusionList, Discussion discussion)
        {
            if (degree == -1) return;

            long foundInCache = storeSentry.DiscussRead(ctx =>
                                        ctx.GuestClearances.Local.
                                        Where(c => c.GatheringId == gatheringId && c.UserId == userId).
                                        Select(c => c.UserId).
                                        SingleOrDefault(), discussion);

            long foundInDatabase = storeSentry.DiscussRead(ctx =>
                                        ctx.GuestClearances.
                                        Where(c => c.GatheringId == gatheringId && c.UserId == userId).
                                        Select(c => c.UserId).
                                        SingleOrDefault(), discussion);

            if (foundInCache == 0 && foundInDatabase == 0)
            {
                storeSentry.DiscussWrite(ctx =>
                ctx.GuestClearances.
                Add(new GuestClearance
                {
                    UserId = userId,
                    GatheringId = gatheringId,
                    Degree = degree,
                }
                ), discussion);
            }
            else return;

            List<long> appreciating = storeSentry.ExecuteRead(ctx =>
                ctx.UserRelationships.
                Where(l => !exclusionList.Contains(l.OtherId) && l.SelfId == userId && l.Type == UserRelationship.UserRelationshipType.Follow).
                Select(l => l.OtherId).
                ToList());

            List<long> appreciatingMe = storeSentry.ExecuteRead(ctx =>
                ctx.UserRelationships.
                Where(l => !exclusionList.Contains(l.SelfId) && l.OtherId == userId && l.Type == UserRelationship.UserRelationshipType.Follow).
                Select(l => l.SelfId).
                ToList());

            List<long> companions = appreciating.Intersect(appreciatingMe).ToList();

            foreach (long companion in companions)
            {
                PropagateClearance(companion, gatheringId, degree - 1, companions.Union(exclusionList).Append(userId).ToList(), discussion);
            }
        }

        private void UpdateClearance(long gatheringId, int previousDegreeOfPrivacy, int newDegreeOfPrivacy)
        {
            if (previousDegreeOfPrivacy < newDegreeOfPrivacy)
            {
                List<long> edgeUsers = storeSentry.ExecuteRead(ctx =>
                    ctx.GuestClearances.
                    Where(c => c.GatheringId == gatheringId && c.Degree == previousDegreeOfPrivacy).
                    Select(c => c.UserId).
                    ToList());

                Discussion discussion = storeSentry.BeginDiscussion();
                foreach (long user in edgeUsers)
                {
                    PropagateClearance(user, gatheringId, newDegreeOfPrivacy - previousDegreeOfPrivacy, new(edgeUsers), discussion);
                }
                storeSentry.EndDiscussion(discussion);
            }
            else if (previousDegreeOfPrivacy > newDegreeOfPrivacy)
            {
                storeSentry.ExecuteWrite(ctx =>
                    ctx.GuestClearances.
                    Where(c => c.GatheringId == gatheringId && c.Degree > newDegreeOfPrivacy).
                    ExecuteDelete());
            }
            else
            {
                return;
            }
        }

        public async Task<CoreGathering> CreateGatheringAsync(long hostId, string title, string description, DateTimeOffset startTime, double latitude, double longitude, string friendlyLocation, int groupMinimum, int groupMaximum, CharacterShard character, double Radius, bool isDynamic, int degreeOfPrivacy, DateTimeOffset timeOfCreation)
        {
            Gathering toCreate = new()
            {
                Title = title,
                Description = description,
                StartTime = startTime,
                HostId = hostId,
                Location = new CoordinateFactory().Create(longitude, latitude),
                FriendlyLocation = friendlyLocation,
                GroupMinimum = groupMinimum,
                GroupMaximum = groupMaximum,
                Radius = Radius,
                IsDynamic = isDynamic,
                Extroversion = character.Extraversion,
                Athleticisme = character.Athleticism,
                Openness = character.Openness,
                Chaos = character.Chaoticness,
                Competitiveness = character.Competitiveness,
                Industriousness = character.Industriousness,
                NightOwl = character.NightOwl,
                DegreeOfPrivacy = degreeOfPrivacy,
                TimeOfCreation = timeOfCreation
            };

            await storeSentry.ExecuteWriteAsync(ctx => ctx.Gatherings.Add(toCreate));

            GatheringLink hostLink = new() 
            { 
                UserId = hostId,
                GatheringId = toCreate.Id,
                Type = GatheringBond.Guest,      
                Time = DateTimeOffset.UtcNow
            };
            
            await SetUserStateAsync(hostId, toCreate.Id, GatheringBond.Guest, DateTimeOffset.UtcNow);

            if (degreeOfPrivacy < 3)
            {
                Discussion discussion = storeSentry.BeginDiscussion();
                PropagateClearance(hostId, toCreate.Id, degreeOfPrivacy, new(), discussion);
                storeSentry.EndDiscussion(discussion);
            }

            return new CoreGathering
                (
                   toCreate.Id,
                   hostId,
                   toCreate.Title,
                   toCreate.Description,
                   toCreate.StartTime,
                   toCreate.Location.Y,
                   toCreate.Location.X,
                   toCreate.FriendlyLocation,
                   toCreate.EndTime,
                   toCreate.State,
                   toCreate.GroupMinimum,
                   toCreate.GroupMaximum,
                   new CharacterShard(
                   toCreate.Age,
                   toCreate.Extroversion,
                   toCreate.Athleticisme,
                   toCreate.Chaos,
                   toCreate.Competitiveness,
                   toCreate.Industriousness,
                   toCreate.NightOwl,
                   toCreate.Openness),
                   toCreate.Radius,
                   toCreate.IsDynamic,
                   toCreate.SoftDeleted,
                   toCreate.NumberOfGuests,
                   toCreate.DegreeOfPrivacy,
                   toCreate.Visibility,
                   toCreate.TimeOfCreation,
                   toCreate.Decay
                   );
        }

        public async Task SoftDeleteAsync(long id)
        {
            await storeSentry.ExecuteWriteAsync(ctx =>
                ctx.Notifications.
                Where(n => n.GatheringId == id).
                ExecuteUpdateAsync(setter => setter.SetProperty(s => s.SoftDeleted, true)));

            List<long> toDelete = await storeSentry.ExecuteReadAsync(ctx => 
                                    ctx.GatheringChats.
                                    Where(c => c.GatheringId == id).
                                    Select(c => c.Id).
                                    ToListAsync());

            await storeSentry.ExecuteWriteAsync(ctx =>
                ctx.ChatLinks.
                Where(l => toDelete.Contains(l.ConversationId)).
                ExecuteUpdateAsync(setter => setter.SetProperty(s => s.SoftDeleted, true)));

            await storeSentry.ExecuteWriteAsync(ctx =>
               ctx.Messages.
               Where(m => toDelete.Contains(m.ConversationId)).
               ExecuteUpdateAsync(setter => setter.SetProperty(s => s.SoftDeleted, true)));

            await storeSentry.ExecuteWriteAsync(ctx =>
               ctx.GatheringChats.
               Where(n => n.GatheringId == id).
               ExecuteUpdateAsync(setter => setter.SetProperty(s => s.SoftDeleted, true)));

            await storeSentry.ExecuteWriteAsync(ctx =>
                ctx.GatheringLinks.
                Where(l => l.GatheringId == id).
                ExecuteUpdate(setter => setter.SetProperty(l => l.SoftDeleted, true)));

            await storeSentry.ExecuteWriteAsync(ctx =>
                ctx.GatheringReports.
                Where(r => r.GatheringId == id).
                ExecuteUpdate(setter => setter.SetProperty(r => r.SoftDeleted, true)));

            await storeSentry.ExecuteWriteAsync(ctx =>
                ctx.GuestClearances.
                Where(c => c.GatheringId == id).
                ExecuteUpdate(setter => setter.SetProperty(c => c.SoftDeleted, true)));

            await storeSentry.ExecuteWriteAsync(ctx =>
                ctx.Gatherings.
                Where(e => e.Id == id).
                ExecuteUpdate(setter => setter.SetProperty(e => e.SoftDeleted, true)));
        }

        public async Task HardDeleteAsync(long id)
        {
            await storeSentry.ExecuteWriteAsync(ctx =>
                ctx.Notifications.
                Where(l => l.GatheringId == id).
                ExecuteDeleteAsync());

            List<long> toDelete = await storeSentry.ExecuteReadAsync(ctx =>
                                    ctx.GatheringChats.
                                    Where(c => c.GatheringId == id).
                                    Select(c => c.Id).
                                    ToListAsync());

            await storeSentry.ExecuteWriteAsync(ctx =>
                ctx.ChatLinks.
                Where(l => toDelete.Contains(l.ConversationId)).
                ExecuteDeleteAsync());

            await storeSentry.ExecuteWriteAsync(ctx =>
                ctx.Messages.
                Where(M => toDelete.Contains(M.ConversationId)).
                ExecuteDeleteAsync());

            await storeSentry.ExecuteWriteAsync(ctx =>
                ctx.GatheringChats.
                Where(l => l.GatheringId == id).
                ExecuteDeleteAsync());

            await storeSentry.ExecuteWriteAsync(ctx =>
                ctx.GatheringLinks.
                Where(l => l.GatheringId == id).
                ExecuteDeleteAsync());

            await storeSentry.ExecuteWriteAsync(ctx =>
                ctx.GatheringReports.
                Where(r => r.GatheringId == id).
                ExecuteDeleteAsync());

            await storeSentry.ExecuteWriteAsync(ctx =>
                ctx.UserReports.
                Where(r => r.GatheringId == id).
                ExecuteUpdate(setter => setter.SetProperty(r => r.GatheringId, (long?)null)));

            await storeSentry.ExecuteWriteAsync(ctx =>
                ctx.GatheringInviteMessages.
                Where(r => r.GatheringId == id).
                ExecuteUpdate(setter => setter.SetProperty(r => r.GatheringId, (long?)null)));

            await storeSentry.ExecuteWriteAsync(ctx =>
                ctx.GatheringShareMessages.
                Where(r => r.GatheringId == id).
                ExecuteUpdate(setter => setter.SetProperty(r => r.GatheringId, (long?)null)));

            await storeSentry.ExecuteWriteAsync(ctx =>
                ctx.GuestClearances.
                Where(c => c.GatheringId == id).
                ExecuteDeleteAsync());

            await storeSentry.ExecuteWriteAsync(ctx =>
                ctx.Gatherings.
                Remove(new Gathering { Id = id }));
        }

        public async Task<List<CoreGathering>> FindOngoingGatheringsForUserAsync(long id, DateTimeOffset currentTime) 
        {
            return await storeSentry.ExecuteReadAsync(ctx =>
            ctx.GatheringLinks
            .Where(l => l.UserId == id && l.Type == GatheringBond.Guest)
            .Join(
                ctx.Gatherings.Where(g => g.State == GatheringState.Alive && g.StartTime <= currentTime),
                l => l.GatheringId,
                e => e.Id,
                (l, e) => new CoreGathering
                (
                    e.Id,
                    e.HostId ?? 0,
                    e.Title,
                    e.Description,
                    e.StartTime,
                    e.Location.Y,
                    e.Location.X,
                    e.FriendlyLocation,
                    e.EndTime,
                    e.State,
                    e.GroupMinimum,
                    e.GroupMaximum,
                    new CharacterShard(
                        e.Age,
                        e.Extroversion,
                        e.Athleticisme,
                        e.Chaos,
                        e.Competitiveness,
                        e.Industriousness,
                        e.NightOwl,
                        e.Openness),
                    e.Radius,
                    e.IsDynamic,
                    e.SoftDeleted,
                    e.NumberOfGuests,
                    e.DegreeOfPrivacy,
                    e.Visibility,
                    e.TimeOfCreation,
                    e.Decay
                )
            ).ToListAsync());
        }
        public async Task<List<CoreGathering>> FindUpcomingGatheringsForUserAsync(long id, DateTimeOffset currentTime) 
        {
            return await storeSentry.ExecuteReadAsync(ctx =>
            ctx.GatheringLinks
            .Where(l => l.UserId == id && l.Type == GatheringBond.Guest)
            .Join(
                ctx.Gatherings.Where(g => g.State == GatheringState.Alive && g.StartTime > currentTime),
                l => l.GatheringId,
                e => e.Id,
                (l, e) => new CoreGathering
                (
                    e.Id,
                    e.HostId ?? 0,
                    e.Title,
                    e.Description,
                    e.StartTime,
                    e.Location.Y,
                    e.Location.X,
                    e.FriendlyLocation,
                    e.EndTime,
                    e.State,
                    e.GroupMinimum,
                    e.GroupMaximum,
                    new CharacterShard(
                        e.Age,
                        e.Extroversion,
                        e.Athleticisme,
                        e.Chaos,
                        e.Competitiveness,
                        e.Industriousness,
                        e.NightOwl,
                        e.Openness),
                    e.Radius,
                    e.IsDynamic,
                    e.SoftDeleted,
                    e.NumberOfGuests,
                    e.DegreeOfPrivacy,
                    e.Visibility,
                    e.TimeOfCreation,
                    e.Decay
                )
            ).ToListAsync());
        }

        public async Task<List<CoreGathering>> FindPastGatheringsForUserAsync(long id)
        {
            return await storeSentry.ExecuteReadAsync(ctx =>
            ctx.GatheringLinks
            .Where(l => l.UserId == id && l.Type == GatheringBond.Guest)
            .Join(
                ctx.Gatherings.Where(g => g.State == GatheringState.Cancelled || g.State == GatheringState.Ended),
                l => l.GatheringId,
                e => e.Id,
                (l, e) => new CoreGathering
                (
                    e.Id,
                    e.HostId ?? 0,
                    e.Title,
                    e.Description,
                    e.StartTime,
                    e.Location.Y,
                    e.Location.X,
                    e.FriendlyLocation,
                    e.EndTime,
                    e.State,
                    e.GroupMinimum,
                    e.GroupMaximum,
                    new CharacterShard(
                        e.Age,
                        e.Extroversion,
                        e.Athleticisme,
                        e.Chaos,
                        e.Competitiveness,
                        e.Industriousness,
                        e.NightOwl,
                        e.Openness),
                    e.Radius,
                    e.IsDynamic,
                    e.SoftDeleted,
                    e.NumberOfGuests,
                    e.DegreeOfPrivacy,
                    e.Visibility,
                    e.TimeOfCreation,
                    e.Decay
                )
            ).ToListAsync());
        }
        public async Task<CoreGathering> FindGatheringAsync(long id)
        {
            return await storeSentry.ExecuteReadAsync(ctx => 
            ctx.Gatherings.
            Where(e => e.Id == id).
            Select(
                e => new CoreGathering
                (
                    e.Id,
                    e.HostId ?? 0,
                    e.Title,
                    e.Description,
                    e.StartTime,
                    e.Location.Y,
                    e.Location.X,
                    e.FriendlyLocation,
                    e.EndTime,
                    e.State,
                    e.GroupMinimum,
                    e.GroupMaximum,
                    new CharacterShard(
                        e.Age,
                        e.Extroversion,
                        e.Athleticisme,
                        e.Chaos,
                        e.Competitiveness,
                        e.Industriousness,
                        e.NightOwl,
                        e.Openness),
                    e.Radius,
                    e.IsDynamic,
                    e.SoftDeleted,
                    e.NumberOfGuests,
                    e.DegreeOfPrivacy,
                    e.Visibility,
                    e.TimeOfCreation,
                    e.Decay
                )
            ).SingleAsync());
        }
        public async Task<List<CoreGathering>> FindGatheringsAsync(double latitude, double longitude, double distance)
        {
            Point currentLocation = new CoordinateFactory().Create(longitude, latitude);
            DateTimeOffset today = DateTimeOffset.UtcNow;
            DateTimeOffset inTwoWeeks = today.AddDays(14);

            return await storeSentry.ExecuteReadAsync(ctx => 
                ctx.Gatherings.
                Where(e => e.Location.Distance(currentLocation) <= distance && e.State == GatheringState.Alive && e.Visibility == GatheringVisibility.Visible).
                Join(
                    ctx.Users, 
                    e => e.HostId, 
                    u => u.Id, 
                    (e,u) => new CoreGathering
                    (
                        e.Id,
                        u.Id,
                        e.Title,
                        e.Description,
                        e.StartTime,
                        e.Location.Y,
                        e.Location.X,
                        e.FriendlyLocation,
                        e.EndTime,
                        e.State,
                        e.GroupMinimum,
                        e.GroupMaximum,
                        new CharacterShard(
                        e.Age,
                        e.Extroversion,
                        e.Athleticisme,
                        e.Chaos,
                        e.Competitiveness,
                        e.Industriousness,
                        e.NightOwl,
                        e.Openness),
                        e.Radius,
                        e.IsDynamic,
                        e.SoftDeleted,
                        e.NumberOfGuests,
                        e.DegreeOfPrivacy,
                        e.Visibility,
                        e.TimeOfCreation,
                        e.Decay
                   )).ToListAsync());
        }     
        public async Task<List<CoreUser>> GetGuestListAsync(long id)
        {
            return await storeSentry.ExecuteReadAsync(ctx =>
            ctx.GatheringLinks.
            Where(l => l.GatheringId == id && l.Type == GatheringBond.Guest).
            Join(
                ctx.Users,
                l => l.UserId,
                u => u.Id,
                (_,u) => new CoreUser(u.Id,
                      u.PhoneNumber,
                      u.Email,
                      u.Name,
                      u.CompanionshipCode,
                      u.DateOfBirth,
                      u.IsPhoneConfirmed,
                      u.IsEmailConfirmed,
                      u.SoftDeleted,
                      u.SecurityStamp,
                      u.LockoutDate,
                      u.AccessTries,
                      u.AccountStatus,
                      u.JoinDate,
                      u.Reputation,
                      new CharacterShard(
                          u.Age,
                          u.Extroversion,
                          u.Athleticisme,
                          u.Chaos,
                          u.Competitiveness,
                          u.Industriousness,
                          u.NightOwl,
                          u.Openness),
                      u.TimeOfUserAgreement,
                      u.NotificationId
                  )
            )
            .ToListAsync());
        }    
        public async Task DeleteUserStateAsync(long userId, long gatheringId) 
        { 
            List<GatheringLink> links = await storeSentry.ExecuteReadAsync(ctx => 
                ctx.GatheringLinks.
                Where(l => l.UserId == userId && l.GatheringId == gatheringId).
                ToListAsync());

            Discussion currentDiscussion = storeSentry.BeginDiscussion();
            foreach (GatheringLink link in links) 
            {
                storeSentry.DiscussWrite(ctx => ctx.GatheringLinks.Remove(link), currentDiscussion);

                switch (link.Type)
                {
                    case GatheringBond.Guest:
                        int num = await storeSentry.ExecuteReadAsync(ctx =>
                            ctx.Gatherings.
                            Where(e => e.Id == gatheringId).
                            Select(e => e.NumberOfGuests).
                            SingleAsync());

                        Gathering e = new() { Id = gatheringId, NumberOfGuests = num - 1 };
                        storeSentry.DiscussWrite(ctx => ctx.Gatherings.Attach(e), currentDiscussion);
                        storeSentry.DiscussWrite(ctx => ctx.Entry(e).Property(nameof(e.NumberOfGuests)).IsModified = true, currentDiscussion);
                        break;
                    case GatheringBond.Arrived:
                        User arrivedUser = new() { Id = userId, CurrentGathering = null };

                        storeSentry.DiscussWrite(ctx => ctx.Users.Attach(arrivedUser), currentDiscussion);
                        storeSentry.DiscussWrite(ctx => ctx.Entry(arrivedUser).Property(nameof(arrivedUser.CurrentGathering)).IsModified = true, currentDiscussion);
                        break;
                    case GatheringBond.Left:
                        break;
                    case GatheringBond.Kicked:
                        break;
                }
            }
            await storeSentry.EndDiscussionAsync(currentDiscussion);
        }
        public async Task UpdateGatheringAsync(long id, List<(string Property, object Value)> edits)
        {
            Discussion currentDiscussion = storeSentry.BeginDiscussion();

            Gathering e = new() { Id = id };
            storeSentry.DiscussWrite(ctx => ctx.Gatherings.Attach(e), currentDiscussion);

            foreach ((string Property, object Value) in edits)
            {
                switch (Property)
                {
                    case nameof(CoreGathering.Title):
                        e.Title = (string)Value;
                        break;
                    case nameof(CoreGathering.Description):
                        e.Description = (string)Value;
                        break;
                    case nameof(CoreGathering.State):
                        e.State = (GatheringState)Value;
                        break;
                    case nameof(CoreGathering.StartTime):
                        e.StartTime = (DateTimeOffset)Value;
                        break;
                    case "Location":
                        var Location = ((double, double))Value;
                        e.Location = new CoordinateFactory().Create(Location.Item2, Location.Item1);
                        break;
                    case nameof(CoreGathering.FriendlyLocation):
                        e.FriendlyLocation = (string)Value;
                        break;
                    case nameof(CoreGathering.Radius):
                        e.Radius = (double)Value;
                        break;
                    case nameof(CoreGathering.IsDynamic):
                        e.IsDynamic = (bool)Value;
                        break;
                    case nameof(CoreGathering.GroupMinimum):
                        e.GroupMinimum = (int)Value;
                        break;
                    case nameof(CoreGathering.GroupMaximum):
                        e.GroupMaximum = (int)Value;
                        break;
                    case nameof(CoreGathering.DegreeOfPrivacy):
                        int prev = await storeSentry.ExecuteReadAsync(ctx => 
                                        ctx.Gatherings.
                                        Where(g => g.Id == id).
                                        Select(g => g.DegreeOfPrivacy).
                                        SingleAsync());

                        e.DegreeOfPrivacy = (int)Value;
                        UpdateClearance(id, prev, e.DegreeOfPrivacy);
                        break;
                    case nameof(CoreGathering.Decay):
                        e.Decay = (float)Value;
                        break;
                    default:
                        throw new InvalidInputException($"Property named \"{Property}\" can not be updated using this method.");
                }
                storeSentry.DiscussWrite(ctx => ctx.Entry(e).Property(Property).IsModified = true, currentDiscussion);
            }
            await storeSentry.EndDiscussionAsync(currentDiscussion);
        }   
        public async Task<List<(long UserId, DateTimeOffset Joined, DateTimeOffset? Left)>> GetGuestHistoryAsync(long id)
        {
            var times = await storeSentry.ExecuteReadAsync(ctx =>
            ctx.GatheringLinks.
            Where(l => l.GatheringId == id && (l.Type == GatheringBond.Arrived || l.Type == GatheringBond.Left)).
            Join(
                ctx.Users,
                l => l.UserId,
                u => u.Id,
                (l,u) => new { u.Id, u.Name, l.Time, l.Type }
                ).
            ToListAsync());

            Dictionary<long,List<(string, DateTimeOffset, GatheringBond)>> history = new();
            foreach (var item in times)
            {
                if (!history.ContainsKey(item.Id)) history.Add(item.Id, new());
                history[item.Id].Add((item.Name, item.Time, item.Type));               
            }

            List<(long UserId, DateTimeOffset Joined, DateTimeOffset? Left)> toReturn = new();
            long userId;
            string userName;
            DateTimeOffset arrivalTime;
            DateTimeOffset? departureTime;
            foreach ( var entry in history)
            {
                entry.Value.Sort((x,y) => DateTimeOffset.Compare(x.Item2, y.Item2));

                (string firstName, DateTimeOffset firstTime, _) = entry.Value.First();
                (_, DateTimeOffset lastTime, GatheringBond lastState) = entry.Value.Last();

                userId = entry.Key;
                userName = firstName;
                arrivalTime = firstTime;
                if (lastState == GatheringBond.Left) departureTime = lastTime;
                else departureTime = null;
                
                toReturn.Add((userId, arrivalTime, departureTime));
            }

            return toReturn;
        }      
        public async Task<List<CoreGathering>> FindGatheringsByUserAsync(long userId)
        {
           return await storeSentry.ExecuteReadAsync(ctx =>
           ctx.Gatherings.
           Where(e => e.HostId == userId).
           Join(
               ctx.Users,
               e => e.HostId,
               u => u.Id,
               (e, u) => new CoreGathering(
                    e.Id, 
                    u.Id, 
                    e.Title, 
                    e.Description, 
                    e.StartTime, 
                    e.Location.Y, 
                    e.Location.X, 
                    e.FriendlyLocation,
                    e.EndTime, 
                    e.State, 
                    e.GroupMinimum, 
                    e.GroupMaximum, 
                    new CharacterShard(
                        e.Extroversion,
                        e.Athleticisme, 
                        e.Chaos, 
                        e.Competitiveness,
                        e.Industriousness, 
                        e.NightOwl,
                        e.Openness,
                        e.Age
                        ), 
                    e.Radius, 
                    e.IsDynamic,
                    e.SoftDeleted,
                    e.NumberOfGuests,
                    e.DegreeOfPrivacy,
                    e.Visibility,
                    e.TimeOfCreation,
                    e.Decay
                    )).ToListAsync());
        }      
        public async Task<GatheringBond?> GetUserStateAsync(long userId, long gatheringId)
        {
            var states = await storeSentry.ExecuteReadAsync(ctx =>
            ctx.GatheringLinks.
            Where(l => l.UserId == userId && l.GatheringId == gatheringId).
            Select(l => new { l.Type, l.Time }).
            ToListAsync());

            states.Sort((x, y) => DateTimeOffset.Compare(x.Time, y.Time));

            return states.Last().Type;
        }
        public async Task SetUserStateAsync(long userId, long gatheringId, GatheringBond userState, DateTimeOffset time)
        {
            GatheringLink toAdd = new() 
            { 
                UserId = userId, 
                GatheringId = gatheringId, 
                Type = userState, 
                Time = time
            };

            long id = await storeSentry.ExecuteReadAsync(ctx => 
                       ctx.GatheringLinks.
                       Where(l => l.UserId == userId && l.GatheringId == gatheringId && l.Type == userState).
                       Select(l => l.Id).
                       SingleOrDefaultAsync());

            if (id != 0)
            {
                toAdd.Id = id;
            }

            Discussion currentDiscussion = storeSentry.BeginDiscussion();
            storeSentry.DiscussWrite(ctx => ctx.GatheringLinks.Update(toAdd), currentDiscussion);

            switch (userState)
            {
                case GatheringBond.Guest:
                    int num = await storeSentry.ExecuteReadAsync(ctx => 
                        ctx.Gatherings.
                        Where(e => e.Id == gatheringId).
                        Select(e => e.NumberOfGuests).
                        SingleAsync());

                    Gathering e = new() { Id = gatheringId, NumberOfGuests = num + 1};
                    storeSentry.DiscussWrite(ctx => ctx.Gatherings.Attach(e), currentDiscussion);
                    storeSentry.DiscussWrite(ctx => ctx.Entry(e).Property(nameof(e.NumberOfGuests)).IsModified = true, currentDiscussion);
                    break;
                case GatheringBond.Arrived:
                    User arrivedUser = new() { Id = userId, CurrentGathering = gatheringId };

                    storeSentry.DiscussWrite(ctx => ctx.Users.Attach(arrivedUser), currentDiscussion);
                    storeSentry.DiscussWrite(ctx => ctx.Entry(arrivedUser).Property(nameof(arrivedUser.CurrentGathering)).IsModified = true, currentDiscussion);
                    break;
                case GatheringBond.Left:
                    User leftUser = new() { Id = userId, CurrentGathering = null };

                    storeSentry.DiscussWrite(ctx => ctx.Users.Attach(leftUser), currentDiscussion);
                    storeSentry.DiscussWrite(ctx => ctx.Entry(leftUser).Property(nameof(leftUser.CurrentGathering)).IsModified = true, currentDiscussion);
                    break;
                case GatheringBond.Kicked:
                    User kickedUser = new() { Id = userId, CurrentGathering = null };

                    storeSentry.DiscussWrite(ctx => ctx.Users.Attach(kickedUser), currentDiscussion);
                    storeSentry.DiscussWrite(ctx => ctx.Entry(kickedUser).Property(nameof(kickedUser.CurrentGathering)).IsModified = true, currentDiscussion);
                    break;
            }
            await storeSentry.EndDiscussionAsync(currentDiscussion);
        }
        public async Task<List<(long UserId, GatheringBond State)>> GetAllUsersAsync(long gatheringId)
        {
            var users = await storeSentry.ExecuteReadAsync(ctx =>
            ctx.GatheringLinks.
            Where(l => l.GatheringId == gatheringId).
            Join(
                ctx.Users,
                l => l.UserId,
                u => u.Id,
                (l, u) => new { u.Id, u.Name, l.Time, l.Type }
                ).
            ToListAsync());

            Dictionary<long, List<(string, DateTimeOffset, GatheringBond)>> history = new();
            foreach (var item in users)
            {
                if (!history.ContainsKey(item.Id)) history.Add(item.Id, new());
                history[item.Id].Add((item.Name, item.Time, item.Type));
            }

            List<(long UserId, GatheringBond State)> toReturn = new();
            long userId;
            string userName;
            GatheringBond userState;
            foreach (var entry in history)
            {
                entry.Value.Sort((x, y) => DateTimeOffset.Compare(x.Item2, y.Item2));

                userId = entry.Key;
                userName = entry.Value.Last().Item1;
                userState = entry.Value.Last().Item3;

                toReturn.Add((userId, userState));
            }
            return toReturn;
        }
        public async Task TerminateGatheringAsync(long id, DateTimeOffset time)
        {
            List<long> guests = await storeSentry.ExecuteReadAsync(ctx => 
                ctx.GatheringLinks.
                Where(l => l.GatheringId == id && l.Type == GatheringBond.Guest).
                Select(l => l.UserId).
                ToListAsync());

            List<Task> tasks = new();
            foreach (long guest in guests)
            {
                tasks.Add(SetUserStateAsync(guest, id, GatheringBond.Left, time));
            }
            await Task.WhenAll(tasks);       

            Discussion currentDiscussion = storeSentry.BeginDiscussion();

            Gathering e = new() { Id = id, EndTime = DateTimeOffset.UtcNow, State = GatheringState.Ended };
            storeSentry.DiscussWrite(ctx => ctx.Gatherings.Attach(e), currentDiscussion);
            storeSentry.DiscussWrite(ctx => ctx.Entry(e).Property(nameof(e.EndTime)).IsModified = true, currentDiscussion);
            storeSentry.DiscussWrite(ctx => ctx.Entry(e).Property(nameof(e.State)).IsModified = true, currentDiscussion);

            await storeSentry.EndDiscussionAsync(currentDiscussion);         
        }
        public async Task<bool> UserIsAuthorizedGuest(long userId, long gatheringId)
        {
            long id = await storeSentry.ExecuteReadAsync(ctx => 
                ctx.GuestClearances.
                Where(c => c.GatheringId == gatheringId && c.UserId == userId).
                Select(c => c.Id).
                SingleOrDefaultAsync());
            
            return id != 0;
        }
        public async Task<List<long>> GetAuthorizedGuests(long gatheringId)
        {
            return await storeSentry.ExecuteReadAsync(ctx => 
                ctx.GuestClearances.
                Where(c => c.GatheringId == gatheringId).
                Select(c => c.UserId).
                ToListAsync());
        }
        public async Task AddGuestAuthorization(long gatheringId, long userId)
        {
            GuestClearance toAdd = new()
            {
                GatheringId = gatheringId,
                UserId = userId,
                Degree = 0,
            };

            await storeSentry.ExecuteWriteAsync(ctx => ctx.GuestClearances.Add(toAdd)); 
        }

        public async  Task CancelGatheringAsync(long gatheringId)
        {
            Discussion currentDiscussion = storeSentry.BeginDiscussion();

            Gathering e = new() { Id = gatheringId, State = GatheringState.Cancelled };
            storeSentry.DiscussWrite(ctx => ctx.Gatherings.Attach(e), currentDiscussion);
            storeSentry.DiscussWrite(ctx => ctx.Entry(e).Property(e => e.State).IsModified = true, currentDiscussion);
 
            await storeSentry.EndDiscussionAsync(currentDiscussion);
        }
    }
}

