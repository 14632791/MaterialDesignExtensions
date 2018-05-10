﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using MaterialDesignThemes.Wpf;

using MaterialDesignExtensions.Model;

using MaterialDesignExtensionsDemo.ViewModel;

namespace MaterialDesignExtensionsDemo
{
    public partial class MainWindow : Window
    {
        public const string DialogHostName = "dialogHost";

        public static RoutedCommand NavigationItemSelectedCommand = new RoutedCommand();

        private List<INavigationItem> m_navigationItems;

        public List<INavigationItem> NavigationItems
        {
            get
            {
                return m_navigationItems;
            }
        }

        public MainWindow()
        {
            m_navigationItems = new List<INavigationItem>()
            {
                new FirstLevelNavigationItem() { Label = "App bar", NavigationItemSelectedCallback = item => new AppBarViewModel() },
                new FirstLevelNavigationItem() { Label = "Oversized Number Spinner", NavigationItemSelectedCallback = item => new OversizedNumberSpinnerViewModel() },
                new FirstLevelNavigationItem() { Label = "Grid list", NavigationItemSelectedCallback = item => new GridListViewModel() },
                new FirstLevelNavigationItem() { Label = "Stepper", NavigationItemSelectedCallback = item => new StepperViewModel() },
                new FirstLevelNavigationItem() { Label = "Open directory", Icon = PackIconKind.Folder, NavigationItemSelectedCallback = item => new OpenDirectoryControlViewModel() },
                new FirstLevelNavigationItem() { Label = "Open file", Icon = PackIconKind.File, NavigationItemSelectedCallback = item => new OpenFileControlViewModel() },
                new FirstLevelNavigationItem() { Label = "Save file", Icon = PackIconKind.File, NavigationItemSelectedCallback = item => new SaveFileControlViewModel() },
                new FirstLevelNavigationItem() { Label = "Directory and file dialogs", NavigationItemSelectedCallback = item => new FileSystemDialogViewModel() },
                new FirstLevelNavigationItem() { Label = "Search", Icon = PackIconKind.Magnify, NavigationItemSelectedCallback = item => new SearchViewModel() }
            };

            InitializeComponent();

            SelectNavigationItem(m_navigationItems[0]);

            CommandBindings.Add(new CommandBinding(NavigationItemSelectedCommand, NavigationItemSelectedHandler));

            sideNav.DataContext = this;
            //navigationItemsControl.DataContext = this;
        }

        private void NavigationItemSelectedHandler(object sender, ExecutedRoutedEventArgs args)
        {
            SelectNavigationItem(args.Parameter as INavigationItem);
        }

        private void SelectNavigationItem(INavigationItem navigationItem)
        {
            m_navigationItems.ForEach(item => item.IsSelected = item == navigationItem);

            if (navigationItem != null)
            {
                contentControl.Content = navigationItem.NavigationItemSelectedCallback(navigationItem);
            }
            else
            {
                contentControl.Content = null;
            }
        }

        private void GoToGitHubButtonClickHandler(object sender, RoutedEventArgs args)
        {
            Process.Start("https://github.com/spiegelp/MaterialDesignExtensions");
        }

        private void GoToDocumentation(object sender, RoutedEventArgs args)
        {

            if (contentControl.Content is ViewModel.ViewModel viewModel && !string.IsNullOrWhiteSpace(viewModel.DocumentationUrl))
            {
                Process.Start(viewModel.DocumentationUrl);
            }
        }
    }
}
