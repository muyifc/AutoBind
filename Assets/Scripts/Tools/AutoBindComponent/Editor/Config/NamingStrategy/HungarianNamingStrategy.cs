/*
 * @Author: MuYiFC 
 * @Date: 2025-03-09 17:55:20 
 * @Last Modified by:   MuYiFC 
 * @Last Modified time: 2025-03-09 17:55:20 
 */
 
using System;

namespace Tools.AutoBindEditor
{
    public class HungarianNamingStrategy : NamingStrategyBase
    {
        public HungarianNamingStrategy(AutoBindGeneratorConfig.NamingConfig config) : base(config) { }

        public override string GenerateFieldName(string originalName, Type componentType)
        {
            string typePrefix = HungarianNotation.GetPrefix(componentType);
            string name = Capitalize(originalName);
            return AddPrefix(name, Config.fieldPrefix + typePrefix);
        }

        public override string GeneratePropertyName(string fieldName)
        {
            // 移除字段前缀
            string name = fieldName;
            if (!string.IsNullOrEmpty(Config.fieldPrefix) && name.StartsWith(Config.fieldPrefix))
            {
                name = name.Substring(Config.fieldPrefix.Length);
            }

            // 移除类型前缀（假设类型前缀是2-3个字符）
            if (name.Length > 3)
            {
                name = Capitalize(name.Substring(3));
            }

            return AddPrefix(name, Config.propertyPrefix);
        }
    }
}
