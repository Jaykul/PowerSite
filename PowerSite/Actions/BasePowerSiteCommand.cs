using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using Microsoft.PowerShell.Commands;

namespace PowerSite
{
    public class BasePowerSiteCommand : PSCmdlet
    {
        private CompositionContainer _container;

        [ImportMany]
        protected IEnumerable<Lazy<IRenderer, IExtension>> renderers;

        private const string ConfigFile = "config.psd1";

        protected dynamic Config;
        protected string SiteRootPath;

        protected override void BeginProcessing()
        {
            if (string.IsNullOrEmpty(SiteRootPath))
            {
                SiteRootPath = CurrentProviderLocation("FileSystem").ProviderPath;
            }

            base.BeginProcessing();
            //An aggregate catalog that combines multiple catalogs
            var catalog = new AggregateCatalog();
            //Adds all the parts found in the same assembly as the Program class
            var assembly = typeof(BasePowerSiteCommand).Assembly;
            catalog.Catalogs.Add(new AssemblyCatalog(assembly));

            if (!string.IsNullOrEmpty(SiteRootPath))
            {
                WriteVerbose("Site root: " + SiteRootPath);
                var pluginRoot = Path.Combine(SiteRootPath, "Plugins");

                if (Directory.Exists(pluginRoot))
                {
                    catalog.Catalogs.Add(new DirectoryCatalog(pluginRoot));
                }
                else
                {
                    WriteVerbose("No Plugins directory found in site root: " + SiteRootPath);
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
                WriteError(new ErrorRecord(compositionException,"PluginFailure",ErrorCategory.ResourceUnavailable, _container));
            }

            ImportSiteConfig();
        }



        protected void ImportSiteConfig()
        {
            AssertDirectoryExists(SiteRootPath, "SiteRootNotFound");
            string configPath = Path.Combine(SiteRootPath, ConfigFile);

            WriteVerbose("Importing Config File: " + configPath);

            AssertFileExists(configPath, "ConfigFileNotFound", "The site.config file cannot be found ({0})");

            // Microsoft.PowerShell.Utility\\Import-LocalizedData [[-BindingVariable] <String>] [[-UICulture] <String>] [-BaseDirectory <String>] [-FileName <String>] [-SupportedCommand <String[]>] [<CommonParameters>]
            // var wrappedCmd = InvokeCommand.GetCommand("Microsoft.PowerShell.Utility\\Import-LocalizedData", CommandTypes.Cmdlet);

            var scriptCmd =
                ScriptBlock.Create(
                    string.Format(
                        "& 'Microsoft.PowerShell.Utility\\Import-LocalizedData' -BaseDirectory '{0}' -FileName '{1}'",
                        SiteRootPath, ConfigFile));

            // Import-LocalizedData returns PSObject even if you InvokeReturnAsIs
            // So the cleanest thing is to always look at the BaseObject
            Config = scriptCmd.Invoke().First();

            // Validation
            if (Config == null)
            {
                ThrowTerminatingError(
                    new ErrorRecord(
                        new FileNotFoundException(
                            string.Format("The site.config file is invalid at {0}", SiteRootPath)), "ConfigFileNotLoaded", ErrorCategory.InvalidData, SiteRootPath));
                return;
            }

            if (Config.Author is Hashtable)
            {
                Config.Author = LanguagePrimitives.ConvertTo<Author>(Config.Author);
            }

            Config.BasePath = SiteRootPath;
            Config.PagesPath = CreateIfNecessary(Path.Combine(SiteRootPath, "Pages"));
            Config.PostsPath = CreateIfNecessary(Path.Combine(SiteRootPath, "Posts"));
            Config.StaticPath = CreateIfNecessary(Path.Combine(SiteRootPath, "Static"));
            Config.ThemesPath = CreateIfNecessary(Path.Combine(SiteRootPath, "Themes"));
            Config.PluginsPath = CreateIfNecessary(Path.Combine(SiteRootPath, "Plugins"));
            Config.OutputPath = CreateIfNecessary(Path.Combine(SiteRootPath, "Output"));
            Config.CachePath = CreateIfNecessary(Path.Combine(SiteRootPath, "Cache"));
        }

        private string CreateIfNecessary(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        private void AssertDirectoryExists(string path, string errorId = "DirectoryNotFound", string messageTemplate = "The path '{0}' is not a valid FileSystem path")
        {
            if (!Directory.Exists(path))
            {
                ThrowTerminatingError(
                    new ErrorRecord(
                        new DirectoryNotFoundException(string.Format(messageTemplate, path)), errorId, ErrorCategory.ObjectNotFound, path));
            }
        }
        private void AssertFileExists(string path, string errorId = "FileNotFound", string messageTemplate = "The file '{0}' does not exist")
        {
            if (!File.Exists(path))
            {
                ThrowTerminatingError(
                    new ErrorRecord(
                        new FileNotFoundException(string.Format(messageTemplate, path), path), errorId, ErrorCategory.ObjectNotFound, path));
            }
        }
    }
}
