
using System.Management.Automation;
using System.Composition;
using Markdig;

using PowerSite.DataModel;

namespace PowerSite.Builtin.Renderers
{

	[Export(typeof(IRenderer))]
	[ExportMetadata("Extension", "md")]
	[Cmdlet(VerbsData.ConvertFrom, "Markdown")]

	public class MarkdownRenderer : IRenderer
	{
		public string Render(string siteKey, NamedContentBase template, dynamic data)
		{
			return Markdown.ToHtml(template.RawContent).Trim();
		}
	}
}
