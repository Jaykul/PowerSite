Feature: ConfigParsing
	In order to configure a PowerSite
	As a PowerShell user
	I want to write PSD1 files

Scenario: Load the sample blog
	Given I have a sample blog with a config.psd1
	When I try to load the site
	Then the site should have matching properties:
	| Property    | Value                                    |
	| RootUrl     | http://HuddledMasses.org/                |
	| BlogPath    | blog/                                    |
	| PrettyUrl   | True                                     |
	| Title       | Huddled Masses                           |
	| Description | You can do more than breathe for free... |
	| Theme       | BootstrapBlog                            |

Scenario: Load Documents
	Given I have loaded the sample blog
	When I load the documents
	Then the site should have Posts with RawContent
	But the site should not have Posts with RenderedContent

Scenario: Render Markup
	Given I have loaded the sample blog
	And I load the documents
	When I render the markup
	Then all the posts in the site should have RenderedContent

Scenario: Render Pages
	Given I have loaded the sample blog
	And I load the documents
	And I render the markup
	When I render the pages
	Then I get actual pages