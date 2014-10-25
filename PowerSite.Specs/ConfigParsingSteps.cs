using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using TechTalk.SpecFlow;
using Xunit;
using PowerSite.DataModel;
namespace PowerSite.Specs
{
	[Binding]
	public class ConfigParsingSteps : Steps
	{
		public ConfigParsingSteps()
		{
			Runspace.DefaultRunspace = PowerShell.Create().Runspace;
		}
	
		[Given(@"I have a sample ([^ ]+) with a config\.psd1")]
		public void GivenASample(string sample)
		{
			Trace.TraceWarning(Environment.CurrentDirectory);
			// C:\Users\Joel\Projects\PowerSite\PowerSite.Specs\bin\Debug
		
			var sampleRoot = string.Format(@"..\..\Samples\{0}", sample);
			var samplePath = string.Format(@"..\..\Samples\{0}\config.psd1", sample);
			Assert.True(Directory.Exists(sampleRoot));
			Assert.True(File.Exists(samplePath));
			ScenarioContext.Current.Add("SiteRoot", sampleRoot);
		}


		[When(@"I try to load the site")]
		public void LoadTheSite()
		{
		
			var site = Site.ForPath(ScenarioContext.Current.Get<string>("SiteRoot"));
			ScenarioContext.Current.Add("Site", site);
		}

		[Given(@"I have loaded the sample ([^ ]+)")]
		public void GivenIHaveLoadedASample(string sample)
		{
			Given(string.Format("I have a sample {0} with a config.psd1", sample));
			When("I try to load the site");
		}


		[Given(@"I load the documents")]
		[When(@"I load the documents")]
		public void LoadDocuments()
		{
			var site = ScenarioContext.Current.Get<DataModel.Site>("Site");
			site.LoadDocuments();
		}

		[Then(@"the site should have matching properties:")]
		public void ThenThePowerSiteShouldHaveMatchingProperties(Table table)
		{
			var site = ScenarioContext.Current.Get<DataModel.Site>("Site");
			var config = (Hashtable)((PSObject)site.Config).BaseObject;

			foreach (var row in table.Rows)
			{
				Assert.True(config.ContainsKey(row["Property"]));

				Assert.Equal(row["Value"], config[row["Property"]].ToString());
			}
		}

		[Then(@"the site should have Posts with RawContent")]
		public void ThenThePowerSiteShouldHavePostsWithRawContent()
		{
			var site = ScenarioContext.Current.Get<DataModel.Site>("Site");
			Assert.True( site.Posts.All(doc => !string.IsNullOrEmpty(doc.RawContent)) );
		}

		[Then(@"the site should not have Posts with RenderedContent")]
		public void ThenThePowerSiteShouldNotHavePostsWithRenderedContent()
		{
			var site = ScenarioContext.Current.Get<DataModel.Site>("Site");
			Assert.True( site.Posts.All(doc => string.IsNullOrEmpty(doc.RenderedContent)) );
		}

		[When(@"I render the markup")]
		[Given(@"I render the markup")]
		public void RenderDocuments()
		{
			var site = ScenarioContext.Current.Get<DataModel.Site>("Site");
			site.RenderDocuments();
		}

		[Then(@"all the posts in the site should have RenderedContent")]
		public void ThenAllThePostsInThePowerSiteShouldHaveRenderedContent()
		{
			var site = ScenarioContext.Current.Get<DataModel.Site>("Site");
			Assert.True(site.Posts.All(doc => !string.IsNullOrEmpty(doc.RenderedContent)));
		}

		[When(@"I render the pages")]
		public void WhenIRenderThePages()
		{
			var site = ScenarioContext.Current.Get<DataModel.Site>("Site");
			site.RenderTemplates();
		}

		[Then(@"I get actual pages")]
		public void ThenIGetActualPages()
		{
			var root = ScenarioContext.Current.Get<string>("SiteRoot");
			var blog = ScenarioContext.Current.Get<DataModel.Site>("Site").BlogPath;
			blog = Path.Combine(root, "Cache", blog);

			foreach (var post in new[] {"a-fresh-start/index.html", "about-huddled-masses/index.html"}.Select(post => Path.Combine(blog, post)))
			{
				Assert.True(File.Exists(post), string.Format("Blog post '{0}' does not exist.", post));
				Assert.NotEqual(0, new FileInfo(post).Length);
			}
		
			foreach (var page in new[] {"index.html"}.Select(page => Path.Combine(blog, page)))
			{
				Assert.True(File.Exists(page), string.Format("Web page '{0}' does not exist.", page));
				Assert.NotEqual(0, new FileInfo(page).Length);
			}
		}
	
		[AfterScenario]
		public void DisposeSite()
		{
			if(ScenarioContext.Current.ContainsKey("Site"))
				ScenarioContext.Current.Get<Site>("Site").Dispose();
		}

	}
}
