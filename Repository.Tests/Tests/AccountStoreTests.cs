using Microsoft.EntityFrameworkCore;
using Core.Boundaries;
using Xunit.Abstractions;
using NetTopologySuite.Geometries;


namespace Repository.Tests
{
    [Collection("Database Collection")]
    public class AccountStoreTests : IDisposable
    {
        private readonly ITestOutputHelper _testOutputHelper;

        private static readonly EFCoreSentry sentry = new(Harbor.Flag.Development);
        private static readonly EFCoreAccountStore store = new(Harbor.Flag.Development);  
        
        private User subject;

        public AccountStoreTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;

            subject = new UserFactory().Create();
            sentry.ExecuteWrite(ctx => ctx.Users.Add(subject));
        }
        public void Dispose()
        {
            sentry.ExecuteWrite(ctx => ctx.Users.ExecuteDelete());
        }

        [Fact]
        public async Task CreateUserAsync_SUCCESS()
        {
            sentry.ExecuteWrite(ctx => ctx.Users.ExecuteDelete());
       
            await store.CreateUserAsync(
                subject.PhoneNumber,
                subject.Email,
                subject.NormalisedEmail,
                subject.Name,
                subject.DateOfBirth,
                subject.JoinDate,
                new CharacterShard(
                    subject.Age,
                    subject.Extroversion,
                    subject.Athleticisme,
                    subject.Chaos,
                    subject.Competitiveness,
                    subject.Industriousness,
                    subject.NightOwl,
                    subject.Openness
                    ),
                subject.NotificationId);

            User created = sentry.ExecuteRead(ctx => ctx.Users.Single());

            Assert.NotNull(created);

            Assert.Equal(subject.PhoneNumber, created.PhoneNumber);
            Assert.Equal(subject.Email, created.Email);
            Assert.Equal(subject.NormalisedEmail, created.NormalisedEmail);
            Assert.Equal(subject.Name, created.Name);
            Assert.Equal(subject.DateOfBirth, created.DateOfBirth);
            Assert.Equal(subject.JoinDate, created.JoinDate);
            Assert.Equal(User.DefaultReputation, created.Reputation);
            Assert.Equal(User.DefaultIsPhoneConfirmed, created.IsPhoneConfirmed);
            Assert.Equal(User.DefaultIsEmailConfirmed, created.IsEmailConfirmed);
            Assert.Equal(User.DefaultSecurityStamp, created.SecurityStamp);
            Assert.Equal(User.DefaultLockoutDate, created.LockoutDate);
            Assert.Equal(User.DefaultAccessTries, created.AccessTries);
            Assert.Equal(User.DefaultAccountStatus, created.AccountStatus);
            Assert.Equal(subject.SoftDeleted, created.SoftDeleted);
            Assert.Equal(subject.Extroversion, created.Extroversion);
            Assert.Equal(subject.Athleticisme, created.Athleticisme);
            Assert.Equal(subject.Chaos, created.Chaos);
            Assert.Equal(subject.Competitiveness, created.Competitiveness);
            Assert.Equal(subject.Industriousness, created.Industriousness);
            Assert.Equal(subject.NightOwl, created.NightOwl);
            Assert.Equal(subject.Openness, created.Openness);

            Assert.Equal(User.DefaultHaunt, created.Haunt);
            Assert.Equal(User.DefaultHauntWeight, created.HauntWeight);
            Assert.Equal(User.DefaultHauntRadius, created.HauntRadius);
            Assert.Equal(User.DefaultCurrentLocation, created.CurrentLocation);
            Assert.Equal(User.DefaultCurrentRadius, created.CurrentRadius);
        }       

        [Fact]
        public async Task FindUserByIdAsync_SUCCESS()
        {
            CoreUser found = await store.FindUserByIdAsync(subject.Id);

            Assert.NotNull(found);

            Assert.Equal(subject.Id, found.Id);
            Assert.Equal(subject.PhoneNumber, found.PhoneNumber);
            Assert.Equal(subject.Email, found.Email);
            Assert.Equal(subject.Name, found.Name);
            Assert.Equal(subject.DateOfBirth, found.DateOfBirth);
            Assert.Equal(subject.IsPhoneConfirmed, found.IsPhoneConfirmed);
            Assert.Equal(subject.IsEmailConfirmed, found.IsEmailConfirmed);
            Assert.Equal(subject.SecurityStamp, found.SecurityStamp);
            Assert.Equal(subject.LockoutDate, found.LockoutDate);
            Assert.Equal(subject.AccessTries, found.AccessTries);
            Assert.Equal(subject.AccountStatus, found.AccountStatus);
            Assert.Equal(subject.JoinDate, found.JoinDate);
            Assert.Equal(subject.Reputation, found.Reputation);
            Assert.Equal(subject.Extroversion, found.Character.Extraversion);
            Assert.Equal(subject.Athleticisme, found.Character.Athleticism);
            Assert.Equal(subject.Chaos, found.Character.Chaoticness);
            Assert.Equal(subject.Competitiveness, found.Character.Competitiveness);
            Assert.Equal(subject.Industriousness, found.Character.Industriousness);
            Assert.Equal(subject.NightOwl, found.Character.NightOwl);
            Assert.Equal(subject.Openness, found.Character.Openness);
        }
        [Fact]
        public async Task FindUserByIdAsync_UserNotFound()
        {
            Func<Task> action = async () => await store.FindUserByIdAsync(long.MaxValue);

            await Assert.ThrowsAsync<DatabaseReadException>(action);
        }     

