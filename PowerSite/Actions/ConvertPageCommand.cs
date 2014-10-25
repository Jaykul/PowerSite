using System.Linq;
using System.Management.Automation;
using PowerSite.DataModel;

namespace PowerSite.Actions
{
	[Cmdlet(VerbsData.Convert, "Page")]
	public class ConvertPageCommand : PSCmdlet
	{
		[Parameter(Position = 0, ValueFromPipelineByPropertyName = true, ValueFromPipeline = true, ParameterSetName = "FromPath")]
		[Alias("PSPath")]
		public string Path { get; set; }
	
		[Parameter()]
		[Alias("Root")]
		public string SiteRootPath { get; set; }
	
		[Parameter(Position = 0, ValueFromPipeline = true, ParameterSetName = "FromPost")]
		public NamedContentBase Markup { get; set; }

		[Parameter()]
		public PSObject Data { get; set; }

		private string GetPluginPath(string path)
		{
			if (SiteRootPath != null) 
				return SiteRootPath;
		
			ProviderInfo providerInfo;
			// Path is something like siteRoot\Posts\File.md
			// and after this, _siteRootPath is siteRoot\Posts
			path = System.IO.Path.GetDirectoryName(path);
			// and after this, _siteRootPath is siteRoot\
			SiteRootPath = System.IO.Path.GetDirectoryName(path);
			SiteRootPath = GetResolvedProviderPathFromPSPath(SiteRootPath, out providerInfo).FirstOrDefault();
			return SiteRootPath;
		}

		protected Site helper;

		protected override void ProcessRecord()
		{
			if (ParameterSetName == "FromPath")
			{
				if (helper == null)
					helper = Site.ForPath(GetPluginPath(Path)); 
			
				ProviderInfo providerInfo;
				var files = GetResolvedProviderPathFromPSPath(Path, out providerInfo);
				foreach (var file in files)
				{
					Markup = new NamedContentBase(file, true);
					var renderer = helper.Engines.First(i => i.Metadata.Extension.Equals(Markup.Extension)).Value;
					WriteObject(renderer.Render(SiteRootPath, Markup.RawContent, Data));
				}
			}
			else
			{
				if (helper == null)
					helper = Site.ForPath(GetPluginPath(Markup.SourcePath));

				var renderer = helper.Engines.First(i => i.Metadata.Extension.Equals(Markup.Extension)).Value;
				Markup.RenderedContent = renderer.Render(SiteRootPath, Markup.RawContent, Data);
				WriteObject(Markup);
			}
		}
	}
}