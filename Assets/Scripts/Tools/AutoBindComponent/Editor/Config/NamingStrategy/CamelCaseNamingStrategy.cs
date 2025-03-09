/*
 * @Author: MuYiFC 
 * @Date: 2025-03-09 17:55:09 
 * @Last Modified by:   MuYiFC 
 * @Last Modified time: 2025-03-09 17:55:09 
 */

using System;
using System.Diagnostics;

namespace Tools.AutoBindEditor
{
    public class CamelCaseNamingStrategy : NamingStrategyBase
    {
        public CamelCaseNamingStrategy(AutoBindGeneratorConfig.NamingConfig config) : base(config) { }

        public override string GenerateFieldName(string originalName, Type componentType)
        {
            string name = char.ToLower(originalName[0]) + originalName.Substring(1);
            return AddPrefix(name, Config.fieldPrefix);
        }

        public override string GeneratePropertyName(string fieldName)
        {
            string name = fieldName;
            if (!string.IsNullOrEmpty(Config.fieldPrefix) && name.StartsWith(Config.fieldPrefix))
            {
                name = name.Substring(Config.fieldPrefix.Length);
            }
            return Capitalize(name);
        }
    }
}
