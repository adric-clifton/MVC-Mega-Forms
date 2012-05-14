﻿/*
Copyright (c) 2012 Andrew Newton (http://about.me/nootn)

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace MvcMega.Forms.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ChangeVisuallyAttribute : ValidationAttribute, IClientValidatable
    {
        private const string Separator = "~";

        public enum ChangeTo
        {
            Hidden,
            Disabled,
        }

        public enum DisplayChangeIf
        {
            Equals,
            NotEquals,
            LessThan,
            LessThanOrEquals,
            GreaterThan,
            GreaterThanOrEquals,
            Contains,
            NotContains,
        }

        public ChangeVisuallyAttribute(ChangeTo to, string whenOtherPropertyName, DisplayChangeIf ifOperator, object value, bool conditionPassesIfNull)
        {
            To = to;
            WhenOtherPropertyName = whenOtherPropertyName;
            If = ifOperator;
            Value = value;
            ConditionPassesIfNull = conditionPassesIfNull;
        }

        public ChangeTo To { get; set; }

        public string WhenOtherPropertyName { get; set; }

        public DisplayChangeIf If { get; set; }

        public object Value { get; set; }

        public bool ConditionPassesIfNull { get; set; }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            return null;
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var rule = new ModelClientValidationRule
            {
                ValidationType = "changevisually",
            };

            var toValues = new List<string>();
            var whenOtherPropertyNameValues = new List<string>();
            var ifValues = new List<string>();
            var valueValues = new List<string>();
            var conditionPassesIfNullValues = new List<string>();

            var prop = metadata == null || metadata.ContainerType == null
                           ? null
                           : metadata.ContainerType.GetProperty(metadata.PropertyName);
            var allChangeVisuallyAttributesOnThisProperty = prop == null ? null : prop.GetCustomAttributes(typeof(ChangeVisuallyAttribute), true);
            if (allChangeVisuallyAttributesOnThisProperty != null && allChangeVisuallyAttributesOnThisProperty.Length > 1)
            {
                foreach (var currAttr in allChangeVisuallyAttributesOnThisProperty.Reverse())
                {
                    var attr = (ChangeVisuallyAttribute)currAttr;
                    toValues.Add(attr.To.ToString());
                    whenOtherPropertyNameValues.Add(attr.WhenOtherPropertyName);
                    ifValues.Add(attr.If.ToString());
                    valueValues.Add(attr.Value == null ? string.Empty : attr.Value.ToString());
                    conditionPassesIfNullValues.Add(attr.ConditionPassesIfNull.ToString());
                }
            }
            else
            {
                toValues.Add(To.ToString());
                whenOtherPropertyNameValues.Add(WhenOtherPropertyName);
                ifValues.Add(If.ToString());
                valueValues.Add(Value == null ? string.Empty : Value.ToString());
                conditionPassesIfNullValues.Add(ConditionPassesIfNull.ToString());
            }

            rule.ValidationParameters.Add("to", string.Join(Separator, toValues).ToLower());
            rule.ValidationParameters.Add("otherpropertyname", string.Join(Separator, whenOtherPropertyNameValues));
            rule.ValidationParameters.Add("ifoperator", string.Join(Separator, ifValues).ToLower());
            rule.ValidationParameters.Add("value", string.Join(Separator, valueValues).ToLower());
            rule.ValidationParameters.Add("conditionpassesifnull", string.Join(Separator, conditionPassesIfNullValues).ToLower());

            yield return rule;
        }

    }
}
