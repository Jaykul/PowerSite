using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerSite.Builtin;
using PowerSite.DataModel;

namespace PowerSite
{
	public class RenderingState : IDisposable
	{
		private bool _disposed = false;
		
		[ImportMany]
		protected IEnumerable<Lazy<IRenderer, IExtension>> Engines;
		
		public RenderingState(IPoshSite site)
		{
			if (RenderingState.Current != null)
			{
				throw new InvalidOperationException("Can only have one rendering transaction active at a time. Ensure the previous rendering transaction was disposed before creating this one.");
			}

			this.Site = site;
			this.Layouts = site.Theme.Layouts;

			RenderingState.Current = this;
		}

		public static RenderingState Current { get; set; }

		public IPoshSite Site { get; set; }
	
	
		//public IEnumerable<DocumentFile> Documents { get; set; }

		//public IEnumerable<StaticFile> Files { get; set; }

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

	public class RenderingEngine
	{
	}
}
