using Google.Protobuf.WellKnownTypes;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace WpfPrismFrameworkTemplate.Model
{
    public class Family : BindableBase
    {
        // 家族前缀（用于文件中的格式）
        public const string DYNASTY_PREFIX = "dynn_";
        private static readonly Regex NumberRegex = new Regex(@"(\d+)$", RegexOptions.Compiled);

        private string _familyName;
        private string _familyName_CN;

        // 声明一个事件用于通知外部
        public event EventHandler MembersChanged;

        public string FamilyName
        {
            get => _familyName;
            set => SetProperty(ref _familyName, value);
        }

        public string FamilyName_CN
        {
            get => _familyName_CN;
            set => SetProperty(ref _familyName_CN, value);
        }

        private ObservableCollection<People> _members;
        public ObservableCollection<People> Members
        {
            get => _members;
            set
            {
                if (_members != null)
                {
                    // 取消旧集合的事件订阅
                    _members.CollectionChanged -= Members_CollectionChanged;
                }

                if (SetProperty(ref _members, value) && value != null)
                {
                    // 订阅新集合的事件
                    _members.CollectionChanged += Members_CollectionChanged;
                }
            }
        }

        public Family(string familyName= "Family",string familyName_CN="")
        {
            FamilyName = familyName;
            FamilyName_CN = familyName_CN;
            Init();
        }

        public Family()
        {
            Init();
        }

        public void Init()
        {
            if(_members == null)
            {
                _members = new ObservableCollection<People>();
            }
            _members.CollectionChanged += Members_CollectionChanged;

        }

        public void SaveCoAPic(BitmapImage ReceivedImage)
        {
            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FamilyCoA");
            string filePath = Path.Combine(folderPath, string.Format(@"{0}.png", FamilyName));
            try
            {
                // 如果文件夹不存在则创建
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // 将 BitmapImage 转换为 BitmapEncoder 可处理的格式
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(ReceivedImage));

                // 创建文件流并保存图像
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    encoder.Save(fileStream);
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine($"{ex.Message}");
            }
        }

        // 添加一个方法来管理成员
        public void AddMember(People person)
        {
            if (!Members.Contains(person))
            {
                Members.Add(person);

                // 订阅此人的 Children 集合变更事件
                person.ChildrenChanged += Person_ChildrenChanged;

                // 将此人已有的子女添加到 Members 中
                foreach (var child in person.Children)
                {
                    AddMember(child);
                }
            }
        }

        public void RemoveMember(People person)
        {
            if (Members.Contains(person))
            {
                // 取消订阅事件
                person.ChildrenChanged -= Person_ChildrenChanged;
                Members.Remove(person);
            }
        }

        public void Person_ChildrenChanged(object sender, PeopleChildrenChangedEventArgs e)
        {
            if (e.AddedChild != null)
            {
                AddMember(e.AddedChild);
            }
            else if (e.RemovedChild != null)
            {
                // 检查这个人是否还是其他人的子女，如果是则不删除
                if (!Members.Any(m => m != e.RemovedChild && m.Children.Contains(e.RemovedChild)))
                {
                    RemoveMember(e.RemovedChild);
                }
            }
        }


        private void Members_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // 触发自定义事件
            MembersChanged?.Invoke(this, EventArgs.Empty);
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (People newChild in e.NewItems)
                {
                    newChild.ChildrenChanged += Person_ChildrenChanged;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (People oldChild in e.OldItems)
                {
                    oldChild.ChildrenChanged -= Person_ChildrenChanged;
                }
            }
        }

        public int FindMemberWithMaxIdNumber()
        {
            if (Members == null || Members.Count <= 0)
            {
                return 0;
            }
            int maxIdNumber = Members
            .Select(p => int.TryParse(p.IdName.Split('_').LastOrDefault(), out int num) ? num : 0)
            .Max();

            return maxIdNumber;
        }

        public override string ToString()
        {
            string content = "";

            foreach (var member in Members)
            {
                content += member.GetString();
                content += "\r\n";
            }
            return content;
        }

        public string GetString()
        {
            // 创建带前缀的ID
            string dynastyId = DYNASTY_PREFIX + FamilyName;
            string culture = Members.FirstOrDefault() == null ? "null" : Members.FirstOrDefault().Culture;

            // 创建家族定义
            string dynastyDefinition = $@"{dynastyId} = {{
	name = ""{dynastyId}""
	culture = ""{culture}""
}}";
            return dynastyDefinition;
        }

        public string GetLocalizationString()
        {
            // 创建带前缀的ID
            string dynastyId = DYNASTY_PREFIX + FamilyName;

            // 创建家族定义
            string dynastyDefinition = $@" {dynastyId}:0 ""{FamilyName}""";
            return dynastyDefinition;
        }

        public string GetLocalizationString_CN()
        {
            // 创建带前缀的ID
            string dynastyId = DYNASTY_PREFIX + FamilyName;

            // 创建家族定义
            string dynastyDefinition = $@" {dynastyId}:0 ""{FamilyName_CN}""";
            return dynastyDefinition;
        }
    }
}
