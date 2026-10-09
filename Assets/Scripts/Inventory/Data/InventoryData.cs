using System.Collections.Generic;
using EventCenter;
using Singleton;
using UnityEngine;

namespace Inventory.Data
{
    public class InventoryData : SingletonBase<InventoryData>
    {
        //背包中的格子数量
        public const int Capacity = 10;
        
        public List<ItemSlotData> slots = new();
        
        //物品数据库
        public ItemDatabase database;
        
        private InventoryData()
        {
            //从 Resources 加载物品数据库
            database = Resources.Load<ItemDatabase>("Inventory/ItemDatabase");
            
            //初始化格子
            for (int i = 0; i < Capacity; i++)
                slots.Add(new ItemSlotData());
        }

        #region 公共方法

        /// <summary>
        /// 为背包添加物品
        /// </summary>
        /// <param name="itemId">物品id</param>
        /// <param name="count">物品数量</param>
        /// <returns>是否添加成功</returns>
        public bool AddItem(string itemId, int count)
        {
            //根据 id从数据库得到物品的定义
            ItemDefinition def = database.Get(itemId);
            
            //如果物品不存在，返回
            if (def == null)
                return false;

            //先找同 id 且没满的格子叠加
            foreach (ItemSlotData s in slots)
            {
                if (s.itemId == itemId && s.count < def.maxStack)
                {
                    s.count = Mathf.Min(def.maxStack, s.count + count);
                    EventCenterMgr.Instance.EventTrigger(E_EventType.InventoryChanged);
                    return true;
                }
            }
            
            //再找空格子
            foreach (ItemSlotData s in slots)
            {
                if (string.IsNullOrEmpty(s.itemId))
                {
                    s.itemId = itemId;
                    s.count = count;
                    EventCenterMgr.Instance.EventTrigger(E_EventType.InventoryChanged);
                    return true;
                }
            }
            
            //上述逻辑都没进，说明背包满了
            return false; 
        }
        
        /// <summary>
        /// 交换或叠加两个格子（拖拽落点用）
        /// </summary>
        /// <param name="fromIndex">原格子的索引</param>
        /// <param name="toIndex">目标格子的索引</param>
        public void SwapOrStack(int fromIndex, int toIndex)
        {
            if (fromIndex == toIndex) return;

            ItemSlotData fromSlotData = slots[fromIndex];
            ItemSlotData toSlotData = slots[toIndex];

            //目标是空格子：整格移过去
            if (string.IsNullOrEmpty(toSlotData.itemId))
            {
                toSlotData.itemId = fromSlotData.itemId;
                toSlotData.count = fromSlotData.count;
                fromSlotData.itemId = null;
                fromSlotData.count = 0;
            }
            //同类且目标没满：能叠多少叠多少
            else if (toSlotData.itemId == fromSlotData.itemId)
            {
                ItemDefinition def = database.Get(fromSlotData.itemId);
                
                int move = Mathf.Min(def.maxStack - toSlotData.count, fromSlotData.count);
                
                toSlotData.count += move;
                fromSlotData.count -= move;
                
                if (fromSlotData.count <= 0)
                {
                    fromSlotData.itemId = null; 
                    fromSlotData.count = 0;
                }
            }
            //不同类：两格互换
            else
            {
                (toSlotData.itemId, fromSlotData.itemId) = (fromSlotData.itemId, toSlotData.itemId);
                (toSlotData.count, fromSlotData.count) = (fromSlotData.count, toSlotData.count);
            }

            EventCenterMgr.Instance.EventTrigger(E_EventType.InventoryChanged);
        }
        
        /// <summary>
        /// 移除指定物品（使用/丢弃物品用），从后往前遍历方便清格
        /// </summary>
        /// <param name="itemId">物品 id</param>
        /// <param name="count">要移除的数量</param>
        /// <returns>是否移除成功</returns>
        public bool RemoveItem(string itemId, int count)
        {
            //剩余要移除的数量
            int remaining = count;
            
            //遍历所有格子
            for (int i = slots.Count - 1; i >= 0 && remaining > 0; i--)
            {
                //如果不是要移除的物品就跳过该格子
                if (slots[i].itemId != itemId) 
                    continue;
                
                int remove = Mathf.Min(remaining, slots[i].count);
                
                slots[i].count -= remove;
                
                remaining -= remove;

                if (slots[i].count <= 0)
                {
                    slots[i].itemId = null; 
                    slots[i].count = 0;
                }
            }

            //判断是否移除了物品
            //只要剩余要移除的物品变少了，说明已经移除了物品
            if (remaining < count)
            {
                EventCenterMgr.Instance.EventTrigger(E_EventType.InventoryChanged);
                return true;
            }
            
            return false;
        }

        #endregion
    }
}
