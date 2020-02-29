#tool nuget:?package=GitVersion.CommandLine&version=5.0.1

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var buildDir = "."; //+ Directory(configuration);
string buildVersion = "";

//////////////////////////////////////////////////////////////////////
// Version
//////////////////////////////////////////////////////////////////////

GitVersion versionInfo = null;

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Version")
  .Description("Retrieves the current version from the git repository")
  .Does(() =>
  {

	versionInfo = GitVersion(new GitVersionSettings {
		UpdateAssemblyInfo = false
	});

	Information("Branch: "+ versionInfo.BranchName);
	Information("Version: "+ versionInfo.FullSemVer);
	Information("Version: "+ versionInfo.MajorMinorPatch);

    if(versionInfo.BranchName != "master")
    {
        configuration = Argument("configuration", "Debug");
    }
  });

Task("Clean")
.IsDependentOn("Version")
    .Does(() =>
{
    CleanDirectories(buildDir + "/src/**/bin/" + configuration);
    CleanDirectories(buildDir + "/tests/**/bin/" + configuration);
    CleanDirectories(buildDir + "/src/**/obj/" + configuration);
    CleanDirectories(buildDir + "/tests/**/obj/" + configuration);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetCoreRestore(buildDir + "/Reimers.Ihe.sln");
    DotNetCoreBuildServerShutdown();
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    buildVersion = versionInfo.MajorMinorPatch + "-" + versionInfo.BranchName.Replace("features/", "") + "." + versionInfo.CommitsSinceVersionSource;
	Information("Build version: " + buildVersion);
    var informationalVersion = versionInfo.MajorMinorPatch + "." + versionInfo.CommitsSinceVersionSourcePadded;
	Information("CommitsSinceVersionSourcePadded: " + versionInfo.CommitsSinceVersionSourcePadded);
    if(versionInfo.BranchName == "master")
    {
        buildVersion = versionInfo.MajorMinorPatch;
    }
    var buildSettings = new DotNetCoreMSBuildSettings()
        .SetConfiguration(configuration)
        .SetVersion(buildVersion)
        .SetInformationalVersion(informationalVersion);
    DotNetCoreMSBuild(buildDir + "/Reimers.Ihe.sln", buildSettings);
    DotNetCoreBuildServerShutdown();
});

Task("Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    var projects = GetFiles(buildDir + "/tests/**/*.tests.csproj");

    foreach(var project in projects)
    {
        Information("Testing: " + project.FullPath);
        var reportName = buildDir + "/artifacts/testreports/" + versionInfo.FullSemVer + "_" + System.IO.Path.GetFileNameWithoutExtension(project.FullPath).Replace('.', '_') + ".xml";
        reportName = System.IO.Path.GetFullPath(reportName);

        Information(reportName);

        var coreTestSettings = new DotNetCoreTestSettings()
          {
			NoBuild = true,
			NoRestore = true,
            // Set configuration as passed by command line
            Configuration = configuration,
            ArgumentCustomization = x => x.Append("--logger \"trx;LogFileName=" + reportName + "\"")
          };

          DotNetCoreTest(
          project.FullPath,
          coreTestSettings);

          DotNetCoreBuildServerShutdown();
    }
});

Task("Pack")
    .IsDependentOn("Tests")
    .Does(()=>
    {
        Information("Package version: " + buildVersion);

        var packSettings = new DotNetCorePackSettings
        {
            Configuration = configuration,
            NoBuild = true,
            NoRestore = true,
            OutputDirectory = "./artifacts/packages",
            IncludeSymbols = true,
            MSBuildSettings = new DotNetCoreMSBuildSettings().SetConfiguration(configuration).SetVersion(buildVersion)
        };

        DotNetCorePack("./src/Reimers.Ihe.Communication/Reimers.Ihe.Communication.csproj", packSettings);
        DotNetCorePack("./src/Reimers.Ihe.Communication.Http/Reimers.Ihe.Communication.Http.csproj", packSettings);
    });

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Pack");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
