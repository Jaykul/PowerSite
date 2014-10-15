using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using PowerSite.DataModel;

namespace PowerSite.Builtin
{
	public class Theme
	{
		private readonly string themeRoot;

		public Theme(string root, string themeName)
		{
			Name = themeName;
			themeRoot = Path.Combine(root, themeName);
		}

		public void Load()
		{
			if (!Directory.Exists(themeRoot))
			{
				throw new DirectoryNotFoundException(string.Format("Cannot find the theme '{0}' in the site themes '{1}'", Name, themeRoot));
			}

			Layouts = IdentityCollection<LayoutFile>.Create(Directory.EnumerateFiles(themeRoot).Select(f => new LayoutFile(f)));
		}

		public IdentityCollection<LayoutFile> Layouts { get; set; }

		public string Name { get; private set; }
	}
}
