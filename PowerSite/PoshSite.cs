using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerSite.Builtin;

namespace PowerSite
{
	public interface IPoshSite
	{
		string BasePath { get; set; }
		string PagesPath { get; set; }
		string PostsPath { get; set; }
		string StaticPath { get; set; }
		string ThemesPath { get; set; }
		string PluginsPath { get; set; }
		string OutputPath { get; set; }
		string CachePath { get; set; }

		Theme Theme { get; set; }
	}
}
