using System.Collections;
using Inventory.Data;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Inventory.View
{
    /// <summary>
    /// 格子视图（挂在格子 Prefab上）
    /// </summary>
    public class ItemSlotView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler,
        IPointerEnterHandler, IPointerExitHandler
    {
        //物品图标显示
        public Image iconImage;

        //物品数量显示
        public TextMeshProUGUI countText;
        
        [Tooltip("鼠标停在物品上多久后显示描述（秒）")]
        public float tooltipDelay = 1f;

        //由 BackpackPanel 创建时赋值
        [HideInInspector] public int index; 
        
        [HideInInspector] public bool hasItem;

        #region 私有字段

        private BackpackPanel _panel;

        private string _itemId;//缓存当前格子物品 id，悬浮时用
        
        private Coroutine _tooltipCoroutine; //悬浮提示的延迟协程

        #endregion

        #region Unity生命周期函数

        private void Awake()
        {
            _panel = GetComponentInParent<BackpackPanel>();
        }

        #endregion

        #region 公开方法

        /// <summary>
        /// 将该格子物品设置为正在拖拽的状态（变透明）
        /// </summary>
        /// <param name="dragging">是否正在拖拽</param>
        public void SetDragging(bool dragging)
        {
            //把物品图标变半透明
            if (iconImage != null)
            {
                Color c = iconImage.color;
                c.a = dragging ? 0.5f : 1f;
                iconImage.color = c;
            }
        }

        /// <summary>
        /// 为格子视图对象设置物品数据
        /// 通过传入的 ItemSlotData 得到物品 id和数量
        /// </summary>
        /// <param name="data">格子物品数据</param>
        public void SetData(ItemSlotData data)
        {
            hasItem = data != null && !string.IsNullOrEmpty(data.itemId);

            if (!hasItem)
            {
                _itemId = null;//空格子：清掉缓存的 id
                iconImage.gameObject.SetActive(false);
                countText.text = "";
                return;
            }

            _itemId = data.itemId;//缓存当前物品 id，悬浮时用
            
            ItemDefinition def = InventoryData.Instance.database.Get(data.itemId);
            iconImage.sprite = def.icon;
            iconImage.gameObject.SetActive(true);
            countText.text = data.count > 1 ? data.count.ToString() : "";
        }

        #endregion


        #region 拖拽事件回调

        //开始拖拽，鼠标按下并且移动超过拖拽阈值（默认 5 像素）那一刻触发
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!hasItem || _panel == null)
                return;

            _panel.BeginDrag(this);
        }

        //拖拽进行中，每帧持续调用
        public void OnDrag(PointerEventData eventData)
        {
            if (hasItem && _panel != null)
                _panel.UpdateDrag(eventData.position);
        }

        //拖拽结束，鼠标抬起瞬间触发
        public void OnEndDrag(PointerEventData eventData)
        {
            _panel?.EndDrag(this);
        }

        //别的物品拖到我身上松手 → 通知面板交换/叠加
        public void OnDrop(PointerEventData eventData)
        {
            var source = eventData.pointerDrag?.GetComponent<ItemSlotView>();

            if (source != null && source != this)
                _panel.SwapOrStack(source.index, index);
        }

        #endregion

        #region 悬浮提示

        //指针从外面移动，进入该 UI 物体的射线区域的瞬间触发
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!hasItem || _panel == null) 
                return;

            //防止反复进出时多个协程叠加
            CancelTooltip();   
            
            _tooltipCoroutine = StartCoroutine(ShowTooltipAfterDelay());
        }

        //指针从该 UI 物体内部，移出到外面的瞬间触发
        public void OnPointerExit(PointerEventData eventData)
        {
            CancelTooltip();
        }
        
        private IEnumerator ShowTooltipAfterDelay()
        {
            //等待设置的时间后，再执行下面的代码
            yield return new WaitForSeconds(tooltipDelay);

            //显示物品描述
            _panel.ShowTooltip(_itemId, transform.position);   
            
            _tooltipCoroutine = null;
        }

        //关闭物品描述
        private void CancelTooltip()
        {
            if (_tooltipCoroutine != null)
            {
                StopCoroutine(_tooltipCoroutine);
                _tooltipCoroutine = null;
            }
            
            _panel?.HideTooltip();
        }

        #endregion
    }
}
