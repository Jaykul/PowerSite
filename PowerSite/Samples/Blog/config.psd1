@{
	# Note that as with any PSD1 in PowerShell, this config file is translateable
	# However, in addition, we want to support translating your posts ...
	# You simply need to provide a copy with the specific file name pattern:
	#	FileName.lang.ext

	Author = @{
		Id = 'Jaykul'
		Name = 'Joel "Jaykul" Bennett'
		Email = "Jaykul@HuddledMasses.org"
		Url = "http://HuddledMasses.org/about_me"
	}

	# This is the main URL for your site. It will be used in a prominent link
	RootUrl = "http://HuddledMasses.org/"
	# This is the root URL where blog posts will be placed
	# If not set, defaults to RootUrl
	# The idea is that you can have a PAGES\index.md that renders to RootUrl\index.html
	# But then the POSTS\ can render to a subdirectory, say "/blog"
	BlogUrl = "blog/"

	# Data about this site
	Title = "Huddled Masses"	# (translatable)
	Description = "You can do more than breathe for free..."	# (translatable)

	# This controls which set of layout templates will be used
	Theme = "BootstrapBlog"

####################
### SETTINGS BELOW THIS ARE WISHFUL THINKING
####################

	# This should control which languages we should generate, but doesn't do anything yet
	DefaultCulture = "En-US"
	# Translations = "Es", "De"

	# The rest of these settings are entirely optional:

	# Post's dates use UTC by default, if you want to use a different timezone,
	# Specify the name here:
	# To get a list of valid names, in PowerShell, run this, and use the "Id":
	#	[System.TimeZoneInfo]::GetSystemTimeZones() | ft Id, DisplayName -auto
	TimeZoneInfo = "Eastern Standard Time"

	# Date format used to display post dates.
	# Passed to DateTimeOffset.ToString(...)
	DateFormat = 'YYYYY-mm-dd H:MM'
}