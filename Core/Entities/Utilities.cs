using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Azure;

namespace Core.Entities
{
    public readonly struct GeoLocation
    {
        #region Olive Branches

        public static GeoLocation None
            => new() { Latitude = 0, Longitude = 0, Exists = false };

        public static Distance DistanceBetween(GeoLocation locationAlpha, GeoLocation locationBeta)
        {
            locationAlpha.ThrowIfNone();
            locationBeta.ThrowIfNone();

            double sdlat = Math.Sin((locationBeta.LatitudeInRadians - locationAlpha.LatitudeInRadians) / 2);
            double sdlon = Math.Sin((locationBeta.LongitudeInRadians - locationAlpha.LongitudeInRadians) / 2);
            double q = sdlat * sdlat + Math.Cos(locationAlpha.LatitudeInRadians) * Math.Cos(locationBeta.LatitudeInRadians) * sdlon * sdlon;
            double d = 2 * EarthRadius.Kilometres * Math.Asin(Math.Sqrt(q));

            Distance distance = new() { Kilometres = d };

            return distance;
        }

        public static bool AreInRange(GeoLocation locationAlpha, GeoLocation locationBeta, Distance range)
        {
            range.ThrowIfNone();

            Distance distance = DistanceBetween(locationAlpha, locationBeta);

            return distance.Kilometres <= range.Kilometres;
        }

        public static Distance EarthRadius => new() { Kilometres = 6371 };

        #endregion

        #region Variables

        public double Latitude { get; init; } = 0;
        public double Longitude { get; init; } = 0;

        public double LatitudeInRadians => (Math.PI / 180) * Latitude;
        public double LongitudeInRadians => (Math.PI / 180) * Longitude;

        public bool Exists { get; init; } = true;

        #endregion

        public GeoLocation()
        { }

        public void ThrowIfNone()
        {
            if (!Exists)
            { throw new UndefinedBehaviourException("GeoLocation does not exist."); }
        }

        #region Dissimilation

        public override bool Equals(object obj)
        {
            return obj is GeoLocation other &&
                Exists == other.Exists &&
                Latitude == other.Latitude &&
                Longitude == other.Longitude;
        }

        public override int GetHashCode()
        {
            return (Latitude + Longitude).GetHashCode();
        }

        public static bool operator ==(GeoLocation left, GeoLocation right)
            => left.Equals(right);

        public static bool operator !=(GeoLocation left, GeoLocation right)
            => !(left == right);

        #endregion
    }


    public struct Distance
    {
        #region Olive Branches

        public static Distance None
            => new() { Metres = 0, Exists = false };

        #endregion

        #region Variables

        public double Kilometres
        {
            get => Metres / 1000d;
            set => Metres = value * 1000d;
        }
        public double Metres { get; set; } = 0;

        public bool Exists { get; init; } = true;

        #endregion

        public Distance()
        { }

        public void ThrowIfNone()
        {
            if (!Exists)
            { throw new UndefinedBehaviourException("Distance does not exist."); }
        }

        #region Dissimilation

        public static bool operator ==(Distance a, Distance b)
            => a.Exists == b.Exists && a.Metres == b.Metres;

        public static bool operator !=(Distance a, Distance b)
            => a.Exists == b.Exists && a.Metres != b.Metres;

        public static bool operator <(Distance a, Distance b)
            => a.Exists == b.Exists && a.Metres < b.Metres;

        public static bool operator >(Distance a, Distance b)
            => a.Exists == b.Exists && a.Metres > b.Metres;

        public override bool Equals(object obj)
        {
            return obj is Distance other &&
                Exists == other.Exists &&
                this == other;
        }

        public override int GetHashCode()
        {
            return Metres.GetHashCode();
        }

        #endregion
    }


    public struct Synced<T>
    {
        class SyncData
        {
		    public T cache;
            public bool isSynced;

            public Func<Task<T>> function;
            public Task<T> task;
        }

        #region Variables

        private SyncData sync;

        #endregion

        #region Initialisation

        public Synced(Func<Task<T>> synchronisationFunction)
        {
            sync = new()
            {
                cache = default,
                isSynced = false,
                function = synchronisationFunction,
                task = null,
            };
        }

        public Synced(T value)
        {
            sync = new()
            {
                cache = value,
                isSynced = true,
                function = null,
                task = null,
            };
        }

		#endregion

		#region Actions

        public async Task<T> Value()
        {
            if (!sync.isSynced)
            { await Sync(); }

            return sync.cache;
        }

        public async Task Sync()
        {
            if (sync.function == null)
            { throw new UndefinedBehaviourException($"Cannot Sync {typeof(T)} without synchronising function."); }

            lock (sync.function)
            {
                if (sync.task == null || sync.task.IsCompleted)
                {
                    sync.isSynced = false;
                    sync.task = sync.function.Invoke();
                }
            }

            sync.cache = await sync.task;
            sync.isSynced = true;
        }

        public void Set(T value)
        {
            sync.cache = value;
            sync.isSynced = true;
		}

        #endregion

        #region Dissimilation

        public TaskAwaiter<T> GetAwaiter()
            => Value().GetAwaiter();

		public static T operator +(Synced<T> a, T b)
            => ((dynamic)a.sync.cache) + ((dynamic)b);

		public static T operator -(Synced<T> a, T b)
            => ((dynamic)a.sync.cache) - ((dynamic)b);

		#endregion
	}

    public struct PagedSync<T>
    {

        #region Variables

        private Func<int, Task<T>> pagedSyncFunction;
        private ConcurrentDictionary<int, Synced<T>> syncs;

        #endregion

        #region Initialisation

        public PagedSync(Func<int, Task<T>> synchronisationFunction)
        {
            pagedSyncFunction = synchronisationFunction;
            syncs = new();
        }

        #endregion

        #region Actions

        public async Task<T> Value(int page)
        {
            var syncFunction = pagedSyncFunction;
            Synced<T> factory(int p) => new(() => syncFunction.Invoke(p));

            var pageSync = syncs.GetOrAdd(page, factory);

            return await pageSync.Value();
        }

        public async Task Sync(int page)
        {
            var syncFunction = pagedSyncFunction;
            Synced<T> factory(int p) => new(() => syncFunction.Invoke(p));

            var pageSync = syncs.GetOrAdd(page, factory);

            await pageSync.Sync();
        }

        public void Set(int page, T value)
        {
            Synced<T> addFactory(int p) => new(value);
            Synced<T> updateFactory(int p, Synced<T> s) { s.Set(value); return s; }

            var pageSync = syncs.AddOrUpdate(page, addFactory, updateFactory);
        }

        #endregion
    }
}
