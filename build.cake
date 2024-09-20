#addin nuget:?package=Cake.VersionReader&version=5.1.0.0

var target = Argument("target", "Zip");
var settings = new DotNetPublishSettings
{
    Configuration = Argument("configuration", "Release"),
    Runtime = Argument("runtime", "win-x64"),
    Framework = Argument("framework", "net8.0"),
};
var publishDir = $"./bin/{settings.Configuration}/{settings.Framework}/{settings.Runtime}/publish";
var zipPath = $"./releases/Calcuhandy-{settings.Runtime}-{settings.Configuration}";
var executableName = "Calcuhandy.exe";

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    //.WithCriteria(c => HasArgument("rebuild"))
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
    var version = GetVersionNumber($"{publishDir}/{executableName}");
    Zip(publishDir, $"{zipPath}-{version}.zip");
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

Task("Test")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetTest("./CalcuhandyTests", new DotNetTestSettings
    {
        //NoBuild = true,
    });
});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);