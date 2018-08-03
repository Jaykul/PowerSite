using System;
using System.Composition;
using System.Composition.Hosting;
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
	
		private CompositionHost _container;

		/// <summary>
		/// Initializes the plugin catalog.
		/// </summary>
		/// <param name="siteRootPath">The site root path.</param>
		protected virtual void InitializePluginCatalog(string siteRootPath)
		{
            //Adds all the parts found in the same assembly as the Program class
            var configuration = new ContainerConfiguration().WithAssembly(typeof(BaseMefCommand).Assembly);

			if (!string.IsNullOrEmpty(siteRootPath) && Directory.Exists(siteRootPath))
			{
				WriteVerbose("Site root: " + siteRootPath);
				var pluginRoot = Path.Combine(siteRootPath, "Plugins");

				if (Directory.Exists(pluginRoot))
				{
                    configuration.WithAssembliesInPath(pluginRoot);
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

			try
			{
				_container = configuration.CreateContainer();
            }
			catch (Exception compositionException)
			{
				WriteError(new ErrorRecord(compositionException, "PluginFailure", ErrorCategory.ResourceUnavailable, _container));
			}
		}
	}
}