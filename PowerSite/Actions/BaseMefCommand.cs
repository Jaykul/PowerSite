using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Management.Automation;

namespace PowerSite.Actions
{
	public class BaseMefCommand : PSCmdlet
	{
		protected string _siteRootPath;

		protected override void BeginProcessing()
		{
			if (string.IsNullOrEmpty(_siteRootPath))
			{
				_siteRootPath = CurrentProviderLocation("FileSystem").ProviderPath;
			}

			base.BeginProcessing();
		}
	
		private CompositionContainer _container;

		/// <summary>
		/// Initializes the plugin catalog.
		/// </summary>
		/// <param name="siteRootPath">The site root path.</param>
		protected virtual void InitializePluginCatalog(string siteRootPath)
		{
			var catalog = new AggregateCatalog();
			//Adds all the parts found in the same assembly as the Program class
			var assembly = typeof (BasePowerSiteCommand).Assembly;
			catalog.Catalogs.Add(new AssemblyCatalog(assembly));

			if (!string.IsNullOrEmpty(siteRootPath) && Directory.Exists(siteRootPath))
			{
				WriteVerbose("Site root: " + siteRootPath);
				var pluginRoot = Path.Combine(siteRootPath, "Plugins");

				if (Directory.Exists(pluginRoot))
				{
					catalog.Catalogs.Add(new DirectoryCatalog(pluginRoot));
				}
				else
				{
					WriteVerbose("No Plugins directory found in site root: " + siteRootPath);
				}
			}
			else
			{
				WriteWarning("Could not determine module root");
			}

			//Create the CompositionContainer with the parts in the catalog
			_container = new CompositionContainer(catalog);

			try
			{
				_container.ComposeParts(this);
			}
			catch (CompositionException compositionException)
			{
				WriteError(new ErrorRecord(compositionException, "PluginFailure", ErrorCategory.ResourceUnavailable, _container));
			}
		}
	}
}