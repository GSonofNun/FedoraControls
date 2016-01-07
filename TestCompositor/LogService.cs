using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace TestCompositor
{
    public static class LogService
    {
        public static async void WriteLine(string line)
        {
            StorageFolder sf = ApplicationData.Current.LocalFolder;
            try
            {
                if (!await FileIOHelper.CheckFileExists("log.dat"))
                {
                    StorageFile file = await sf.CreateFileAsync("log.dat", CreationCollisionOption.OpenIfExists);
                }

                using (StreamWriter sw = File.AppendText(sf.Path + "\\log.dat"))//, CreationCollisionOption.OpenIfExists)))// await file.OpenStreamForWriteAsync()))
                {
                    sw.WriteLine(string.Format("{0} - {1}", DateTime.Now.ToString("dd/MM/yyyy-hh:mm:ss.ff t"), line));
                }
            }
            catch (Exception)
            {

            }
        }

        public static async void WriteLine(params string[] lines)
        {
            StorageFolder sf = ApplicationData.Current.LocalFolder;
            try
            {
                if (!await FileIOHelper.CheckFileExists("log.dat"))
                {
                    StorageFile file = await sf.CreateFileAsync("log.dat", CreationCollisionOption.OpenIfExists);
                }

                using (StreamWriter sw = File.AppendText(sf.Path + "\\log.dat"))//, CreationCollisionOption.OpenIfExists)))// await file.OpenStreamForWriteAsync()))
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var item in lines)
                    {
                        sb.AppendFormat("{0},", item);
                    }
                    sb.Remove(sb.Length - 1, 1);
                    sw.WriteLine(sb.ToString());
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
