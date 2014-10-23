using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using PowerSite.Actions;
using PowerSite.Builtin;

namespace PowerSite.DataModel
{
	public class RenderingState : IDisposable
	{
		private bool _disposed = false;
		
		[ImportMany]
		protected IEnumerable<Lazy<IRenderer, IExtension>> Engines;
		
		public RenderingState()
		{
			if (RenderingState.Current != null)
			{
				throw new InvalidOperationException("Can only have one rendering transaction active at a time. Ensure the previous rendering transaction was disposed before creating this one.");
			}
			RenderingState.Current = this;
		}

		private static readonly string[] Keys = new[]
		{
			"Pages","Posts","Theme","ThemesPath","Title"
		};
		public static void Initialize(PowerSiteHelper config)
		{
			Current = new RenderingState
			{
				Theme = config.Theme,
				Posts = config.Posts,
				Pages = config.Pages,
				Layouts = config.Theme.Layouts,
				Config = config
			};
			
		}
	
		public static RenderingState Current { get; private set; }

		public dynamic Config { get; private set; }

		public Theme Theme { get; set; }
		public IdentityCollection<Document> Posts { get; set; }
		public IdentityCollection<Document> Pages { get; set; }
		public IdentityCollection<LayoutFile> Layouts { get; set; }

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			if (disposing)
			{
				if (RenderingState.Current == this)
				{
					RenderingState.Current = null;
				}
			}

			_disposed = true;
		}
	}
}
