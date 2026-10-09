using System.Collections.Generic;
using EventCenter;
using Inventory.Data;
using Inventory.View;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class BackpackPanel : BasePanel
    {
        public GameObject slotPrefab;//格子预制体
        public Transform slotParent;//带 GridLayoutGroup 的空物体
        public Button closeButton;
        public ItemTooltip tooltipPrefab;//提示预制体（Image + 描述文本，挂 ItemTooltip）

        #region 私有字段

        //背包面板中的各个格子对象
        private List<ItemSlotView> _views = new();
        
        //拖拽中的图标
        private static Image _dragGhost;
        
        private ItemTooltip _tooltip;

        #endregion

        protected override void AddEventListeners()
        {
            //Model一变，就整块刷新
            EventCenterMgr.Instance.AddEventListener(E_EventType.InventoryChanged, Refresh);
            
            closeButton.onClick.AddListener(() =>
            {
                UIManager.Instance.HidePanel<BackpackPanel>();
            });
        }

        protected override void RemoveEventListeners()
        {
            EventCenterMgr.Instance.RemoveEventListener(E_EventType.InventoryChanged, Refresh);
            
            closeButton.onClick.RemoveAllListeners();
        }

        protected override void Init()
        {
            CreateSlots();
            Refresh();
        }

        #region 公开方法

        /// <summary>
        /// 在物品旁显示描述
        /// </summary>
        /// <param name="itemId">物品id</param>
        /// <param name="slotWorldPos">格子世界坐标</param>
        public void ShowTooltip(string itemId, Vector3 slotWorldPos)
        {
            if (tooltipPrefab == null) 
                return;

            ItemDefinition def = InventoryData.Instance.database.Get(itemId);
            
            if (def == null)
                return;

            EnsureTooltip();
            
            _tooltip.Show($"{def.itemName}\n{def.description}", slotWorldPos);
        }
        
        /// <summary>
        /// 隐藏物品描述信息
        /// </summary>
        public void HideTooltip()
        {
            if (_tooltip != null)
                _tooltip.Hide();
        }

        #endregion
        
        #region 私有方法
        
        /// <summary>
        /// 生成 Capacity 个格子，并记下每个格子的索引
        /// </summary>
        private void CreateSlots()
        {
            for (int i = 0; i < InventoryData.Capacity; i++)
            {
                ItemSlotView view = Instantiate(slotPrefab, slotParent).GetComponent<ItemSlotView>();
                view.index = i;
                _views.Add(view);
            }
        }
        
        /// <summary>
        /// 刷新背包面板
        /// </summary>
        private void Refresh()
        {
            var slots = InventoryData.Instance.slots;
            
            for (int i = 0; i < slots.Count; i++)
                _views[i].SetData(slots[i]);
        }
        
        /// <summary>
        /// 动态创建拖拽图标，挂到根 Canvas 下
        /// </summary>
        private void EnsureDragGhost()
        {
            if (_dragGhost != null)
                return;

            Canvas canvas = GetComponentInParent<Canvas>();

            GameObject go = new GameObject("DragGhost", typeof(RectTransform), typeof(Image));
            
            go.transform.SetParent(canvas.transform, false);

            _dragGhost = go.GetComponent<Image>();
            _dragGhost.raycastTarget = false;//不挡鼠标的落点检测
            _dragGhost.gameObject.SetActive(false);//默认隐藏
        }
        
        /// <summary>
        /// 按需创建提示对象，挂到面板下（随面板一起销毁，不会泄漏）
        /// </summary>
        private void EnsureTooltip()
        {
            if (_tooltip != null) 
                return;
            
            _tooltip = Instantiate(tooltipPrefab, transform);
            _tooltip.Hide();
        }

        #endregion
        
        #region 拖拽

        /// <summary>
        /// 开始拖拽
        /// </summary>
        /// <param name="slot">格子视图对象</param>
        public void BeginDrag(ItemSlotView slot)
        {
            EnsureDragGhost();
            
            _dragGhost.sprite = slot.iconImage.sprite;
            _dragGhost.rectTransform.sizeDelta = slot.iconImage.rectTransform.sizeDelta;//大小跟格子图标一致
            _dragGhost.gameObject.SetActive(true);
            _dragGhost.transform.SetAsLastSibling();//每次拖拽都确保盖在所有面板最上面
            
            slot.SetDragging(true);//源格子物品变为半透明，说明被拿起了
        }

        /// <summary>
        /// 更新拖拽中物品的显示位置
        /// </summary>
        /// <param name="screenPos">鼠标的屏幕坐标</param>
        public void UpdateDrag(Vector2 screenPos)
        {
            if (_dragGhost == null || !_dragGhost.gameObject.activeSelf)
                return;
            
            //根 Canvas的 RectTransform
            RectTransform parentRect = _dragGhost.transform.parent as RectTransform;
            
            //将鼠标的屏幕坐标转换为物品的本地坐标
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPos, null, out Vector2 local))
                _dragGhost.rectTransform.localPosition = local;
        }

        /// <summary>
        /// 结束拖拽
        /// </summary>
        /// <param name="slot"></param>
        public void EndDrag(ItemSlotView slot)
        {
            if (_dragGhost != null)
                _dragGhost.gameObject.SetActive(false);
            
            slot.SetDragging(false);
        }

        /// <summary>
        /// 交换或叠加
        /// </summary>
        /// <param name="from">原格子索引</param>
        /// <param name="to">目标格子索引</param>
        public void SwapOrStack(int from, int to)
        {
            InventoryData.Instance.SwapOrStack(from, to);//真正改数据在 Model
        }

        #endregion
    }
}
