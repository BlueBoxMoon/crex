## Major Version Releases

When making an update such as a new feature that would require
a client update, the following changes must be made:

* Crex/Application.cs
    * Update CrexVersion to next integer up
* Crex.Roku/Libs/Utilities.brs
    * Update the GetCrexVersion() method to the new major version number
* Follow procedures of the Minor Version Release
* Build a release version
* Push nupkg files (TBD)

## Minor Version Release

* Crex.Android/Properties/AssemblyInfo.cs
    * Update AssemblyVersion to new major/minor version number
* Crex.Android/Crex.Android.csproj (or use GUI)
    * Update PackageVersion to match the version number of AssemblyVersion
* Crex.tvOS/Properties/AssemblyInfo.cs
    * Update AssemblyVersion to new major/minor version number
* Crex.tvOS/Crex.tvOS.csproj (or use GUI)
    * Update PackageVersion to match the version number of AssemblyVersion
* Crex.Roku/crex.version.txt
    * Update version number to new major/minor version number

## Push NuGet Packages

* `mkdir /tmp/neko && sshfs HOSTNAME:/ /tmp/neko`
* `msbuild Crex.Android/Crex.Android.csproj /p:Configuration=Release`
* `msbuild Crex.tvOS/Crex.tvOS.csproj /p:Configuration=Release`
* `sleet push -c /tmp/neko/var/www/nuget/sleet-osx.json Crex.Android/bin/Release/Crex.Android.MAJOR.MINOR.nupkg`
* `sleet push -c /tmp/neko/var/www/nuget/sleet-osx.json Crex.tvOS/bin/Release/Crex.tvOS.MAJOR.MINOR.nupkg`
* `umount /tmp/neko`
