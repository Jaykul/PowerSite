function New-PowerSitePost {
	[CmdletBinding(DefaultParameterSetName="PowerSite")]
	param(
		# The post title (headline)
		[Parameter(Mandatory=$true, Position=0, ValueFromRemainingArguments=$true)]
		[String]$Name,

		# keyword tags for the post
		[String[]]$Tags,

		# The root of the PowerSite to put it in
		[Parameter(ParameterSetName="Path")]
		[Alias("Path")]
		[String]$Root = $Pwd,

		# The PowerSite to use
		[Parameter(ValueFromPipeline=$true, ParameterSetName="PowerSite")]
		[PowerSite.Site]$PowerSite = $(Get-PowerSite -Root $Root)
	)

	
	$date = Get-Date -f "yyyy-MM-dd HH:mm zzz"
	New-Item "$(Join-Path $PowerSite.Paths.posts $Name).md" -Value "---`ntitle: $Name`ndate: $date`ntags:$($Tags -join ', ')`n---`n`n"
}

function Get-PowerSitePost {
	[CmdletBinding(DefaultParameterSetName="PowerSite")]
	param(
		[Parameter(Mandatory=$true, Position=0, ValueFromRemainingArguments=$true)]
		[String]$Name,

		# The root of the PowerSite to put it in
		[Parameter(ParameterSetName="Path")]
		[Alias("Path")]
		[String]$Root = $Pwd,

		# The PowerSite to use
		[Parameter(ValueFromPipeline=$true, ParameterSetName="PowerSite")]
		[PowerSite.Site]$PowerSite = $(Get-PowerSite -Root $Root)
	)
	Get-Item (Join-Path $PowerSite.Paths.posts $Name)
}


function FastPushGitHub {
	#.Synopsis
	#	Push changes to github
	#.Description
	#	Based on github pages model:
	#	1. Assume everything is checked in.
	#	2. Use a "source" branch for editing pages: build it to an ignored "Output" folder
	#	3. Switch to the "master" branch and copy from the output, push.
	[CmdletBinding(DefaultParameterSetName="PowerSite")]
	param(
		[Parameter(Position=0, ValueFromRemainingArguments=$true)]
		[String]$Message = "FastPush from PowerSite",

		# The root of the PowerSite to put it in
		[Parameter(ParameterSetName="Path")]
		[Alias("Path")]
		[String]$Root = $Pwd,

		# The Source branch in git (defaults to source)
		[String]$SourceBranchName = "source",

		# The Publish branch in git (defaults to master)
		[String]$PublishBranchName = "master",

		# The PowerSite to use
		[Parameter(ValueFromPipeline=$true, ParameterSetName="PowerSite")]
		[PowerSite.Site]$PowerSite = $(Get-PowerSite -Root $Root)
	)
	$site = Get-PowerSite -SiteRootPath $Root

	Push-Location $site.SiteRootPath
	if(git status -s) {
		Pop-Location
		throw "Your site directory isn't clean, please commit or clean up and try again."
	}
	$ErrorActionPreference = "Stop"

	git checkout $SourceBranchName --force

	Update-PowerSite -SiteRootPath $Root

	git checkout $PublishBranchName --force

	ls -exclude output, .git\, .gitignore | rm -Recurse -ErrorActionPreference SilentlyContinue -ErrorVariable UnDeleted
	if($UnDeleted) {
		$UnDeleted | %{ Write-Warning ($_ | fl * -force | Out-String) }
		throw "Couldn't delete all the files. Stupid thumbs.db"
	}
	cp .\Output\* $Pwd -Force -Recurse
	git add *
	git commit -a -m $Message
	git push
}