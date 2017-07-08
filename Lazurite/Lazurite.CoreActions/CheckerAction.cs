﻿using Lazurite.ActionsDomain;
using Lazurite.ActionsDomain.Attributes;
using Lazurite.ActionsDomain.ValueTypes;
using Lazurite.CoreActions.ComparisonTypes;
using Lazurite.CoreActions.CoreActions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lazurite.MainDomain;

namespace Lazurite.CoreActions
{
    [OnlyGetValue]
    [VisualInitialization]
    [HumanFriendlyName("Условие")]
    [SuitableValueTypes(typeof(ToggleValueType))]
    public class CheckerAction : IAction, IMultipleAction, IChecker
    {
        public ActionHolder TargetAction1Holder { get; set; } = new ActionHolder();
        public ActionHolder TargetAction2Holder { get; set; } = new ActionHolder();
        public IComparisonType ComparisonType { get; set; } = new EqualityComparisonType();
        
        public string Caption
        {
            get
            {
                return string.Empty;
            }
            set
            {
                //
            }
        }
                
        public string GetValue(ExecutionContext context)
        {
            return ComparisonType.Calculate(TargetAction1Holder.Action, TargetAction2Holder.Action, context) ? ToggleValueType.ValueON : ToggleValueType.ValueOFF;
        }

        public void SetValue(ExecutionContext context, string value)
        {
            //
        }

        private ToggleValueType _valueType = new ToggleValueType();
        public ValueTypeBase ValueType
        {
            get
            {
                return _valueType;
            }
            set
            {
                //
            }
        }

        public bool IsSupportsEvent
        {
            get
            {
                return ValueChanged != null;
            }
        }
        
        public IAction[] GetAllActionsFlat()
        {
            return new[] { TargetAction1Holder.Action, TargetAction2Holder.Action };
        }

        public void Initialize()
        {
            //do nothing
        }

        public bool UserInitializeWith(ValueTypeBase valueType, bool inheritsSupportedValues)
        {
            return false;
        }

        public bool Evaluate(ExecutionContext context)
        {
            return GetValue(context) == ToggleValueType.ValueON;
        }
        
        public event ValueChangedDelegate ValueChanged;
    }
}