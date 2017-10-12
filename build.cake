var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var projectDir = Directory("./src/Carrot");
var testsDir = Directory("./src/Carrot.Tests");

Task("Clean").Does(() => {
    DotNetCoreClean(projectDir);
    DotNetCoreClean(testsDir);
});

Task("Restore").IsDependentOn("Clean").Does(() => {
    DotNetCoreRestore(projectDir);
    DotNetCoreRestore(testsDir);
});

Task("Build").IsDependentOn("Restore").Does(() => {
    DotNetCoreBuild(projectDir);
    DotNetCoreBuild(testsDir);
});

Task("Test").IsDependentOn("Build").Does(() => {
    DotNetCoreTest(testsDir);
});

Task("Default").IsDependentOn("Test");

RunTarget(target);
