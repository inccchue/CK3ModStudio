using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using WpfPrismFrameworkTemplate.Helper;

namespace WpfPrismFrameworkTemplate.ViewModels
{
    public class AssignmentWindowViewModel : BindableBase, IDialogAware
    {
        private string _title = "属性设置窗口";
        private dynamic _DynamicObj = new ExpandoObject();
        public AssignmentWindowViewModel()
        {
            
        }

        private PropertyGridEditableProperties _selectedObject;

        public PropertyGridEditableProperties SelectedObject
        {
            get => _selectedObject;
            set => SetProperty(ref _selectedObject, value);
        }
        public dynamic DynamicObj
        {
            get => _DynamicObj;
            set => SetProperty(ref _DynamicObj, value);
        }
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }
        public event Action<IDialogResult> RequestClose;
        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            DialogParameters parm = new DialogParameters()
                {
                    {"value", DynamicObj }
                };
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK, parm));
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            DynamicObj = parameters.GetValue<ExpandoObject>("value");
            SelectedObject = new PropertyGridEditableProperties(DynamicObj);
        }

    }
}
