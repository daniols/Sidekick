using System;
using System.Windows;
using Bindables;

namespace Sidekick.Views.Leagues
{
    /// <summary>
    /// Interaction logic for LeagueView.xaml
    /// </summary>
    [DependencyProperty]
    public partial class LeagueView : BaseOverlay
    {
        public LeagueView(LeagueViewModel leagueViewModel, IServiceProvider serviceProvider)
            : base("league", serviceProvider)
        {
            InitializeComponent();
            ViewModel = leagueViewModel;
            DataContext = ViewModel;

            Show();
            Activate();

            SetTopPercent(0, LocationSource.Begin);
            SetLeftPercent(50, LocationSource.Center);
        }

        public LeagueViewModel ViewModel { get; set; }
    }
}
