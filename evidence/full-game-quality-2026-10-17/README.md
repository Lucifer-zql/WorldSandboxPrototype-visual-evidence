# full-game-quality-2026-10-17（D-068 修复批次）

## 本批实际完成

| 项 | 结果 |
| --- | --- |
| `Civ_System_GrowsPopulationAndUsesEconomy` | 通过（实测 pop=6 / houses=3 / 资源 529→493） |
| 外交：`Civ_System_ProducesDiplomacyStates` + 3 个可控场景 | 4 / 4 通过（近邻→Hostile、远邻→Friendly、中距→Neutral、所有配对 Evaluations>0） |
| `CivilizationSimulationTests` 全量 | 10 / 10 通过 |
| `SaveService_` 回归 | 5 / 5 通过 |
| 自动存档轮换（上一轮修复） | 4 / 4 通过（同刻唯一 ID、保留 3 份、手动档隔离） |

## 修复要点

1. **人口/经济**：人口为 0 时不再扣食物（旧实现每次决策"收 1 吃 1"，食物恒 0、永远无法出生）；
   出生改为"容量未满 + 食物 ≥ 2+人口"，成功出生才扣粮；树木食物 +2、食物点 +5；房屋成本改为木材 12（金币 0，MVP）。
2. **外交**：距离阈值改为**地图相对尺度**（近邻 18% 对角线 / 远邻 45%），修复 64 图固定 10/20 格不可达的问题；
   新增 `RelationState.Evaluations`（评估次数，与 `Interactions` 交互次数语义分离）并纳入存档；
   近邻有概率摩擦、远邻且不饥荒有概率通商、敌对超 240 tick 可缓和。

## 已知限制（本批未完成）

- 文明详情面板、事件日志的文明事件与本地化、自动存档生命周期 PlayMode 用例、地形/素材审查、全量 EditMode 与 D-067 系列回归、双语截图：**本批未执行**。
- 本目录只有测试报告，**没有本批截图**（截图未采集，未用历史截图冒充）。

## 复现

```
Unity.exe -batchmode -projectPath <repo> -runTests -testPlatform EditMode \
  -testFilter CivilizationSimulationTests -testResults tests/civilization-simulation.xml
```
