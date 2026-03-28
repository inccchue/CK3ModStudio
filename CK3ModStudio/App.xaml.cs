using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;
using WpfPrismFrameworkTemplate.Helper;
using WpfPrismFrameworkTemplate.ViewModels;
using WpfPrismFrameworkTemplate.Views;

namespace WpfPrismFrameworkTemplate
{
	/// <summary>
	/// App.xaml 的交互逻辑
	/// </summary>
	public partial class App
	{
		protected override Window CreateShell()
		{
			return Container.Resolve<MainWindow>();
		}

		protected override void RegisterTypes(IContainerRegistry containerRegistry)
		{
            containerRegistry.RegisterSingleton<MainWindowViewModel>();
            containerRegistry.RegisterSingleton<IFamilyRepository, FamilyRepository>();
            containerRegistry.RegisterSingleton<GenealogyUserControl>();
            containerRegistry.RegisterSingleton<SqlServerDatabaseHelper>(
            _ => new SqlServerDatabaseHelper());

            containerRegistry.RegisterDialog<AssignmentWindow>(nameof(AssignmentWindow));
            containerRegistry.RegisterDialog<CommonAssignmentWindow>(nameof(CommonAssignmentWindow));
            containerRegistry.RegisterDialog<FamilyTreeWindow>(nameof(FamilyTreeWindow));
            
            containerRegistry.RegisterForNavigation<GenealogyUserControl, GenealogyUserControlViewModel>("Genealogy");
            containerRegistry.RegisterForNavigation<FileReadWriteUserControl, FileReadWriteUserControlViewModel>("FileReadWrite");
            containerRegistry.RegisterForNavigation<FileContentUserControl, FileContentUserControlViewModel>("FileContent");
            containerRegistry.RegisterForNavigation<CountyTimelineUserControl, CountyTimelineUserControlViewModel>("CountyTimeline");
            containerRegistry.RegisterForNavigation<StatisticsUserControl, StatisticsUserControlViewModel>("Statistics");
            containerRegistry.RegisterForNavigation<DatabaseUserControl, DatabaseUserControlViewModel>("Database");
            containerRegistry.RegisterForNavigation<LandedTitlesUserControl, LandedTitlesViewModel>("LandedTitles");
        }
	}
}
