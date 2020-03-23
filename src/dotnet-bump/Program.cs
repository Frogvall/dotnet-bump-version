using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Extensions.CommandLineUtils;

namespace Frogvall
{
  public class Program
  {
    public static void Main(string[] args)
    {

      var app = new CommandLineApplication
      {
        Name = "dotnet bump-all",
        Description = "Bump project version",
        ExtendedHelpText = "Extended",
        Syntax = "dotnet bump-all [major | minor | patch | revision] [-s] [-v value] [path-to-project-file]"
      };
      try
      {
        app.Command("major", CommandBuilder);
        app.Command("minor", CommandBuilder);
        app.Command("patch", CommandBuilder);
        app.Command("revision", CommandBuilder);

        app.HelpOption("-?|-h|--help");
        app.Execute(args);
      }
      catch (Exception)
      {
          Console.WriteLine($"Syntax: {app.Syntax}");
      }
    }

    public static void CommandBuilder(CommandLineApplication command)
    {
      command.Description = $"Increase in one the {command.Name} part of the version.";
      var prefixOption = command.Option("-s", "Use <VersionPrefix> instead <Version> tag to bump the version.", CommandOptionType.NoValue);
      var valueOption = command.Option("--value", "Bump version to a fixed value, instead of incrementing by 1.",
          CommandOptionType.SingleValue);
      var projectFilePathArgument = command.Argument("[path-to-project-file]", "Path to the .csproj file.");
      command.HelpOption("-?|-h|--help");
      command.OnExecute(() =>
      {
        var projectFilePath = projectFilePathArgument.Value ?? Directory.GetFiles(Directory.GetCurrentDirectory(), "*.csproj").FirstOrDefault();
        if (string.IsNullOrEmpty(projectFilePath))
        {
          Console.Error.WriteLine("Cannot find project file");
          command.ShowHelp();
          return 0;
        }
        var versionElementName = prefixOption.HasValue() ? "VersionPrefix" : "Version";

        Console.Error.WriteLine($"Loading project from {projectFilePath}...");
        XDocument projectFile;
        using (var f = File.OpenRead(projectFilePath))
          projectFile = XDocument.Load(f);

        var propertyGroupElement = projectFile.Root?.Elements("PropertyGroup").FirstOrDefault();
        if (propertyGroupElement == null)
        {
          projectFile.Root.Add(new XElement("PropertyGroup", new XElement(versionElementName, "0.0.0.0")));
          propertyGroupElement = projectFile.Root?.Elements("PropertyGroup").FirstOrDefault();
        }

        var versionElement = projectFile.Root?.Elements("PropertyGroup").Select(x => x.Element(versionElementName)).FirstOrDefault();
        if (string.IsNullOrEmpty(versionElement?.Value))
        {
          Console.Error.WriteLine($"Missing project <{versionElementName}> tag, setting default.");
          propertyGroupElement.Add(new XElement(versionElementName, "0.0.0.0"));
          versionElement = propertyGroupElement.Element(versionElementName);
        }

        var version = versionElement.Value;
        var suffix = version.EndsWith("-*");
        if (suffix)
        {
          version = version.Substring(0, version.Length - 2);
        }
        var oldVersion = new SemVer(version);
        SemVer newVersion;
        switch (command.Name)
        {
          case "major":
            newVersion = new SemVer(valueOption.HasValue() ? int.Parse(valueOption.Value()) : oldVersion.Major + 1, oldVersion.Minor.HasValue ? 0 : oldVersion.Build, oldVersion.Build.HasValue ? 0 : oldVersion.Build, oldVersion.Fix.HasValue ? 0 : oldVersion.Fix, oldVersion.Suffix, oldVersion.Buildvars);
            break;
          case "minor":
            newVersion = new SemVer(oldVersion.Major, valueOption.HasValue() ? int.Parse(valueOption.Value()) : oldVersion.Minor + 1, oldVersion.Build.HasValue ? 0 : oldVersion.Build, oldVersion.Fix.HasValue ? 0 : oldVersion.Fix, oldVersion.Suffix, oldVersion.Buildvars);
            break;
          case "patch":
            newVersion = new SemVer(oldVersion.Major, oldVersion.Minor, valueOption.HasValue() ? int.Parse(valueOption.Value()) : oldVersion.Build + 1, oldVersion.Fix.HasValue ? 0 : oldVersion.Fix, oldVersion.Suffix, oldVersion.Buildvars);
            break;
          case "revision":
            newVersion = new SemVer(oldVersion.Major, oldVersion.Minor, oldVersion.Build, valueOption.HasValue() ? int.Parse(valueOption.Value()) : oldVersion.Fix + 1, oldVersion.Suffix, oldVersion.Buildvars);
            break;
          default:
            throw new InvalidOperationException();
        }
        Console.Error.WriteLine($"Changing version from \"{oldVersion}\" to \"{newVersion}\"");
        version = newVersion.ToString();
        if (suffix)
          version = version + "-*";
        versionElement.Value = version;
        Console.Error.WriteLine($"Saving project....");
        using (var f = File.CreateText(projectFilePath)) projectFile.Save(f);
        Console.Error.WriteLine($"Project saved.");
        Console.WriteLine(newVersion);
        return 0;
      });
    }
  }
}