        [Fact]
        public async Task FindUserByPhoneNumberAsync_SUCCESS()
        { 
            CoreUser found = await store.FindUserByPhoneNumberAsync(subject.PhoneNumber);

            Assert.NotNull(found);

            Assert.Equal(subject.Id, found.Id);
            Assert.Equal(subject.PhoneNumber, found.PhoneNumber);
            Assert.Equal(subject.Email, found.Email);
            Assert.Equal(subject.Name, found.Name);
            Assert.Equal(subject.DateOfBirth, found.DateOfBirth);
            Assert.Equal(subject.IsPhoneConfirmed, found.IsPhoneConfirmed);
            Assert.Equal(subject.IsEmailConfirmed, found.IsEmailConfirmed);
            Assert.Equal(subject.SecurityStamp, found.SecurityStamp);
            Assert.Equal(subject.LockoutDate, found.LockoutDate);
            Assert.Equal(subject.AccessTries, found.AccessTries);
            Assert.Equal(subject.AccountStatus, found.AccountStatus);
            Assert.Equal(subject.JoinDate, found.JoinDate);
            Assert.Equal(subject.Reputation, found.Reputation);
            Assert.Equal(subject.Extroversion, found.Character.Extraversion);
            Assert.Equal(subject.Athleticisme, found.Character.Athleticism);
            Assert.Equal(subject.Chaos, found.Character.Chaoticness);
            Assert.Equal(subject.Competitiveness, found.Character.Competitiveness);
            Assert.Equal(subject.Industriousness, found.Character.Industriousness);
            Assert.Equal(subject.NightOwl, found.Character.NightOwl);
            Assert.Equal(subject.Openness, found.Character.Openness);
        }
        [Fact]
        public async Task FindUserByPhoneNumberAsync_UserNotFound()
        {
            Func<Task> action = async () => await store.FindUserByPhoneNumberAsync("");
            await Assert.ThrowsAsync<DatabaseReadException>(action);
        }

        [Fact]
        public async Task FindUserByEmailAsync_SUCCESS()
        {
            CoreUser found = await store.FindUserByEmailAsync(subject.Email);

            Assert.NotNull(found);

            Assert.Equal(subject.Id, found.Id);
            Assert.Equal(subject.PhoneNumber, found.PhoneNumber);
            Assert.Equal(subject.Email, found.Email);
            Assert.Equal(subject.Name, found.Name);
            Assert.Equal(subject.DateOfBirth, found.DateOfBirth);
            Assert.Equal(subject.IsPhoneConfirmed, found.IsPhoneConfirmed);
            Assert.Equal(subject.IsEmailConfirmed, found.IsEmailConfirmed);
            Assert.Equal(subject.SecurityStamp, found.SecurityStamp);
            Assert.Equal(subject.LockoutDate, found.LockoutDate);
            Assert.Equal(subject.AccessTries, found.AccessTries);
            Assert.Equal(subject.AccountStatus, found.AccountStatus);
            Assert.Equal(subject.JoinDate, found.JoinDate);
            Assert.Equal(subject.Reputation, found.Reputation);
            Assert.Equal(subject.Extroversion, found.Character.Extraversion);
            Assert.Equal(subject.Athleticisme, found.Character.Athleticism);
            Assert.Equal(subject.Chaos, found.Character.Chaoticness);
            Assert.Equal(subject.Competitiveness, found.Character.Competitiveness);
            Assert.Equal(subject.Industriousness, found.Character.Industriousness);
            Assert.Equal(subject.NightOwl, found.Character.NightOwl);
            Assert.Equal(subject.Openness, found.Character.Openness);
        }
        [Fact]
        public async Task FindUserByEmailAsync_UserNotFound()
        {
            Func<Task> action = async () => await store.FindUserByEmailAsync("");

            await Assert.ThrowsAsync<DatabaseReadException>(action);     
        }

