cd C:\Users\Joel\Projects\PowerSite\PowerSite.Specs\Samples\Blog        
ipmo C:\Users\Joel\projects\PowerSite\PowerSite\bin\Debug\PowerSite.dll 
Update-PowerSite -Path $pwd                                             
iisexpress /path:$pwd\Output                                            