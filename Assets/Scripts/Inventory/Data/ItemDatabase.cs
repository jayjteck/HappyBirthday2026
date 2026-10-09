using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Data
{
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/ItemDatabase")]
    public class ItemDatabase : ScriptableObject
    {   
        public List<ItemDefinition> items;   // 在编辑器里把所有定义好的物品拖进来
        
        private Dictionary<string, ItemDefinition> _lookup;
        
        /// <summary>
        /// 通过物品 id获取物品对象
        /// </summary>
        /// <param name="id">物品 id</param>
        /// <returns>物品对象</returns>
        public ItemDefinition Get(string id)
        {
            // 传 null 或空串直接返回，避免字典抛异常
            if (string.IsNullOrEmpty(id))
                return null;
            
            if (_lookup == null)
            {
                _lookup = new Dictionary<string, ItemDefinition>();

                if (items != null) // 顺便防 items 为空
                {
                    foreach (var item in items)
                    {
                        if (item != null && !_lookup.ContainsKey(item.itemId))
                            _lookup.Add(item.itemId, item);
                    }
                }
            }
            return _lookup.GetValueOrDefault(id, null);
        }
    }
}
