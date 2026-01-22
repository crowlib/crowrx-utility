using System.IO;
using UnityEditor;


namespace CrowRx.Utility.Editor
{
    public static class FileIO
    {
        public static void CopyDirectory(string sourcePath, string outputPath)
        {
            if (!Directory.Exists(sourcePath))
            {
                Log.Info($"<CopyDirectory> source:[{sourcePath}] is not exist.");

                return;
            }

            if (Directory.Exists(outputPath))
            {
                FileUtil.DeleteFileOrDirectory(outputPath);
            }

            CopyDirectoryRecursively(sourcePath, outputPath);

            EditorUtility.ClearProgressBar();
        }

        private static void CopyDirectoryRecursively(string srcPath, string dstPath)
        {
            if (Directory.Exists(dstPath))
            {
                Directory.Delete(dstPath);
            }

            Directory.CreateDirectory(dstPath);

            string[] srcFiles = Directory.GetFiles(srcPath, "*.*");
            int srcFileCount = srcFiles.Length;

            for (int i = 0; i < srcFileCount; ++i)
            {
                string srcFile = srcFiles[i];
                string dstFile = Path.Combine(dstPath, Path.GetFileName(srcFile));

                EditorUtility.DisplayProgressBar("Copy Files", srcFile, (float)i / srcFileCount);

                File.Copy(srcFile, dstFile, true);
            }

            string[] srcSubDirs = Directory.GetDirectories(srcPath);
            foreach (string srcSubDir in srcSubDirs)
            {
                CopyDirectoryRecursively(srcSubDir, Path.Combine(dstPath, Path.GetFileName(srcSubDir)));
            }
        }
    }
}