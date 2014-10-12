using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerSite
{
    public class RenderingState: IDisposable
    {
            private bool _disposed = false;

            public RenderingState(IDictionary<string, RenderingEngine> engines, PoshSite site)
            {
                if (RenderingState.Current != null)
                {
                    throw new InvalidOperationException("Can only have one rendering transaction active at a time. Ensure the previous rendering transaction was disposed before creating this one.");
                }

                this.Engines = engines;
                this.Site = site;
                //this.Documents = site.Documents;
                //this.Files = site.Files;
                //this.Layouts = site.Layouts;

                RenderingState.Current = this;
            }

            public static RenderingState Current { get; set; }

            public IDictionary<string, RenderingEngine> Engines { get; set; }

            public PoshSite Site { get; set; }

            //public IEnumerable<DocumentFile> Documents { get; set; }

            //public IEnumerable<StaticFile> Files { get; set; }

            //public LayoutFileCollection Layouts { get; set; }

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
