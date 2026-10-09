# 剧情向 2D 平台跳跃游戏(单人 / 双人联机)

一款基于 **Unity + Photon PUN 2** 的 2D 剧情冒险游戏,支持**单人**与**双人联机**两种模式。玩家在多个章节场景中探索、拾取物品、推进剧情,并可通过联机与好友共同游玩。

项目重点在于**一套完整可复用的游戏框架**:事件驱动架构、单例框架、UI 面板框架、数据驱动的背包系统,以及基于 Photon 的场景同步与房间状态同步方案。

> 项目定位:个人技术展示项目,重点展示 **联网同步**、**数据驱动设计** 与 **完整玩法系统**。

---

## 🎮 游戏演示

![实机演示](Screenshots/demo.gif)

---

## ✨ 功能特性

### 核心玩法
- 2D 横板角色控制(移动、跳跃、接地检测、朝向翻转、动画状态机)
- 章节式剧情流程:章节入口 / 章节标题 / 章节总览 / 过场视频 / 结局
- 场景交互:物品拾取点、传送点、章节出口
- 单人 / 双人联机两种模式

### 联网功能(Photon PUN 2)
- 房间加入 / 创建,双人联机游玩
- **场景自动同步**(`AutomaticallySyncScene`),切场景后所有客户端同步进入
- 房主发起的**倒计时**:用真实 Unix 时间戳(而非 `ServerTimestamp`)同步结束时刻,避免重连/新连客户端时间不一致
- 通过房间自定义属性广播倒计时、取消、游戏开始等状态
- 用 **RPC 手动同步**角色朝向与传送(Photon 内置同步组件不处理子物体缩放)
- 玩家昵称经自定义属性同步,本地/远端玩家头顶均正确显示

### 工程能力
- **数据驱动的背包系统**:基于 ScriptableObject 的物品定义与数据库,Data/View 分层,支持拖拽、交换、叠加、悬浮提示
- **事件中心(EventCenter)**,逻辑与 UI 解耦
- 通用 **单例框架**(`SingletonBase` / `SingletonAutoMono`)
- **UI 面板框架**(`UIManager` + `BasePanel`)

---

## 🛠 技术栈

| 类别 | 技术 |
|------|------|
| 引擎 | Unity 2022.3 LTS(2022.3.62f3c1) |
| 语言 | C# |
| 网络 | Photon PUN 2(Photon Cloud) |
| UI | UGUI + TextMeshPro |
| 数据 | ScriptableObject + JsonUtility |

---

## 🚀 如何运行

1. 使用 **Unity 2022.3 LTS** 及以上版本打开项目。
2. 打开场景 `Assets/Scenes/MainMenu.unity`,点击 Play。
3. 单人模式直接游玩;联机模式输入房间名加入,或由系统随机匹配创建房间。

---

## 📁 项目结构

```
Assets/
├── Scenes/
│   ├── MainMenu.unity              # 主菜单
│   ├── Lobby.unity                 # 大厅(角色选择 + 倒计时)
│   ├── Story/                      # 章节场景
│   └── End.unity                   # 结局
├── Scripts/
│   ├── Main.cs                     # 入口,显示主菜单
│   ├── EventCenter/                # 事件中心框架
│   ├── Singleton/                  # 单例框架
│   ├── Player/
│   │   ├── PlayerController.cs     # 2D 平台跳跃角色控制
│   │   ├── CameraFollow.cs         # 相机跟随
│   │   └── Data/PlayerData.cs      # 玩家数据
│   ├── Inventory/
│   │   ├── Data/                   # 背包数据层(Model)
│   │   │   ├── InventoryData.cs    # 背包数据(增删/交换/叠加)
│   │   │   ├── ItemDatabase.cs     # 物品数据库(ScriptableObject)
│   │   │   ├── ItemDefinition.cs   # 物品定义
│   │   │   └── ItemSlotData.cs     # 格子数据
│   │   └── View/                   # 背包视图层(View)
│   │       ├── ItemSlotView.cs     # 格子视图(拖拽/悬浮)
│   │       └── ItemTooltip.cs      # 物品悬浮提示
│   ├── Network/
│   │   ├── NetworkManager.cs       # Photon 联网管理
│   │   └── PlayerPropertyKey.cs    # 玩家自定义属性 key
│   ├── Scene/                      # 场景交互(传送/拾取/章节/视频)
│   └── UI/                         # UI 面板与 UIManager
│       ├── UIManager.cs
│       ├── BasePanel.cs
│       ├── BackpackPanel.cs
│       └── ...                     # 各业务面板
└── Resources/                      # 动态加载的资源(UI/角色/物品数据库)
```

---

## 🧩 核心架构

### 1. 事件驱动(EventCenter)

游戏逻辑与 UI 通过事件中心通信。以背包为例:数据层 `InventoryData` 修改物品后触发 `InventoryChanged` 事件,`BackpackPanel` 订阅该事件并整块刷新视图——**数据层不直接操作 UI,视图层不直接改数据**。

### 2. 数据驱动的背包系统(Data / View 分层)

- `ItemDefinition` 用 **ScriptableObject** 在编辑器中配置物品(图标、名称、描述、叠加上限);
- `ItemDatabase` 维护物品列表并提供惰性字典查找(空值/重复 id 防御);
- `ItemSlotView`(View)只负责显示与拖拽交互,交换/叠加逻辑全在 `InventoryData`(Model),拖拽由 `BackpackPanel`(Controller)协调。

### 3. 网络同步(Photon PUN 2)

- **场景同步**:`AutomaticallySyncScene` 开启后,房主切场景会同步所有客户端;离开房间前主动关闭,避免主菜单场景被误同步。
- **倒计时**:房主把「真实世界 Unix 时间戳」写入房间属性,所有客户端基于同一时刻计算剩余时间——比 `ServerTimestamp` 更可靠(重连/新连客户端读到的 `ServerTimestamp` 不可靠)。
- **朝向/传送**:`PhotonTransformView` / `PhotonAnimatorView` 不同步子物体缩放,角色朝向改的是子物体 `localScale`,因此用 RPC 手动同步;`PhotonView.Owner` 在 `Awake` 时尚未赋值,读取需放在 `Start`。

---

## 👤 作者

- GitHub:[jayjteck](https://github.com/jayjteck)
- 邮箱:299866@qq.com

---

## 📄 License

MIT License
