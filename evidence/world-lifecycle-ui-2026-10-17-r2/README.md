# D-067-R2 证据（Toast 遮挡彻底修复）

## 根因

`ScreenFlow` 节点是**普通 Transform**（没有 RectTransform），其下所有 Stretch 布局退化为"屏幕中央 100×100"，
因此底部安全区的 Toast 实际被算到屏幕中部、并压住暂停面板。屏幕上看到的是 Toast，不是世界信息条。

## 修复

1. `ScreenFlow` 改为铺满 Canvas 的 RectTransform（anchor 0..1、offset 0）——所有流程界面的布局坐标自此与屏幕一致；
2. Toast 位置按状态分离：菜单/新建/读档/设置 → 底部 y=8；游戏运行/暂停 → 底部工具栏之上 y=124；
3. 宽度 `min(760, 屏宽-40)`、`WaitForSecondsRealtime` 计时、切页 `HideToast()`（R1 已加，本轮沿用）。

## 几何验收（真实 RectTransform 世界角点 → 屏幕矩形）

| 场景 | 结果 |
| --- | --- |
| 暂停面板 + 保存 Toast | Toast rect y=3.08（旧：y=243 覆盖面板），与面板**不相交** |
| 主菜单 / 新建世界 / 存档列表 / 设置 + Toast | 均不相交 |
| 切页后旧 Toast | alpha=0（已清理） |
| 暂停中过期 | 4.5s 内 alpha<0.05（真实时间，timeScale=0 也过期） |

## 文件

`tests/playmode-toast-layout.xml`（2/2 通过）、`tests/playmode-capture.xml`（1/1）、`shots/`（16 张复拍）。

## 未完成

本地化（P1）本轮**未完成**：`D067Strings` 双语目录已存在，但 `ScreenFlowUI` 尚未接线，语言切换不影响新界面。
