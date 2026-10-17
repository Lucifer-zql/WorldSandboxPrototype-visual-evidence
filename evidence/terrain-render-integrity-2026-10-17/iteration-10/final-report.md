# D-065 Iteration-10 最终报告（final-r3 终版）

> 日期：2026-10-17｜Unity 6000.6.5f1 / URP 2D｜三种子 20261013/14/15
> 依据：制作人 Iteration-10 任务书 + **官方 Tiny Swords Tilemap Guide**（本轮实抓 / 4 张官方示例图逐图查看 / 素材像素实测）。

## 1. 每个缺陷的真实根因

| 缺陷 | 根因（本轮实证） |
| --- | --- |
| **P0 悬崖位置与多层地表** | ① iter-08 删除了 Mount→Hill 的悬崖墙（官方：每级 Elevated 都含面向下方地形的悬崖）；② iter-10 中段把"Mount 立面格"误判为**北/东/西三向**邻 Mount——东西向本应只有草边（官方），三向豁免造成"无顶面又无墙"的空洞（midbench 82/129/64 违规即此）。 |
| **P1 三角坡路孤立/错位** | ① 楼梯形态错误：官方为 **2 格高列**（36 上 + 45 下 / 39+48 镜像），旧实现用单格 `_36/_39` 或反配对；② **配对反了**（`_45` 石脚 x=42 在右、`_48`=21 在左——低端在左应 39+48）；③ `PostProcessSlopes` 剔除孤立坡；④ 自创东西缘石墙（官方无）。 |
| **P2 内陆水缝/海岸误用** | iter-09 分族逻辑正确；本轮发现**楼梯格冗余 band**与 **Mount 立面格冗余 band**（墙/楼梯整格覆盖，band 无意义）。 |

## 2. 修改的生产文件与函数

| 文件 | 函数/位置 | 修改 |
| --- | --- | --- |
| `TerrainOverlayPlan.cs` | `CollectWalls`（南缘） | 恢复 Mount→Hill 南面墙 |
| 同上 | `CollectSideWall` | **调用删除**（东西缘无石墙，官方） |
| 同上 | `CollectSlopes` | 重写为官方两格楼梯：高台南缘两行、北邻恰为 Hill、低端侧两行干燥低地、高侧两行 Hill（r25 排除水格误判） |
| 同上 | `ReservedByOverlay` | 楼梯两格 + 南侧落点 + 低端侧向两格让位；**北邻 Mount 时不让位**（立面墙优先） |
| 同上 | `PostProcessSlopes` | 保留全部合法坡（不再剔除/补墙） |
| 同上 | `IsStairTile`（新公开 API） | 楼梯两格判定（扫描/测试共用） |
| `TerrainTilemapRenderer.cs` | `IsMountFacadeCell` | **仅北邻 Mount**：无顶面、无 band；东西邻为正常台面 |
| 同上 | `LayOverlays`（坡/台阶） | 两格渲染：左 39+48、右 36+45（素材实测配对） |
| 同上 | `ApplyCell` | 楼梯两格豁免顶面与 band |
| 测试 | 4 个测试文件 | 语义更新（见 §11） |

**未修改**：地形生成、SortingOrders、Main.unity、官方原始 PNG、素材导入设置。

## 3. 修改前后行为对照

| 场景 | 修改前（iter-09 / iter-10 中间版） | 修改后（final-r3） |
| --- | --- | --- |
| Mount 南缘 | 无立面 / 三向空洞 | Mount 顶面 → **南面崖壁墙** → Hill 台面 |
| 东西向高差 | 自创石墙 / 侧向坡 | **仅草边描边**（官方） |
| 楼梯 | 单格三角 / 反配对 | **2 格列**：39+48（左）/ 36+45（右），石脚低端 |
| 楼梯格顶面/band | 铺（与楼梯重叠） | 不铺（楼梯整格覆盖） |
| 孤立坡 | 剔除补墙 | 保留（官方 Example 1 语义） |

## 4. 悬崖位置/方向/端帽/转角

- 墙坐标：Mount/Hill 的**南邻**低格（含水格深水收口 Row5），逐格核对（Iter09 扫描 P0 全格 49152 格）。
- 端帽/转角：Variant 由相邻墙连续性推导；楼梯让位后相关端帽同步更新（测试按实际输出更新）。
- 实拍：`final-r3/seed-20261013/TS9f-p0-two-tier-south-seed-20261013.png`（Mount→墙→Hill 台→墙→低地）、
  `TS2a-waterfoot-cliff.png`（真实水岸逐级悬崖）、`TS6c-1-slope-left.png`（多级台阶状墙）。

## 5. 坡路位置/方向/上下端

- 两格楼梯：上段 (X,Y) 贴高台南缘、下段 (X,LowerY) 石脚与低地齐平（TS9d、TS6c-1、TS3b 实拍）。
- 配对：左 39+48、右 36+45（石脚在低端）。
- 墙连接：楼梯两格与南侧落点让位（Mount 立面除外）；旁边墙保持豁口。
- 实拍：`TS9d-p2-slope-run-west-seed-20261013.png`、`TS9e`、`TS9f`（左右侧楼梯）。

