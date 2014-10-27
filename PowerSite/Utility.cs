using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PowerSite.DataModel;

namespace PowerSite.Actions
{
	public static class Utility
	{
		public static string Slugify(this string id)
		{
			if (String.IsNullOrWhiteSpace(id))
			{
				return null;
			}
			id = Path.GetFileNameWithoutExtension(id);
			id = Regex.Replace(id, @"[^\w\s_\-\.]+", String.Empty); // first, allow only words, spaces, underscores, dashes and dots.
			id = Regex.Replace(id, @"\.{2,}", String.Empty); // strip out any dots stuck together (no pathing attempts).
			id = Regex.Replace(id, @"\s{2,}", " "); // convert multiple spaces into single space.
			id = id.Trim(new[] { ' ', '.' }); // ensure the string does not start or end with a dot
			return id.Replace(' ', '-').ToLowerInvariant(); // finally, replace all spaces with dashes and lowercase it.
		}

		public static string CreateDirectoryIfNecessary(string path)
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			return path;
		}

		public static void AssertDirectoryExists(string path, string messageTemplate = "The path '{0}' is not a valid FileSystem path")
		{
			if (!Directory.Exists(path))
			{
				throw new DirectoryNotFoundException(String.Format(messageTemplate, path));
			}
		}

		public static void AssertFileExists(string path, string messageTemplate = "The file '{0}' does not exist")
		{
			if (!File.Exists(path))
			{
				throw new FileNotFoundException(String.Format(messageTemplate, path), path);
			}
		}
	}
}
