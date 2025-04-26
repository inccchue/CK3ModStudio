using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Documents;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using WpfPrismFrameworkTemplate.Model;

namespace WpfPrismFrameworkTemplate.ViewModels
{
    public class FileContentUserControlViewModel : BindableBase, INavigationAware
    {
        private IEventAggregator _eventAggregator;
        private FileReadWrite _fileReadWrite;
        private CharacterDefFileModel _CharacterDefFile =new CharacterDefFileModel();
        private DynastyDefFileModel _DynastyDefFile =new DynastyDefFileModel();
        private LocalizationEnglishFileModel _LocalizationEnglishFile =new LocalizationEnglishFileModel();
        private LocalizationChineseFileModel _LocalizationChineseFile = new LocalizationChineseFileModel();
        public ObservableCollection<Family> _FamilyList = new ObservableCollection<Family>();
        public DelegateCommand SaveCharacterDefFileCmd { get; private set; }
        public DelegateCommand SaveDynastyDefFileCmd { get; private set; }
        public DelegateCommand SaveLocalizationEnglishFileCmd { get; private set; }
        public DelegateCommand SaveLocalizationChineseFileCmd { get; private set; }
        public FileContentUserControlViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            eventAggregator.GetEvent<FileSettingChangeEvent>().Subscribe(LoadPathChangeFileContent);
            eventAggregator.GetEvent<SaveMessageEvent>().Subscribe(SaveFileContent);
            SaveCharacterDefFileCmd = new DelegateCommand(SaveCharacterDefFile);
            SaveDynastyDefFileCmd = new DelegateCommand(SaveDynastyDefFile);
            SaveLocalizationEnglishFileCmd = new DelegateCommand(SaveLocalizationEnglishFile);
            SaveLocalizationChineseFileCmd = new DelegateCommand(SaveLocalizationChineseFile);
        }

        public ObservableCollection<Family> FamilyList
        {
            get => _FamilyList;
            set
            {
                SetProperty(ref _FamilyList, value);
            }
        }
        public FileReadWrite FileReadWrite
        {
            get => _fileReadWrite;
            set => SetProperty(ref _fileReadWrite, value);
        }
        public LocalizationChineseFileModel LocalizationChineseFile
        {
            get => _LocalizationChineseFile;
            set => SetProperty(ref _LocalizationChineseFile, value);
        }
        public LocalizationEnglishFileModel LocalizationEnglishFile
        {
            get => _LocalizationEnglishFile;
            set => SetProperty(ref _LocalizationEnglishFile, value);
        }
        public DynastyDefFileModel DynastyDefFile
        {
            get => _DynastyDefFile;
            set => SetProperty(ref _DynastyDefFile, value);
        }
        public CharacterDefFileModel CharacterDefFile
        {
            get => _CharacterDefFile;
            set => SetProperty(ref _CharacterDefFile, value);
        }

        public void LoadFileContent()
        {
            CharacterDefFile.FilePath = FileReadWrite.CharacterDefFile;
            CharacterDefFile.Load();
            DynastyDefFile.FilePath =FileReadWrite.DynastyDefFile;
            DynastyDefFile.Load();
            LocalizationEnglishFile.FilePath = FileReadWrite.LocalizationEnglishFile;
            LocalizationEnglishFile.Load();
            LocalizationChineseFile.FilePath = FileReadWrite.LocalizationChineseFile;
            LocalizationChineseFile.Load();
        }

        public void LoadPathChangeFileContent(string changeFile)
        {
            switch (changeFile)
            {
                case nameof(FileReadWrite.CharacterDefFile):
                    CharacterDefFile.FilePath = FileReadWrite.CharacterDefFile;
                    CharacterDefFile.Load();
                    break;
                case nameof(FileReadWrite.DynastyDefFile):
                    DynastyDefFile.FilePath = FileReadWrite.CharacterDefFile;
                    DynastyDefFile.Load();
                    break;
                case nameof(FileReadWrite.LocalizationEnglishFile):
                    LocalizationEnglishFile.FilePath = FileReadWrite.LocalizationEnglishFile;
                    LocalizationEnglishFile.Load();
                    break;
                case nameof(FileReadWrite.LocalizationChineseFile):
                    LocalizationChineseFile.FilePath = FileReadWrite.LocalizationChineseFile;
                    LocalizationChineseFile.Load();
                    break;
            }

        }
        public void SaveCharacterDefFile()
        {
            CharacterDefFile.Save();
            CharacterDefFile.Load();
        }

        public void SaveDynastyDefFile()
        {
            DynastyDefFile.Save();
            DynastyDefFile.Load();
        }

        public void SaveLocalizationEnglishFile()
        {
            LocalizationEnglishFile.Save();
            LocalizationEnglishFile.Load();
        }

        public void SaveLocalizationChineseFile()
        {
            LocalizationChineseFile.Save();
            LocalizationChineseFile.Load();
        }

        public void UpdateFileContent()
        {
            CharacterDefFile.Add(FamilyList);
            DynastyDefFile.Add(FamilyList);
            LocalizationEnglishFile.Add(FamilyList);
            LocalizationChineseFile.Add(FamilyList);
        }

        public void SaveFileContent()
        {
            if (FileReadWrite.EnableAutoSave)
            {
                UpdateFileContent();
                CharacterDefFile.Save();
                DynastyDefFile.Save();
                LocalizationEnglishFile.Save();
                LocalizationChineseFile.Save();
            }
            
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters.ContainsKey("FileReadWrite")
                && navigationContext.Parameters.ContainsKey("FamilyList"))
            {
                FileReadWrite = navigationContext.Parameters.GetValue<FileReadWrite>("FileReadWrite");
                LoadFileContent();
                FamilyList = navigationContext.Parameters.GetValue<ObservableCollection<Family>>("FamilyList");
                UpdateFileContent();
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}
