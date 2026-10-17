# 环境与取证方法（r3）

## 机器

- OS：Windows 11 家庭版 中文版，10.0.26200
- CPU：Intel Core Ultra 9 185H（16 核）
- GPU：Intel Arc Graphics，驱动 32.0.101.8132
- 另有两个虚拟显示适配器（OrayIddDriver / GameViewer），不影响 batchmode 渲染

## 引擎与工具

- Unity Editor **6000.6.5f1**
- Unity CLI **1.0.0-beta.12**（本轮按用户指示**没有**执行 `unity self-update`；beta.13 已发布但未升级）
- 渲染：URP 2D，Unity Test Framework 3.5.0
- 测试模式：EditMode + PlayMode，`-batchmode`（Unity CLI 自动加）

## 被测代码

- 主仓库：`D:\data\UnityProjects\WorldSandboxPrototype`
- HEAD：`f85fbe346118e6ee5a6a0e2a33f76f8e653d4259`
- 工作树：**有未提交改动**（本轮视觉/可玩性修复），**未提交、未推送**
- 证据仓库：`D:\data\UnityProjects\WorldSandboxPrototype-visual-evidence`
- 本批次：`evidence/full-game-quality-2026-10-17-r3/`（只新增本目录，不覆盖 r2）

## 取证方法

- 使用 PlayMode 测试 `D068FinalCaptureTests`。
- 世界相机 + ScreenSpaceCamera Canvas 合成到 RenderTexture，`ReadPixels` 写 PNG；画面是真实运行中的世界与真实 uGUI 层级。
- 分辨率：1600×900（21 张）与 1280×720（5 张）。
- 输出目录由 `D068_FINAL_SHOT_DIR` 指定，指向本批次 `screenshots/`。
- `CaptureWriteGuard` / `CapturePathGuard` **未关闭**；写入前清空本批次旧的 `r3-*.png`，没有覆盖历史批次。
- 截图测试通过条件包含文件存在、尺寸正确；4/4 通过，26/26 落盘。

## 为什么不用 ScreenCapture

批处理 `-batchmode` 下没有 GameView 渲染目标，`ScreenCapture.CaptureScreenshot` 会静默不落盘（D-067 §8 已登记，本轮再次复现于 `D067_FullFlow` 的 640×480 图）。因此 D-068 使用 RenderTexture 合成；D-067 的 640×480 旧路径保留真实失败，不伪造。

## 复现命令

见 `README.md` §6 与 `tests/README.md`。所有命令均在主仓库根目录执行。

## 日期

- 取证/测试：2026-10-17
- 时区：Asia/Shanghai（UTC+8）
