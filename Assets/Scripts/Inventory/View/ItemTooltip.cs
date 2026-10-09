using TMPro;
using UnityEngine;

namespace Inventory.View
{
    public class ItemTooltip : MonoBehaviour
    {
        #region 公开字段
        
        public TextMeshProUGUI descriptionText;

        [Tooltip("相对物品的世界空间偏移（像素级）")]
        public Vector2 offset = new Vector2(30f, -10f);
        
        #endregion

        #region 公开方法

        /// <summary>
        /// 在世界坐标位置显示描述文本
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="worldPos">世界坐标</param>
        public void Show(string text, Vector3 worldPos)
        {
            descriptionText.text = text;
            gameObject.SetActive(true);
            transform.position = worldPos + (Vector3)offset;//显示在物品旁边
            transform.SetAsLastSibling();//保证物品描述显示在格子上方，不会被格子挡住
        }

        /// <summary>
        /// 隐藏描述文本
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        #endregion
    }
}
