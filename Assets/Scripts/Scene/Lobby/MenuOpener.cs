using Scene;
using UI;
using UnityEngine;

public class MenuOpener : MonoBehaviour
{
    void Update()
    {
        // 视频正在播放时，Esc 用于退出视频，不弹菜单
        if (Input.GetKeyDown(KeyCode.Escape) && !VideoPlayTrigger.IsPlaying)
        {
            UIManager.Instance.ShowPanel<MenuPanel>();
        }
    }
}
