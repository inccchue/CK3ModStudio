using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Xml.Linq;

namespace WpfPrismFrameworkTemplate.Model
{
    public class Family : BindableBase
    {
        private static readonly Regex NumberRegex = new Regex(@"(\d+)$", RegexOptions.Compiled);

        private string _familyName;

        // 声明一个事件用于通知外部
        public event EventHandler MembersChanged;

        public string FamilyName
        {
            get => _familyName;
            set => SetProperty(ref _familyName, value);
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

        public Family(string familyName= "Family")
        {
            FamilyName = familyName;
            Members = new ObservableCollection<People>();
            _members.CollectionChanged += Members_CollectionChanged;
        }


        private void Members_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // 触发自定义事件
            MembersChanged?.Invoke(this, EventArgs.Empty);
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
                content += member.ToString();
                content += "\r\n";
            }
            return content;
        }
    }
}
