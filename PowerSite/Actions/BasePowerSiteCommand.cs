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
	public class BasePowerSiteCommand : PSCmdlet
	{
		protected string _siteRootPath;
		protected DataModel.Site _helper;
	
		protected dynamic Config { get { return _helper.Config; } }

		protected override void BeginProcessing()
		{
			base.BeginProcessing();
			try
			{
				if (string.IsNullOrEmpty(_siteRootPath))
				{
					_siteRootPath = CurrentProviderLocation("FileSystem").ProviderPath;
				}
				_helper = new DataModel.Site(_siteRootPath);
			}
			catch (IOException ex)
			{
				ThrowTerminatingError( new ErrorRecord( ex, "ConfigFileNotLoaded", ErrorCategory.InvalidData, _siteRootPath));
			}
		}

		protected override void EndProcessing()
		{
			_helper.Dispose();
			base.EndProcessing();
		}
	}
}
