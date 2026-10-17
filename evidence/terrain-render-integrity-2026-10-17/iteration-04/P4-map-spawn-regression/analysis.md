# P4 · 肉块/鸭子移除回归（D-065 iter-04，不改实现、仅回归）

> 制作人：P4 的 iter-03 处理已被暂时接受，本轮**不重构 ScatterStep**，只做三种固定种子回归验证。

## 一、回归结果（三种子）

| 检查项 | 结果 |
| --- | --- |
| 世界地图不生成肉块视觉对象 | ✅ `scan-iter04.txt`：meat 像素 **0**（三种子全部机位）；EditMode `ScatterExclusionTests.D065I3_NoMeatFoodNodes` ×3 全过 |
| 世界地图不生成鸭子装饰 | ✅ `ScatterExclusionTests.D065I3_NoRubberDuckDecorations` ×3 全过；画面扫描 duck 残留 **1486px 与 iter-03 完全一致**（=金矿同色客观误报 (233,240,68)≈(222,236,75)，已打标实证，非鸭子） |
| 树木/金矿/灌木/云/岩石无因 P4 回归改变 | ✅ 离线逐位对照（iter-03 批 `offline-compare` 结论保留）；iter-04 画面扫描的 duck/meat 数值与 iter-03 完全相同（无新增无消失） |
| 资源枚举/Sprite/必要引用保留 | ✅ 未改任何资产/枚举/配置（本轮 P4 零代码改动） |

## 二、与 P1/P2/P3 的交叉影响说明
iter-04 的 P1 底片重选与 P2 贴花会改变部分像素（Mount 区/边界格），故逐像素"与 iter-03 全同"
不成立；P4 的回归判据用**散布数据断言**（EditMode 图层）+ **扫描计数一致性**（1486/0）双重口径，
不受渲染层改动影响。
