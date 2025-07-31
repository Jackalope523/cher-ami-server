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
