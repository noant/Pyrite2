﻿using System;
using System.Globalization;
using Xamarin.Forms;

namespace LazuriteMobile.App.Switches
{
    public class DateTimeValueTypeToSplittedString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString().Replace(" ", "\r\n");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}