﻿using Lazurite.MainDomain;
using System.Windows.Controls;

namespace LazuriteUI.Windows.Main.Switches
{
    /// <summary>
    /// Логика взаимодействия для ToggleView.xaml
    /// </summary>
    public partial class ButtonView : UserControl
    {
        public ButtonView()
        {
            InitializeComponent();
        }

        public ButtonView(ScenarioBase scenario): this()
        {
            var model = new ScenarioModel(scenario);
            DataContext = model;
            Unloaded += (o, e) => model.Dispose();
            itemView.Click += (o, e) => model.ScenarioValue = string.Empty;
        }
    }
}
