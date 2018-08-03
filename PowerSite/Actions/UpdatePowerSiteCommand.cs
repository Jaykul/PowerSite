using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PowerSite.Actions
{
	[Cmdlet(VerbsData.Update, "PowerSite")]
	public class UpdatePowerSiteCommand : GetPowerSiteCommand
	{
		protected override void EndProcessing()
		{
			try
			{
				// 1. Parallel: Collect All Files (done)
				_helper.LoadDocuments();
                // 2. Calculate destinations (output paths and URLs)
                var themeRoot = _helper.Theme.ThemeRoot;
				var cachePath = _helper.Paths["cache"];
				var outputPath = _helper.Paths["output"];
			
				// 2. XCopy static Theme parts (or copy everything and delete ones which need to be rendered)
				XCopy(themeRoot, cachePath, true, true);

				foreach (var file in _helper.Keys.
					SelectMany(extension => Directory.EnumerateFiles(cachePath, string.Format("*.{0}", extension))))
				{
					File.Delete(file);
				}

                // 3. XCopy static pages over the top of the theme
                XCopy(_helper.Paths["static"], cachePath, true);

				// 4. Parallel: Render Markdown for each
				_helper.RenderDocuments();
				// 5. Parallel: Render Page for each
				_helper.RenderTemplates();
			

				// If we get to this point, we can replace the Output with the cache:
				if (Directory.Exists(outputPath))
				{
					Directory.Delete(outputPath, true);
				}
                Directory.Move(cachePath, outputPath);
			}
			catch (Exception ex)
			{
				WriteError(new ErrorRecord(ex, "FailedToProcess", ErrorCategory.NotSpecified, _helper));
			}
		}


        /// <summary>
        /// Copies a directory and optionally sub directories
        /// </summary>
        /// <param name="sourcePath">source directory name</param>
        /// <param name="destinationPath">destination directory name</param>
        /// <param name="recurse">whether to copy subdirectories, defaults to false</param>
        /// <remarks>syncronous operation, possibly quicker than async if files remain on the same drive as only headers require updating</remarks>
        public static void XCopy(string sourcePath, string destinationPath, bool recurse = false, bool clean = false)
        {
            try
            {
                if (!Directory.Exists(sourcePath))
                {
                    throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourcePath);
                }

                if (clean)
                {
                    if (Directory.Exists(destinationPath))
                    {
                        Directory.Delete(destinationPath, true);
                    }
                }
                Directory.CreateDirectory(destinationPath);

                // Get the files in the directory and copy them to the new location.
                foreach (var file in Directory.EnumerateFiles(sourcePath))
                {
                    var destFileName = Path.Combine(destinationPath, Path.GetFileName(file));
                    File.Copy(file, destFileName, true);
                }

                if (recurse)
                {
                    foreach (var subdir in Directory.EnumerateDirectories(sourcePath))
                    {
                        string destSubdirName = Path.Combine(destinationPath, Path.GetFileName(subdir));
                        XCopy(subdir, destSubdirName, true);
                    }
                }
            }
            catch (System.Exception e)
            {
                throw new IOException(e.Message, e);
            }
        }
    }
}
