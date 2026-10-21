# UnityTemplate · 诡舍框架

Unity + C# 恐怖游戏框架，包含底层工具、实体/FSM/交互抽象、全局管理器、玩家与鬼怪 AI、叙事、UI、动画，以及 23 种 GoF 设计模式。

## 快速开始

1. 使用 **Unity 2022.3.62f3c1** 打开工程，等待依赖导入（Addressables 1.22.3、uGUI、Test Framework 1.1.33）。
2. 打开 `Assets/GuishaDemo/Scenes/Guisha.unity`，按 Play。
3. WASD 移动，鼠标视角，Shift 跑，Ctrl 蹲，E 交互；Tab 背包，1 使用鬼器，Esc 暂停，M 选择玩法，F5/F9 存读档。

输入使用 Input Manager (Old)。四种玩法为剧情、探索、限时挑战、无尽生存；挑战与生存禁用存档。

## 文档

- [逐行中文注释说明](Docs/Guisha/逐行注释说明.md)：89 个脚本、523 个方法、6,525 行有效代码已补齐中文说明，可在 IDE 悬停查看方法文档。
- [方法与继承调用手册 PDF](Docs/Guisha/Unity框架_方法与继承调用手册.pdf)：147 页，完整收录 219 个类型、523 个显式方法/构造/局部函数声明，包含继承模板、参数与返回类型、源码位置、可点击调用关系及测试附录。
- [接入与扩展](Docs/Guisha/接入与扩展.md)
- [设计模式与动画](Docs/Guisha/设计模式与动画.md)
- [验证说明](Docs/Guisha/验证说明.md)
- [项目接入说明](README-Guisha.md)

PDF 包含 **465 个业务方法声明 + 58 个测试方法声明**，不含 Unity/.NET 第三方 API 与编译器生成成员。属性、字段、枚举和事件载荷另列；15 个代码片段补齐上下文后编译通过。类型书签和 API 编号可以快速查找。

源码现已加入 XML 方法文档及逐行中文注释，代码逻辑保持一致；PDF 中的源码行号对应 2026-10-20 快照，当前源码使用类名/方法名定位。

仓库依照 `.gitattributes` 使用 Git LFS 保存 PDF 及二进制资产；克隆后执行 `git lfs pull` 获取对应文件。

## 源码结构

| 目录 | 内容 |
| --- | --- |
| `Assets/Scripts/Core` | MonoSingleton、PureSingleton、EventBus、对象池、异步、日志、数学、序列化、设计模式 |
| `Assets/Scripts/Framework` | 实体、工厂、FSM、契约、配置、事件、资源拥有权、玩法规则、动画适配器 |
| `Assets/Scripts/Managers` | Game、Audio、UI、Resource、SceneFlow、GameMode 与显式启动器 |
| `Assets/Scripts/Gameplay` | 玩家、背包、实体、AI、动画驱动组件、剧情、世界交互与生存生成 |
| `Assets/Scripts/UI` | 面板基类、HUD、对话、背包、菜单、玩法面板与输入路由 |
| `Assets/Scripts/Samples` | 灰盒场景组合与设计模式调用示例 |
| `Assets/Scripts/Editor` | 灰盒、动画和玩法资产生成工具 |
| `Assets/Tests` | EditMode 与 PlayMode 回归测试 |

全局管理器继承 `MonoSingleton<T>`，由 `GameBootstrap` 显式创建并跨场景保留。模块间通过类型事件传递消息；同一实体内通过组件组合协作。实体与资源生成采用 Addressables 异步流程，使用 lease 管理释放。

## 验证与打包

真实 Unity 编辑器在隔离副本中通过 **44 项 EditMode + 3 项 PlayMode**。文档索引基于当前 89 个 C# 文件提取，Roslyn 语义解析错误为 0。可在 Test Runner 复跑测试；PlayMode 测试会切换场景，运行前保存编辑。

正式打包前，先在 Addressables Groups 执行 **Build → New Build → Default Build Script**，再构建 Player。当前尚未执行正式 Player 构建，示例动画和灰盒资源需按正式美术资产替换。
