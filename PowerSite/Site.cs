using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;
using PowerSite.Actions;
using PowerSite.DataModel;

namespace PowerSite
{
	public class Site : EngineHost, IDisposable
	{
		public static Site Current;

		public static Site ForPath(string siteRootPath)
		{
			Utility.AssertDirectoryExists(siteRootPath);
			siteRootPath = Path.GetFullPath(siteRootPath).ToLowerInvariant();
			return ActiveSites.ContainsKey(siteRootPath) ? ActiveSites[siteRootPath] : new Site(siteRootPath);
		}

		#region Initialization
		/// <summary>
		/// Initializes a new instance of the <see cref="Site"/> class.
		/// </summary>
		/// <param name="siteRootPath">The site root path.</param>
		private Site(string siteRootPath)
			: base(siteRootPath)
		{
			foreach (var path in new[] { "Pages", "Posts", "Static", "Themes", "Plugins", "Output", "Cache" })
			{
				Paths[path] = Utility.CreateDirectoryIfNecessary(Path.Combine(siteRootPath, path));
			}

			Config = ImportSiteConfig();
		
			ActiveSites.Add(siteRootPath, this);
		}

		private dynamic ImportSiteConfig()
		{
			string configPath = Path.Combine(SiteRootPath, ConfigFile);

			Utility.AssertFileExists(configPath, string.Format("The {0} file cannot be found ({{0}})", ConfigFile));

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
			RootUrl = siteConfig.RootUrl;
			Title = siteConfig.Title;
			Description = siteConfig.Description;
			Author = LanguagePrimitives.ConvertTo<Author>(siteConfig.Author);

			BlogPath = siteConfig.BlogPath ?? "";
			PrettyUrl = siteConfig.PrettyUrl ?? true;
			PageSize = siteConfig.PostsPerArchivePage ?? 5;

			Utility.CreateDirectoryIfNecessary(Path.Combine(Paths["Cache"], BlogPath));
			Utility.CreateDirectoryIfNecessary(Path.Combine(Paths["Output"], BlogPath));

			Theme = new Theme(Paths["Themes"], siteConfig.Theme ?? "BootstrapBlog");

			return siteConfig;
		}
		#endregion Initialization

		public string RootUrl
		{
			get { return _rootUrl; }
			set { _rootUrl = value.TrimEnd(new[] { '/', '\\' }) + '/'; }
		}

		public string BlogUrl
		{
			get { return RootUrl + BlogPath; }
		}


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

		public string BlogPath { get; private set; }

		public IdentityCollection<Document> Pages { get; set; }
		public IdentityCollection<Document> Posts { get; set; }

		private Dictionary<string, int> _tags;
		private string _rootUrl;
		public readonly static Dictionary<string, Site> ActiveSites = new Dictionary<string, Site>();

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
			Pages = IdentityCollection<Document>.Create(
				from file in Directory.EnumerateFiles(Paths["Pages"], "*.*", SearchOption.AllDirectories).AsParallel()
						  let fileId = Path.GetFileNameWithoutExtension(file).Slugify()
						  let relativeUrl = Path.Combine(Path.GetDirectoryName(file).Substring(Paths["Pages"].Length), fileId + ".html").Replace('\\', '/')
						  select new Document(Author, file, fileId)
						  {
							  RelativeUrl = "/" + relativeUrl,
							  FullyQualifiedUrl = RootUrl.TrimEnd('/') + "/" + relativeUrl
						  });

			Posts = IdentityCollection<Document>.Create(
				from file in Directory.EnumerateFiles(Paths["Posts"], "*.*", SearchOption.TopDirectoryOnly).AsParallel()
						  let fileId = Path.GetFileNameWithoutExtension(file).Slugify()
						  let relativeUrl = Path.Combine(BlogPath, (fileId + (PrettyUrl ? "/index.html" : ".html"))).Replace('\\', '/')
						  select new Document(Author, file, fileId)
						  {
							  RelativeUrl = "/" + relativeUrl,
							  FullyQualifiedUrl = RootUrl.TrimEnd('/') + "/" + relativeUrl
						  });
		
			Theme.Load();
		}
	
		public void RenderDocuments()
		{
			Current = this;
			Parallel.ForEach(Pages, p => Render(p));
			Parallel.ForEach(Posts, p => Render(p));
		}
	
		public void RenderTemplates()
		{
			Current = this;
			Parallel.ForEach(Posts, p => Render(Theme.Layouts["post"], p, Path.Combine(Paths["Cache"], p.RelativeUrl.TrimStart('/'))));
			Parallel.ForEach(Pages, p => Render(Theme.Layouts["page"], p, Path.Combine(Paths["Cache"], p.RelativeUrl.TrimStart('/'))));

			Console.WriteLine("Rendered {0} blog posts", Posts.Count);
			Console.WriteLine("Rendered {0} pages", Pages.Count);

			if (Theme.Layouts.Contains("archive"))
			{
				var layout = Theme.Layouts["archive"];

				// the main site index
				RenderIndex(layout, Posts.ToList(), Path.Combine(Paths["Cache"], BlogPath));
				// tag indexes
				foreach (var tag in Tags.Keys)
				{
					RenderIndex(layout, GetPostsByTag(tag), Path.Combine(Paths["Cache"], BlogPath, "tags", tag));
				}

				Console.WriteLine("Rendered archive");
			}
			if (Theme.Layouts.Contains("feed"))
			{
				var layout = Theme.Layouts["feed"];
				var outputPath = Path.Combine(Paths["Cache"], BlogPath, "feed.xml");
				var output = base[layout.Extension].Render(SiteRootPath, layout.RawContent, Posts.OrderByDescending(doc => doc.Date).Take(5));
				File.WriteAllText(outputPath, output);
			}
		}

		#region IDisposable
		private bool _disposed = false;
		protected virtual void Dispose(bool disposing)
		{
			if (_disposed) return;

			if (disposing)
			{
				if (Site.ActiveSites.ContainsKey(this.SiteRootPath))
				{
					Site.ActiveSites.Remove(this.SiteRootPath);
				}
			}

			_disposed = true;
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion IDisposable
	}
}