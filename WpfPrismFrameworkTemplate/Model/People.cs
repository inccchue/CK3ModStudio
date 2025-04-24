using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Prism.Regions;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using HandyControl.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using WpfPrismFrameworkTemplate.Converter;
using System.Linq;
using Prism.Events;
using GongSolutions.Wpf.DragDrop;
using Newtonsoft.Json;

namespace WpfPrismFrameworkTemplate.Model
{
    // 用于传递 Children 变更信息的事件参数类
    public class PeopleChildrenChangedEventArgs : EventArgs
    {
        public People AddedChild { get; set; }
        public People RemovedChild { get; set; }
    }

    [TypeConverter(typeof(GenderTypeConverter))]
    public enum GenderType
    {
        [Description("男性")]
        Male,

        [Description("女性")]
        Female
    }

    public class People : BindableBase
    {
        private string _idName = "";
        private string _name;
        private string _dynasty;
        private string _religion;
        private string _culture;  
        private People _Mom ;
        private People _Dad ;
        private People _Spouse;
        private GenderType _Gender=GenderType.Male;
        private ObservableCollection<LifeEvent> _LifeEventList=new ObservableCollection<LifeEvent>();
        private ObservableCollection<People> _Children;
        // 定义一个事件，当 Children 集合发生变化时触发
        public event EventHandler<PeopleChildrenChangedEventArgs> ChildrenChanged;

        public People(int id, string name = "test_name", GenderType gender=GenderType.Male, string dynasty = "test_dynasty", string religion = "test_religion", string culture = "test_culture")
        {
            IdName = dynasty + "_" + id;
            Name = name;
            Gender = gender;
            Dynasty = dynasty;
            Religion = religion;
            Culture = culture;

            Init();
        }

        public People()
        {
            Init();
        }

        public void Init()
        {
            _LifeEventList.CollectionChanged += (s, e) => {
                if (e.NewItems != null)
                {
                    foreach (LifeEvent item in e.NewItems)
                    {
                        item.PropertyChanged += Item_PropertyChanged;
                    }
                }
                if (e.OldItems != null)
                {
                    foreach (LifeEvent item in e.OldItems)
                    {
                        item.PropertyChanged -= Item_PropertyChanged;
                    }
                }
                RaisePropertyChanged(nameof(LifeEventList));
            };

            // 为已有的元素注册事件
            foreach (var item in _LifeEventList)
            {
                item.PropertyChanged += Item_PropertyChanged;
            }

            if (_Children == null)
            {
                _Children = new ObservableCollection<People>();
                
            }
            _Children.CollectionChanged += Children_CollectionChanged;


        }

        private void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (People newChild in e.NewItems)
                {
                    if (newChild.Dynasty == this.Dynasty)
                    {
                        ChildrenChanged?.Invoke(this, new PeopleChildrenChangedEventArgs { AddedChild = newChild });
                    }
                    
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (People oldChild in e.OldItems)
                {
                    ChildrenChanged?.Invoke(this, new PeopleChildrenChangedEventArgs { RemovedChild = oldChild });
                }
            }
        }

        public void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // 当任何LifeEvent的属性改变时，触发LifeEventList的属性改变通知
            RaisePropertyChanged(nameof(LifeEventList));
        }

        private bool _isExpanded = true;
        [Browsable(false)]
        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        [Browsable(false)]
        public string IdName
        {
            get => _idName;
            set => SetProperty(ref _idName, value);
        }

        [Browsable(false)]
        public ObservableCollection<People> Children
        {
            get => _Children;
            set => SetProperty(ref _Children, value);
        }

        [Browsable(false)]
        public ObservableCollection<LifeEvent> LifeEventList
        {
            get => _LifeEventList;
            set => SetProperty(ref _LifeEventList, value);
        }

        [Browsable(false)]
        public People Spouse
        {
            get => _Spouse;
            set => SetProperty(ref _Spouse, value);
        }
        [Browsable(false)]
        public People Dad
        {
            get => _Dad;
            set => SetProperty(ref _Dad, value);
        }

        [Browsable(false)]
        public People Mom
        {
            get => _Mom;
            set => SetProperty(ref _Mom, value);
        }

        [Category("个人信息")]
        [DisplayName("姓名")]
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        [Editor(typeof(ReadOnlyTextPropertyEditor), typeof(ReadOnlyTextPropertyEditor))]
        [Category("个人信息")]
        [DisplayName("家族")]
        public string Dynasty
        {
            get => _dynasty;
            set => SetProperty(ref _dynasty, value);
        }

        [Category("个人信息")]
        [DisplayName("性别")]
        public GenderType Gender
        {
            get => _Gender;
            set => SetProperty(ref _Gender, value);
        }

        [Editor(typeof(ReadOnlyTextPropertyEditor), typeof(ReadOnlyTextPropertyEditor))]
        [Category("宗教和文化")]
        [DisplayName("宗教")]
        [Browsable(false)]
        public string Religion
        {
            get => _religion;
            set => SetProperty(ref _religion, value);
        }

        [Editor(typeof(ReadOnlyTextPropertyEditor), typeof(ReadOnlyTextPropertyEditor))]
        [Category("宗教和文化")]
        [DisplayName("文化")]
        [Browsable(false)]
        public string Culture
        {
            get => _culture;
            set => SetProperty(ref _culture, value);
        }

        // 添加一个属性用于绑定
        [Browsable(false)]
        [JsonIgnore]
        public string DisplayText => GetString();

        public string GetString()
        {
            var sb = new StringBuilder();

            // 主要信息
            sb.AppendLine($"{IdName} = {{");
            sb.AppendLine($"\tname = {Name}");
            sb.AppendLine($"\tdynasty = dynn_{Dynasty}");

            // 仅在性别为女性时添加 female = yes
            if (Gender == GenderType.Female)
            {
                sb.AppendLine("\tfemale = yes");
            }

            sb.AppendLine($"\treligion = {Religion}");
            sb.AppendLine($"\tculture = {Culture}");

            if (Dad != null)
            {
                sb.AppendLine($"\tfather = {Dad.IdName}");
            }

            if (Mom != null)
            {
                sb.AppendLine($"\tmother = {Mom.IdName}");
            }

            // 遍历 LifeEventList，按照 EventDate 排序输出
            foreach (var lifeEvent in LifeEventList.OrderBy(e => e.EventDate))
            {
                switch (lifeEvent.EventType)
                {
                    case LifeEventType.Marriage:
                        if (Spouse != null)
                        {
                            sb.AppendLine($"\t{lifeEvent.EventDate} = {{ add_spouse = {Spouse.IdName} }}");
                        }
                        
                        break;

                    case LifeEventType.Birth:
                    case LifeEventType.Death:
                    default:
                        sb.AppendLine($"\t{lifeEvent.EventDate} = {{ {lifeEvent.EventType.ToString().ToLower()} = yes }}");
                        break;
                }

            }

            sb.Append("}");
            return sb.ToString();
        }
        public override string ToString()
        {
            return IdName;
        }

        // 重写这个方法以确保任何属性变化时触发 DisplayText 的通知
        protected override void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            base.OnPropertyChanged(args);

            // 如果不是 DisplayText 本身变化的通知，则通知 DisplayText 也变化了
            if (args.PropertyName != nameof(DisplayText))
            {
                RaisePropertyChanged(nameof(DisplayText));
            }
        }
    }
}