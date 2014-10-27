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
	public class BasicallyPowerSiteCommand : PSCmdlet
	{
		protected string _siteRootPath;
		protected Site _helper;
	
		protected dynamic Config { get { return _helper.Config; } }

		protected override void BeginProcessing()
		{
			base.BeginProcessing();

		}

		protected override void EndProcessing()
		{
			_helper.Dispose();
			base.EndProcessing();
		}
	}
}
