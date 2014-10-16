using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;
using PowerSite.Actions;

namespace PowerSite.DataModel
{
	public class Document : NamedContentBase
	{
		public Document(string path, Author author) : base(path)
		{
			if (Author == null)
			{
				Author = author;
			}
		}

		public string Title { get; set; }

		public Author Author { get; set; }

		public DateTime Date { get; set; }

		public string[] Tags { get; set; }

		public bool Draft { get; set; }

	}

	public class NamedContentBase : IIdentityObject
	{
		public NamedContentBase(string path)
		{
			SourcePath = path;
			Id = Path.GetFileNameWithoutExtension(path).Slugify();
			Extension = (Path.GetExtension(path) ?? "md").Trim(new []{'.'});

			dynamic metadata = new PSObject(this);
			RawContent = GetRawContent(path, ref metadata);
		}
		public string SourcePath { get; protected set; }
		public string Id { get; set; }
		public string Extension { get; set; }
		public string RawContent { get; protected set; }
		public string RenderedContent { get; set; }
		public dynamic Metadata { get; set; }

		private static readonly Regex MetadataKeyValue = new Regex(@"^(?<key>\w+):\s?(?<value>.+)$", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Singleline);

		public static string GetRawContent(string path, ref dynamic metadata)
		{
			var preambleOpened = false;
			var preambleSkipped = false;
			var saveMetadata = metadata != null;
			
			if (saveMetadata)
			{
				try
				{
					metadata.Draft = path.ToLowerInvariant().Contains(".draft.");
					metadata.Metadata = new PSObject();
				}
				catch
				{
					saveMetadata = false;
				}
			}

			using (var reader = new StreamReader(path))
			{
				string line;

				while ((line = reader.ReadLine()) != null)
				{
					// Eat any blank lines or comments at the top of the document or in the header.
					if (String.IsNullOrEmpty(line) || line.StartsWith("#") || line.StartsWith("//"))
					{
						if (preambleSkipped) break;
						continue;
					}

					if (line.Equals("---"))
					{
						line = String.Empty;
						// if we already parsed metadata, then when we see ---, we're done
						if (preambleOpened || preambleSkipped) break;
						preambleOpened = true;
						continue;
					}

					var match = MetadataKeyValue.Match(line);
					if (match.Success)
					{
						preambleSkipped = !preambleOpened;
					
						if (saveMetadata)
						{
							string key = match.Groups[1].Value.ToLowerInvariant();
							string value = match.Groups[2].Value.Trim();
							switch (key)
							{
								case "date":
									DateTime date;
									if (!DateTime.TryParse(value, out date))
									{
										date = DateTime.MinValue;
									}
									metadata.Date = date;
									break;

								case "title":
									metadata.Title = value;
									break;

								case "author":
									metadata.Author = LanguagePrimitives.ConvertTo<Author>(value);
									break;

								case "tag":
								case "tags":
									var tags = value.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
										.Select(t => t.Trim()).Where(t => !String.IsNullOrEmpty(t)).ToArray();

									metadata.Tags = tags;
									break;

								default:
									((PSObject)metadata.Metadata).Properties.Add(new PSNoteProperty(key, value));
									break;
							}
						}
					}
					else if (!preambleOpened) // no preamble, non-matches mean we're done with the header.
					{
						break;
					}
				}

				if (saveMetadata && metadata.Date == default(DateTime))
				{
					var rawFile = new FileInfo(path);
					metadata.Date = rawFile.LastWriteTime;
				}

				return String.Concat(line, "\n", reader.ReadToEnd()).Trim();
			}

		}
	}
}