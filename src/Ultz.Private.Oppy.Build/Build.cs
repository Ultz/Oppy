// This file is part of Oppy.
// 
// You may modify and distribute Oppy under the terms
// of the MIT license. See the LICENSE file for details.

using System;
using System.IO;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
class Build : NukeBuild
{
    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [GitRepository] readonly GitRepository GitRepository;

    [Solution] readonly Solution Solution;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath TestsDirectory => RootDirectory / "tests";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(ArtifactsDirectory);
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });

    Target BuildDistribution => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            const string tfm = "netcoreapp3.1";

            var rids = new[] {"win-x64", "linux-x64", "osx-x64"};
            var proj = Solution.GetProject("Ultz.Oppy") ?? throw new InvalidOperationException();

            var template = ArtifactsDirectory / "installation_structure";
            foreach (var rid in rids)
            {
                var outputDir = proj.Directory / "bin" / Configuration / tfm / rid / "publish";
                var rootInstallDir = ArtifactsDirectory / rid;
                var installDir = rootInstallDir / "bin";

                if (Directory.Exists(installDir))
                {
                    Directory.Delete(installDir, true);
                }

                if (!Directory.Exists(rootInstallDir))
                {
                    Directory.CreateDirectory(rootInstallDir);
                }

                Directory.CreateDirectory(installDir);
                CopyDirectoryRecursively(template, rootInstallDir, DirectoryExistsPolicy.Merge,
                    FileExistsPolicy.Overwrite);

                if (Directory.Exists(outputDir))
                {
                    Directory.Delete(outputDir, true);
                }

                DotNetPublish(s => s
                    .SetProject(proj)
                    .SetFramework(tfm)
                    .SetRuntime(rid)
                    .SetConfiguration(Configuration)
                    .EnableNoRestore()
                    .EnableSelfContained());

                CopyDirectoryRecursively(outputDir, installDir, DirectoryExistsPolicy.Merge,
                    FileExistsPolicy.Overwrite);
            }
        });

    /// Support plugins are available for:
    /// - JetBrains ReSharper        https://nuke.build/resharper
    /// - JetBrains Rider            https://nuke.build/rider
    /// - Microsoft VisualStudio     https://nuke.build/visualstudio
    /// - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<Build>(x => x.Compile);
}