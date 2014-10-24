param(
	$path = $(Get-Location -PSProvider FileSystem)
)
try {
	$ErrorActionPreference = "Stop"
	Import-Module "$PSScriptRoot\..\bin\Debug\PowerSite.dll"
	if(!(Test-Path $Path)) { 
		throw "That path doesn't exist!" 
	}
	if(Test-Path $Path -PathType Container) {
		if(!(Test-Path (Join-Path $Path "config.psd1"))) { 
			throw "There is no config.psd1 in that path" 
		}
	}
	## 1. Parallel: Collect All Files (done)
	$Site = New-Object PowerSite.DataModel.Site $path
	$Site.LoadDocuments()
	## 2. Copy static 
	Write-Host "xcopy $($Site.Paths.Static) $($Site.Paths.Cache) /E /I /Q /Y"
	xcopy "$($Site.Paths.Static)" "$($Site.Paths.Cache)" /E /I /Q /Y
	## 2. Copy static Theme parts
	Write-Host "xcopy '$(Join-Path $Site.Paths.Themes $Site.Theme.Name)' '$($Site.Paths.Cache)' /E /I /Q /Y /EXCLUDE:'${PSScriptRoot}\templates.txt'"
	xcopy "$(Join-Path $Site.Paths.Themes $Site.Theme.Name)" "$($Site.Paths.Cache)" /E /I /Q /Y /EXCLUDE:"${PSScriptRoot}\templates.txt"

	## 3. TODO: Render SASS/LESS/TypeScript/CoffeeScript
	## 4. Parallel: Render Markdown (or whatever) for each page and post
	$Site.RenderDocuments()
	## 5. Parallel: Render Page for each page, post
	$Site.RenderTemplates()
} finally {
	if($Site) {
		$Site.Dispose()
	}
}