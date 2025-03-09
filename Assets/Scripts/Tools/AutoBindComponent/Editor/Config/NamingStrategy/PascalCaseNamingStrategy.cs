/*
 * @Author: MuYiFC 
 * @Date: 2025-03-09 17:55:41 
 * @Last Modified by:   MuYiFC 
 * @Last Modified time: 2025-03-09 17:55:41 
 */
 
using System;

namespace Tools.AutoBindEditor
{
    public class PascalCaseNamingStrategy : NamingStrategyBase
    {
        public PascalCaseNamingStrategy(AutoBindGeneratorConfig.NamingConfig config) : base(config) { }

        public override string GenerateFieldName(string originalName, Type componentType)
        {
            string name = Capitalize(originalName);
            return AddPrefix(name, Config.fieldPrefix);
        }

        public override string GeneratePropertyName(string fieldName)
        {
            return fieldName;
        }
    }
}
