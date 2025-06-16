using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Boundaries;
using Core.Entities;

namespace Core.Controls
{
	internal class DebugDirector : AbstractDirector, IDebugOperations
	{
		#region Initialisation

		private IDebugDatabase Debug;

		public DebugDirector(DebugTerminal terminal) : base(terminal)
		{
			Debug = terminal.DebugDatabase;
		}

		#endregion

		#region Operations

		public async Task SeedDatabaseAsync()
		{
			await Debug.DrainDatabaseAsync();
		}

		#endregion

		#region Favours

		#endregion
	}
}
