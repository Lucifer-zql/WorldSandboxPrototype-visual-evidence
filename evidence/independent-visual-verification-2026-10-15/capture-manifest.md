# 独立视觉复验捕获清单（2026-10-15 制作人复检批）

## 捕获环境与可靠性

| 项 | 值 |
|---|---|
| 场景 | `Assets/_Project/Scenes/Main.unity`（生产场景，真实工作区代码/资源） |
| 地图种子 | 20261013（GameBootstrap 默认） |
| 分辨率 | 1280×720（GameView） |
| 捕获方式 | **主证据 = GameView ScreenCapture**（捕获前 `cam.Render()` 强制渲染——修复已知合成滞后/旧缓冲问题，v11 实证）；辅助轨 `cam-*.png` = cam.Render+ReadPixels 直渲染帧 |
| 运行方式 | Unity 6000.6.5f1 batchmode + 显卡（PlayMode 测试驱动真实运行） |
| 已知环境噪音 | URP BlitFinalToBackBuffer 分辨率告警（窗口 2870×1644 vs GameView 1280×720）——LogAssert 已屏蔽（断言不放松） |

## 图片清单（39 张原始 PNG + 本清单 + results.md + overview.png）

| 文件 | 分辨率 | 镜头(格,ortho) | 验证项 | 结论 |
|---|---|---|---|---|
| A1-coast-foam-occlusion.png | 1280×720 | 岸线格,6 | 泡沫实际遮挡/岸线形态 | 通过：贴岸细边、无圆角方块/断线/穿透 |
| A2-narrow-water-corner.png | 1280×720 | 窄水道,6 | 凹角/窄水/独立小岛泡沫 | 通过：窄条/小岛泡沫细边正常（关键证据——撤销误判修改依据） |
| A3-grass-forest-edge.png | 1280×720 | 草林交界,6 | 草林交界/内芯/边缘 | 通过 |
| A4-foam-anim-t0/t1/t2.png | 1280×720×3 | 岸线,5（间隔 0.35s） | 泡沫动画帧变化 | 通过：三帧相位/形态变化（10fps 错相） |
| B1-cliff-band-connection.png | 1280×720 | Cliff,6 | 崖带连接/层级 | 通过 |
| B2-cliff-along-shore.png | 1280×720 | 崖邻水,6 | 沿岸崖段 | 通过 |
| B3-tree-shadow-cliff-overlap.png | 1280×720 | 树邻崖,5 | 树影落地/被高地遮挡/遮挡穿插 | 通过：影在根南侧、不穿崖 |
| C1-tree-variant0..3.png | 1280×720×4 | 各变体树,4 | 全树种变体根落地/树冠/遮挡 | 通过（4 变体全覆盖） |
| C2-tree-overlap-north-south.png | 1280×720 | 南北双树,5 | 树木前后遮挡 | 通过：南遮北 |
| C3-tree-stump-exhausted.png | 1280×720 | 耗尽树,4 | 树木耗尽转树桩位置不跳变/接地 | 通过：桩在原树位、接地 |
| C4-bush-groundrock.png / C4-goldstone.png | 1280×720×2 | 装饰/金矿,4 | 灌木悬浮/岩石比例遮挡 | 通过 |
| D1D2-waterrocks-duck-decorations.png | 1280×720 | 装饰集群,6 | 水岩露出比例/鸭尺寸位置 | 通过 |
| D3-clouds-t0/t1.png | 1280×720×2 | 全图,24（间隔 1.2s） | 云层级与移动 | 部分通过：帧间云位移可测（云在 y 高位带）；全图视野内云稀少 |
| E0-spawn-dust-fx.png | 1280×720 | 生物行,6 | 落地尘特效（SpawnCreature 事件链） | 通过：真实事件触发 Dust |
| E1-creatures-all-variants-wide.png | 1280×720 | 生物行,6 | 全工具变体+接地+遮挡 | 通过：7 变体+3 羊全辨 |
| E2-creatures-closeup.png | 1280×720 | 生物行,3 | 变体细节 | 通过 |
| E3-idle-anim-t0/t1.png | 1280×720×2 | 生物行,6（间隔 0.5s） | 动画帧变化 | 通过：帧序推进 |
| E4-axe-work-state.png | 1280×720 | 生物行,6 | Work 状态切换 | 通过 |
| F1-buildings-castle-houses.png | 1280×720 | 建筑行,7 | Castle+House 变体/占地/遮挡 | 通过 |
| F2-explosion-fire-remove.png | 1280×720 | 拆除点,5 | RemoveAt→Explosion+Fire 真实事件链 | 通过：亮帧可见+事件面板(5) |
| F3-water-splash-reject.png | 1280×720 | 水格,5 | 水域拒绝反馈→Splash | 通过（语义=命令拒绝反馈非落水） |
| G1-hud-detailcard-gameview.png | 1280×720 | 全图,16 | HUD/详情卡/按钮/图标 | 通过 |
| G2..G5-palette-color1..4.png | 1280×720×4 | 岸线,8 | 地形色板 4 色调 | 通过：四色可辨、九槽自洽 |
| H1-paint-before/after-water.png | 1280×720×2 | 目标格,5 | PaintTerrain 真实数据+渲染 | 通过：草→水前后对照+数据断言 |
| H2-placeres-before/after-tree.png | 1280×720×2 | 目标格,5 | PlaceResource 真实写入 | 通过：数据断言+画面 |
| H3-removeat-after.png | 1280×720 | 目标格,5 | RemoveAt 真实移除 | 通过：数据断言 |
