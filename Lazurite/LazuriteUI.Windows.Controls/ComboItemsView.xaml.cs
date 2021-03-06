﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace LazuriteUI.Windows.Controls
{
    /// <summary>
    /// Логика взаимодействия для ComboItemsView.xaml
    /// </summary>
    public partial class ComboItemsView : UserControl
    {
        public ComboItemsView()
        {
            InitializeComponent();
        }

        private ComboItemsViewInfo _info;

        public ComboItemsViewInfo Info
        {
            get => _info;
            set
            {
                _info = value;
                Refresh();
                InfoChanged?.Invoke(this, Info);
            }
        }

        private void Refresh()
        {
            if (Info.SelectedObjects?.Length > 1)
            {
                var strs = Info.SelectedObjects
                    .Select(x => x != null ? Info.GetCaption(x) : "[пусто]")
                    .Aggregate((x1, x2) => x1 + ", " + x2);
                mainItem.Content = strs;
                mainItem.Icon = Icons.Icon._None;
            }
            else if (Info.SelectedObjects?.Length == 1)
            {
                var item = Info.SelectedObjects.FirstOrDefault();
                mainItem.Content = item != null ? Info.GetCaption(item) : "[пусто]";
                mainItem.Icon = Info.GetIcon != null ? Info.GetIcon(item) : Icons.Icon._None;
            }
            else
            {
                mainItem.Content = "Не выбрано";
                mainItem.Icon = Icons.Icon._None;
            }
        }

        private void BtSelect_Click(object sender, RoutedEventArgs e)
        {
            DialogView dialog = null;
            var listView = new ComboBoxItemsView_List(
                Info,
                (info) =>
                {
                    dialog.Close();
                    Info = info;
                });
            dialog = new DialogView(listView);
            dialog.Show(Info.MainPanel);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        public event EventHandler<ComboItemsViewInfo> InfoChanged;
    }
}