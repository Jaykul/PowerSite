using System.IO;

namespace PowerSite.DataModel
{
	public class LayoutFile : NamedContentBase
	{
		public LayoutFile(string path) : base(path, preloadContent: true)
		{
		}
	}
}