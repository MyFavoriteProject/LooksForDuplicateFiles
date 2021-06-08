using GalaSoft.MvvmLight.Command;
using LooksForDuplicateFiles.Model;
using Microsoft.Toolkit.Uwp.Helpers;
using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;

namespace LooksForDuplicateFiles.ViewModel
{
    public class FilesViewModel : BaseViewModel
    {
        public FilesViewModel()
        {
            OpenFolderCommand = new RelayCommand(OpenFolderExecute);
        }

        public RelayCommand OpenFolderCommand { get; }

        public ObservableCollection<DuplicatModel> DuplicatModelCollection { get; set; }

        private async void OpenFolderExecute()
        {
            FolderPicker folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");

            StorageFolder storageFolder = await folderPicker.PickSingleFolderAsync();

            if (storageFolder != null)
            {
                var dupticates = await ScannFoldersAsync(storageFolder);

                DuplicatModelCollection = new ObservableCollection<DuplicatModel>(dupticates);

                OnPropertyChanged(nameof(DuplicatModelCollection));
            }

        }

        private async Task<List<DuplicatModel>> ScannFoldersAsync(StorageFolder folder)
        {
            List<DuplicatModel> fileCollection = new List<DuplicatModel>();

            IReadOnlyList<StorageFile> fileReadOnlyList = await folder.GetFilesAsync();

            for (int i = 0; i < fileReadOnlyList.Count; i++)
            {
                for (int j = i + 1; j < fileReadOnlyList.Count; j++)
                {
                    if (fileReadOnlyList[i].FileType == fileReadOnlyList[j].FileType && i != j)
                    {
                        BasicProperties basicIFile = await fileReadOnlyList[i].GetBasicPropertiesAsync();
                        BasicProperties basicJFile = await fileReadOnlyList[j].GetBasicPropertiesAsync();

                        if (basicIFile.Size == basicJFile.Size)
                        {
                            string strAlgName = HashAlgorithmNames.Md5;

                            var byteI = await StorageFileHelper.ReadBytesAsync(fileReadOnlyList[i]).ConfigureAwait(true);
                            var byteJ = await StorageFileHelper.ReadBytesAsync(fileReadOnlyList[j]).ConfigureAwait(true);

                            if (byteI.SequenceEqual(byteJ))
                            {
                                var duplicate = fileCollection.FirstOrDefault(c => c.BasicProperties.Size == basicIFile.Size);

                                if (duplicate != null)
                                {
                                    if(!duplicate.StorageFiles.Contains(fileReadOnlyList[j]))
                                        duplicate.StorageFiles.Add(fileReadOnlyList[j]);
                                }
                                else
                                {
                                    var storageFile = new List<StorageFile>();
                                    storageFile.Add(fileReadOnlyList[i]);
                                    storageFile.Add(fileReadOnlyList[j]);

                                    fileCollection.Add(new DuplicatModel(storageFile, basicIFile));
                                }
                            }
                        }
                    }
                }
            }

            return fileCollection;
        }
    }
}
