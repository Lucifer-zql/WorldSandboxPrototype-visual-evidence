# 独立视觉复验结果（2026-10-15 制作人复检批）

> 判定口径：通过/未通过/部分通过/无法验证。测试通过、资源存在、GUID 引用不单独构成"通过"——一律以真实生产画面为准。

## 一、复验结论总表

| 项 | 旧声明 | 复验结论 | 依据 |
|---|---|---|---|
| A. 泡沫形态/遮挡/动画 | TV-02-R4 已修复 | **通过** | A1/A2/A3/A4：贴岸细边、无圆角方块/断线/穿透；动画三帧相位变化。A2 证实窄条/小岛泡沫正常 |
| B. 悬崖/高地/阴影 | TV-02-R2 已修复 | **通过** | B1/B2/B3：崖带连接自然、树影落地不穿崖、遮挡正确 |
| C. 树木/树桩/灌木/岩石 | TV-02-R1~R3 已修复 | **通过** | C1×4 变体/C2 南遮北/C3 树桩接地不跳变/C4 装饰自然 |
| D. 水岩/鸭/云 | 已接入 | **部分通过** | D1D2 装饰正确；D3 云移动可测但全图视野内云稀少（云在高位带，取景覆盖有限） |
| E. 人物/羊/动画 | T1 已修复 | **通过**（修复 1 处位置 bug） | E0/E1/E2/E3/E4：7 变体+3 羊全辨、动画帧推进、Work 切换、接地遮挡正确 |
| F. 建筑/特效 | P2 已接入 | **通过** | F1 建筑变体/占地/遮挡；F2/F3 特效真实事件链触发+生命周期 |
| G. UI/色板 | P2 已接入 | **通过** | G1 HUD/详情卡/按钮/图标；G2-G5 四色板可辨、九槽自洽 |
| H. 工具交互 | PaintTerrain/PlaceResource 原为占位 | **通过**（本轮实现真实命令） | H1/H2/H3 前后对照 + 数据断言（世界数据真实改变+画面刷新） |

## 二、本轮发现并修复的复现问题

| # | 问题 | 根因 | 修复文件 | 证据 |
|---|---|---|---|---|
| 1 | **PaintTerrain/PlaceResource 占位未实现**（点击进命令不等于操作实现） | IMPL-08 判定体缺失 | `CommandBusPlaceholder.cs`（真实写入+校验+高度一致性）、`SimulationContext.cs`（Terrain/Elevation 通道）、`ResourceNodeStore.cs`（AllocateId D-10）、`FxCommandBus.cs`（渲染刷新链）、`GameBootstrap.cs`（装配） | H1/H2/H3 前后对照 + 数据断言 |
| 2 | **FX 位置半格偏移**（特效不与对象中心对齐） | 命令层传格坐标非格心 | `FxCommandBus.cs`（9 处格心对齐） | E0 落地尘对齐生物、F2 特效对齐拆除点 |
| 3 | **"白棉球"误判澄清**（曾疑泡沫块状残留） | Dust 落地特效与截图时机混淆；渲染器诊断定位 `Dust_01_1` | `VisualVerificationTests.cs`（E0/E1 时机分离） | E0（尘团特效）vs E1（尘散后干净画面）对照 |
| 4 | 误判引发的泡沫规则修改已**撤销** | 基于误判的"内芯条件"改动改变 TV-02-R4 已验证语义 | `WaterFoamRenderer.cs`（恢复全岸覆盖+误判澄清注释）、`TerrainFoamPlacementTests.cs`（窄条锁改断言 True） | A2 窄条/小岛泡沫正常 |

## 三、旧修复声明的复验判定

- **成立**：泡沫 TV-02-R4（A 组图）、悬崖/阴影 TV-02-R2（B 组）、树木家族 TV-02-R1/R3（C 组）、人物变体与动画 T1（E 组）、建筑/特效接入 P2（F 组）、UI/色板 P2（G 组）
- **不成立→已修**：PaintTerrain/PlaceResource 真实操作（H 组复现为占位——本轮实现）
- **瑕疵→已修**：FX 位置半格偏移（E0 诊断发现）
- **再次出现→排除**："白棉球"现象经查为 Dust 特效（非泡沫回归），已以 E0/E1 对照+渲染器诊断闭环

## 四、测试结果

| 套件 | 结果 | 说明 |
|---|---|---|
| EditMode 全量 | 178/178（含泡沫窄条锁回归用例） | final5-em.xml |
| PlayMode 全量 | 31/31 | final5-pm.xml |
| VisualVerificationTests | 8/8（39 图捕获） | vv-pm7.xml |

## 五、GitHub 交付状态（重要）

**未上传——仓库规范明确禁止 Git 写操作**：
`docs/13-项目操作与仓库管理规范.md` §2.1（Phase 2 执行边界）与 OP-05 明文禁止**任何** Git 写操作（`add`/`commit`/`push`/配置修改），并注明 Phase 2 起"仍完全有效"。用户指令："如果仓库规范明确禁止提交，则遵守该规范，并明确说明该限制"——故本轮**不执行证据上传**，证据完整保留于本地目录：
`docs/impl/evidence/independent-visual-verification-2026-10-15/`（39 PNG + manifest + results + overview）。
若制作人解禁规范（书面授权覆盖 OP-05），建议提交命令（仅证据路径）：
```
git add docs/impl/evidence/independent-visual-verification-2026-10-15/
git diff --cached --name-only   # 核对仅证据文件
git commit -m "docs(evidence): 独立视觉复验 2026-10-15 证据批次"
git push
```
注意 GR-01：PNG 提交涉 Git LFS（.gitattributes 规则），提交前须过"首次提交 PNG 强制检查清单"（docs/13 §4.3）。
