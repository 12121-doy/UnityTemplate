# UPM 消费项目验证（2026-10-22）

使用 Unity 2022.3.62f3c1，在独立的空项目中通过以下地址实际安装：

```text
https://github.com/12121-doy/UnityTemplate.git?path=/UPM/com.guisha.framework
```

Unity 确认为 Git 来源，包版本 `1.0.1`，代码提交为 `981dee85260938711bbfd0f5a889cd08485c3f2c`。安装声明的 Addressables、uGUI、Test Framework 与内置模块后，框架程序集独立编译成功；未依赖原开发工程的 Assets。

验证顺序：

1. 不带任何框架配置安装，确认没有创建游戏管理器或示例场景。
2. 从菜单生成完整示例资源、场景、动画、玩法目录与 Addressables 配置。
3. 执行 **53 项 EditMode**：全部通过，包括新建动画控制器后强制重导入，参数与状态仍保留的回归检查。
4. 执行 **5 项 PlayMode**：全部通过，覆盖场景、存档、动画、玩法、动态实体、材质生命周期与主相机输出。

首次消费测试发现新生成的 AnimatorController 参数在导入子资产后丢失；已在 1.0.1 中通过创建子资产前保存根参数、完成后保存控制器与状态机修正。修正后清空测试示例资源，重新通过 Git 安装并从零生成，再次完成上述测试。

最终 NUnit 结果见本目录 XML。批处理编辑器在测试结果保存、Input System Shutdown 后偶尔停滞，测试脚本只在结果已保存后结束该隔离进程；该退出记录与 NUnit 成败分开处理。

原 `Template` 工程的 ProjectSettings/Packages 与本机另一个 `HorrorGame` 工程的 manifest 均未被验证流程改动。`HorrorGame` 当前配置 URP 15.0.6，超出已有适配器的 SRP 14.x 范围；本次未替换其版本。此消费回归使用内置管线，三管线的既有验证另见渲染文档。本次未执行正式 Player 构建。
