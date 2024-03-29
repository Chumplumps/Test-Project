﻿// -------------------------------------------------------------------------------------------------
// Assets/Editor/JenkinsBuild.cs
// -------------------------------------------------------------------------------------------------
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.Build.Reporting;

// ------------------------------------------------------------------------
// https://docs.unity3d.com/Manual/CommandLineArguments.html
// ------------------------------------------------------------------------
public class JenkinsBuild
{

    static string[] EnabledScenes = FindEnabledEditorScenes();
    // ------------------------------------------------------------------------
    // called from Jenkins
    // ------------------------------------------------------------------------
    [MenuItem("Build/Windows")]
    public static void BuildWindows()
    {
        string appName = "-TestProject-Release";
        string targetDir = "Builds/Windows";
        string fullPathAndName = targetDir + System.IO.Path.DirectorySeparatorChar + appName + ".apk";
        BuildProject(EnabledScenes, fullPathAndName, BuildTargetGroup.Standalone, BuildTarget.Android, BuildOptions.None);
    }
    [MenuItem("Build/Android")]
    public static void BuildAndroid()
    {
        string appName = "TestProject-Release";
        string targetDir = "Builds/Android";
        string fullPathAndName = targetDir + System.IO.Path.DirectorySeparatorChar + appName + ".apk";
        BuildProject(EnabledScenes, fullPathAndName, BuildTargetGroup.Standalone, BuildTarget.Android, BuildOptions.None);
    }

    [MenuItem("Build/MacOS")]
    public static void BuildMacOS()
    {

        string appName = "TestProject";
        string targetDir = "~/Desktop";

        // find: -executeMethod
        //   +1: JenkinsBuild.BuildMacOS
        //   +2: VRDungeons
        //   +3: /Users/Shared/Jenkins/Home/jobs/VRDungeons/builds/47/output
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-executeMethod")
            {
                if (i + 3 < args.Length)
                {
                    // BuildMacOS method is args[i+1]
                    appName = args[i + 2];
                    targetDir = args[i + 3];
                }
                else
                {
                    for (int x = 0; x < args.Length; x++)
                    {
                        System.Console.WriteLine("Argument " + x + ": " + args[x]);
                    }
                    throw new System.Exception("[JenkinsBuild] Incorrect Parameters for -executeMethod Format: -executeMethod BuildMacOS <app name> <output dir>");
                }
            }
        }

        // e.g. // /Users/Shared/Jenkins/Home/jobs/VRDungeons/builds/47/output/VRDungeons.app
        string fullPathAndName = targetDir + System.IO.Path.DirectorySeparatorChar + appName + ".app";
        BuildProject(EnabledScenes, fullPathAndName, BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX, BuildOptions.None);
    }

    // ------------------------------------------------------------------------
    // ------------------------------------------------------------------------
    private static string[] FindEnabledEditorScenes()
    {

        List<string> EditorScenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
            {
                EditorScenes.Add(scene.path);
            }
        }
        return EditorScenes.ToArray();
    }

    // ------------------------------------------------------------------------
    // e.g. BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX
    // ------------------------------------------------------------------------
    private static void BuildProject(string[] scenes, string targetDir, BuildTargetGroup buildTargetGroup, BuildTarget buildTarget, BuildOptions buildOptions)
    {
        PlayerSettings.keyaliasPass = "Qpalzm.10";
        PlayerSettings.keystorePass = "Qpalzm.10";

        System.Console.WriteLine("[JenkinsBuild] Building:" + targetDir + " buildTargetGroup:" + buildTargetGroup.ToString() + " buildTarget:" + buildTarget.ToString());

        // https://docs.unity3d.com/ScriptReference/EditorUserBuildSettings.SwitchActiveBuildTarget.html
        bool switchResult = EditorUserBuildSettings.SwitchActiveBuildTarget(buildTargetGroup, buildTarget);
        if (switchResult)
        {
            System.Console.WriteLine("[JenkinsBuild] Successfully changed Build Target to: " + buildTarget.ToString());
        }
        else
        {
            System.Console.WriteLine("[JenkinsBuild] Unable to change Build Target to: " + buildTarget.ToString() + " Exiting...");
            return;
        }

        // https://docs.unity3d.com/ScriptReference/BuildPipeline.BuildPlayer.html
        BuildReport buildReport = BuildPipeline.BuildPlayer(scenes, targetDir, buildTarget, buildOptions);
        BuildSummary buildSummary = buildReport.summary;
        if (buildSummary.result == BuildResult.Succeeded)
        {
            System.Console.WriteLine("[JenkinsBuild] Build Success: Time:" + buildSummary.totalTime + " Size:" + buildSummary.totalSize + " bytes");
        }
        else
        {
            System.Console.WriteLine("[JenkinsBuild] Build Failed: Time:" + buildSummary.totalTime + " Total Errors:" + buildSummary.totalErrors);
        }
    }
}