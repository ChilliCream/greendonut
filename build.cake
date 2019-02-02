#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0
#addin "nuget:?package=Cake.Sonar"
#tool "nuget:?package=MSBuild.SonarQube.Runner.Tool"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var sonarLogin = Argument("sonarLogin", default(string));
var sonarPrKey = Argument("sonarPrKey", default(string));
var sonarBranch = Argument("sonarBranch", default(string));
var sonarBranchBase = Argument("sonarBranch", default(string));
var packageVersion = Argument("packageVersion", default(string));

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////
var testOutputDir = Directory("./testoutput");
var publishOutputDir = Directory("./artifacts");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////
Task("EnvironmentSetup")
    .Does(() =>
{
    if(string.IsNullOrEmpty(packageVersion))
    {
        packageVersion = EnvironmentVariable("CIRCLE_TAG")
            ?? EnvironmentVariable("APPVEYOR_REPO_TAG_NAME")
            ?? EnvironmentVariable("Version");
    }
    Environment.SetEnvironmentVariable("Version", packageVersion);

    if(string.IsNullOrEmpty(sonarPrKey))
    {
        sonarPrKey = EnvironmentVariable("APPVEYOR_PULL_REQUEST_NUMBER");
        sonarBranch = EnvironmentVariable("APPVEYOR_PULL_REQUEST_HEAD_REPO_BRANCH");
        sonarBranchBase = EnvironmentVariable("APPVEYOR_REPO_BRANCH");
    }

    if(string.IsNullOrEmpty(sonarLogin))
    {
        sonarLogin = EnvironmentVariable("SONAR_TOKEN");
    }
});

Task("Clean")
    .IsDependentOn("EnvironmentSetup")
    .Does(() =>
{
    DotNetCoreClean("./src");
});

Task("Restore")
    .IsDependentOn("EnvironmentSetup")
    .Does(() =>
{
    DotNetCoreRestore("./src/GreenDonut.sln");
});

Task("Build")
    .IsDependentOn("EnvironmentSetup")
    .Does(() =>
{
    var settings = new DotNetCoreBuildSettings
    {
        Configuration = configuration,
    };

    DotNetCoreBuild("./src/GreenDonut.sln", settings);
});

Task("Publish")
    .IsDependentOn("EnvironmentSetup")
    .Does(() =>
{
    using(var process = StartAndReturnProcess("msbuild",
        new ProcessSettings{ Arguments = "./src/GreenDonut.sln /t:restore /p:configuration=" + configuration }))
    {
        process.WaitForExit();
    }

    using(var process = StartAndReturnProcess("msbuild",
        new ProcessSettings{ Arguments = "./src/GreenDonut.sln /t:build /p:configuration=" + configuration }))
    {
        process.WaitForExit();
    }

    using(var process = StartAndReturnProcess("msbuild",
        new ProcessSettings{ Arguments = "./src/GreenDonut.sln /t:pack /p:configuration=" + configuration + " /p:IncludeSource=true /p:IncludeSymbols=true" }))
    {
        process.WaitForExit();
    }
});

Task("Tests")
    .IsDependentOn("EnvironmentSetup")
    .Does(() =>
{
    var buildSettings = new DotNetCoreBuildSettings
    {
        Configuration = "Debug"
    };

    int i = 0;
    var testSettings = new DotNetCoreTestSettings
    {
        Configuration = "Debug",
        ResultsDirectory = $"./{testOutputDir}",
        Logger = "trx",
        NoRestore = true,
        NoBuild = true,
        ArgumentCustomization = args => args
            .Append("/p:CollectCoverage=true")
            .Append("/p:Exclude=[xunit.*]*")
            .Append("/p:CoverletOutputFormat=opencover")
            .Append($"/p:CoverletOutput=\"../../{testOutputDir}/full_{i++}\" --blame")
    };

    DotNetCoreBuild("./src/GreenDonut.sln", buildSettings);

    foreach(var file in GetFiles("./src/**/*.Tests.csproj"))
    {
        DotNetCoreTest(file.FullPath, testSettings);
    }
});

Task("SonarBegin")
    .IsDependentOn("EnvironmentSetup")
    .Does(() =>
{
    SonarBegin(new SonarBeginSettings
    {
        Url = "https://sonarcloud.io",
        Login = sonarLogin,
        Key = "GreenDonut",
        Organization = "chillicream",
        VsTestReportsPath = "**/*.trx",
        OpenCoverReportsPath = "**/*.opencover.xml",
        Exclusions = "**/*.js,**/*.html,**/*.css,**/src/Benchmark.Tests/**/*.*",
        Verbose = false,
        Version = packageVersion,
        ArgumentCustomization = args =>
        {
            var a = args;

            if(!string.IsNullOrEmpty(sonarPrKey))
            {
                a = a.Append($"/d:sonar.pullrequest.key=\"{sonarPrKey}\"");
                a = a.Append($"/d:sonar.pullrequest.branch=\"{sonarBranch}\"");
                a = a.Append($"/d:sonar.pullrequest.base=\"{sonarBranchBase}\"");
                a = a.Append($"/d:sonar.pullrequest.provider=\"github\"");
                a = a.Append($"/d:sonar.pullrequest.github.repository=\"ChilliCream/greendonut\"");
                // a = a.Append($"/d:sonar.pullrequest.github.endpoint=\"https://api.github.com/\"");
            }

            return a;
        }
    });
});

Task("SonarEnd")
    .Does(() =>
{
    SonarEnd(new SonarEndSettings
    {
        Login = sonarLogin,
    });
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////
Task("Default")
    .IsDependentOn("Tests");

Task("Sonar")
    .IsDependentOn("SonarBegin")
    .IsDependentOn("Tests")
    .IsDependentOn("SonarEnd");

Task("Release")
    .IsDependentOn("Sonar")
    .IsDependentOn("Publish");


//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////
RunTarget(target);
