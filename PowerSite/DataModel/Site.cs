using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Security.Policy;
using System.Threading.Tasks;
using PowerSite.Actions;

namespace PowerSite.DataModel
{
	public class Site : IDisposable
	{
	
		public static Site Current { get; private set; }
	
		#region IDisposable
	
		private bool _disposed = false;
		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			if (disposing)
			{
				if (Current == this)
				{
					Current = null;
				}
			}

			_disposed = true;
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion

		private IEnumerable<Lazy<IRenderer, IExtension>> _engines;

		private readonly Dictionary<string, IRenderer> _renderEngines = new Dictionary<string, IRenderer>();
	
		[ImportMany]
		public IEnumerable<Lazy<IRenderer, IExtension>> Engines
		{
			get { return _engines; }
			set
			{
				_engines = value;
				_engines.ToList().ForEach(ex => _renderEngines.Add(ex.Metadata.Extension, ex.Value));
			}
		}
	
		private CompositionContainer _container;

		public readonly List<Exception> Errors = new List<Exception>();

		public bool? HasException = null;

		#region Initialization
		/// <summary>
		/// Initializes a new instance of the <see cref="Site"/> class.
		/// </summary>
		/// <param name="siteRootPath">The site root path.</param>
		public Site(string siteRootPath)
		{
			AssertDirectoryExists(siteRootPath);
			SiteRootPath	= siteRootPath;

			foreach (var path in new[] {"Pages", "Posts", "Static", "Themes", "Plugins", "Output", "Cache"})
			{
				Paths[path] = CreateDirectoryIfNecessary(Path.Combine(siteRootPath, path));
			}

			InitializePluginCatalog();
			Config = ImportSiteConfig();

			if (Current != null)
			{
				throw new InvalidOperationException("Can only have one rendering transaction active at a time. Ensure the previous rendering transaction was disposed before creating this one.");
			}
			Current = this;
		}
		/// <summary>
		/// Initializes the plugin catalog.
		/// </summary>
		private void InitializePluginCatalog()
		{
			var catalog = new AggregateCatalog();
			//Adds all the parts found in the same assembly as the Program class
			var assembly = typeof(BasePowerSiteCommand).Assembly;
			catalog.Catalogs.Add(new AssemblyCatalog(assembly));

			if (!string.IsNullOrEmpty(SiteRootPath) && Directory.Exists(SiteRootPath))
			{
				// WriteVerbose("Site root: " + siteRootPath);
				var pluginRoot = Path.Combine(SiteRootPath, "Plugins");

				if (Directory.Exists(pluginRoot))
				{
					catalog.Catalogs.Add(new DirectoryCatalog(pluginRoot));
				}
				else
				{
					// WriteVerbose("No Plugins directory found in site root: " + siteRootPath);
				}
			}
			else
			{
				// WriteWarning("Could not determine module root");
			}

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

		private dynamic ImportSiteConfig()
		{
			string configPath = Path.Combine(SiteRootPath, ConfigFile);

			AssertFileExists(configPath, string.Format("The {0} file cannot be found ({{0}})", ConfigFile));

			// Microsoft.PowerShell.Utility\\Import-LocalizedData [[-BindingVariable] <String>] [[-UICulture] <String>] [-BaseDirectory <String>] [-FileName <String>] [-SupportedCommand <String[]>] [<CommonParameters>]
			// var wrappedCmd = InvokeCommand.GetCommand("Microsoft.PowerShell.Utility\\Import-LocalizedData", CommandTypes.Cmdlet);

			var scriptCmd =
				ScriptBlock.Create(
					String.Format("& 'Microsoft.PowerShell.Utility\\Import-LocalizedData' -BaseDirectory '{0}' -FileName '{1}'", SiteRootPath, ConfigFile));

			// Import-LocalizedData returns PSObject even if you InvokeReturnAsIs
			// So the cleanest thing is to always look at the BaseObject
			dynamic siteConfig = scriptCmd.Invoke().First();

			// Validation
			if (siteConfig == null)
			{
				throw new FileNotFoundException(String.Format("The {0} file is invalid at {1}", ConfigFile, SiteRootPath));
			}

			Title = siteConfig.Title;
			Description = siteConfig.Description;
			Author = LanguagePrimitives.ConvertTo<Author>(siteConfig.Author);

			BlogPath = siteConfig.BlogPath ?? "";
			PrettyUrl = siteConfig.PrettyUrl ?? true;
			PageSize = siteConfig.PostsPerArchivePage ?? 5;
		
			CreateDirectoryIfNecessary(Path.Combine(Paths["Cache"], BlogPath));
			CreateDirectoryIfNecessary(Path.Combine(Paths["Output"], BlogPath));
			
			Theme = new Theme(Paths["Themes"], siteConfig.Theme ?? "BootstrapBlog");

			return siteConfig;
		}

		public int PageSize { get; set; }

		#endregion Initialization
		
		private const string ConfigFile = "config.psd1";
		public dynamic Config { get; private set; }

		/// <summary>
		/// Gets the site's default author.
		/// </summary>
		/// <value>The author.</value>
		public Author Author { get; private set; }

		/// <summary>
		/// Gets the active site theme.
		/// </summary>
		/// <value>The site theme.</value>
		public Theme Theme { get; private set; }

		/// <summary>
		/// Gets the site title.
		/// </summary>
		/// <value>The site title.</value>
		public string Title { get; private set; }

		/// <summary>
		/// Get the site description.
		/// </summary>
		/// <value>The description.</value>
		public string Description { get; private set; }

		public bool PrettyUrl { get; set; }

		public Dictionary<string, string> Paths = new Dictionary<string, string>();
	
		public string SiteRootPath { get; private set; }
		public string BlogPath { get; private set; }

		public IdentityCollection<Document> Pages { get; set; }
		public IdentityCollection<Document> Posts { get; set; }

		private Dictionary<string, int> _tags;
		public Dictionary<string, int> Tags
		{
			get
			{
				return _tags ?? (_tags = Posts.SelectMany(p => p.Tags)
					.GroupBy(t => t)
					.ToDictionary(grouping => grouping.Key, grouping => grouping.Count()));
			}
		}

		public IEnumerable<Document> GetPostsByTag(string tag)
		{
			return Posts.Where(doc => doc.Tags.Contains(tag));
		}
	
		public void LoadDocuments()
		{
			Pages = LoadPages(Paths["Pages"], Author);
			Posts = LoadPages(Paths["Posts"], Author);
			Theme.Load();
		}
		private static IdentityCollection<Document> LoadPages(string path, Author author, bool preLoadContent=false)
		{
			var docs = from file in Directory.EnumerateFiles(path,"*.*", SearchOption.AllDirectories).AsParallel()
						  select new Document(file, author, preLoadContent);
			return IdentityCollection<Document>.Create(docs);
		}

		public void RenderDocuments()
		{
			Parallel.ForEach(Pages, doc => RenderMarkup(doc, Path.GetDirectoryName(doc.SourcePath).Substring(Paths["Pages"].Length).Trim(new[] { '\\' })));
			Parallel.ForEach(Posts, doc => RenderMarkup(doc, BlogPath));
		}

		private void RenderMarkup(Document doc, string baseUrl)
		{
			doc.RelativeUrl = PrettyUrl ? Path.Combine(baseUrl, doc.Id, "index.html") : Path.Combine(baseUrl, doc.Id + ".html");
			doc.RenderedContent = _renderEngines[doc.Extension].Render(doc.RawContent, doc);
		}

		public void RenderTemplates()
		{
			Parallel.ForEach(Posts, RenderPost);
			Parallel.ForEach(Pages, RenderPage);

			Console.WriteLine("Rendered {0} blog posts", Posts.Count);
			Console.WriteLine("Rendered {0} pages", Pages.Count);

			if (Theme.Layouts.Contains("archive"))
			{
				var layout = Theme.Layouts["archive"];
				int skip = 0;
				while (skip < Posts.Count)
				{
					var outputPath = Path.Combine(Paths["Cache"], BlogPath, string.Format("index{0}.html", skip == 0 ? "": skip.ToString(CultureInfo.InvariantCulture)));
					var output = _renderEngines[layout.Extension].Render(layout.RawContent, Posts.Skip(skip).Take(PageSize));
					File.WriteAllText(outputPath, output);
					skip += PageSize;
				}
				Console.WriteLine("Rendered archive", Posts.Count);
			}
			if (Theme.Layouts.Contains("feed"))
			{
				var layout = Theme.Layouts["feed"];
				var outputPath = Path.Combine(Paths["Cache"], BlogPath, "feed.xml");
				var output = _renderEngines[layout.Extension].Render(layout.RawContent, layout);
				File.WriteAllText(outputPath, output);
			}
		}

		private void RenderPost(Document doc)
		{
			var layout = Theme.Layouts["post"];
			var outputPath = Path.Combine(Paths["Cache"], BlogPath, doc.Id, "index.html");
			var output = _renderEngines[layout.Extension].Render(layout.RawContent, doc);

			CreateDirectoryIfNecessary(Path.GetDirectoryName(outputPath));
			File.WriteAllText(outputPath, output);
		}
	
		private void RenderPage(Document doc)
		{
			var layout = Theme.Layouts["page"];
			var pagePath = Path.GetDirectoryName(doc.SourcePath);
			if (!string.IsNullOrEmpty(pagePath))
			{
				pagePath = pagePath.Substring(Paths["Pages"].Length).Trim(new []{'\\'});
			}
			var outputPath = Path.Combine(Paths["Cache"], pagePath ?? "");
			outputPath = Path.Combine(outputPath, doc.Id + ".html");
		
			var output = _renderEngines[layout.Extension].Render(layout.RawContent, doc);

			CreateDirectoryIfNecessary(Path.GetDirectoryName(outputPath));
			File.WriteAllText(outputPath, output);
		}

		#region Static Directory Helpers
		public static string CreateDirectoryIfNecessary(string path)
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			return path;
		}
	
		public static void AssertDirectoryExists(string path, string messageTemplate = "The path '{0}' is not a valid FileSystem path")
		{
			if (!Directory.Exists(path))
			{
				throw new DirectoryNotFoundException(String.Format(messageTemplate, path));
			}
		}

		public static void AssertFileExists(string path, string messageTemplate = "The file '{0}' does not exist")
		{
			if (!File.Exists(path))
			{
				throw new FileNotFoundException(String.Format(messageTemplate, path), path);
			}
		}
		#endregion Static Directory Helpers
	}
}