using System;
using System.Collections.Generic;
using System.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using PowerSite.Actions;
using PowerSite.DataModel;
using RazorEngine;
using RazorEngine.Compilation;
using RazorEngine.Compilation.ReferenceResolver;
using RazorEngine.Configuration;
using RazorEngine.Templating;

namespace PowerSite.Builtin.Renderers
{
	[Export(typeof(IRenderer))]
	[ExportMetadata("Extension", "cshtml")]
	public class RazorRenderer : IRenderer
	{
		public string Render(string siteKey, NamedContentBase template, dynamic data)
		{
			string result = null;

			var config = new TemplateServiceConfiguration { TemplateManager = new TemplateManager(siteKey) };
			config.Namespaces.Add("System.IO");
			config.Namespaces.Add("RazorEngine.Text");
            
            // The templates being used are inherently trusted, not isolated
            config.DisableTempFileLocking = true;
            config.CachingProvider = new DefaultCachingProvider(t => { });

            using (var service = RazorEngineService.Create(config))
			{
				Engine.Razor = service;

				try
				{
					var pso = data as PSObject;
					if (pso != null)
					{
						data = pso.BaseObject;
					}
				    var key = config.TemplateManager.GetKey(template.SourcePath, ResolveType.Layout, null);
				    using (var writer = new StringWriter())
				    {
				        service.RunCompile(key, writer, null, data, viewBag: null);
				        result = writer.ToString();
				    }
				}
				catch (TemplateCompilationException e)
				{
					Console.Error.WriteLine(e.Message);
                    throw;
				}
			}

			return result;
		}

		private class TemplateManager : ITemplateManager
		{
			private readonly string _siteRootPath;

			public TemplateManager(string siteRootPath)
			{
				_siteRootPath = siteRootPath;
			}

		    public ITemplateSource Resolve(ITemplateKey key)
		    {
                var id = (Path.GetFileNameWithoutExtension(key.Name) ?? "default").ToLowerInvariant().Slugify();

                var extension = (Path.GetExtension(key.Name) ?? ".cshtml").TrimStart('.');

                try
                {
                    var layout = Site.ForPath(_siteRootPath).Theme.Layouts[id];

                    if (!layout.Extension.Equals(string.IsNullOrEmpty(extension) ? "cshtml" : extension, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.Error.WriteLine("Resolved wrong. Layout {0} is not razor. Extension: {1}", key.Name, extension);
                        return new LoadedTemplateSource("Raw(@Model)", null);
                    }
                    return new LoadedTemplateSource(layout.RawContent, layout.SourcePath);
                }
                catch (KeyNotFoundException)
                {
                    Console.Error.WriteLine("Layout not found: Layout {0} extension {1}", key.Name, extension);
                    return new LoadedTemplateSource("Raw(@Model)", null);
                }
            }

		    public ITemplateKey GetKey(string name, ResolveType resolveType, ITemplateKey context)
		    {
                return new NameOnlyTemplateKey(name, resolveType, context);
            }

		    public void AddDynamic(ITemplateKey key, ITemplateSource source)
		    {
                // Disable dynamic templates (for now)
                // Convenience methods (Compile and RunCompile) with a TemplateSource don't work
                throw new NotImplementedException("dynamic templates are not supported!");
            }
		}
	}

	public class PowerSiteTemplate<T> : TemplateBase<T>
	{
		public Site Site { get; set; }

	}

}
