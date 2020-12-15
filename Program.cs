using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using iRacing_setups.Model;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace iRacing_setups
{
    internal class Program
    {
        private const string IRACING_FOLDER = "1d_xYzbgp-M8S-xJRidPyk65Na1NYtNPN";
        private const string IRACING_FOLDER_TEST = "1w43ytfea9ZzA5tQe-ZqCyPunEForkmv_";

        static void Main(string[] args)
        {
            var configuration = ReadConfiguration();
            var files = GetSetupFiles(configuration.SetupsPath);

            string[] scopes = new string[] { DriveService.Scope.Drive, DriveService.Scope.DriveFile };

            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets
            {
                ClientId = configuration.ClientId,
                ClientSecret = configuration.ClientSecret
            }, scopes, Environment.UserName, CancellationToken.None, new FileDataStore("MyAppsToken")).Result;

            DriveService service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "iRacing-setups",
            });
            service.HttpClient.Timeout = TimeSpan.FromMinutes(100);

            int cont = 0;
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                SaveFileToBackup(file, configuration.SetupsPath, configuration.BackupPath);
                if (CheckIfUpload(file, configuration.IncludeFolders))
                {
                    var driveDirectoryId = GetDriveDirectoryId(service, file, configuration.SetupsPath);
                    if (FindFileOrDirectory(service, fileName, driveDirectoryId) == null)
                    {
                        UploadFile(service, file, driveDirectoryId);
                        Console.WriteLine($"[{++cont}/{files.Count}] [UPLOADED] {fileName}");
                    }
                    else
                    {
                        Console.WriteLine($"[{++cont}/{files.Count}] [ALREADY FOUND] {fileName}");
                    }
                }
                else
                {
                    Console.WriteLine($"[{++cont}/{files.Count}] [BACKUP ONLY] {fileName}");
                }
            }
        }

        private static ConfigDTO ReadConfiguration()
        {
            var location = Assembly.GetExecutingAssembly().Location;
            var currentPath = Path.GetDirectoryName(location);
            var configPath = Path.Combine(currentPath, "configuration.json");

            if (!System.IO.File.Exists(configPath))
                throw new FileNotFoundException(configPath);

            var file = System.IO.File.ReadAllText(configPath);
            return JsonConvert.DeserializeObject<ConfigDTO>(file);
        }

        private static List<string> GetSetupFiles(string path)
        {
            if (!Directory.Exists(path))
                throw new FileNotFoundException(path);

            return Directory.EnumerateFiles(path, "*.sto", SearchOption.AllDirectories).ToList();
        }

        private static void SaveFileToBackup(string file, string setupPath, string backupPath)
        {
            var newFile = file.Replace(setupPath, backupPath);
            var newDir = Path.GetDirectoryName(newFile);
            if (!Directory.Exists(newDir))
                Directory.CreateDirectory(newDir);
            System.IO.File.Copy(file, newFile, true);
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

        private static Google.Apis.Drive.v2.Data.File UploadFile(DriveService service, string uploadFile, string parent)
        {
            if (!System.IO.File.Exists(uploadFile))
                throw new FileNotFoundException(uploadFile);

            Google.Apis.Drive.v2.Data.File body = new Google.Apis.Drive.v2.Data.File();
            body.Title = Path.GetFileName(uploadFile);
            body.MimeType = GetMimeType(uploadFile);
            body.Parents = new List<ParentReference>() { new ParentReference() { Id = parent } };

            byte[] byteArray = System.IO.File.ReadAllBytes(uploadFile);
            MemoryStream stream = new MemoryStream(byteArray);

            FilesResource.InsertMediaUpload request = service.Files.Insert(body, stream, GetMimeType(uploadFile));
            request.Upload();
            return request.ResponseBody;
        }

        public static string CreateDirectory(DriveService service, string name, string parent)
        {
            var body = new Google.Apis.Drive.v2.Data.File();
            body.Title = name;
            body.MimeType = "application/vnd.google-apps.folder";
            body.Parents = new List<ParentReference>() { new ParentReference() { Id = parent } };

            FilesResource.InsertRequest request = service.Files.Insert(body);
            var newDirectory = request.Execute();

            return newDirectory.Id;
        }

        public static string FindFileOrDirectory(DriveService service, string name, string parent)
        {
            FilesResource.ListRequest request = service.Files.List();
            request.Q = $"trashed=false and title='{name}' and parents in '{parent}'";
            var response = request.Execute();

            if (response.Items.Count == 0)
                return null;

            var gDriveFolder = response.Items.First();
            return gDriveFolder.Id;
        }

        private static string GetMimeType(string fileName)
        {
            string mimeType = "application/unknown";
            string ext = Path.GetExtension(fileName).ToLower();
            var regKey = Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
            return mimeType;
        }

        private static string GetDriveDirectoryId(DriveService service, string file, string setupsPath)
        {
            var dirName = Path.GetDirectoryName(file);
            var folders = dirName.Replace($"{setupsPath}\\", "");
            var arrayFolders = folders.Split("\\");
            var folderId = IRACING_FOLDER;
            foreach (var folder in arrayFolders)
            {
                folderId = FindOrCreateDirectory(service, folder, folderId);
            }
            return folderId;
        }

        private static string FindOrCreateDirectory(DriveService service, string name, string parent)
        {
            var directoryFound = FindFileOrDirectory(service, name, parent);

            if (directoryFound != null)
                return directoryFound;

            return CreateDirectory(service, name, parent);
        }
    }
}
