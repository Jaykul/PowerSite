
using System.Management.Automation;
using PowerSite.DataModel;

namespace PowerSite.Builtin.Renderers
{
	using System.ComponentModel.Composition;

	using MarkdownSharp;

	[Export(typeof(IRenderer))]
	[ExportMetadata("Extension", "md")]
	[Cmdlet(VerbsData.ConvertFrom, "Markdown")]

	public class MarkdownRenderer : IRenderer
	{

		public MarkdownRenderer()
		{
			MarkdownEngine = new Markdown(new MarkdownOptions() { AutoHyperlink = true });
		}

		private Markdown MarkdownEngine { get; set; }

		public string Render(string siteKey, NamedContentBase template, dynamic data)
		{
			var engine = new Markdown(new MarkdownOptions() { AutoHyperlink = true });
			return engine.Transform(template.RawContent).Trim();
		}
	}
}
