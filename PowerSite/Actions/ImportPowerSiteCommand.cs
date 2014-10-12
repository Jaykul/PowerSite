using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PowerSite.Actions;

namespace PowerSite
{
    [Cmdlet(VerbsData.Import, "PowerSite")]
    public class ImportPowerSiteCommand : BasePowerSiteCommand
    {
        [Parameter()]
        [Alias("PSPath")]
        public PSObject Configuration
        {
            get { return base.Config; }
            set { base.Config = value; }
        }
        public string PagesPath { get; set; }

        protected override void ProcessRecord()
        {
            // base.BeginProcessing asserts the existence of our root, se we can just parse away
            Config.Pages = LoadPages(Config.PagesPath);
            Config.Posts = LoadPages(Config.PostsPath);

            WriteObject(Config);
        }

        private IdentityCollection LoadPages(string path)
        {
            return IdentityCollection.Create(Directory.EnumerateFiles(path).Select(f => new Post(f, Config.Author)));
        }

    }

    public class Post : IIdentityObject
    {
        
        public Post(string path, Author author)
        {
            SourcePath = path;
            Id = path.Slugify();

            using (var reader = new StreamReader(path))
            {
                string firstLine = ParseMetadataHeader(reader);
                
                if (Date == default(DateTime))
                {
                    var rawFile = new FileInfo(path);
                    Date = rawFile.LastWriteTime;
                }

                if (Author == null)
                {
                    Author = author;
                }

                Draft = path.ToLowerInvariant().Contains(".draft.");

                RawContent = String.Concat(firstLine, "\n", reader.ReadToEnd()).Trim();
            }
        }

        private static readonly Regex MetadataKeyValue = new Regex(@"^(?<key>\w+):\s?(?<value>.+)$", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Singleline);

        private string ParseMetadataHeader(StreamReader reader)
        {
            var preambleOpened = false;
            var preambleSkipped = false;
            string line;

            Metadata = new PSObject();

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
                    // if we already parsed metadata, then when we see ---, we're done
                    if (preambleOpened || preambleSkipped) break;
                    preambleOpened = true;
                    continue;
                }

                var match = MetadataKeyValue.Match(line);
                if (match.Success)
                {
                    preambleSkipped = !preambleOpened;

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
                            Date = date;
                            break;

                        case "title":
                            Title = value;
                            break;

                        case "author":
                            Author = LanguagePrimitives.ConvertTo<Author>(value);
                            break;

                        case "tag":
                        case "tags":
                            var tags = value.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(t => t.Trim()).Where(t => !String.IsNullOrEmpty(t)).ToArray();

                            Tags = tags;
                            break;

                        default:
                            ((PSObject)Metadata).Properties.Add(new PSNoteProperty(key,value));
                            break;
                    }
                }
                else if (!preambleOpened) // no preamble, non-matches mean we're done with the header.
                {
                    break;
                }
            }

            return line;
        }

        public string SourcePath { get; private set; }

        public string Id { get; set; }
        public string Title { get; set; }

        public Author Author { get; set; }

        public DateTime Date { get; set; }

        public string[] Tags { get; set; }

        public bool Draft { get; set; }

        public dynamic Metadata { get; set; }

        public string RawContent { get; private set; }

        public string RenderedContent { get; set; }

    }


    public class Author : IIdentityObject
    {
        private readonly Regex _parser = new Regex(@"(?<name>.*)\s+ (?:\((?<id>[^\)].*)\))?\s* (?:<(?<email>[^>]+)>)?\s* (?<url>https?://.*)?(?:$|\s)", RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline);

        public Author() {}

        public Author(string name)
        {
            var match = _parser.Match(name);
            if (match.Success)
            {
                Name = match.Groups["name"].Value;
                if (match.Groups["id"].Length > 0)
                {
                    Id = match.Groups["id"].Value;
                }
                else
                {
                    Id = Name.Slugify();
                }
                if (match.Groups["email"].Length > 0)
                {
                    Email = match.Groups["email"].Value;
                }
                if (match.Groups["url"].Length > 0)
                {
                    Url = match.Groups["url"].Value;
                }
            }
            else
            {
                Name = name;
                Id = name.Slugify();
            }
        }

        public string Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Url { get; set; }

        public override string ToString()
        {
            return string.Format("{0} ({1})", Name, Id) + (string.IsNullOrEmpty(Email) ? "" : string.Format(" <{0}>",Email)) +(string.IsNullOrEmpty(Url) ? "" : " " + Url);
        }
    }
}
