# Frogvall.DotnetBumpVersion

[![NuGet Version and Downloads count](https://img.shields.io/nuget/vpre/Frogvall.DotnetBumpVersion.svg)](http://www.nuget.org/packages/Frogvall.DotnetBumpVersion/)

[![NuGet Download count](https://img.shields.io/nuget/dt/Frogvall.DotnetBumpVersion.svg)](http://www.nuget.org/packages/Frogvall.DotnetBumpVersion/)

A dotnet-cli command that bumps the version number of the current project. This is useful when working with multiple .NET Core projects
placed in different solutions, referencing each other as NuGet packages. Use this command before `dotnet pack` to increment a specific part of
the version number in `project.json` before pushing your project to your local NuGet feed. This ensures that NuGet will not fetch the package from cache,
and all your .NET Core projects in different solutions can reference the latest compiled version.

## Usage

Add `Frogvall.DotnetBumpVersion` as a tool to your project by running the following:

> `dotnet tool install Frogvall.DotnetBumpVersion --version 3.0.1`

Then to bump a file run:

> `dotnet bump-version [major | minor | patch | revision] [-s] [-v value] [path-to-project-file]`.

Arguments:  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`[major | minor | patch | revision]` What part of the version to bump. (Mandatory)  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`[path-to-project-file]` Path to the .csproj file. (Optional)

Options:  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`-s` Use `<VersionPrefix>` instead `<Version>` tag to bump the version. (Optional)  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`--value` Bump version to a fixed value, instead of incrementing by 1. (Optional)