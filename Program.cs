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
        static void Main(string[] args)
        {
            var configuration = ReadConfiguration();
            var files = GetSetupFiles(configuration.SetupsPath);

            int cont = 0;
            foreach (var file in files)
            {
                cont++;
                var newFile = file.Replace(configuration.SetupsPath, configuration.BackupPath);
                var newDir = Path.GetDirectoryName(newFile);
                if (!Directory.Exists(newDir))
                {
                    Directory.CreateDirectory(newDir);
                }
                File.Copy(file, newFile, true);
                Console.WriteLine($"[{cont}/{files.Count}] {newFile}");
            }

            //string[] scopes = new string[] { DriveService.Scope.Drive, DriveService.Scope.DriveFile };

            //var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets
            //{
            //    ClientId = configuration.ClientId,
            //    ClientSecret = configuration.ClientSecret
            //}, scopes, Environment.UserName, CancellationToken.None, new FileDataStore("MyAppsToken")).Result;

            //DriveService service = new DriveService(new BaseClientService.Initializer()
            //{
            //    HttpClientInitializer = credential,
            //    ApplicationName = "iRacing-setups",
            //});
            //service.HttpClient.Timeout = TimeSpan.FromMinutes(100);

            //foreach (var file in files)
            //{
            //    UploadFile(service, file, "iRacing Setups");
            //}
        }

        private static ConfigDTO ReadConfiguration()
        {
            var location = Assembly.GetExecutingAssembly().Location;
            var currentPath = Path.GetDirectoryName(location);
            var configPath = Path.Combine(currentPath, "configuration.json");

            if (!File.Exists(configPath))
            {
                throw new FileNotFoundException(configPath);
            }

            var file = File.ReadAllText(configPath);
            return JsonConvert.DeserializeObject<ConfigDTO>(file);
        }

        private static List<string> GetSetupFiles(string path)
        {
            if (!Directory.Exists(path))
            {
                throw new FileNotFoundException(path);
            }

            return Directory.EnumerateFiles(path, "*.sto", SearchOption.AllDirectories).ToList();
        }

        //private static Google.Apis.Drive.v2.Data.File UploadFile(DriveService service, string uploadFile, string parent)
        //{
        //    if (!System.IO.File.Exists(uploadFile))
        //    {
        //        throw new FileNotFoundException(uploadFile);
        //    }

        //    Google.Apis.Drive.v2.Data.File body = new Google.Apis.Drive.v2.Data.File();
        //    body.Title = Path.GetFileName(uploadFile);
        //    body.MimeType = GetMimeType(uploadFile);
        //    body.Parents = new List<ParentReference>() { new ParentReference() { Id = parent } };

        //    byte[] byteArray = System.IO.File.ReadAllBytes(uploadFile);
        //    MemoryStream stream = new MemoryStream(byteArray);

        //    FilesResource.InsertMediaUpload request = service.Files.Insert(body, stream, GetMimeType(uploadFile));
        //    request.Upload();
        //    return request.ResponseBody;
        //}

        //private static string GetMimeType(string fileName)
        //{
        //    string mimeType = "application/unknown";
        //    string ext = Path.GetExtension(fileName).ToLower();
        //    var regKey = Registry.ClassesRoot.OpenSubKey(ext);
        //    if (regKey != null && regKey.GetValue("Content Type") != null)
        //        mimeType = regKey.GetValue("Content Type").ToString();
        //    return mimeType;
        //}
    }
}
