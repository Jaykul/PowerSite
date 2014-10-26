using System.IO;
using System.Linq;

namespace PowerSite.DataModel
{
	public class Theme
	{
		public readonly string ThemeRoot;

		public Theme(string root, string themeName)
		{
			Name = themeName;
			ThemeRoot = Path.Combine(root, themeName);
		}

		public void Load()
		{
			if (!Directory.Exists(ThemeRoot))
			{
				throw new DirectoryNotFoundException(string.Format("Cannot find the theme '{0}' in the site themes '{1}'", Name, ThemeRoot));
			}

			Layouts = IdentityCollection<LayoutFile>.Create(Directory.EnumerateFiles(ThemeRoot).Select(f => new LayoutFile(f)));
		}

		public IdentityCollection<LayoutFile> Layouts { get; set; }

		public string Name { get; private set; }
	}
}
