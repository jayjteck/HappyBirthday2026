using Inventory.Data;
using Photon.Pun;
using Player.Data;
using UnityEngine;

namespace Scene
{
    public class ItemPickupPoint : MonoBehaviour
    {
        #region 公开字段

        [Header("要拾取的物品 id（在 ItemDatabase 里定义的）")]
        public string itemId;

        [Header("拾取数量")]
        public int count = 1;

        [Header("提示文字对象（进入触发区时显示）")]
        public GameObject hintObject;
        
        public SpriteRenderer itemSprite;

        #endregion

        #region 私有字段

        // 本地玩家是否在触发区内
        private bool _localPlayerInside;

        // 是否已经拾取过（场景对象，每个客户端各一份 → 每人各自独立记一次）
        private bool _picked;

        #endregion

        #region 生命周期函数

        private void Start()
        {
            ShowHint(false);
        }

        private void Update()
        {
            if (!_localPlayerInside || _picked)
                return;

            if (Input.GetKeyDown(KeyCode.J))
                Pickup();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsLocalPlayer(other))
                return;

            _localPlayerInside = true;

            if (!_picked)
                ShowHint(true);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!IsLocalPlayer(other))
                return;

            _localPlayerInside = false;
            ShowHint(false);
        }

        #endregion

        #region 私有方法

        // 判断进入触发区的角色是不是本地玩家（单人模式只有一个角色，直接视为本地玩家）
        private bool IsLocalPlayer(Collider2D other)
        {
            if (PlayerData.Instance.isSinglePlayer)
                return true;

            var view = other.GetComponentInParent<PhotonView>();
            return view != null && view.IsMine;
        }

        private void Pickup()
        {
            bool ok = InventoryData.Instance.AddItem(itemId, count);

            if (!ok)
            {
                // 物品 id 不存在、或背包已满时 AddItem 会返回 false
                Debug.LogWarning($"拾取失败：物品 {itemId} 不存在，或背包已满");
                return;
            }

            _picked = true;
            ShowHint(false);
            ShowSprite(false);
            Debug.Log($"拾取成功：{itemId} x{count}");
        }
        
        private void ShowHint(bool show)
        {
            if (hintObject != null)
                hintObject.SetActive(show);
        }

        private void ShowSprite(bool show)
        {
            if (itemSprite != null)
                itemSprite.enabled = show;
        }

        #endregion
    }
}
