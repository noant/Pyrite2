﻿using Lazurite.ActionsDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lazurite.ActionsDomain.ValueTypes;
using Lazurite.ActionsDomain.Attributes;
using LazuriteUI.Icons;
using System.Diagnostics;

namespace CommonPlugin
{
    [HumanFriendlyName("Убить процесс")]
    [LazuriteIcon(Icon.AppRemove)]
    [SuitableValueTypes(typeof(InfoValueType))]
    public class KillProcessAction : IAction
    {
        public string Caption
        {
            get
            {
                return string.Empty;
            }
            set
            {
                //do nothing
            }
        }

        public bool IsSupportsEvent
        {
            get
            {
                return false;
            }
        }

        public bool IsSupportsModification
        {
            get
            {
                return false;
            }
        }

        public ValueTypeBase ValueType
        {
            get;
            set;
        } = new InfoValueType();

        public event ValueChangedDelegate ValueChanged;

        public string GetValue(ExecutionContext context)
        {
            return string.Empty;
        }

        public void Initialize()
        {
            //do nothing
        }

        public void SetValue(ExecutionContext context, string value)
        {
            var processes = Process.GetProcessesByName(value);
            foreach (Process p in processes)
                p.Kill();
        }

        public bool UserInitializeWith(ValueTypeBase valueType, bool inheritsSupportedValues)
        {
            return true;
        }
    }
}
