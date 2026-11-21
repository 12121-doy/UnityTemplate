# Guisha URP 2D 项目模板

克隆本仓库的 `template/2d` 分支。在 Unity Hub 中选择「添加 → 从磁盘添加项目」，打开仓库里的 `Templates/2D` 目录。项目使用 Unity **2022.3.62f3c1** 和 URP **14.0.12**。

首次打开时，Unity Package Manager 会通过 Git 安装 `com.guisha.framework` 1.0.1 及其依赖。需要安装 Git，并能读取此 GitHub 仓库。模板锁定了框架的 Git 提交，便于在其他电脑复现相同的依赖版本。

打开 `Assets/Scenes/SampleScene.unity` 开始制作。场景已配置正交主相机、全局 2D 光源和 URP 2D Renderer，并启用在 Build Settings 中。可直接使用已导入的框架接口。仓库根目录仍可作为完整的 3D 演示工程打开。
