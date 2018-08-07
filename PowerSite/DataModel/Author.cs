using System.Text.RegularExpressions;
using PowerSite.Actions;

namespace PowerSite.DataModel
{
    public class Author : IIdentityObject
    {
        private readonly Regex _parser = new Regex(@"(?<name>.*)\s+(?:\((?<id>[^\)].*)\))?(?:\s*<(?<email>[^>]+)>\s*)?(?<url>https?://.*)?(?:$|\s)", RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline);

        public Author() {}

        public Author(string name)
        {
            var match = _parser.Match(name);
            if (match.Success)
            {
                Name = match.Groups["name"].Value;
                Id = match.Groups["id"].Length > 0 ? match.Groups["id"].Value : Name.Slugify();

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