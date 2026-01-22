using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;

//#if UNITY_2017_1_OR_NEWER
//using UnityEditor.Build.Reporting;
//#endif
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace CrowRx.Utility.Editor
{
    public class AppVersion
    {
        public static readonly uint MaxVersionCode = 2100000000;

        public const byte MaxMajor = 21;
        public const byte MaxMinor = 99;
        public const byte MaxRevision = 99;
        public const ushort MaxBuild = 9999;

        private static AppVersion _current = null;

        public static AppVersion Current
        {
            get
            {
                if (_current == null)
                {
                    _current = new AppVersion();
                    _current.Read();
                }

                return _current;
            }
        }

        private byte _major = 0;
        private byte _minor = 0;
        private byte _revision = 0;
        private ushort _build = 0;

        public byte Major
        {
            get => _major;
            set
            {
                if (value > MaxMajor)
                {
                    Debug.LogErrorFormat("[AppVersion.{0}] can not be <{1}> bigger than [{2}:{3}]", "Major", value, "MaxMajor", MaxMajor);
                    return;
                }

                _major = value;
            }
        }

        public byte Minor
        {
            get => _minor;
            set
            {
                if (value > MaxMinor)
                {
                    Debug.LogErrorFormat("[AppVersion.{0}] can not be <{1}> bigger than [{2}:{3}]", "Minor", value, "MaxMinor", MaxMinor);
                    return;
                }

                _minor = value;
            }
        }

        public byte Revision
        {
            get => _revision;
            set
            {
                if (value > MaxRevision)
                {
                    Debug.LogErrorFormat("[AppVersion.{0}] can not be <{1}> bigger than [{2}:{3}]", "Revision", value, "MaxRevision", MaxRevision);
                    return;
                }

                _revision = value;
            }
        }

        public ushort Build
        {
            get => _build;
            set
            {
                if (value > MaxBuild)
                {
                    Debug.LogErrorFormat("[AppVersion.{0}] can not be <{1}> bigger than [{2}:{3}]", "Build", value, "MaxBuild", MaxBuild);
                    return;
                }

                _build = value;
            }
        }

        public string Version => $"{Major}.{Minor}.{Revision}.{Build}";

        public int VersionCode => int.Parse($"{Major:00}{Minor:00}{Revision:00}{Build:0000}");

        public void Read()
        {
            var currentVersion = PlayerSettings.bundleVersion;
            if (string.IsNullOrEmpty(currentVersion) || currentVersion.Split('.').Length < 4)
                currentVersion = "0.0.0.0";

            var currentVersions = currentVersion.Split('.');

            Major = byte.Parse(currentVersions[0]);
            Minor = byte.Parse(currentVersions[1]);
            Revision = byte.Parse(currentVersions[2]);
            Build = ushort.Parse(currentVersions[3]);
        }

        public void Apply()
        {
            PlayerSettings.bundleVersion = Version;

            PlayerSettings.Android.bundleVersionCode = VersionCode;
            PlayerSettings.iOS.buildNumber = VersionCode.ToString();
        }
    }

    /*
#if UNITY_2017_1_OR_NEWER
    public class PostBuildPlayer : IPostprocessBuildWithReport
#else
    public class PostBuildPlayer : IPostprocessBuild
#endif
    {
        public int callbackOrder
        {
            get
            {
                return (int)PostprocessBuildOrder.iOSXCodeVersioning;
            }
        }

#if UNITY_2017_1_OR_NEWER
        public void OnPostprocessBuild(BuildReport report)
        {
            var target = report.summary.platform;
            var pathToBuiltProject = report.summary.outputPath;
#else
        public void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
#endif
            if (target != BuildTarget.iOS)
                return;

#if UNITY_IOS
            Debug.Log("iOS >> PostprocessBuildVersion.OnPostprocessBuild");

            AppVersion.Current.Read();

            // Initialize PbxProject
            //var projectPath = pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj";
            //PBXProject pbxProject = new PBXProject();
            //pbxProject.ReadFromFile(projectPath);
            //string targetGuid = pbxProject.TargetGuidByName("Unity-iPhone");

            var plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
            var plist = new PlistDocument();
            plist.ReadFromFile(plistPath);

            // Version
            plist.root.SetString("CFBundleShortVersionString", AppVersion.Current.VersionCode.ToString());
            plist.root.SetString("CFBundleVersion", AppVersion.Current.Version);

            // <수출 규정 관련 문서가 누락됨> 방지
            plist.root.SetBoolean("ITSAppUsesNonExemptEncryption", false);

            plist.WriteToFile(plistPath);
#endif
        }
    }
    */
}