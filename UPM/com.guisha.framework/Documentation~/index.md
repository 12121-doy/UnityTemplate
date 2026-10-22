# 在其他项目中通过 Package Manager 安装

仓库根目录是完整的 Unity 开发工程，不是 UPM 包根目录。直接添加 `https://github.com/12121-doy/UnityTemplate.git` 会提示 `No package manifest was found`。

在 Unity 的 **Window > Package Manager > + > Add package from git URL** 中粘贴完整地址：

```text
https://github.com/12121-doy/UnityTemplate.git?path=/UPM/com.guisha.framework
```

UPM 包名为 `com.guisha.framework`。包目录包含 `package.json`、Runtime、Editor、Tests 和 Documentation~，声明了 Addressables、uGUI、Test Framework 与所需 Unity 内置模块。URP/HDRP 仍为可选依赖，不会因安装框架而自动替换项目渲染设置。此前失败的添加操作没有安装成功，直接使用上面的地址重新添加即可。

也可以在项目的 `Packages/manifest.json` 的 `dependencies` 中增加这一项，保留原来其他依赖：

```json
"com.guisha.framework": "https://github.com/12121-doy/UnityTemplate.git?path=/UPM/com.guisha.framework"
```

仓库为私有时，Package Manager 使用的 Git 必须能够通过你的 GitHub 账号读取它；不要把访问令牌写进 URL。已经手动复制过同一套 Guisha 脚本的项目，应先移除重复副本，再使用包，避免重复程序集和 GUID。

## 安装后使用

- Core 工具、单例、事件总线和 23 种设计模式可直接引用其命名空间使用。
- 安装包本身不生成示例、不修改项目场景列表，也不自动启动整套游戏管理器。
- 启用完整框架：通过 `Assets > Create > Guisha > Framework Settings` 创建配置，保存为 `Assets/Resources/Guisha/FrameworkSettings.asset`，再填写自己的实体目录与常驻场景等配置。进入 Play 时由 `GameBootstrap` 创建管理器。
- 体验完整示例：先保存当前场景，再执行 **Guisha > Create Greybox Demo**。此显式操作会在项目 Assets 内创建演示资源、Addressables 配置与框架配置，并把三个演示场景追加到 Build Settings。
- 渲染配置入口为 **Guisha > Rendering**；Unity 2022.3 与 SRP 14.x 是已验证的适配范围。新 Input System 项目体验示例时，需要把 Active Input Handling 设为 Both 或 Input Manager (Old)。

## 维护与测试

`Assets/Scripts` 是开发源文件。`UPM/com.guisha.framework` 是可安装分发副本，不直接编辑其中的脚本。修改开发源后运行：

```text
python Tools/rebuild_upm.py
python Tools/rebuild_upm.py --check
```

同步工具保留源码 GUID，并检查分发副本是否与开发源一致。UPM 目录位于 Assets/Packages 之外，因此打开开发工程时不会重复编译分发副本。

需要运行包内测试时，在使用方 `Packages/manifest.json` 的顶层增加 `"testables": ["com.guisha.framework"]`。EditMode 测试可直接执行；PlayMode 集成测试需要先通过上面的菜单生成演示资产与场景。

官方规则参考：[Git 包的子目录地址](https://docs.unity3d.com/2022.3/Documentation/Manual/upm-git.html)、[UPM 目录结构](https://docs.unity3d.com/2022.3/Documentation/Manual/cus-layout.html)。
