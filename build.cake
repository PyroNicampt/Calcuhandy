var target = Argument("target", "Zip");
var settings = new DotNetPublishSettings
{
    Configuration = Argument("configuration", "Release"),
    Runtime = Argument("runtime", "win-x64"),
    Framework = Argument("framework", "net8.0"),
};
var publishDir = $"./bin/{settings.Configuration}/{settings.Framework}/{settings.Runtime}/publish";
var zipPath = $"./releases/Calcuhandy-{settings.Runtime}-{settings.Configuration}.zip";

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .WithCriteria(c => HasArgument("rebuild"))
    .Does(() =>
{
    CleanDirectory($"./bin/{settings.Configuration}");
});

Task("Publish")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetPublish("./", settings);
});

Task("Zip")
    .IsDependentOn("Publish")
    .Does(() =>
{
    EnsureDirectoryExists("./releases/");
    Zip(publishDir, zipPath);
});

Task("DebugBuild")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetBuild("./", new DotNetBuildSettings
    {
        Configuration = Argument("configuration", "Debug"),
        //Runtime = settings.Runtime,
        //Framework = settings.Framework,
    });
});

Task("Debug")
    .IsDependentOn("DebugBuild")
    .Does(() =>
{
    DotNetRun("./", new DotNetRunSettings
    {
        Configuration = Argument("configuration", "Debug"),
        //Runtime = settings.Runtime,
        //Framework = settings.Framework,
        NoBuild = true,
    });
});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);