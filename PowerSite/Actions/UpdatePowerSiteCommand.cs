using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;
namespace PowerSite.Actions
{
	[Cmdlet(VerbsData.Update, "PowerSite")]
	public class UpdatePowerSiteCommand : ImportPowerSiteCommand
	{
		protected override void EndProcessing()
		{
			try
			{
				base.EndProcessing();
				// 1. Parallel: Collect All Files (done)
				_helper.LoadDocuments();
				var themeRoot = _helper.Theme.ThemeRoot;
				var cachePath = _helper.Paths["Cache"];
				var outputPath = _helper.Paths["Output"];

				if (FileSystem.DirectoryExists(cachePath))
				{
					FileSystem.DeleteDirectory(cachePath, DeleteDirectoryOption.DeleteAllContents);
				}

				FileSystem.CreateDirectory(cachePath);
				// 2. XCopy static Theme parts (or copy everything and delete ones which need to be rendered)
				FileSystem.CopyDirectory(themeRoot, cachePath);

				foreach (var file in _helper.Engines.Select(e => e.Metadata.Extension).
					SelectMany(extension => Directory.EnumerateFiles(cachePath, string.Format("*.{0}", extension))))
				{
					File.Delete(file);
				}

				// 3. XCopy static pages over the top of the theme
				FileSystem.CopyDirectory(_helper.Paths["Static"], cachePath);

				// 4. Parallel: Render Markdown for each
				_helper.RenderDocuments();
				// 5. Parallel: Render Page for each
				_helper.RenderTemplates();
			

				// If we get to this point, we can replace the Output with the cache:
				if (FileSystem.DirectoryExists(outputPath))
				{
					FileSystem.DeleteDirectory(outputPath, DeleteDirectoryOption.DeleteAllContents);
				}
				FileSystem.MoveDirectory(cachePath, outputPath);
			}
			catch (Exception ex)
			{
				WriteError(new ErrorRecord(ex, "FailedToProcess", ErrorCategory.NotSpecified, _helper));
			}
		}
	}
}
