using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using PowerSite.Actions;
using PowerSite.DataModel;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;

namespace PowerSite.Builtin.Renderers
{
	[Export(typeof(IRenderer))]
	[ExportMetadata("Extension", "cshtml")]
	public class RazorRenderer : IRenderer
	{
		public string Render(string siteKey, string template, dynamic data)
		{
			string result = null;

			var config = new TemplateServiceConfiguration { Resolver = new TemplateResolver(siteKey) };
			config.Namespaces.Add("System.IO");
			config.Namespaces.Add("RazorEngine.Text");
		
			using (var service = new TemplateService(config))
			{
				Razor.SetTemplateService(service);

				var cacheName = template.GetHashCode().ToString(CultureInfo.InvariantCulture);

				try
				{
					var pso = data as PSObject;
					if (pso != null)
					{
						data = pso.BaseObject;
					}
					result = Razor.Parse(template, data, cacheName);
				}
				catch (TemplateCompilationException e)
				{
					foreach (var error in e.Errors)
					{
						Console.Error.WriteLine(error);
					}
				}
			}

			return result;
		}

		private class TemplateResolver : ITemplateResolver
		{
			private readonly string _siteRootPath;

			public TemplateResolver(string siteRootPath)
			{
				_siteRootPath = siteRootPath;
			}
		
			public string Resolve(string name)
			{
				var id = (Path.GetFileNameWithoutExtension(name) ?? "default").ToLowerInvariant().Slugify();
			
				var extension = (Path.GetExtension(name) ?? ".cshtml").TrimStart('.');

				try
				{
					var layout = Site.ForPath(_siteRootPath).Theme.Layouts[id];

					if (!layout.Extension.Equals(String.IsNullOrEmpty(extension) ? "cshtml" : extension, StringComparison.OrdinalIgnoreCase))
					{
						Console.Error.WriteLine("Resolved wrong. Layout {0} is not razor. Extension: {1}", name, extension);
						return "Raw(@Model)";
					}
					return layout.RawContent;
				}
				catch (KeyNotFoundException knf)
				{
					Console.Error.WriteLine("Layout not found: Layout {0} extension {1}", name, extension);
					return "Raw(@Model)";
				}
			}
		}
	}

	public class PowerSiteTemplate<T> : TemplateBase<T>
	{
		public Site Site { get; set; }
	
	}
}
