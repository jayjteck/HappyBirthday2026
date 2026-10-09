namespace EventCenter
{
    public enum E_EventType
    {
        /// <summary>
        /// 设置Canvas模式事件（用于在选择角色面板时将Canvas设置为Camera模式）
        /// </summary>
        SetCanvasMode,
        
        /// <summary>
        /// 创建房间失败事件
        /// </summary>
        CreateRoomFailed,

        /// <summary>
        /// 倒计时结束事件（用于通知游戏正式开始）
        /// </summary>
        CountDownFinish,
        
        /// <summary>
        /// 背包物品变化事件
        /// </summary>
        InventoryChanged,
    }
}