## 6. 内陆水缝与海岸素材扫描

- 扫描器：Iter09 真实输出扫描（P0 全格 49152、P1 水向暴露、P2 坡+台阶+计划外）+ Iter10 测试。
- 结果：**0 违规**（final-r3，三种子）；混合缘全 15 掩码/纯水缘全掩码实拍（TS9a/i/j）。
- 视觉：`TS1a-fullmap-ortho40.png`（全图无内陆水色穿透）、`TS7e`（混合缘白浪只在水的方向）。

## 7. Mount/Hill/低地逐层覆盖

- 判定（实际 Tilemap 输出）：每组相邻高程差由"南面墙或楼梯"覆盖；Mount 南邻 Hill 格有墙无顶面；
  东西邻为正常台面；Hill 台面有顶面；低地有 Terrain。
- 实拍：TS9f/TS9g（确定性）、TS2a/TS2b（随机地图）、TS1a（全图）。

## 8. color1 / color3 边带

- 维持 iter-09：混合缘/纯水缘缺片 → 右族 + band；band 派生件为 **color1 色带**（15 掩码）。
- **color3 Mount 顶面使用 color1 色带**的色差为**已知局限**（iter-09 起如实登记）。

## 9-10. 截图

- 三种子全量：`final-r3/seed-20261013|14|15/`（各 73-79 图，含 XML/log）。
- P0：`P0-cliff-and-layer-stack/post-fix/` 与 `final-r3/*/TS9f|TS9g|TS2a|TS2b|TS6c-1`。
- P1：`P1-triangle-slope-placement/post-fix/` 与 `final-r3/*/TS9d|TS9e|TS6c-1|TS6c-2`。
- P2：`P2-inland-water-gap/post-fix/` 与 `final-r3/*/TS9a|TS9i|TS9j|TS1a|TS7e`。
- 中间轨迹：`verify-r1/`、`P0P1-experiment/`、`P1-single-stair/`、`P1-official-r2/`、`regression-final/`
  （保留，不冒充最终）。

## 11. 自动化测试

- **EditMode 368/368 全过**（`test-evidence/final-r3/editmode-r29.xml`；过程失败 XML 另存 `test-evidence/`）。
- PlayMode 三种子：`final-r3/seed-*/test-results.xml`（16 passed / 1 skipped / 0 failed，skip 为既有 tpr3）。
- 扫描：Iter09 P0/P1/P2 + Iter10 P0/P1/P2 全部 0 违规（三种子）。

## 12. 证据仓库提交

`2b9f117`（证据仓库 main；本轮新增 final-r3 / P1-official-r2 / P1-official-2cell /
test-evidence/final-r3，更新 README/final-report/SHA256SUMS；历史轨迹目录全部保留）

## 13. 远端 SHA-256 核验

稀疏 clone（分叶子目录检出，代理 127.0.0.1:7897）+ `sha256sum -c`：**853/853 全部 OK，0 失败**
（远端真实字节；复算明细 `remote-verify.txt` 归档于 test-evidence/final-r3/）。

## 14. 历史证据零覆盖

远端 `git ls-tree` 核验 iter-00~09 目录全部保留，历史文件未覆盖。

## 15. 主仓库状态

- HEAD 仍为 `f85fbe3`（零 git 写）；工作区保留本轮生产修改与测试更新（未提交/未推送）。

## 16. 仍然存在的缺陷、未验证项与限制

1. **Mount→低地无 Hill 楼梯**：两级高差需两级台阶，素材只有 color1 Hill 坡；Mount 南缘以崖壁承担。
2. **`_36/_39` 与 `_45/_48` 配对**：按素材像素实测（石脚左右）配对；官方示意图差异**待验证**。
3. **color3 边带色差**：Mount 顶面水缘使用 color1 band（已知局限）。
4. **立面格可走性**：数据层 Hill 可走、视觉为悬崖格（官方素材体系固有）——**待制作人确认**。
5. **Shadow 层半透明方块**：阴影贴图 3×3 覆盖的硬边在部分镜头可见（既有层行为，非本轮缺陷）。
6. **Agent D 视觉复核（verify-r1 批）逐项对照**：其 P0-1/P0-2/P0-3/P0-5/P0-6 在 final-r3 不成立
   （东西墙按官方删除、墙单行无纵向叠加、楼梯形态与官方 guide 一致、直切为素材端头/阴影矩形、
   水脚白浪为素材设计）；P0-4 内陆水泡沫边**不采纳**（规范 D.2 明确岸线泡沫语义、iter-01 已裁定
   内陆水域合法）。完整对照见 `peer-review/agent-d-verify-r1-review.md` 与裁剪件 `peer-review/crops/`。