        [Fact]
        public async Task UpdateUserAsync_PhoneNumber()
        {
            string newPhoneNumber = "111-111-1111";

            List<(string, object)> updates = new List<(string, object)>();
            updates.Add(("PhoneNumber", newPhoneNumber));

            await store.UpdateUserAsync(subject.Id, updates);

            User updated = await sentry.ExecuteReadAsync(ctx => ctx.Users.FirstAsync());
           
            Assert.NotNull(updated);

            Assert.Equal(subject.Id, updated.Id);
            Assert.NotEqual(subject.PhoneNumber, updated.PhoneNumber);
            Assert.Equal(newPhoneNumber, updated.PhoneNumber);
            Assert.Equal(subject.Email, updated.Email);
            Assert.Equal(subject.NormalisedEmail, updated.NormalisedEmail);
            Assert.Equal(subject.Name, updated.Name);
            Assert.Equal(subject.DateOfBirth, updated.DateOfBirth);
            Assert.Equal(subject.JoinDate, updated.JoinDate);
            Assert.Equal(subject.Reputation, updated.Reputation);
            Assert.Equal(subject.IsPhoneConfirmed, updated.IsPhoneConfirmed);
            Assert.Equal(subject.IsEmailConfirmed, updated.IsEmailConfirmed);
            Assert.Equal(subject.SecurityStamp, updated.SecurityStamp);
            Assert.Equal(subject.LockoutDate, updated.LockoutDate);
            Assert.Equal(subject.AccessTries, updated.AccessTries);
            Assert.Equal(subject.AccountStatus, updated.AccountStatus);

            Assert.Equal(subject.Extroversion, updated.Extroversion);
            Assert.Equal(subject.Athleticisme, updated.Athleticisme);
            Assert.Equal(subject.Chaos, updated.Chaos);
            Assert.Equal(subject.Competitiveness, updated.Competitiveness);
            Assert.Equal(subject.Industriousness, updated.Industriousness);
            Assert.Equal(subject.NightOwl, updated.NightOwl);
            Assert.Equal(subject.Openness, updated.Openness);

            Assert.Equal(subject.Haunt, updated.Haunt);
            Assert.Equal(subject.HauntWeight, updated.HauntWeight);
            Assert.Equal(subject.HauntRadius, updated.HauntRadius);
            Assert.Equal(subject.CurrentLocation, updated.CurrentLocation);
            Assert.Equal(subject.CurrentRadius, updated.CurrentRadius);
        }        
        [Fact]
        public async Task UpdateUserAsync_Email()
        {
            string newEmail = "newEmail@test.com";

            List<(string, object)> updates = new List<(string, object)>();
            updates.Add(("Email", newEmail));

            await store.UpdateUserAsync(subject.Id, updates);

            User updated = await sentry.ExecuteReadAsync(ctx => ctx.Users.FirstAsync());
          
            Assert.NotNull(updated);

            Assert.Equal(subject.Id, updated.Id);
            Assert.Equal(subject.PhoneNumber, updated.PhoneNumber);
            Assert.NotEqual(subject.Email, updated.Email);
            Assert.Equal(newEmail, updated.Email);
            Assert.Equal(subject.NormalisedEmail, updated.NormalisedEmail);
            Assert.Equal(subject.Name, updated.Name);
            Assert.Equal(subject.DateOfBirth, updated.DateOfBirth);
            Assert.Equal(subject.JoinDate, updated.JoinDate);
            Assert.Equal(subject.Reputation, updated.Reputation);
            Assert.Equal(subject.IsPhoneConfirmed, updated.IsPhoneConfirmed);
            Assert.Equal(subject.IsEmailConfirmed, updated.IsEmailConfirmed);
            Assert.Equal(subject.SecurityStamp, updated.SecurityStamp);
            Assert.Equal(subject.LockoutDate, updated.LockoutDate);
            Assert.Equal(subject.AccessTries, updated.AccessTries);
            Assert.Equal(subject.AccountStatus, updated.AccountStatus);

            Assert.Equal(subject.Extroversion, updated.Extroversion);
            Assert.Equal(subject.Athleticisme, updated.Athleticisme);
            Assert.Equal(subject.Chaos, updated.Chaos);
            Assert.Equal(subject.Competitiveness, updated.Competitiveness);
            Assert.Equal(subject.Industriousness, updated.Industriousness);
            Assert.Equal(subject.NightOwl, updated.NightOwl);
            Assert.Equal(subject.Openness, updated.Openness);

            Assert.Equal(subject.Haunt, updated.Haunt);
            Assert.Equal(subject.HauntWeight, updated.HauntWeight);
            Assert.Equal(subject.HauntRadius, updated.HauntRadius);
            Assert.Equal(subject.CurrentLocation, updated.CurrentLocation);
            Assert.Equal(subject.CurrentRadius, updated.CurrentRadius);

        }
        [Fact]
        public async Task UpdateUserAsync_NormalisedEmailS()
        {
            string newNormalizedEmail = "newEmail@test.com";

            List<(string, object)> updates = new List<(string, object)>();
            updates.Add(("NormalisedEmail", newNormalizedEmail));

            await store.UpdateUserAsync(subject.Id, updates);

            User updated = await sentry.ExecuteReadAsync(ctx => ctx.Users.FirstAsync());
            
            Assert.NotNull(updated);

            Assert.Equal(subject.Id, updated.Id);
            Assert.Equal(subject.PhoneNumber, updated.PhoneNumber);
            Assert.Equal(subject.Email, updated.Email);
            Assert.NotEqual(subject.NormalisedEmail, updated.NormalisedEmail);
            Assert.Equal(newNormalizedEmail, updated.NormalisedEmail);
            Assert.Equal(subject.Name, updated.Name);
            Assert.Equal(subject.DateOfBirth, updated.DateOfBirth);
            Assert.Equal(subject.JoinDate, updated.JoinDate);
            Assert.Equal(subject.Reputation, updated.Reputation);
            Assert.Equal(subject.IsPhoneConfirmed, updated.IsPhoneConfirmed);
            Assert.Equal(subject.IsEmailConfirmed, updated.IsEmailConfirmed);
            Assert.Equal(subject.SecurityStamp, updated.SecurityStamp);
            Assert.Equal(subject.LockoutDate, updated.LockoutDate);
            Assert.Equal(subject.AccessTries, updated.AccessTries);
            Assert.Equal(subject.AccountStatus, updated.AccountStatus);

            Assert.Equal(subject.Extroversion, updated.Extroversion);
            Assert.Equal(subject.Athleticisme, updated.Athleticisme);
            Assert.Equal(subject.Chaos, updated.Chaos);
            Assert.Equal(subject.Competitiveness, updated.Competitiveness);
            Assert.Equal(subject.Industriousness, updated.Industriousness);
            Assert.Equal(subject.NightOwl, updated.NightOwl);
            Assert.Equal(subject.Openness, updated.Openness);

            Assert.Equal(subject.Haunt, updated.Haunt);
            Assert.Equal(subject.HauntWeight, updated.HauntWeight);
            Assert.Equal(subject.HauntRadius, updated.HauntRadius);
            Assert.Equal(subject.CurrentLocation, updated.CurrentLocation);
            Assert.Equal(subject.CurrentRadius, updated.CurrentRadius);
        }
        [Fact]
        public async Task UpdateUserAsync_Name()
        {
            string newName = "USER";

            List<(string, object)> updates = new List<(string, object)>();
            updates.Add(("Name", newName));

            await store.UpdateUserAsync(subject.Id, updates);

            User updated = await sentry.ExecuteReadAsync(ctx => ctx.Users.FirstAsync());
            
            Assert.NotNull(updated);

            Assert.Equal(subject.Id, updated.Id);
            Assert.Equal(subject.PhoneNumber, updated.PhoneNumber);
            Assert.Equal(subject.Email, updated.Email);
            Assert.Equal(subject.NormalisedEmail, updated.NormalisedEmail);
            Assert.NotEqual(subject.Name, updated.Name);
            Assert.Equal(newName, updated.Name);
            Assert.Equal(subject.DateOfBirth, updated.DateOfBirth);
            Assert.Equal(subject.JoinDate, updated.JoinDate);
            Assert.Equal(subject.Reputation, updated.Reputation);
            Assert.Equal(subject.IsPhoneConfirmed, updated.IsPhoneConfirmed);
            Assert.Equal(subject.IsEmailConfirmed, updated.IsEmailConfirmed);
            Assert.Equal(subject.SecurityStamp, updated.SecurityStamp);
            Assert.Equal(subject.LockoutDate, updated.LockoutDate);
            Assert.Equal(subject.AccessTries, updated.AccessTries);
            Assert.Equal(subject.AccountStatus, updated.AccountStatus);

            Assert.Equal(subject.Extroversion, updated.Extroversion);
            Assert.Equal(subject.Athleticisme, updated.Athleticisme);
            Assert.Equal(subject.Chaos, updated.Chaos);
            Assert.Equal(subject.Competitiveness, updated.Competitiveness);
            Assert.Equal(subject.Industriousness, updated.Industriousness);
            Assert.Equal(subject.NightOwl, updated.NightOwl);
            Assert.Equal(subject.Openness, updated.Openness);

            Assert.Equal(subject.Haunt, updated.Haunt);
            Assert.Equal(subject.HauntWeight, updated.HauntWeight);
            Assert.Equal(subject.HauntRadius, updated.HauntRadius);
            Assert.Equal(subject.CurrentLocation, updated.CurrentLocation);
            Assert.Equal(subject.CurrentRadius, updated.CurrentRadius);
        }
        [Fact]
        public async Task UpdateUserAsync_PhoneConfirmation()
        {
            List<(string, object)> updates = new List<(string, object)>();
            updates.Add(("IsPhoneConfirmed", true));

            await store.UpdateUserAsync(subject.Id, updates);

            User updated = await sentry.ExecuteReadAsync(ctx => ctx.Users.FirstAsync());

            Assert.NotNull(updated);

            Assert.Equal(subject.Id, updated.Id);
            Assert.Equal(subject.PhoneNumber, updated.PhoneNumber);
            Assert.Equal(subject.Email, updated.Email);
            Assert.Equal(subject.NormalisedEmail, updated.NormalisedEmail);
            Assert.Equal(subject.Name, updated.Name);
            Assert.Equal(subject.DateOfBirth, updated.DateOfBirth);
            Assert.Equal(subject.JoinDate, updated.JoinDate);
            Assert.Equal(subject.Reputation, updated.Reputation);
            Assert.NotEqual(subject.IsPhoneConfirmed, updated.IsPhoneConfirmed);
            Assert.True(updated.IsPhoneConfirmed);
            Assert.Equal(subject.IsEmailConfirmed, updated.IsEmailConfirmed);
            Assert.Equal(subject.SecurityStamp, updated.SecurityStamp);
            Assert.Equal(subject.LockoutDate, updated.LockoutDate);
            Assert.Equal(subject.AccessTries, updated.AccessTries);
            Assert.Equal(subject.AccountStatus, updated.AccountStatus);

            Assert.Equal(subject.Extroversion, updated.Extroversion);
            Assert.Equal(subject.Athleticisme, updated.Athleticisme);
            Assert.Equal(subject.Chaos, updated.Chaos);
            Assert.Equal(subject.Competitiveness, updated.Competitiveness);
            Assert.Equal(subject.Industriousness, updated.Industriousness);
            Assert.Equal(subject.NightOwl, updated.NightOwl);
            Assert.Equal(subject.Openness, updated.Openness);

            Assert.Equal(subject.Haunt, updated.Haunt);
            Assert.Equal(subject.HauntWeight, updated.HauntWeight);
            Assert.Equal(subject.HauntRadius, updated.HauntRadius);
            Assert.Equal(subject.CurrentLocation, updated.CurrentLocation);
            Assert.Equal(subject.CurrentRadius, updated.CurrentRadius);
        }
        [Fact]
        public async Task UpdateUserAsync_EmailConfirmation()
        {
            List<(string, object)> updates = new List<(string, object)>();
            updates.Add(("IsEmailConfirmed", true));

            await store.UpdateUserAsync(subject.Id, updates);

            User updated = await sentry.ExecuteReadAsync(ctx => ctx.Users.FirstAsync());           

            Assert.NotNull(updated);

            Assert.Equal(subject.Id, updated.Id);
            Assert.Equal(subject.PhoneNumber, updated.PhoneNumber);
            Assert.Equal(subject.Email, updated.Email);
            Assert.Equal(subject.NormalisedEmail, updated.NormalisedEmail);
            Assert.Equal(subject.Name, updated.Name);
            Assert.Equal(subject.DateOfBirth, updated.DateOfBirth);
            Assert.Equal(subject.JoinDate, updated.JoinDate);
            Assert.Equal(subject.Reputation, updated.Reputation);
            Assert.Equal(subject.IsPhoneConfirmed, updated.IsPhoneConfirmed);
            Assert.NotEqual(subject.IsEmailConfirmed, updated.IsEmailConfirmed);
            Assert.True(updated.IsEmailConfirmed);
            Assert.Equal(subject.SecurityStamp, updated.SecurityStamp);
            Assert.Equal(subject.LockoutDate, updated.LockoutDate);
            Assert.Equal(subject.AccessTries, updated.AccessTries);
            Assert.Equal(subject.AccountStatus, updated.AccountStatus);

            Assert.Equal(subject.Extroversion, updated.Extroversion);
            Assert.Equal(subject.Athleticisme, updated.Athleticisme);
            Assert.Equal(subject.Chaos, updated.Chaos);
            Assert.Equal(subject.Competitiveness, updated.Competitiveness);
            Assert.Equal(subject.Industriousness, updated.Industriousness);
            Assert.Equal(subject.NightOwl, updated.NightOwl);
            Assert.Equal(subject.Openness, updated.Openness);

            Assert.Equal(subject.Haunt, updated.Haunt);
            Assert.Equal(subject.HauntWeight, updated.HauntWeight);
            Assert.Equal(subject.HauntRadius, updated.HauntRadius);
            Assert.Equal(subject.CurrentLocation, updated.CurrentLocation);
            Assert.Equal(subject.CurrentRadius, updated.CurrentRadius);
        }
        [Fact]
        public async Task UpdateUserAsync_SecurityStamp()
        {
            string newSecurityStamp = "STAMP";

            List<(string, object)> updates = new List<(string, object)>();
            updates.Add(("SecurityStamp", newSecurityStamp));

            await store.UpdateUserAsync(subject.Id, updates);

            User updated = await sentry.ExecuteReadAsync(ctx => ctx.Users.FirstAsync());          

            Assert.NotNull(updated);

            Assert.Equal(subject.Id, updated.Id);
            Assert.Equal(subject.PhoneNumber, updated.PhoneNumber);
            Assert.Equal(subject.Email, updated.Email);
            Assert.Equal(subject.NormalisedEmail, updated.NormalisedEmail);
            Assert.Equal(subject.Name, updated.Name);
            Assert.Equal(subject.DateOfBirth, updated.DateOfBirth);
            Assert.Equal(subject.JoinDate, updated.JoinDate);
            Assert.Equal(subject.Reputation, updated.Reputation);
            Assert.Equal(subject.IsPhoneConfirmed, updated.IsPhoneConfirmed);
            Assert.Equal(subject.IsEmailConfirmed, updated.IsEmailConfirmed);
            Assert.NotEqual(subject.SecurityStamp, updated.SecurityStamp);
            Assert.Equal(newSecurityStamp, updated.SecurityStamp);
            Assert.Equal(subject.LockoutDate, updated.LockoutDate);
            Assert.Equal(subject.AccessTries, updated.AccessTries);
            Assert.Equal(subject.AccountStatus, updated.AccountStatus);

            Assert.Equal(subject.Extroversion, updated.Extroversion);
            Assert.Equal(subject.Athleticisme, updated.Athleticisme);
            Assert.Equal(subject.Chaos, updated.Chaos);
            Assert.Equal(subject.Competitiveness, updated.Competitiveness);
            Assert.Equal(subject.Industriousness, updated.Industriousness);
            Assert.Equal(subject.NightOwl, updated.NightOwl);
            Assert.Equal(subject.Openness, updated.Openness);

            Assert.Equal(subject.Haunt, updated.Haunt);
            Assert.Equal(subject.HauntWeight, updated.HauntWeight);
            Assert.Equal(subject.HauntRadius, updated.HauntRadius);
            Assert.Equal(subject.CurrentLocation, updated.CurrentLocation);
            Assert.Equal(subject.CurrentRadius, updated.CurrentRadius);
        }
        [Fact]
        public async Task UpdateUserAsync_LockoutDate()
        {
            DateTimeOffset newLockoutDate = new DateTimeOffset(new DateTime(1));

            List<(string, object)> updates = new List<(string, object)>();
            updates.Add(("LockoutDate", newLockoutDate));

            await store.UpdateUserAsync(subject.Id, updates);

            User updated = await sentry.ExecuteReadAsync(ctx => ctx.Users.FirstAsync());

            Assert.NotNull(updated);

            Assert.Equal(subject.Id, updated.Id);
            Assert.Equal(subject.PhoneNumber, updated.PhoneNumber);
            Assert.Equal(subject.Email, updated.Email);
            Assert.Equal(subject.NormalisedEmail, updated.NormalisedEmail);
            Assert.Equal(subject.Name, updated.Name);
            Assert.Equal(subject.DateOfBirth, updated.DateOfBirth);
            Assert.Equal(subject.JoinDate, updated.JoinDate);
            Assert.Equal(subject.Reputation, updated.Reputation);
            Assert.Equal(subject.IsPhoneConfirmed, updated.IsPhoneConfirmed);
            Assert.Equal(subject.IsEmailConfirmed, updated.IsEmailConfirmed);
            Assert.Equal(subject.SecurityStamp, updated.SecurityStamp);
            Assert.NotEqual(subject.LockoutDate, updated.LockoutDate);
            Assert.Equal(newLockoutDate, updated.LockoutDate);
            Assert.Equal(subject.AccessTries, updated.AccessTries);
            Assert.Equal(subject.AccountStatus, updated.AccountStatus);

            Assert.Equal(subject.Extroversion, updated.Extroversion);
            Assert.Equal(subject.Athleticisme, updated.Athleticisme);
            Assert.Equal(subject.Chaos, updated.Chaos);
            Assert.Equal(subject.Competitiveness, updated.Competitiveness);
            Assert.Equal(subject.Industriousness, updated.Industriousness);
            Assert.Equal(subject.NightOwl, updated.NightOwl);
            Assert.Equal(subject.Openness, updated.Openness);

            Assert.Equal(subject.Haunt, updated.Haunt);
            Assert.Equal(subject.HauntWeight, updated.HauntWeight);
            Assert.Equal(subject.HauntRadius, updated.HauntRadius);
            Assert.Equal(subject.CurrentLocation, updated.CurrentLocation);
            Assert.Equal(subject.CurrentRadius, updated.CurrentRadius);
        }
        [Fact]
        public async Task UpdateUserAsync_AccessTries()
        {
            int newAccessTries = 10;

            List<(string, object)> updates = new List<(string, object)>();
            updates.Add(("AccessTries", newAccessTries));

            await store.UpdateUserAsync(subject.Id, updates);

            User updated = await sentry.ExecuteReadAsync(ctx => ctx.Users.FirstAsync());
         
            Assert.NotNull(updated);

            Assert.Equal(subject.Id, updated.Id);
            Assert.Equal(subject.PhoneNumber, updated.PhoneNumber);
            Assert.Equal(subject.Email, updated.Email);
            Assert.Equal(subject.NormalisedEmail, updated.NormalisedEmail);
            Assert.Equal(subject.Name, updated.Name);
            Assert.Equal(subject.DateOfBirth, updated.DateOfBirth);
            Assert.Equal(subject.JoinDate, updated.JoinDate);
            Assert.Equal(subject.Reputation, updated.Reputation);
            Assert.Equal(subject.IsPhoneConfirmed, updated.IsPhoneConfirmed);
            Assert.Equal(subject.IsEmailConfirmed, updated.IsEmailConfirmed);
            Assert.Equal(subject.SecurityStamp, updated.SecurityStamp);
            Assert.Equal(subject.LockoutDate, updated.LockoutDate);
            Assert.NotEqual(subject.AccessTries, updated.AccessTries);
            Assert.Equal(newAccessTries, updated.AccessTries);
            Assert.Equal(subject.AccountStatus, updated.AccountStatus);

            Assert.Equal(subject.Extroversion, updated.Extroversion);
            Assert.Equal(subject.Athleticisme, updated.Athleticisme);
            Assert.Equal(subject.Chaos, updated.Chaos);
            Assert.Equal(subject.Competitiveness, updated.Competitiveness);
            Assert.Equal(subject.Industriousness, updated.Industriousness);
            Assert.Equal(subject.NightOwl, updated.NightOwl);
            Assert.Equal(subject.Openness, updated.Openness);

            Assert.Equal(subject.Haunt, updated.Haunt);
            Assert.Equal(subject.HauntWeight, updated.HauntWeight);
            Assert.Equal(subject.HauntRadius, updated.HauntRadius);
            Assert.Equal(subject.CurrentLocation, updated.CurrentLocation);
            Assert.Equal(subject.CurrentRadius, updated.CurrentRadius);
        }
        [Fact]
        public async Task UpdateUserAsync_AccountStatus()
        {
            UserAccountStatus newAccountStatus = UserAccountStatus.Blacklisted;

            List<(string, object)> updates = new List<(string, object)>();
            updates.Add(("AccountStatus", newAccountStatus));

            await store.UpdateUserAsync(subject.Id, updates);

            User updated = await sentry.ExecuteReadAsync(ctx => ctx.Users.FirstAsync());
           
            Assert.NotNull(updated);

            Assert.Equal(subject.Id, updated.Id);
            Assert.Equal(subject.PhoneNumber, updated.PhoneNumber);
            Assert.Equal(subject.Email, updated.Email);
            Assert.Equal(subject.NormalisedEmail, updated.NormalisedEmail);
            Assert.Equal(subject.Name, updated.Name);
            Assert.Equal(subject.DateOfBirth, updated.DateOfBirth);
            Assert.Equal(subject.JoinDate, updated.JoinDate);
            Assert.Equal(subject.Reputation, updated.Reputation);
            Assert.Equal(subject.IsPhoneConfirmed, updated.IsPhoneConfirmed);
            Assert.Equal(subject.IsEmailConfirmed, updated.IsEmailConfirmed);
            Assert.Equal(subject.SecurityStamp, updated.SecurityStamp);
            Assert.Equal(subject.LockoutDate, updated.LockoutDate);
            Assert.Equal(subject.AccessTries, updated.AccessTries);
            Assert.NotEqual(subject.AccountStatus, updated.AccountStatus);
            Assert.Equal(newAccountStatus, updated.AccountStatus);

            Assert.Equal(subject.Extroversion, updated.Extroversion);
            Assert.Equal(subject.Athleticisme, updated.Athleticisme);
            Assert.Equal(subject.Chaos, updated.Chaos);
            Assert.Equal(subject.Competitiveness, updated.Competitiveness);
            Assert.Equal(subject.Industriousness, updated.Industriousness);
            Assert.Equal(subject.NightOwl, updated.NightOwl);
            Assert.Equal(subject.Openness, updated.Openness);

            Assert.Equal(subject.Haunt, updated.Haunt);
            Assert.Equal(subject.HauntWeight, updated.HauntWeight);
            Assert.Equal(subject.HauntRadius, updated.HauntRadius);
            Assert.Equal(subject.CurrentLocation, updated.CurrentLocation);
            Assert.Equal(subject.CurrentRadius, updated.CurrentRadius);
        }
        [Fact]
        public async Task UpdateUserAsync_Reputation()
        {
            int newReputation = 10;

            List<(string, object)> updates = new List<(string, object)>();
            updates.Add(("Reputation", newReputation));

            await store.UpdateUserAsync(subject.Id, updates);

            User updated = await sentry.ExecuteReadAsync(ctx => ctx.Users.FirstAsync());
          
            Assert.NotNull(updated);

            Assert.Equal(subject.Id, updated.Id);
            Assert.Equal(subject.PhoneNumber, updated.PhoneNumber);
            Assert.Equal(subject.Email, updated.Email);
            Assert.Equal(subject.NormalisedEmail, updated.NormalisedEmail);
            Assert.Equal(subject.Name, updated.Name);
            Assert.Equal(subject.DateOfBirth, updated.DateOfBirth);
            Assert.Equal(subject.JoinDate, updated.JoinDate);
            Assert.NotEqual(subject.Reputation, updated.Reputation);
            Assert.Equal(newReputation, updated.Reputation);
            Assert.Equal(subject.IsPhoneConfirmed, updated.IsPhoneConfirmed);
            Assert.Equal(subject.IsEmailConfirmed, updated.IsEmailConfirmed);
            Assert.Equal(subject.SecurityStamp, updated.SecurityStamp);
            Assert.Equal(subject.LockoutDate, updated.LockoutDate);
            Assert.Equal(subject.AccessTries, updated.AccessTries);
            Assert.Equal(subject.AccountStatus, updated.AccountStatus);

            Assert.Equal(subject.Extroversion, updated.Extroversion);
            Assert.Equal(subject.Athleticisme, updated.Athleticisme);
            Assert.Equal(subject.Chaos, updated.Chaos);
            Assert.Equal(subject.Competitiveness, updated.Competitiveness);
            Assert.Equal(subject.Industriousness, updated.Industriousness);
            Assert.Equal(subject.NightOwl, updated.NightOwl);
            Assert.Equal(subject.Openness, updated.Openness);

            Assert.Equal(subject.Haunt, updated.Haunt);
            Assert.Equal(subject.HauntWeight, updated.HauntWeight);
            Assert.Equal(subject.HauntRadius, updated.HauntRadius);
            Assert.Equal(subject.CurrentLocation, updated.CurrentLocation);
            Assert.Equal(subject.CurrentRadius, updated.CurrentRadius);
        }
        /*
        [Fact]
        public async Task UpdateUserAsync_UserNotFound()
        {
            Func<Task> action = async () => await store.UpdateUserAsync(Guid.Empty, new List<(string, object)>());

            await Assert.ThrowsAsync<UserNotFoundException>(action);
        }
        */
        [Fact]
        public async Task UpdateHauntAsync_SUCCESS()
        {
            Point newHaunt = new CoordinateFactory().Create(35.712, -22.006);
            double newRadius = 35.7128;
            int newStability = 12;

            await store.UpdateHauntAsync(subject.Id, newHaunt.Y, newHaunt.X, newRadius, newStability);

            User updated = await sentry.ExecuteReadAsync(ctx => ctx.Users.FirstAsync());
            
            Assert.NotNull(updated);

            Assert.Equal(subject.Id, updated.Id);
            Assert.Equal(subject.PhoneNumber, updated.PhoneNumber);
            Assert.Equal(subject.Email, updated.Email);
            Assert.Equal(subject.NormalisedEmail, updated.NormalisedEmail);
            Assert.Equal(subject.Name, updated.Name);
            Assert.Equal(subject.DateOfBirth, updated.DateOfBirth);
            Assert.Equal(subject.JoinDate, updated.JoinDate);
            Assert.Equal(subject.Reputation, updated.Reputation);
            Assert.Equal(subject.IsPhoneConfirmed, updated.IsPhoneConfirmed);
            Assert.Equal(subject.IsEmailConfirmed, updated.IsEmailConfirmed);
            Assert.Equal(subject.SecurityStamp, updated.SecurityStamp);
            Assert.Equal(subject.LockoutDate, updated.LockoutDate);
            Assert.Equal(subject.AccessTries, updated.AccessTries);
            Assert.Equal(subject.AccountStatus, updated.AccountStatus);

            Assert.Equal(subject.Extroversion, updated.Extroversion);
            Assert.Equal(subject.Athleticisme, updated.Athleticisme);
            Assert.Equal(subject.Chaos, updated.Chaos);
            Assert.Equal(subject.Competitiveness, updated.Competitiveness);
            Assert.Equal(subject.Industriousness, updated.Industriousness);
            Assert.Equal(subject.NightOwl, updated.NightOwl);
            Assert.Equal(subject.Openness, updated.Openness);

            Assert.NotEqual(subject.Haunt, updated.Haunt);
            Assert.NotEqual(subject.HauntRadius, updated.HauntRadius);
            Assert.NotEqual(subject.HauntWeight, updated.HauntWeight);
            Assert.Equal(newHaunt, updated.Haunt);
            Assert.Equal(newRadius, updated.HauntRadius);
            Assert.Equal(newStability, updated.HauntWeight);
            Assert.Equal(subject.CurrentLocation, updated.CurrentLocation);
            Assert.Equal(subject.CurrentRadius, updated.CurrentRadius);
        }
        /*
        [Fact]
        public async Task UpdateHauntAsync_UserNotFound()
        {
            Func<Task> action = async () => await store.UpdateHauntAsync(Guid.Empty, 0.0, 0.0, 0, 0);

            await Assert.ThrowsAsync<UserNotFoundException>(action);
        }
        */
        [Fact]
        public async Task UpdateRecentLocationAsync_SUCCESS()
        {
            Point newLocation = new CoordinateFactory().Create(35.718, -22.060);
            double newRadius = 35.7128;

            await store.UpdateRecentLocationAsync(subject.Id, newLocation.Y, newLocation.X, newRadius);

            User updated = await sentry.ExecuteReadAsync(ctx => ctx.Users.FirstAsync());
         
            Assert.NotNull(updated);

            Assert.Equal(subject.Id, updated.Id);
            Assert.Equal(subject.PhoneNumber, updated.PhoneNumber);
            Assert.Equal(subject.Email, updated.Email);
            Assert.Equal(subject.NormalisedEmail, updated.NormalisedEmail);
            Assert.Equal(subject.Name, updated.Name);
            Assert.Equal(subject.DateOfBirth, updated.DateOfBirth);
            Assert.Equal(subject.JoinDate, updated.JoinDate);
            Assert.Equal(subject.Reputation, updated.Reputation);
            Assert.Equal(subject.IsPhoneConfirmed, updated.IsPhoneConfirmed);
            Assert.Equal(subject.IsEmailConfirmed, updated.IsEmailConfirmed);
            Assert.Equal(subject.SecurityStamp, updated.SecurityStamp);
            Assert.Equal(subject.LockoutDate, updated.LockoutDate);
            Assert.Equal(subject.AccessTries, updated.AccessTries);
            Assert.Equal(subject.AccountStatus, updated.AccountStatus);

            Assert.Equal(subject.Extroversion, updated.Extroversion);
            Assert.Equal(subject.Athleticisme, updated.Athleticisme);
            Assert.Equal(subject.Chaos, updated.Chaos);
            Assert.Equal(subject.Competitiveness, updated.Competitiveness);
            Assert.Equal(subject.Industriousness, updated.Industriousness);
            Assert.Equal(subject.NightOwl, updated.NightOwl);
            Assert.Equal(subject.Openness, updated.Openness);

            Assert.Equal(subject.Haunt, updated.Haunt);
            Assert.Equal(subject.HauntWeight, updated.HauntWeight);
            Assert.Equal(subject.HauntRadius, updated.HauntRadius);
            Assert.NotEqual(subject.CurrentLocation, updated.CurrentLocation);
            Assert.NotEqual(subject.CurrentRadius, updated.CurrentRadius);
            Assert.Equal(newLocation, updated.CurrentLocation);
            Assert.Equal(newRadius, updated.CurrentRadius);
        }
        /*
        [Fact]
        public async Task UpdateRecentLocationAsync_UserNotFound()
        {
            Func<Task> action = async () => await store.UpdateRecentLocationAsync(Guid.Empty, 0.0, 0.0, 0);
            await Assert.ThrowsAsync<UserNotFoundException>(action);
        }
        */
        [Fact]
        public async Task GetUserHauntAsync_SUCCESS()
        {
            (double latitude, double longitude, double radius, int stability) = await store.GetUserHauntAsync(subject.Id);

            Assert.Equal(subject.Haunt.Y, latitude);
            Assert.Equal(subject.Haunt.X, longitude);
            Assert.Equal(subject.HauntRadius, radius);
            Assert.Equal(subject.HauntWeight, stability);
        }
        [Fact]
        public async Task GetUserHauntAsync_UserNotFound()
        {
            Func<Task> action = async () => await store.GetUserHauntAsync(long.MaxValue);
            await Assert.ThrowsAsync<DatabaseReadException>(action);
        }

        [Fact]
        public async Task GetRecentUserLocationAsync_SUCCESS()
        {
            (double latitude, double longitude, double radius) = await store.GetRecentLocationAsync(subject.Id);

            Assert.Equal(subject.Haunt.Y, latitude);
            Assert.Equal(subject.Haunt.X, longitude);
            Assert.Equal(subject.CurrentRadius, radius);
        }
        [Fact]
        public async Task GetRecentUserLocationAsync_UserNotFound()
        {
            Func<Task> action = async () => await store.GetRecentLocationAsync(long.MaxValue);
            await Assert.ThrowsAsync<DatabaseReadException>(action);
        }

        [Fact]
        public async Task RerollUserCodeAsync_SUCCESS()
        {
            string code = await store.RerollUserCodeAsync(subject.Id);

            _testOutputHelper.WriteLine(code);
        }

    }
}
