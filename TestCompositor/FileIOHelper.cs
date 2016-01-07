using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace TestCompositor
{
    public static class FileIOHelper
    {
        private static AsyncStorageService service = new AsyncStorageService();
        

        public static async Task<string> WriteStringToFile(string content, string fileName, string path = null)
        {
            StorageFolder sf = null;

            if (path != null)
            {
                string folderPath = path;
                if (await service.DirectoryExistsAsync(folderPath))
                {
                    sf = await ApplicationData.Current.LocalFolder.GetFolderAsync(path);
                }
                else
                {
                    sf = await ApplicationData.Current.LocalFolder.CreateFolderAsync(path);
                }
            }
            else
            {
                sf = ApplicationData.Current.LocalFolder;
            }



            string localPath = "";

            try
            {
                StorageFile file = await sf.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                using (StreamWriter sw = new StreamWriter(await file.OpenStreamForWriteAsync()))
                {
                    sw.Write(content);
                }
                if (path != null)
                {
                    localPath += path;
                }
                localPath += "\\" + fileName;
            }
            catch (Exception)
            {

            }



            return localPath;
        }

        public static async void DeleteFile(string fileName, string path = null)
        {
            StorageFolder sf = ApplicationData.Current.LocalFolder;
            StringBuilder fullPath = new StringBuilder();
            if (path != null)
            {
                fullPath.Append(path);
                fullPath.Append("\\");
            }
            //fullPath.Append("\\");
            fullPath.Append(fileName);

            if (await service.FileExistsAsync(fullPath.ToString()))
            {
                await service.DeleteFileAsync(fullPath.ToString());
            }
        }

        public static async Task<bool> CheckFileExists(string fileName, string path = null)
        {
            StringBuilder fullPath = new StringBuilder();
            if (path != null)
            {
                //fullPath.Append("\\");
                fullPath.Append(path);
                fullPath.Append("\\");
            }

            fullPath.Append(fileName);
            if (await service.FileExistsAsync(fullPath.ToString()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
