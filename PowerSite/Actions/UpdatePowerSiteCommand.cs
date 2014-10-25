using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PowerSite.Actions
{
	[Cmdlet(VerbsData.Update, "PowerSite")]
	public class UpdatePowerSiteCommand : ImportPowerSiteCommand
	{
		protected override void EndProcessing()
		{
			base.EndProcessing();
			// 1. Parallel: Collect All Files (done)
			_helper.LoadDocuments();
			// 2. XCopy static Theme parts (or copy everything and delete ones which need to be rendered)
		
			// 3. XCopy static pages over the top of the theme
		
			// 4. Parallel: Render Markdown for each
			_helper.RenderDocuments();
			// 5. Parallel: Render Page for each
			_helper.RenderTemplates();
		}
	}
}
