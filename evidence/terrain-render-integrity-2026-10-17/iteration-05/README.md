# iteration-05 —— D-065 对照官方示例的五项整改

> 采集：2026-10-17｜Unity 6000.6.5f1 / URP 2D｜窗口化真实渲染 1280×720｜三种子 20261013/14/15
> 本轮依据：**官方素材示意图（项目根 Tiny-Swords-官方素材示意图.jpg）** + **官方 Tilemap Guide**
> + 原始 PNG/切片/pivot/导入设置实测，逐项修复。官方示意图核心结论：**高地顶面=草坪块（与低地
> 相同的草地拼接）+ 崖壁环，无额外纹理层**；开放水面在官方风格中有小石，但**本项目规则明令禁止
> 独立石头散布于开放海面**（任务书原件）。

## 一、目录

```
iteration-05/
  P1-inland-ground-and-color-boundaries/  # 结构性论证+三口径扫描+选片清单+前后图+交界面裁片核查
  P2-official-elevated-ground-top/        # 移除贴花的官方地表拼接：iter-03/04/05 三版对照
  P3-cliffs-slopes-stairs/                # 墙脚族/坡向/地形合法性四层验证 + 无遮挡近景
  P4-no-open-water-rocks/                 # 散布契约禁用：前后图+离线数据+diff
  P5-resource-size-variants/              # 素材清单(bbox 实测)+变体分布+树桩修正
  regression-final/
    seed-20261013|14|15/                  # 本批全量截图（33/36/36 张）+XML/log
    test-evidence/                        # EditMode r1(失败留证)/r1b/r2（**327/327 全过**）
  README.md / SHA256SUMS.txt
```

## 二、结论速览

1. **P2（已修复——官方语义）**：**移除 MountGrassOverlay 贴花全链**（iter-03 整片叠加被拒因格线；
   iter-04 派生贴花格线消除但非官方机制）——顶面回归 **官方地表片拼接**（color1/color3 双族显式选片，
   Elevated 层 160）；三版对照图随批。**生产删除**：SortingOrders 常量 161、Scene 接线、渲染器贴花代码。
2. **P4（已修复）**：水中岩石散布**禁令**（discard 契约，同肉块/鸭子模式）——离线 waterRock=0
   （decor 减少量=原水石数 24/30/30 精确吻合）、断言锁定、**真实画面对照**（iter-04 崖脚/水面多颗
   青白小石 → iter-05 同机位全部消失，仅草地陆石合法保留）。
3. **P5（已核查+一处修正）**：金矿官方 **6 变体=真实大小递进**（alpha bbox 32×29→92×81，约 3×）——
   场景 6/6 全接、散布 variant 0-5 实测出现；树 2 种（119×190 / 80×122 不同高度）；树桩 4 变体
   （本轮修正曾误加的重复接线）；岩石/灌木 4 变体。
4. **P1（三层验证 + 如实）**：垫底 100% 不透明 ⇒ **非水格在结构上不可能显示水背景色**（数学必然）；
   三口径扫描（纯水色/浅青白/青蓝广窗逐格）**内陆格 0**；青蓝细节全部归属**墙素材石壁高光**
   （(140,194,195)，素材实测）；全图选片清单三种子 **0 违规**。**当前构建无法复现制作人所述的
   内陆水色缝隙**——若持有具体截图/坐标请提供，逐格清单可即时定位（不宣称"问题不存在"）。
5. **P3（四层验证 + 无遮挡近景）**：新增**墙脚族 vs 南邻地形**维度（水脚 _50-53↔南临水、陆脚
   _41-44↔南非水，176/314/311 实例 **0 不一致**）；坡向（_36↔东高、_39↔西高）0 违规；逐实例
   地形合法性 0 违规；素材几何与映射一致。生产未改（未发现需修复项——如实）。

## 三、修复代码差异（相对 iteration-04）

| 文件 | 改动 |
| --- | --- |
| `Assets/_Project/Scripts/World/GenSteps/ScatterStep.cs` | P4：WaterRock discard（散布契约禁用） |
| `Assets/_Project/Scripts/Presentation/TerrainTilemapRenderer.cs` | P2：移除贴花全链（层/字段/属性/构建/ApplyCell 段） |
| `Assets/_Project/Scripts/Presentation/SortingOrders.cs` | P2：删除 MountGrassOverlay=161（注释留历史） |
| `Assets/_Project/Scenes/Main.unity` | P2：删除贴花 4 sprite 接线；P5：树桩数组修正回 4 |
| `Assets/Tests/EditMode/*` | P2 契约重写、P4 新断言（水岩=0/陆岩落水=0）、水岩旧区间断言更新、AuditTests 列调整 |
| `Assets/Tests/PlayMode/MapVisualOverhaulCaptureTests.cs` | 移除 TS6a（贴花三档）；TS6b 无遮挡近景保留 |

**历史素材保留**：`Art/Terrain/Derived/MountGrassDetail_1..4.png`（贴花 PNG）与 iter-03/04 批截图
**原样保留**（未覆盖）；04 表 R-LAYER-07b 行标注**已废弃**。

## 四、回归与保护
- **EditMode 327/327 全过**（r2；r1 的 6 失败为水岩旧断言与 stump 数组误加，均已修正留证）；
- PlayMode 截图套件三种子 11 total/10 passed（1 skipped=机位未命中）×3，XML/日志持久化；
- 历史证据（iter-00~04）采前/采后 `sha256sum -c` 零覆盖；本批写盘全经 CaptureWriteGuard
  （白名单根+tri- 前缀+已存在拒绝）；主仓库零 git 写。

## 五、局限与如实声明
1. P1 的"无法复现"以三层验证（结构性/扫描/清单）为据——**不构成对制作人观察的否定**，请求具体位置；
2. P3 未发现规则缺陷属四层独立验证结论（含本轮新增墙脚族维度），生产未改；
3. P5 的"官方大小变体"以 alpha bbox 实测为据，不虚构素材清单外的变体；
4. 官方示意图中开放水面确有装饰小石（官方风格），**本项目按任务书规则禁用**——差异已记录。
