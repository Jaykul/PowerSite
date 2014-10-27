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
		public Document(Author author, string path, string id = null, bool preLoadContent = false) : base(path, id, preLoadContent)
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

		private string _summary;
		public string Summary {
			get
			{
				if (_summary == null)
				{
					Match match = Regex.Match(RenderedContent, "<p>.*?</p>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
					if (match.Success && match.Value != RenderedContent)
					{
						_summary = match.Value;
					}
				}

				return _summary;
			}
		}
	}

	public class NamedContentBase : IIdentityObject
	{
		public NamedContentBase(string path, string id = null, bool preloadContent = false)
		{
			SourcePath = path;
			Id = id ?? Path.GetFileNameWithoutExtension(path).Slugify();
			Extension = (Path.GetExtension(path) ?? "md").Trim(new []{'.'});
			LoadFile(true, preloadContent);
		}
	
		public string SourcePath { get; protected set; }
		public string Id { get; set; }
		public string Extension { get; set; }

		private string _rawContent;
		public string RawContent
		{
			get
			{
				if (string.IsNullOrEmpty(_rawContent))
				{
					LoadRawContent();
				}
				return _rawContent;
			}
			private set { _rawContent = value; }
		}

		public string RenderedContent { get; set; }
		public dynamic Metadata { get; set; }
		public string RelativeUrl { get; set; }
		public string FullyQualifiedUrl { get; set; }
	
		private static readonly Regex MetadataKeyValue = new Regex(@"^(?<key>\w+):\s?(?<value>.+)$", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Singleline);

		protected void LoadRawContent()
		{
			LoadFile(false, true);
		}

		protected void LoadMetadata()
		{
			LoadFile(true, false);
		}
	
		private void LoadFile(bool keepMetadata, bool keepContent)
		{
			var preambleOpened = false;
			var preambleSkipped = false;
			dynamic o = new PSObject(this);
			if (keepMetadata)
			{
				try
				{
					o.Draft = SourcePath.ToLowerInvariant().Contains(".draft.");
					o.Metadata = new PSObject();
				}
				catch
				{
					keepMetadata = false;
				}
			}
		
			using (var reader = new StreamReader(SourcePath))
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

						if (keepMetadata)
						{
							string key = match.Groups[1].Value.ToLowerInvariant();
							string value = match.Groups[2].Value.Trim();
							switch (key)
							{
								case "date":
									DateTime date;
									if (!DateTime.TryParse(value, out date))
									{
										date = default(DateTime);
									}
									o.Date = date;
									break;

								case "title":
									o.Title = value;
									break;

								case "author":
									o.Author = LanguagePrimitives.ConvertTo<Author>(value);
									break;

								case "tag":
								case "tags":
									var tags = value.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
										.Select(t => t.Trim()).Where(t => !String.IsNullOrEmpty(t)).ToArray();

									o.Tags = tags;
									break;

								default:
									((PSObject)o.Metadata).Properties.Add(new PSNoteProperty(key, value));
									break;
							}
						}
					}
					else if (!preambleOpened) // no preamble, non-matches mean we're done with the header.
					{
						break;
					}
				}

				if (keepMetadata)
				{
					if (o.Date == default(DateTime))
					{
						o.Date = File.GetLastWriteTime(SourcePath);
					}
					Metadata = o;
				}


				if (keepContent)
				{
					RawContent = String.Concat(line, "\n", reader.ReadToEnd()).Trim();
				}
				
			}

		}
	}
}