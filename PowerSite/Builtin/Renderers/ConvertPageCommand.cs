using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Management.Automation;
using PowerSite.Actions;
using PowerSite.DataModel;

namespace PowerSite.Builtin.Renderers
{
	[Cmdlet(VerbsData.Convert, "Page")]
	public class ConvertPageCommand : BaseMefCommand
	{
		[ImportMany]
		protected IEnumerable<Lazy<IRenderer, IExtension>> Engines;
	
		[Parameter(Position = 0, ValueFromPipelineByPropertyName = true, ValueFromPipeline = true, ParameterSetName = "FromPath")]
		[Alias("PSPath")]
		public string Path { get; set; }
	
		[Parameter()]
		[Alias("Root")]
		public string SiteRootPath
		{
			get { return _siteRootPath; }
			set { _siteRootPath = value; }
		}

		[Parameter(Position = 0, ValueFromPipeline = true, ParameterSetName = "FromPost")]
		public NamedContentBase Markup { get; set; }

		[Parameter()]
		public PSObject Data { get; set; }

		protected override void InitializePluginCatalog(string path)
		{
			ProviderInfo providerInfo;
			// Path is something like siteRoot\Posts\File.md
			// and after this, _siteRootPath is siteRoot\Posts
			path = System.IO.Path.GetDirectoryName(path);
			// and after this, _siteRootPath is siteRoot\
			_siteRootPath = System.IO.Path.GetDirectoryName(path);
			_siteRootPath = GetResolvedProviderPathFromPSPath(_siteRootPath, out providerInfo).FirstOrDefault();
			_isPluggedIn = true;
			base.InitializePluginCatalog(_siteRootPath);
		}

		private bool _isPluggedIn;

		protected override void ProcessRecord()
		{
			if (ParameterSetName == "FromPath")
			{
				if (!_isPluggedIn) InitializePluginCatalog(Path);
				ProviderInfo providerInfo;
				var files = GetResolvedProviderPathFromPSPath(Path, out providerInfo);
				foreach (var file in files)
				{
					Markup = new NamedContentBase(file);
					var renderer = Engines.First(i => i.Metadata.Extension.Equals(Markup.Extension)).Value;
					WriteObject(renderer.Render(Markup.RawContent, Data));
				}
			}
			else
			{
				if (!_isPluggedIn) InitializePluginCatalog(Markup.SourcePath);
				var renderer = Engines.First(i => i.Metadata.Extension.Equals(Markup.Extension)).Value;
				Markup.RenderedContent = renderer.Render(Markup.RawContent, Data);
				WriteObject(Markup);
			}
		}
	}
}