using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Core.Boundaries;

namespace Core.Entities
{
	internal static class Arbiter
	{
		#region Skills

		public static void Verify<E>(bool success, E exception) where E : HollowException
		{
			if (!success)
			{ throw exception; }
		}

		public static void FailIf<E>(bool failure, E exception) where E : HollowException
		{
			if (failure)
			{ throw exception; }
		}

		#endregion
	}

	internal static class Artificer
	{
		#region Skills

		public static bool IsNull<T>(T? obj) where T : struct
		{
			return obj == null || !obj.HasValue;
		}

		public static bool IsNotNull<T>(T? obj) where T : struct
		{
			return obj != null && obj.HasValue;
		}

		public static bool AreNull<T>(params T?[] objs) where T : struct
		{
			return objs.All(obj => IsNull(obj));
		}

		public static bool AreNotNull<T>(params T?[] objs) where T : struct
		{
			return objs.All(obj => IsNotNull(obj));
		}

		#endregion
	}

	internal static class Psijic
	{
		#region Knowledge

		public static DateTimeOffset Time
			=> DateTimeOffset.UtcNow;

		public static TimeSpan OneYear
			=> TimeSpan.FromDays(365);
		public static TimeSpan OneWeek
			=> TimeSpan.FromDays(7);
		public static TimeSpan OneDay
			=> TimeSpan.FromDays(1);
		public static TimeSpan OneHour
			=> TimeSpan.FromHours(1);
		public static TimeSpan HalfHour
			=> TimeSpan.FromMinutes(30);
		public static TimeSpan FifteenMinutes
			=> TimeSpan.FromMinutes(15);
		public static TimeSpan FiveMinutes
			=> TimeSpan.FromMinutes(5);

		#endregion

		#region Skills

		public static bool HappenedBefore(DateTimeOffset time, DateTimeOffset time2)
			=> time < time2;

		public static bool After(DateTimeOffset time, DateTimeOffset time2)
			=> time > time2;

		public static bool HasYet(DateTimeOffset time)
			=> HappenedBefore(Time, time);

		public static bool HasAlready(DateTimeOffset time)
			=> HappenedBefore(time, Time);

		public static bool IsWithin(TimeSpan time, TimeSpan time2)
			=> time.Duration() < time2.Duration();

		public static async Task Once(params Task[] tasks)
			=> await Task.WhenAll(tasks);

		public static async Task<T[]> Once<T>(params Task<T>[] tasks)
			=> await Task.WhenAll(tasks);

        public static async Task<IEnumerable<T>> Once<T>(IEnumerable<Task<T>> tasks)
            => await Task.WhenAll(tasks);
        #endregion
    }

	internal static class Smithing
    {
        public static async Task<(IEnumerable<T> TrueList, IEnumerable<T> FalseList)> PartitionAsync<T>(
            this IEnumerable<T> source, Func<T, Task<bool>> predicate)
        {
            var results = await Psijic.Once(source.Select(async item => (Item: item, Result: await predicate(item))));

            List<T> trueList = new();
            List<T> falseList = new();

            foreach (var (item, result) in results)
            {
                if (result)
                { trueList.Add(item); }
                else
                { falseList.Add(item); }
            }

            return (trueList, falseList);
        }

        public static MembershipShard ToShard(this CoreMembership membership)
        {
            return new(membership.UserId, membership.Type, membership.LastSeen);
        }
    }
}
