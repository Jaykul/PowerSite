
using System.Management.Automation;
using PowerSite.DataModel;

namespace PowerSite.Builtin.Renderers
{
	using System.Composition;

	using Kiwi.Markdown;

    [Export(typeof(IRenderer))]
	[ExportMetadata("Extension", "md")]
	[Cmdlet(VerbsData.ConvertFrom, "Markdown")]

	public class MarkdownRenderer : IRenderer
	{

		public string Render(string siteKey, NamedContentBase template, dynamic data)
        {
            var markdown = new MarkdownService(null);
            return markdown.ToHtml(template.RawContent).Trim();
		}
    }
}
