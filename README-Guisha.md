# 诡舍框架 · 项目接入结果

项目位置：`E:\unity\Template`。

Unity 2022.3.62f3c1，Addressables 1.22.3，uGUI 与 Unity Test Framework 已就绪。当前输入模式为 Input Manager (Old)，可直接使用示例按键。

## 打开演示

Unity 中打开 **Assets/GuishaDemo/Scenes/Guisha.unity**，按 Play。场景与 Addressables 资源已生成，无需再次点击 Create Greybox Demo。

- `Guisha.unity`：诡舍主场景。
- `BloodVilla.unity`：血门别墅副本灰盒。
- `RainVillage.unity`：祈雨村副本灰盒。
- `Assets/GuishaDemo/Config`：鬼怪、鬼器、对话、实体 Catalog。
- `Assets/GuishaDemo/Prefabs/RedGhost.prefab`：Addressables 演示鬼怪。
- `Assets/Resources/Guisha/FrameworkSettings.asset`：主场景路径、实体 Catalog、音频与日志设置。

按键：WASD 移动、鼠标视角、Shift 跑、Ctrl 蹲、E 交互、Tab 背包、1 使用鬼器、Esc 暂停、F5/F9 存读档。

## 已验证

在该工程的隔离副本中，使用真实 Unity 编辑器运行：

- EditMode 核心与设计模式测试 **44/44 通过**。
- PlayMode 集成与回归测试 **3/3 通过**，覆盖玩家/HUD 初始化、背包和 San 状态恢复、UI 输入锁、附加场景切换、Addressables 鬼怪生成、NavMesh 绑定、返回主场景与唯一活动玩家/AudioListener，以及玩法与动画集成。

验证后将 Unity 生成的场景、资源、GUID、Addressables 设置和同一份测试同步到原项目；保留原来打开的场景和依赖版本。未执行正式 Player 构建，也未人工验收画面、手动输入或真实音频素材。

项目内测试位于 `Assets/Tests/EditMode` 与 `Assets/Tests/PlayMode`，可在 Test Runner 复跑。首次运行 PlayMode 测试会切换场景，建议先保存当前场景编辑。

正式打包前，在 Addressables Groups 执行 Build → New Build → Default Build Script，再构建 Player。

## 设计模式与动画扩展

已补齐 23 种 GoF 模式、MonoBehaviour 单例、动画模块及四种玩法模式。按 M 选择玩法；详见 Docs/Guisha/设计模式与动画.md。最新验证：44 项 EditMode、3 项 PlayMode 通过。

## 方法与继承调用手册

完整 PDF 位于 [Docs/Guisha/Unity框架_方法与继承调用手册.pdf](Docs/Guisha/Unity框架_方法与继承调用手册.pdf)，共 147 页，收录 219 个类型与 523 个显式方法声明，含继承示例、调用关系与测试附录。
