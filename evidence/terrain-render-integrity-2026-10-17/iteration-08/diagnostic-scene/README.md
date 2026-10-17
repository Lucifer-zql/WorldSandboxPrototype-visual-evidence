# 确定性诊断场景（iteration-08，任务书 §4.5/§6.4）

> 全部经**真实 Unity 运行时**：场景内真实 `TerrainTilemapRenderer.RenderWorld(store, elevation)`——
> 走生产 `TerrainOverlayPlan.Compute` + Flat/Elevated/Wall/Slope/Stairs/Shadow 全 Tilemap 链路；
> 装饰/资源/云层临时隐藏（`SetActive(false)`）并在 `finally` 恢复；相机只做 Aim（不改地形数据）。
> 测试方法：`MapVisualOverhaulCaptureTests.TS8_Iter08DiagnosticScenes`（TS8a–TS8g）与
> `TS8h_Iter08SlopeBottomCloseup`（TS8h/TS8i），三种子各一份（文件名带种子后缀）。

## 场景数据（固定）

**P0/P2 场景**（128×128 全 Grass）：
- Mount 块：x=60..68, y=60..68（9×9，elev2）；
- Hill 环：x=57..71, y=57..71（13×13，elev1，含 Mount 内部）；
- 低地：其余 elev0。
- 由生产规则自动产生：南缘墙排（Hill 南缘）、东西缘竖列墙、Mount→Hill 中间台面（无墙）、
  **西/东缘坡列**（坡上段 `_36/_39` + 底部下段 `_45/_48`）、北缘顶面过渡、阴影。

**P1 场景**（128×128 全 Grass + 下列覆盖）：
- 水带：x=62..70, y=67（Water）；
- 森林块：x=68..72, y=62..66（Forest）；
- Hill 块：x=60..64, y=62..66（elev1）；
- **混合缘验证格 (66,66)**：北邻水、东邻 Forest、南邻 Grass、西邻 Hill（任务书 §5.1 示例形态）。

## 机位与图
| 图 | 机位（世界中心, ortho） | 验证点 |
| --- | --- | --- |
| TS8a-p0-two-tier-south | (64,58.5), 6 | Mount 顶→Hill 中间台面→Hill 墙→低地逐层 |
| TS8b-p0-west-edge-slope | (56.5,63.5), 6 | 西缘墙列/坡列/下级陆地 |
| TS8c-p0-far-terrace | (64,62), 11 | 退台全貌 |
| TS8d-p1-mixed-edge-water-forest-hill | (66,66), 4.5 | 混合缘四向语义（水白/林流苏/草/Hill 坡） |
| TS8e-p1-mixed-edge-far | (67,65), 8 | 混合缘周边 |
| TS8f-p2-slope-pair-west | (56.5,64), 4 | 坡列（上段斜线连续、直角缘贴高台） |
| TS8g-p2-slope-pair-east | (72.5,64), 4 | 东缘坡列 |
| TS8h-p2-slope-bottom-west | (56.5,57.5), 3 | **坡列底部**：上段末格 + 下段断面落低地 |
| TS8i-p2-slope-bottom-east | (72.5,57.5), 3 | 东缘坡列底部 |
