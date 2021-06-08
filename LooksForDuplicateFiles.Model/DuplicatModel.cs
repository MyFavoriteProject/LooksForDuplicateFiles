using System.Collections.Generic;
using System.Linq;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace LooksForDuplicateFiles.Model
{
    public class DuplicatModel
    {
        public DuplicatModel(List<StorageFile> storageFiles, BasicProperties basicProperties)
        {
            StorageFiles = storageFiles;
            BasicProperties = basicProperties;
            FileName = storageFiles.FirstOrDefault().DisplayName;
        }

        public List<StorageFile> StorageFiles { get; }
        public BasicProperties BasicProperties { get; }
        public string FileName { get; set; }
    }
}
