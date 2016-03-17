using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using PowerSite.Actions;
using PowerSite.DataModel;

namespace PowerSite
{
	public class EngineHost : IReadOnlyDictionary<string, IRenderer>
	{
		private IEnumerable<Lazy<IRenderer, IExtension>> _renderingEngines;
		private readonly Dictionary<string, IRenderer> _renderEngines = new Dictionary<string, IRenderer>();
		private CompositionContainer _container;

		public readonly List<Exception> Errors = new List<Exception>();
		public bool? HasException = null;

		protected EngineHost(string siteRootPath)
		{
			Utility.AssertDirectoryExists(siteRootPath);
			siteRootPath = Path.GetFullPath(siteRootPath.ToLowerInvariant());
			if (Site.ActiveSites.ContainsKey(siteRootPath))
			{
				throw new InvalidOperationException("Can only have one rendering transaction active on a site at a time. Ensure the previous rendering transaction was disposed before creating this one.");
			}

			SiteRootPath = siteRootPath;
		
			InitializeEngineCatalog();

		}

		[ImportMany]
		protected IEnumerable<Lazy<IRenderer, IExtension>> RenderingEngines
		{
			get { return _renderingEngines; }
			set
			{
				_renderingEngines = value;
				_renderingEngines.ToList().ForEach(ex => _renderEngines.Add(ex.Metadata.Extension, ex.Value));
			}
		}

		public string SiteRootPath { get; private set; }

		/// <summary>
		/// Initializes the plugin catalog.
		/// </summary>
		protected void InitializeEngineCatalog()
		{
			var catalog = new AggregateCatalog();
			//Adds all the parts found in the same assembly as the Program class
			var assembly = typeof(Site).Assembly;
			catalog.Catalogs.Add(new AssemblyCatalog(assembly));

			if (!string.IsNullOrEmpty(SiteRootPath) && Directory.Exists(SiteRootPath))
			{
				// WriteVerbose("Site root: " + siteRootPath);
				var pluginRoot = Path.Combine(SiteRootPath, "Plugins");

				if (Directory.Exists(pluginRoot))
				{
					catalog.Catalogs.Add(new DirectoryCatalog(pluginRoot));
				}
				// Otherwise: WriteVerbose("No Plugins directory found in site root: " + siteRootPath);
			}
			// Otherwise: WriteWarning("Could not determine module root");

			//Create the CompositionContainer with the parts in the catalog
			_container = new CompositionContainer(catalog);

			try
			{
				_container.ComposeParts(this);
				HasException = true;
			}
			catch (CompositionException compositionException)
			{
				Errors.Add(compositionException);
				HasException = false;
			}
		}

		public IEnumerator<KeyValuePair<string, IRenderer>> GetEnumerator()
		{
			return _renderEngines.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public int Count { get; private set; }
		public bool ContainsKey(string key)
		{
			return _renderEngines.ContainsKey(key);
		}

		public bool TryGetValue(string key, out IRenderer value)
		{
			return _renderEngines.TryGetValue(key, out value);
		}

		public IRenderer this[string key]
		{
			get { return _renderEngines[key]; }
		}

		public IEnumerable<string> Keys { get { return _renderEngines.Keys; } }

		public IEnumerable<IRenderer> Values { get { return _renderEngines.Values; } }
		public int PageSize { get; set; }


		public void Render(NamedContentBase doc)
		{
			doc.RenderedContent = Render(doc, doc);
		}

		public string Render(NamedContentBase layout, dynamic model)
		{
			return _renderEngines[layout.Extension].Render(SiteRootPath, layout, model);
		}

		protected void Render(NamedContentBase layout, dynamic model, string outputPath)
		{
			Utility.CreateDirectoryIfNecessary(Path.GetDirectoryName(outputPath));
			File.WriteAllText(outputPath, Render(layout, model));
		}

		protected void RenderIndex(LayoutFile layout, IEnumerable<Document> posts, string basePath)
		{
			int skip = 0;
			IEnumerable<Document> page;
			while ((page = posts.Skip(skip).Take(PageSize)).Any())
			{
				var outputPath = Path.Combine(basePath, skip == 0 ? "index.html" : string.Format("index{0}.html", skip));
				Render(layout, page, outputPath);
				skip += PageSize;
			}
		}
	}
}