using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using WpfPrismFrameworkTemplate.ViewModels;
using WpfPrismFrameworkTemplate.Views;

namespace WpfPrismFrameworkTemplate.Model
{
    // 为了更可靠地存储连接线与TextBlock的关系，添加一个辅助类来存储连接信息
    public class ConnectionLineInfo
    {
        public Path LinePath { get; set; }
        public string SourceId { get; set; }
        public string TargetId { get; set; }
    }
    public class ConnectionInfo : BindableBase
    {
        public string SourceId { get; set; }
        public string TargetId { get; set; }
    }

    public class DroppedTextInfo : BindableBase
    {
        private string _id;
        public string Id
        {
            get { return _id; }
            set { SetProperty(ref _id, value); }
        }
        private string _text;
        public string Text
        {
            get { return _text; }
            set { SetProperty(ref _text, value); }
        }

        private double _x;
        public double X
        {
            get { return _x; }
            set { SetProperty(ref _x, value); }
        }

        private double _y;
        public double Y
        {
            get { return _y; }
            set { SetProperty(ref _y, value); }
        }
    }
    public class DragDropModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            // 模块初始化代码
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // 注册视图和视图模型
            containerRegistry.RegisterForNavigation<FamilyTreeWindow, FamilyTreeWindowViewModel>();
        }
    }
}
