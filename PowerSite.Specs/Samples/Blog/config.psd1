@{
	# Note that as with any PSD1 in PowerShell, this config file is translateable
	# However, in addition, we want to support translating your posts ...
	# You simply need to provide a copy with the specific file name pattern:
	#	FileName.lang.ext

	# Please see the ReadMe for more complete documentation of the settings...
	Author = @{
		Id = 'Jaykul'
		Name = 'Joel "Jaykul" Bennett'
		Email = "Jaykul@HuddledMasses.org"
		Url = "http://HuddledMasses.org/about_me"
	}

	# This is the main URL for your site. It will be used in a prominent link
	RootUrl = "http://HuddledMasses.org/"

	# This is the path where blog posts will be placed, below the RootUel
	# If not set, it defaults to empty, which is just fine for a blog, but if
	# if you want to also have static pages, and not worry about confusing the paths,
	# just set this path to something like "blog/"
	BlogPath = "blog/"

	# Data about this site
	Title = "Huddled Masses"	# (translatable)
	Description = "You can do more than breathe for free..."	# (translatable)

	# This controls which set of layout templates will be used
	Theme = "BootstrapBlog"

	# The rest of these settings are entirely optional:

	# Instead of saving files as BlogUrl/<slug>.html, store them in BlogUrl/<slug>/index.html
	# Generates links to BlogUrl/<slug>, make sure "index.html" is your site's default document.
	PrettyUrl = $True

	# Controls how many posts are rendered on each index page
	PostsPerArchivePage = 5

	# Date format used to display post dates.
	# Passed to DateTimeOffset.ToString(...)
	DateFormat = 'yyyy-MM-dd H:mm'
####################
### SETTINGS BELOW THIS ARE WISHFUL THINKING
####################

	# This should control which languages we should generate, but doesn't do anything yet
	DefaultCulture = "En-US"
	# Translations = "Es", "De"


	# Post's dates use UTC by default, if you want to use a different timezone,
	# Specify the name here:
	# To get a list of valid names, in PowerShell, run this, and use the "Id":
	#	[System.TimeZoneInfo]::GetSystemTimeZones() | ft Id, DisplayName -auto
	TimeZoneInfo = "Eastern Standard Time"

}