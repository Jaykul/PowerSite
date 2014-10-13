
using System.Management.Automation;
using PowerSite.DataModel;

namespace PowerSite.Builtin.Renderers
{
    using System.ComponentModel.Composition;

    using MarkdownSharp;

    [Export(typeof(IRenderer))]
    [ExportMetadata("Extension", "md")]
    [Cmdlet(VerbsData.ConvertFrom, "Markdown")]

    public class MarkdownRenderer : PSCmdlet, IRenderer
    {
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, ValueFromPipeline = true, ParameterSetName = "FromPath")]
        [Alias("PSPath")]
        public string Path { get; set; }

        [Parameter(Position=0, ValueFromPipeline = true, ParameterSetName = "FromPost")]
        public Post Post { get; set; }

        protected override void ProcessRecord()
        {
            var rootPath = CurrentProviderLocation("FileSystem").ProviderPath;

            if (ParameterSetName == "FromPath")
            {
                ProviderInfo providerInfo;
                var files = GetResolvedProviderPathFromPSPath(Path, out providerInfo);
                foreach (var file in files)
                {
                    Post = new Post(file, new Author());
                    WriteObject(Render(Post.RawContent, Post));
                }
            }
            else
            {
                Post.RenderedContent = Render(Post.RawContent, Post);
                WriteObject(Post);
            }
        }

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
