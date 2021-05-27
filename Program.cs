using iRacing_setups.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace iRacing_setups
{
    internal class Program
    {
        static void Main()
        {
            try
            {
                var configuration = ReadConfiguration();
                var files = GetSetupFiles(configuration.SetupsPath);

                int cont = 0;
                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file);
                    SaveFile(file, configuration.SetupsPath, configuration.BackupPath);
                    if (CheckIfUpload(file, configuration.IncludeFolders))
                    {
                        SaveFile(file, configuration.SetupsPath, configuration.DrivePath);
                        Console.WriteLine($"[{++cont}/{files.Count}] [FILE SYNCED] {fileName}");
                    }
                    else
                    {
                        Console.WriteLine($"[{++cont}/{files.Count}] [BACKUP ONLY] {fileName}");
                    }
                }

                Console.WriteLine($"Included folders: {configuration.IncludeFolders}");
                Console.WriteLine("Done!");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.ReadLine();
            }
        }

        private static ConfigDTO ReadConfiguration()
        {
            var location = Assembly.GetExecutingAssembly().Location;
            var currentPath = Path.GetDirectoryName(location);
            var configPath = Path.Combine(currentPath, "configuration.json");

            if (!File.Exists(configPath))
                throw new FileNotFoundException(configPath);

            var file = File.ReadAllText(configPath);
            return JsonConvert.DeserializeObject<ConfigDTO>(file);
        }

        private static List<string> GetSetupFiles(string path)
        {
            if (!Directory.Exists(path))
                throw new FileNotFoundException(path);

            return Directory.EnumerateFiles(path, "*.sto", SearchOption.AllDirectories).ToList();
        }

        private static void SaveFile(string file, string setupPath, string savePath)
        {
            var newFile = file.Replace(setupPath, savePath);
            var newDir = Path.GetDirectoryName(newFile);
            if (!Directory.Exists(newDir))
                Directory.CreateDirectory(newDir);
            File.Copy(file, newFile, true);
        }

        private static bool CheckIfUpload(string file, string includeFolders)
        {
            var includeArray = includeFolders.Split(',');
            foreach (var item in includeArray)
            {
                if (file.Contains(item))
                    return true;
            }
            return false;
        }
    }
}
