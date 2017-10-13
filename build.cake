var target = Argument("target", "Default");
var buildnumber = Argument("buildnumber", "0");
var configuration = Argument("configuration", "Release");

var folders = new System.Collections.Generic.Dictionary<String, Cake.Common.IO.Paths.ConvertableDirectoryPath>();
folders.Add("Carrot.csproj", Directory("./src/Carrot"));
folders.Add("Carrot.NLog.csproj", Directory("./src/Carrot.NLog"));
folders.Add("Carrot.log4net.csproj", Directory("./src/Carrot.log4net"));
folders.Add("Carrot.Tests.csproj", Directory("./src/Carrot.Tests"));

Task("Clean").Does(() => {
    foreach (var folder in folders) {
	    DotNetCoreClean(folder.Value);
    }
});

Task("Restore").IsDependentOn("Clean").Does(() => {
    foreach (var folder in folders) {
        DotNetCoreRestore(folder.Value);
    }
});

Task("Version").Does(() => {
    foreach (var folder in folders.Where(_ => _.Key != "Carrot.Tests.csproj")) {
        var path = String.Concat(folder.Value, "\\", folder.Key);
        var content = System.IO.File.ReadAllText(path);
        var document = new System.Xml.XmlDocument();
        document.LoadXml(content);
        var element = document.DocumentElement["PropertyGroup"]["VersionPrefix"];
        var segments = element.InnerText.Split('.');
	    var version = String.Format("{0}.{1}.{2}", segments[0], segments[1], buildnumber);
        element.InnerText = version;
	    document.DocumentElement["PropertyGroup"]["AssemblyVersion"].InnerText = version;
	    document.DocumentElement["PropertyGroup"]["FileVersion"].InnerText = version;
        System.IO.File.WriteAllText(path, document.InnerXml);
	}
});

Task("Build").IsDependentOn("Restore").Does(() => {
    foreach (var folder in folders) {
        DotNetCoreBuild(folder.Value);
    }
});

Task("Test").IsDependentOn("Build").Does(() => {
    DotNetCoreTest(folders["Carrot.Tests.csproj"]);
});

Task("Pack").IsDependentOn("Test").Does(() => {
    foreach (var folder in folders.Where(_ => _.Key != "Carrot.Tests.csproj")) {
        DotNetCorePack(folder.Value);
    }
});

Task("Default").IsDependentOn("Test");

Task("Release").IsDependentOn("Version").IsDependentOn("Pack");

RunTarget(target);
