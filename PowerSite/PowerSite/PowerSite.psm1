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


function FastPostGitHub {
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
	$site = Get-PowerSite -SiteRootPath $Root

	Push-Location $site.SiteRootPath

	git checkout source --force

	New-PowerSitePost $Name | edit
	Update-PowerSite -SiteRootPath $pwd
	git commit -a -m "Added post $name"
	git checkout master --force
	cp .\Output\* $Pwd -Force -Recurse
	git commit -a -m "Added post $name"
	git push
}