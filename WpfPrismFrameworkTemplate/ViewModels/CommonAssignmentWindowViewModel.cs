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
    public class CommonAssignmentWindowViewModel : BindableBase,IDialogAware
    {
        private string _title = "属性设置窗口";
        public CommonAssignmentWindowViewModel()
        {

        }

        private Object _selectedObject;

        public Object SelectedObject
        {
            get => _selectedObject;
            set => SetProperty(ref _selectedObject, value);
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
                    {"value", SelectedObject }
                };
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK, parm));
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            SelectedObject = parameters.GetValue<Object>("value");
        }
    }
}
