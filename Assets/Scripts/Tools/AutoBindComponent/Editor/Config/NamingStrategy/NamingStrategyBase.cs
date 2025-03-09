/*
 * @Author: MuYiFC 
 * @Date: 2025-03-09 17:55:38 
 * @Last Modified by:   MuYiFC 
 * @Last Modified time: 2025-03-09 17:55:38 
 */
 
using System;

namespace Tools.AutoBindEditor
{

    public abstract class NamingStrategyBase : INamingStrategy
    {
        protected readonly AutoBindGeneratorConfig.NamingConfig Config;

        protected NamingStrategyBase(AutoBindGeneratorConfig.NamingConfig config)
        {
            Config = config;
        }

        public abstract string GenerateFieldName(string originalName, Type componentType);
        public abstract string GeneratePropertyName(string fieldName);

        protected string AddPrefix(string name, string prefix)
        {
            return string.IsNullOrEmpty(prefix) ? name : prefix + name;
        }

        protected string Capitalize(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            return char.ToUpper(name[0]) + name.Substring(1);
        }
    }
}