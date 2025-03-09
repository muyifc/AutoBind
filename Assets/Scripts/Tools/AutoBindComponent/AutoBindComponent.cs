/*
 * @Author: MuYiFC 
 * @Date: 2025-03-09 17:54:18 
 * @Last Modified by:   MuYiFC 
 * @Last Modified time: 2025-03-09 17:54:18 
 */

using UnityEngine;
using System.Collections.Generic;

namespace Tools.AutoBind
{
    public class AutoBindComponent : MonoBehaviour
    {
        [System.Serializable]
        public class BindInfo
        {
            public string name;        // 组件名称
            public string type;        // 组件类型
            public Object component;   // 绑定的组件
        }

        public List<BindInfo> bindings = new List<BindInfo>();
        private Dictionary<string, Object> _cache = new Dictionary<string, Object>();

        public T Get<T>(string name) where T : Object
        {
            if (_cache.TryGetValue(name, out Object value))
            {
                return value as T;
            }

            var binding = bindings.Find(x => x.name == name);
            if (binding != null)
            {
                _cache[name] = binding.component;
                return binding.component as T;
            }

            Debug.LogError($"Component {name} not found!");
            return null;
        }
    }
}
