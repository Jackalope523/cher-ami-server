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
