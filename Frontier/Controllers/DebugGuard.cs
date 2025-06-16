using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Frontier.Manifests;
using Core.Boundaries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

/*
namespace Frontier.Controllers
{
	[Route("debug")]
	public class DebugGuard : AbstractGuard
	{
		#region Initialisation

		private IDebugOperations debug;

		public DebugGuard(GuardBox box, UserManager<CoreUser> aspUserManager, IDebugOperations debugOperations) :
			base(box, aspUserManager)
		{
			debug = debugOperations;
		}

		#endregion

		#region Actions

		[AllowAnonymous]
		[HttpPost("seed")]
		public async Task<IActionResult> Seed([FromBody] SeedManifest seed)
		{
			// Verify parameters
			if (seed == null || !ModelState.IsValid)
			{ return MissingInformationError(); }

			return await Execute(async () =>
			{
				log.LogError("Begining database seeding.\nDraining the database.");

				await debug.SeedDatabaseAsync();

				List<CoreUser> seedUsers = new();
				List<GatheringShard> seedGatherings = new();
				List<SnapshotShard> seedSnapshots = new();

                log.LogError("Adding users..");

                foreach (var user in seed.Users)
				{
					await accounts.CreateUserAsync(user.PhoneNumber, user.Email, user.Name, user.DateOfBirth);

					var coreUser = await accounts.GetCoreUserAsync(user.PhoneNumber);

                    seedUsers.Add(coreUser);

					// Confirm user phone
					var code = await userManager.GenerateChangePhoneNumberTokenAsync(coreUser, user.PhoneNumber);
					await userManager.VerifyChangePhoneNumberTokenAsync(coreUser, TokenOptions.DefaultPhoneProvider, code);

					// Confirm user email
					var token = await userManager.GenerateEmailConfirmationTokenAsync(coreUser);
					await userManager.ConfirmEmailAsync(coreUser, token);
				}

                log.LogError("Making companions..");

                for (int i = 0; i < seed.Appreciations.Count; i++)
				{
					var tuple = seed.Appreciations[i];
					var self = tuple[0]; var other = tuple[1];

					await nests.AppreciateUserAsync(seedUsers[self - 1].Id, seedUsers[other - 1].Id);
				}

                log.LogError("Making enemies..");

                for (int i = 0; i < seed.Blocks.Count; i++)
                {
                    var tuple = seed.Blocks[i];
                    var self = tuple[0]; var other = tuple[1];

                    await nests.BlockUserAsync(seedUsers[self - 1].Id, seedUsers[other - 1].Id);
                }

                log.LogError("Creating gatherings..");

                foreach (var gathering in seed.Gatherings)
                {
                    var host = await accounts.GetCoreUserAsync(seedUsers[(int)gathering.Host.Id - 1].PhoneNumber);

                    // Move host to proposed gathering location
                    // await accounts.UpdateUserLocationAsync(host.Id, gathering.Latitude, gathering.Longitude);

                    seedGatherings.Add(await gatherings.CreateGatheringAsync(host.Id,
                        gathering.Name, gathering.Description,
                        gathering.StartTime,
						gathering.Latitude, gathering.Longitude, gathering.FriendlyLocation,
                        gathering.Radius, gathering.IsDynamic,
                        gathering.GroupMinimum, gathering.GroupMaximum, new System.IO.MemoryStream { }));
                }

                log.LogError("Joining users to gatherings..");

                for (int i = 0; i < seed.Attendance.Count; i++)
                {
                    var tuple = seed.Attendance[i];
                    var who = tuple[0]; var gathering = tuple[1];

                    // Move user to gathering location
                    await accounts.UpdateUserLocationAsync(seedUsers[who - 1].Id, seedGatherings[gathering - 1].Latitude, seedGatherings[gathering - 1].Longitude);

                    // Attempt to join the gathering
                    await gatherings.JoinGatheringAsync(seedUsers[who - 1].Id, seedGatherings[gathering - 1].Id);
                }

				log.LogError("Forging Snapshots...");

				for (int i = 0; i < seed.Snapshots.Count; i++)
				{
                    var tuple = seed.Snapshots[i];
                    var who = tuple[0]; var gathering = tuple[1];

					seedSnapshots.Add(await snapshots.AddSnapshotAsync(seedUsers[who - 1].Id, seedGatherings[gathering - 1].Id, new() { }));
				}

				log.LogError("Acclaiming Snapshots...");

				for (int i = 0; i < seed.Acclaims.Count; i++)
                {
                    var tuple = seed.Acclaims[i];
                    var who = tuple[0]; var snapshot = tuple[1];

					await snapshots.AcclaimSnapshotAsync(seedUsers[who - 1].Id, seedSnapshots[snapshot - 1].Id, SnapshotAcclaim.Acclaim);
				}
            });
		}

		#endregion
	}
}
*/