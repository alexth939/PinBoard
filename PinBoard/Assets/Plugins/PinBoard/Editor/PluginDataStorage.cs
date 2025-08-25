using System;
using System.IO;
using UnityEngine;

namespace PinBoard.Editor
{
    internal static class PluginDataStorage
    {
        private const string LibraryFolderName = "Library";
        private const string PinBoardPageFileName = "PinBoardPage.json";
        private const string PluginFolderName = "PinBoard";

        private static Lazy<string> LazyLibraryFolderPath = new(GetLibraryFolderPath);
        private static Lazy<string> LazyPageFilePath = new(() => Path.Combine(PluginFolderPath, PinBoardPageFileName));
        private static Lazy<string> LazyPluginFolderPath = new(() => Path.Combine(LibraryFolderPath, PluginFolderName));

        private static string LibraryFolderPath => LazyLibraryFolderPath.Value;
        private static string PageFilePath => LazyPageFilePath.Value;
        private static string PluginFolderPath => LazyPluginFolderPath.Value;

        public static PinBoardPage LoadOrCreatePinBoardPage()
        {
            if(File.Exists(PageFilePath))
            {
                string json = File.ReadAllText(PageFilePath);
                PinBoardPage page = JsonUtility.FromJson<PinBoardPage>(json);

                if(page != null)
                    return page;
            }

            PinBoardPage newPage = new();
            SavePinBoardPage(newPage);

            return newPage;
        }

        public static void SavePinBoardPage(PinBoardPage page)
        {
            if(Directory.Exists(PluginFolderPath) is false)
                Directory.CreateDirectory(PluginFolderPath);

            string json = JsonUtility.ToJson(page, prettyPrint: true);
            File.WriteAllText(PageFilePath, json);
        }

        private static string GetLibraryFolderPath()
        {
            string assetsPath = Application.dataPath;
            string projectRoot = Directory.GetParent(assetsPath).FullName;
            string libraryPath = Path.Combine(projectRoot, LibraryFolderName);

            return libraryPath;
        }
    }
}
