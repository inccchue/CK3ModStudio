using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;
using Prism.Mvvm;
using WpfPrismFrameworkTemplate.Model;
using static Mysqlx.Notice.Warning.Types;

namespace WpfPrismFrameworkTemplate.Helper
{
    public class DebugHelper:BindableBase
    {
        private IEventAggregator _eventAggregator;
        private static Lazy<DebugHelper> _instance;

        public DebugHelper(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }
        public static DebugHelper Instance
        {
            get
            {
                if (_instance == null)
                    throw new InvalidOperationException("请先调用 Initialize 方法初始化 DebugHelper！");
                return _instance.Value;
            }
        }
        public static void Initialize(IEventAggregator eventAggregator)
        {
            if (_instance != null)
                return;

            _instance = new Lazy<DebugHelper>(() => new DebugHelper(eventAggregator));
        }
        
        public void SetEventAggregator(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }
        public void Log(string message, MsgLevel level= MsgLevel.Normal)
        {
            if (_eventAggregator != null)
            {
                _eventAggregator.GetEvent<DebugMsgEvent>().Publish(new DebugMsgEventArgs
                {
                    content = message,
                    level = level,
                    timestamp = DateTime.Now
                });
            }
        }
    }
}
