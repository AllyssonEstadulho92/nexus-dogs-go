#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

namespace NexusDogsGo.EditorTools
{
    public static class AndroidProjectConfigurator
    {
        private const string ProductName = "NEXUS DOGS GO";
        private const string CompanyName = "AllyssonEstadulho92";
        private const string ApplicationId = "com.allyssonestadulho92.nexusdogsgo";
        private const string VersionName = "0.2.0";
        private const int VersionCode = 2;
        private const string PrototypeScene = "Assets/Scenes/Prototype.unity";

        [MenuItem("NEXUS DOGS GO/Configure/Configure Android Project")]
        public static void ConfigureAndroidProject()
        {
            PlayerSettings.companyName = CompanyName;
            PlayerSettings.productName = ProductName;
            PlayerSettings.bundleVersion = VersionName;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, ApplicationId);

            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.Android.bundleVersionCode = VersionCode;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            // AR Foundation 6.0/ARCore baseline: use OpenGL ES 3 to avoid a Vulkan dependency
            // until the project is explicitly upgraded to the newer Vulkan AR path.
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.OpenGLES3 });

            if (File.Exists(PrototypeScene))
                EnsureSceneInBuild(PrototypeScene);

            AssetDatabase.SaveAssets();
            Debug.Log(
                "NEXUS DOGS GO Android configuration applied. " +
                "Package=" + ApplicationId +
                ", version=" + VersionName + " (" + VersionCode + ")" +
                ", minSdk=24, architecture=ARM64, backend=IL2CPP, graphics=OpenGLES3.");
        }

        [MenuItem("NEXUS DOGS GO/Configure/Validate Android Project")]
        public static void ValidateAndroidProject()
        {
            var errors = 0;
            errors += Check(PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android) == ApplicationId,
                "Application ID must be " + ApplicationId + ".");
            errors += Check(PlayerSettings.Android.minSdkVersion >= AndroidSdkVersions.AndroidApiLevel24,
                "Minimum Android API must be 24 or newer for ARCore functionality.");
            errors += Check(PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android) == ScriptingImplementation.IL2CPP,
                "Android scripting backend must be IL2CPP.");
            errors += Check((PlayerSettings.Android.targetArchitectures & AndroidArchitecture.ARM64) != 0,
                "ARM64 must be enabled.");
            errors += Check(PlayerSettings.GetGraphicsAPIs(BuildTarget.Android).Contains(GraphicsDeviceType.OpenGLES3),
                "OpenGL ES 3 must be enabled for the current AR baseline.");
            errors += Check(File.Exists(PrototypeScene),
                "Prototype scene is missing. Run NEXUS DOGS GO > Build Playable Vertical Slice first.");

            if (errors == 0)
                Debug.Log("NEXUS DOGS GO Android configuration validation passed.");
            else
                Debug.LogError("NEXUS DOGS GO Android configuration has " + errors + " issue(s). See the Console.");
        }

        [MenuItem("NEXUS DOGS GO/Build/Development APK")]
        public static void BuildDevelopmentApk()
        {
            ConfigureAndroidProject();
            EditorUserBuildSettings.buildAppBundle = false;
            BuildAndroid("Builds/Android/NexusDogsGo-dev.apk", BuildOptions.Development | BuildOptions.AllowDebugging);
        }

        [MenuItem("NEXUS DOGS GO/Build/Release AAB")]
        public static void BuildReleaseAab()
        {
            ConfigureAndroidProject();
            EditorUserBuildSettings.buildAppBundle = true;
            BuildAndroid("Builds/Android/NexusDogsGo-release.aab", BuildOptions.None);
        }

        private static void BuildAndroid(string outputPath, BuildOptions options)
        {
            if (!File.Exists(PrototypeScene))
                throw new InvalidOperationException(
                    "Prototype scene not found. Run NEXUS DOGS GO > Build Playable Vertical Slice before building Android.");

            EnsureSceneInBuild(PrototypeScene);
            var scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled && File.Exists(scene.path))
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
                throw new InvalidOperationException("No enabled scenes are available for the Android build.");

            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = options
            });

            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException("Android build failed: " + report.summary.result);

            Debug.Log("Android build completed: " + outputPath + " (" + report.summary.totalSize + " bytes)");
        }

        private static void EnsureSceneInBuild(string scenePath)
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            var existing = scenes.FindIndex(scene => string.Equals(scene.path, scenePath, StringComparison.Ordinal));
            if (existing >= 0)
            {
                if (!scenes[existing].enabled)
                    scenes[existing] = new EditorBuildSettingsScene(scenePath, true);
            }
            else
            {
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            }
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static int Check(bool condition, string message)
        {
            if (condition) return 0;
            Debug.LogError("[Android Config] " + message);
            return 1;
        }
    }
}
#endif
