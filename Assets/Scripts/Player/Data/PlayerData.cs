using Singleton;

namespace Player.Data
{
    public class PlayerData : SingletonBase<PlayerData>
    {
        #region 公开字段

        public string playerName;

        /// <summary>
        /// 玩家选择的角色索引（0 / 1 / 2，对应 ChooseCharacterPanel 的三个按钮）
        /// </summary>
        public int characterIndex;

        /// <summary>
        /// 是否为单人模式（true = 不连接 Photon，选完角色直接进入章节场景）
        /// </summary>
        public bool isSinglePlayer;

        #endregion
    
        private PlayerData() { }

        #region 公开方法

        public string GetCharacterPath()
        {
            switch (characterIndex)
            {
                case 0:
                    return "Player/Wizard";
                case 1:
                    return "Player/CollegeStudent";
                case 2:
                    return "Player/Student";
                default:
                    return "Player/Wizard";
            }
        }

        #endregion
    }
}
