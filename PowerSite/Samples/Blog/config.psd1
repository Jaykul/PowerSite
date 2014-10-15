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
	# This is the URL where nikola's output will be deployed.
	# If not set, defaults to RootUrl
	# BaseUrl = "http://HuddledMasses.org/blog"

	# Data about this site
	Title = "Huddled Masses"	# (translatable)
	Description = "You can do more than breathe for free..."	# (translatable)

####################
### SETTINGS BELOW THIS ARE WISHFUL THINKING
####################	

	DefaultCulture = "En-US"

	# This should control which languages we should generate, but doesn't do anything yet
	# Translations = "Es", "De"

	# This should control which set of layout templates will be used
	Theme = "BootstrapBlog"


	# The rest of these settings are entirely optional:

	# Post's dates use the time zone of the generating machine by default
	# If you want to use a different timezone, you'd want to use the name here:
	# To get a list of valid names, in PowerShell, run this, and use the "Id"
	#	[System.TimeZoneInfo]::GetSystemTimeZones() | ft Id, DisplayName -auto
	TimeZoneInfo = "Eastern Standard Time"

	# Date format used to display post dates.
	# Passed to DateTimeOffset.ToString()
	DateFormat = 'YYYYY-mm-dd H:MM'
}