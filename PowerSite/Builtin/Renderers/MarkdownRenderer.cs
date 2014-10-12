
namespace PowerSite.Builtin.Renderers
{
    using System.ComponentModel.Composition;

    using MarkdownSharp;

    [Export(typeof(IRenderer))]
    [ExportMetadata("Extension", "md")]
    public class MarkdownRenderer : IRenderer
    {
        public MarkdownRenderer()
        {
            MarkdownEngine = new Markdown(new MarkdownOptions() { AutoHyperlink = true });
        }

        private Markdown MarkdownEngine { get; set; }

        public string Render(string template, object data)
        {
            var result = MarkdownEngine.Transform(template).Trim();

            return result;
        }
    }
}
