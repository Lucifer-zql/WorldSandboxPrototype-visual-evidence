# CAM-INIT-01：修复新游戏启动时地图偏在屏幕右上角 · 修复后真实运行证据

- 捕获日期：2026-10-14
- 分支：`main`
- 场景：`Assets/_Project/Scenes/Main.unity`（真实加载，非 prefab 直载）
- 捕获方式：**正常 Play Mode** + `ScreenCapture.CaptureScreenshot`
  （Unity Test Framework PlayMode 测试 `WorldSandbox.Tests.PlayMode.CameraFramingEvidenceTests`；
   **非**离屏 RenderTexture 捕获、**非**放大后改名）
- 运行前提：必须在**窗口化**编辑器下运行（`-batchmode` 无帧缓冲，截图不会产出）

---

## 一、问题与根因

进入 HUD 后地图只出现在画面**右上角**，左侧与中部大片空白。

根因：`Main.unity` 主相机初始位于世界原点 `(0, 0, -10)`，而地图格子由
`TerrainTilemapRenderer.ApplyCell()` 用 `new Vector3Int(x, y, 0)` 从 `(0,0)` 起铺，
`DefaultWorldGenConfig` 为 128×128。相机在原点时可视范围约为
`x ∈ [−28.4, 28.4]`、`y ∈ [−16, 16]`，只覆盖地图**左下角**一小块；由于世界 +y 向上，
该角落显示在屏幕**右上**，其余为空白。

修复：地图生成成功、相机与手势装配完成后，由 `GameBootstrap.WireWorld()` 调用新增的
`CameraRig.CenterOnMap(result.Width, result.Height)`，把相机中心移到地图中心
`(Width/2, Height/2)`，**Z 保持不变**，正交尺寸不变（不拉远到看完整张地图）。

## 二、截图清单（正常 Play Mode 实拍）

| 文件 | 实测分辨率 | 目标分辨率 | 是否一致 | 相机位置 | 地图尺寸 |
|---|---|---|---|---|---|
| `hud_1280x720.png` | 1280×720 | 1280×720 | ✅ | (64.000, 64.000, −10.000) | 128×128 |
| `hud_1920x1080.png` | 1920×1080 | 1920×1080 | ✅ | (64.000, 64.000, −10.000) | 128×128 |
| `hud_2560x1440.png` | 2560×1440 | 2560×1440 | ✅ | (64.000, 64.000, −10.000) | 128×128 |

- 文件名中的尺寸 = **实测 `Screen.width/height`**，已用图像解码复核（PIL 读取的像素尺寸与文件名一致）。
- 三档均由编辑器 Game View 的固定分辨率档位真实设置（1280×720 / Full HD 1920×1080 / QHD 2560×1440），
  **未**使用 RenderTexture 放大，**未**对图片改名伪称。
- 相机 Z 三档均为 −10（与场景原值一致），正交尺寸均为 16（`CameraConfig.CameraOrthoSizeDefault`，未改）。

`capture-manifest.txt` 记录每次捕获的 target / actualScreen / match / setGameView 详情、
相机位置、正交尺寸与地图尺寸。

### 附：从修复后 HUD 裁出的底部放大图（CAM-INIT-02 追加）

| 文件 | 实测分辨率 | 来源 |
|---|---|---|
| `zoom_hud_woodborder_2x.png` | 1240×460 | 由 `hud_2560x1440.png` 裁剪 ×2（时间控制面板 + 工具栏左段木质边框） |
| `zoom_hud_bottombar_2x.png` | 5120×460 | 由 `hud_2560x1440.png` 裁剪 ×2（整条底部工具栏） |

两张均由**相机修复后**的 HUD 裁出（非修复前画面），供更新版联系表使用。

## 三、目视核对要点

- 地图**铺满整个观察区域**，主体位于中央；不再只占右上角。
- 画面不再因相机从世界原点起步而留下大块无意义空白。
- UI 顶栏（资源 + 「第1年1天」+ 菜单/设置按钮）、底部时间控制（暂停 / 1× / 3×）、
  工具栏、提示栏、事件面板在三种分辨率下均正常。
- 未修改 UI 九宫格素材、`UIPrefabBuilder.cs`、`MainCanvas.prefab` 或地形规则瓦片。

## 四、自动化验证

- **EditMode 全量：108 通过 / 0 失败 / 0 跳过**（`-batchmode -nographics`）。
- **PlayMode 全量：18 通过 / 0 失败 / 0 跳过**（**窗口化**运行；含本任务新增的 2 个相机回归用例）。
- 新增回归用例 `CameraInitialFramingTests`（2 个）：
  1. `MainScene_InitialCamera_FramedOnMapCentre` —— 真实加载 Main.unity 后断言
     MapReader 尺寸 == 生成结果尺寸；相机 X/Y == 地图中心（误差 ≤ 1/64 像素对齐单位）；
     Z 保持 −10；正交尺寸仍由配置决定；初始中心落在边界钳制合法区间内；
     平移仍可用且极端平移后被钳制在地图 + 留白内。
  2. `RepeatedWireWorld_DoesNotOverrideAdjustedView` —— 用户主动调整视角后再执行一次初始化流程，
     相机位置不得被覆盖。
- 编译错误：0。

## 五、已知局限（如实声明）

- 相机初始取景只在**首次**世界装配时应用；后续重复初始化不覆盖用户视角（已在用例 2 断言）。
- 本组截图仅验证「初始取景」与 HUD 渲染；未验证长时间运行、真实鼠标拖拽的端到端手感。
- 响应式断点（事件日志折叠、提示条显隐）行为未单独验证。
- UI / 玩法最终验收结论仍未确定，本目录不构成验收通过。
