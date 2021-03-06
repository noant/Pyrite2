﻿using Lazurite.ActionsDomain.ValueTypes;
using LazuriteUI.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace LazuriteUI.Windows.Main.Switches
{
    /// <summary>
    /// Логика взаимодействия для StatusViewSwitch.xaml
    /// </summary>
    public partial class StatusViewSwitch : UserControl
    {
        private Timer _timer;
        private static Dictionary<string, string> SearchCache = new Dictionary<string, string>();

        public StatusViewSwitch()
        {
            InitializeComponent();
        }

        public StatusViewSwitch(ScenarioModel scenarioModel): this()
        {
            DataContext = scenarioModel;

            BeginInit();
            
#if !DEBUG
            spSearch.Visibility = scenarioModel.AcceptedValues.Length > 150 ? Visibility.Visible : Visibility.Collapsed;
#endif

            tbScenarioName.Text = scenarioModel.ScenarioName;
            ItemViewFast initialSelectedItem = null;
            foreach (var state in scenarioModel.AcceptedValues)
            {
                var itemView = new ItemViewFast();
                itemView.Text = state;
                itemView.Margin = new Thickness(0, 1, 0, 1);
                if (scenarioModel.ScenarioValue?.Equals(state) ?? false)
                    initialSelectedItem = itemView;
                listItemsStates.Children.Add(itemView);
            }

            tbSearch.Text = GetSearchCache(scenarioModel.Scenario.Id);

            if (!string.IsNullOrEmpty(tbSearch.Text))
                ShowSearchResult();

            Loaded += (o, e) =>
            {
                if (initialSelectedItem != null)
                {
                    initialSelectedItem.Selected = true;
                    initialSelectedItem.Focus();
                }
            };

            listItemsStates.SelectionChanged += (o, e) =>
            {
                var selectedItem = listItemsStates.GetSelectedItems().FirstOrDefault() as ItemViewFast;
                if (selectedItem != null)
                {
                    if (selectedItem.Text != scenarioModel.ScenarioValue)
                    {
                        scenarioModel.ScenarioValue = selectedItem.Text;
                        StateChanged?.Invoke(this, new RoutedEventArgs());
                    }
                }
                else
                {
                    scenarioModel.ScenarioValue = (scenarioModel.Scenario.ValueType as StateValueType).DefaultValue;
                    StateChanged?.Invoke(this, new RoutedEventArgs());
                }
            };

            tbSearch.TextChanged += (o, e) => {
                _timer?.Dispose();
                SetSearchCache(scenarioModel.Scenario.Id, tbSearch.Text);
                _timer = new Timer((s) => {
                    _timer = null;
                    Dispatcher.BeginInvoke(new Action(ShowSearchResult));
                }, null, 600, Timeout.Infinite);
            };

            EndInit();
        }

        private void ShowSearchResult()
        {
            var text = 
                GetSearchCache((DataContext as ScenarioModel).Scenario.Id)
                .ToLowerInvariant()
                .Trim();
            foreach (ItemViewFast itemView in listItemsStates.Children)
                itemView.Visibility =
                string.IsNullOrEmpty(text) ||
                itemView.Text.ToLowerInvariant().Contains(text) ?
                Visibility.Visible :
                Visibility.Collapsed;
        }

        private static string GetSearchCache(string scenarioId)
        {
            if (SearchCache.ContainsKey(scenarioId))
                return SearchCache[scenarioId] ?? string.Empty;
            else
                return string.Empty;
        }

        private static void SetSearchCache(string scenarioId, string searchString)
        {
            if (SearchCache.ContainsKey(scenarioId))
                SearchCache[scenarioId] = searchString;
            else
                SearchCache.Add(scenarioId, searchString);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        public event Action<object, RoutedEventArgs> StateChanged; 
    }
}
