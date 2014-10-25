using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using Microsoft.PowerShell.Commands;
using PowerSite.Builtin;
using PowerSite.DataModel;

namespace PowerSite.Actions
{
	[Cmdlet(VerbsData.Import, "PowerSite")]
	public class ImportPowerSiteCommand : PSCmdlet
	{
		protected string _siteRootPath;
		protected Site _helper;
		//[Parameter(ParameterSetName = "FromConfiguration")]
		//public PSObject Configuration
		//{
		//	get { return base.Config; }
		//	set { base.Config = value; }
		//}

		[Parameter(ParameterSetName = "FromPath")]
		[Alias("PSPath", "Path", "Root")]
		public string SiteRootPath
		{
			get { return _siteRootPath; }
			set { _siteRootPath = value; }
		}

		protected override void BeginProcessing()
		{
			base.BeginProcessing();
			switch (ParameterSetName)
			{
				case "FromPath":
				{
					ProviderInfo provider;
					_siteRootPath = GetResolvedProviderPathFromPSPath(_siteRootPath, out provider).SingleOrDefault();
					if (provider.ImplementingType != typeof (FileSystemProvider))
					{
						ThrowTerminatingError(
							new ErrorRecord(
								new DirectoryNotFoundException(
									string.Format("The SiteRootPath '{0}' is not a valid folder path", _siteRootPath)), "InvalidSiteRootPath",
								ErrorCategory.InvalidData, _siteRootPath));
					}

					break;
				}
				default:
				{
					if (string.IsNullOrEmpty(_siteRootPath))
					{
						_siteRootPath = CurrentProviderLocation("FileSystem").ProviderPath;
					}
					break;
				}
			}
			try
			{
				if (File.Exists(_siteRootPath))
				{
					_siteRootPath = Path.GetDirectoryName(_siteRootPath);
				}

				_helper = Site.ForPath(_siteRootPath);
			}
			catch (IOException ex)
			{
				ThrowTerminatingError(new ErrorRecord(ex, "ConfigFileNotLoaded", ErrorCategory.InvalidData, _siteRootPath));
			}
		}

		protected override void ProcessRecord()
		{
			this.SessionState.PSVariable.Set("script:PowerSiteActiveSessions",Site.ActiveSites);
			// base.BeginProcessing asserts the existence of our root, se we can just parse away
			WriteObject(_helper);
		}
	}
}
