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
	public class ImportPowerSiteCommand : BasePowerSiteCommand
	{
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
			if (ParameterSetName == "FromPath")
			{
				ProviderInfo provider;
				_siteRootPath = GetResolvedProviderPathFromPSPath(_siteRootPath, out provider).SingleOrDefault();
				if (provider.ImplementingType != typeof(FileSystemProvider))
				{
					ThrowTerminatingError(
					new ErrorRecord(
						 new DirectoryNotFoundException(
							  string.Format("The SiteRootPath '{0}' is not a valid folder path", _siteRootPath)), "InvalidSiteRootPath", ErrorCategory.InvalidData, _siteRootPath));
				}
				base.BeginProcessing();
			}
		}

		protected override void ProcessRecord()
		{
			// base.BeginProcessing asserts the existence of our root, se we can just parse away
			_helper.LoadDocuments();
			WriteObject(_helper);
		}
	}
}
