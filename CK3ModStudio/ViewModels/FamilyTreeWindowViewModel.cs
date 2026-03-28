using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using WpfPrismFrameworkTemplate.Model;

namespace WpfPrismFrameworkTemplate.ViewModels
{
    public class FamilyTreeWindowViewModel : BindableBase, IDialogAware
    {
        private string _title = "";
        private string _draggableText;
        public string DraggableText
        {
            get { return _draggableText; }
            set { SetProperty(ref _draggableText, value); }
        }

        private ObservableCollection<DroppedTextInfo> _droppedItems;
        public ObservableCollection<DroppedTextInfo> DroppedItems
        {
            get { return _droppedItems; }
            set { SetProperty(ref _droppedItems, value); }
        }

        private ObservableCollection<ConnectionInfo> _connections;
        public ObservableCollection<ConnectionInfo> Connections
        {
            get { return _connections; }
            set { SetProperty(ref _connections, value); }
        }
        public FamilyTreeWindowViewModel()
        {
            DraggableText = "拖我到右边的Canvas上";
            DroppedItems = new ObservableCollection<DroppedTextInfo>();
            Connections = new ObservableCollection<ConnectionInfo>();
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }
        public event Action<IDialogResult> RequestClose;

        public void NotifyTextBlockDropped(string text, Point position)
        {
            DroppedItems.Add(new DroppedTextInfo
            {
                Text = text,
                X = position.X,
                Y = position.Y
            });
        }

        public void UpdateTextBlockPosition(string id, double x, double y)
        {
            // 查找并更新文本块的位置
            var item = DroppedItems.FirstOrDefault(i => i.Id == id);
            if (item != null)
            {
                item.X = x;
                item.Y = y;
            }
        }

        public void NotifyTextBlockDropped(string text, Point position, string id)
        {
            DroppedItems.Add(new DroppedTextInfo
            {
                Id = id,
                Text = text,
                X = position.X,
                Y = position.Y
            });
        }

        public void AddConnection(string sourceId, string targetId)
        {
            // 确保不重复添加连接
            if (!Connections.Any(c =>
                (c.SourceId == sourceId && c.TargetId == targetId) ||
                (c.SourceId == targetId && c.TargetId == sourceId)))
            {
                Connections.Add(new ConnectionInfo
                {
                    SourceId = sourceId,
                    TargetId = targetId
                });
            }
        }

        public void RemoveConnection(string sourceId, string targetId)
        {
            var connection = Connections.FirstOrDefault(c =>
                (c.SourceId == sourceId && c.TargetId == targetId) ||
                (c.SourceId == targetId && c.TargetId == sourceId));

            if (connection != null)
            {
                Connections.Remove(connection);
            }
        }
        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            //DialogParameters parm = new DialogParameters()
            //    {
            //        {"value", DynamicObj }
            //    };
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            //DynamicObj = parameters.GetValue<ExpandoObject>("value");
            //SelectedObject = new PropertyGridEditableProperties(DynamicObj);
        }
    }
}
