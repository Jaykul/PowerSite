using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PowerSite.Actions
{
	[Cmdlet(VerbsData.Update, "PowerSite")]
	public class UpdatePowerSiteCommand : BasePowerSiteCommand
	{
		protected override void EndProcessing()
		{
			base.EndProcessing();
		
			// 1. Parallel: Collect All Files (done)
			_helper.LoadDocuments();
			// 2. Copy static 
			// 2. Copy static Theme parts
			// 3. Parallel: Generate and cache url patterns
			// 4. Parallel: Render Markdown for each
			// 5. Parallel: Render Page for each
		}
	}
}
