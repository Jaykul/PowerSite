using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using PowerSite.Builtin;
using PowerSite.DataModel;

namespace PowerSite.Actions
{
	public class BasePowerSiteCommand : BaseMefCommand
	{

		private const string ConfigFile = "config.psd1";

		protected dynamic Config;

		protected override void BeginProcessing()
		{
			base.BeginProcessing();
			ImportSiteConfig();
		}

		protected void ImportSiteConfig()
		{
			AssertDirectoryExists(_siteRootPath, "SiteRootNotFound");
			string configPath = Path.Combine(_siteRootPath, ConfigFile);

			WriteVerbose("Importing Config File: " + configPath);

			AssertFileExists(configPath, "ConfigFileNotFound", "The site.config file cannot be found ({0})");

			// Microsoft.PowerShell.Utility\\Import-LocalizedData [[-BindingVariable] <String>] [[-UICulture] <String>] [-BaseDirectory <String>] [-FileName <String>] [-SupportedCommand <String[]>] [<CommonParameters>]
			// var wrappedCmd = InvokeCommand.GetCommand("Microsoft.PowerShell.Utility\\Import-LocalizedData", CommandTypes.Cmdlet);

			var scriptCmd =
				ScriptBlock.Create(
					string.Format(
						"& 'Microsoft.PowerShell.Utility\\Import-LocalizedData' -BaseDirectory '{0}' -FileName '{1}'",
						_siteRootPath, ConfigFile));

			// Import-LocalizedData returns PSObject even if you InvokeReturnAsIs
			// So the cleanest thing is to always look at the BaseObject
			Config = scriptCmd.Invoke().First();

			// Validation
			if (Config == null)
			{
				ThrowTerminatingError(
					new ErrorRecord(
						new FileNotFoundException(
							string.Format("The site.config file is invalid at {0}", _siteRootPath)), "ConfigFileNotLoaded", ErrorCategory.InvalidData, _siteRootPath));
				return;
			}

			if (Config.Author is Hashtable)
			{
				Config.Author = LanguagePrimitives.ConvertTo<Author>(Config.Author);
			}

			Config.BasePath	= _siteRootPath;
			Config.PagesPath	= CreateIfNecessary(Path.Combine(_siteRootPath, "Pages"));	// Pages that aren't blog posts go here (and have a "path" metadata property)
			Config.PostsPath	= CreateIfNecessary(Path.Combine(_siteRootPath, "Posts"));	// For a blog, blog posts go in this folder
			Config.StaticPath	= CreateIfNecessary(Path.Combine(_siteRootPath, "Static"));	// Static content like images and downloads go here, and will be uploaded to the site without processing.
			Config.ThemesPath	= CreateIfNecessary(Path.Combine(_siteRootPath, "Themes"));	// \Layout templates, \js and \css for the themes go here
			Config.PluginsPath	= CreateIfNecessary(Path.Combine(_siteRootPath, "Plugins"));	// Any plugins used by this site should be installed in this folder
			Config.OutputPath	= CreateIfNecessary(Path.Combine(_siteRootPath, "Output"));	// The output of rendering the site. 
			Config.CachePath	= CreateIfNecessary(Path.Combine(_siteRootPath, "Cache"));	// A cache of partially rendered files with meta information
				
			var themeName	= Config.Theme ?? "BootstrapBlog";
			Config.Theme	= new Theme(Config.ThemesPath, themeName);
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
