using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TransJakartaLocator.Model;
using Windows.Storage;

namespace TransJakartaLocator.Utils
{
    class FileReader
    {
        public async Task<List<Shelter>> GetData()
        {
            StorageFolder local = Windows.Storage.ApplicationData.Current.LocalFolder;
            List<Shelter> shelters = new List<Shelter>();

            if (local != null)
            {
                // Get the DataFolder folder.
                var dataFolder = await local.GetFolderAsync("Assets/Data");

                // Get the file.
                var file = await dataFolder.OpenStreamForReadAsync("data.csv");

                // Read the data.
                using (StreamReader streamReader = new StreamReader(file))
                {
                    string raw = await streamReader.ReadToEndAsync();

                    string[] datas = raw.Split('\n');

                    foreach (string item in datas)
                    {
                        string[] temp = item.Split(',');

                        Shelter shelter = new Shelter();
                        shelter.Name = temp[0];
                        shelter.Latitude = int.Parse(temp[1]);
                        shelter.Longitude = int.Parse(temp[2]);

                        shelters.Add(shelter);
                    }
                }

            }
            return shelters;
        }

        public static string ReadFile()
        {
            var ResrouceStream = Application.GetResourceStream(new Uri(@"Assets\Data\data.csv", UriKind.Relative));
            if (ResrouceStream != null)
            {
                Stream myFileStream = ResrouceStream.Stream;
                if (myFileStream.CanRead)
                {
                    StreamReader myStreamReader = new StreamReader(myFileStream);

                    return myStreamReader.ReadToEnd();
                }
            }
            return "";
        }
    }
}
