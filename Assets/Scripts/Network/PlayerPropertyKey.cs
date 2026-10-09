namespace Network
{
    /// <summary>
    /// Photon 玩家自定义属性的 key，统一在这里维护，
    /// 避免在多个脚本里手写相同的字符串（改 key 时只改这一处）
    /// </summary>
    public static class PlayerPropertyKey
    {
        /// <summary>
        /// 玩家名字
        /// </summary>
        public const string PlayerName = "PlayerName";

        /// <summary>
        /// 玩家已加载到的章节场景名（值 = 场景名，如 "Chapter01"）。
        /// 用于章节标题的“全员到齐才显示”判断：切到下一章时旧值自然不等于新场景名，无需手动重置。
        /// </summary>
        public const string LoadedChapter = "LoadedChapter";
    }
}
