using UnityEngine;

namespace Inventory.Data
{
    [CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/ItemDefinition")]
    public class ItemDefinition : ScriptableObject
    {
        public string itemId;//唯一 id
        public string itemName;//物品名
        public Sprite icon;//背包里显示的图标
        [TextArea] 
        public string description;//物品描述
        public int maxStack = 99;//一格最多叠几个
    }
}
