# D-067-R1 证据（第二世界地形 + Toast 遮挡修复）

## 内容

| 路径 | 说明 |
| --- | --- |
| `shots/` | 修复后重新拍摄的 16 张真实运行截图（同机位同流程，便于与 D-067 对比） |
| `tests/playmode-terrain-rebuild.xml` | 世界尺寸切换专项：192→64→128→256→64 与 64→256→128，2/2 通过 |
| `tests/playmode-capture.xml` | 截图用例，1/1 通过 |

## 根因与修复

1. **第二世界地图不可见**：`CameraRig.CenterOnMap` 默认只在**首次**装配生效（保护玩家已调整的视角），
   换世界时未强制居中 → 相机仍停在上一个世界中心（192→64 时停在 (96,96)，地图只有 0..63，视野全是地图外）。
   修复：`BuildWorldScene` 调 `CenterOnMap(w,h,force:true)`。地形数据本身一直正确
   （诊断：64 世界 landCells=2767、tilemap 单元格=2767、bounds 在新地图内）。
2. **Toast 遮挡界面**：Toast 底条挂在未正确约束的容器上，实际渲染在屏幕中部且宽 720，
   覆盖主菜单/存档列表/设置面板；计时用 `WaitForSeconds`（受 timeScale 影响）。
   修复：Toast 重挂到流程根节点、底部安全区（菜单 y=8 / 游戏内 y=124）、宽度随屏幕收缩到 ≤760、
   改用 `WaitForSecondsRealtime`、切页/开弹窗/回主菜单时 `HideToast()`。
3. **世界信息条切换世界后消失**：`ShowMenuUi` 隐藏后 `EnterGameUi` 未重新启用 → 已修复。

## 对比

- `20-second-world-64.png`：32 KB（纯海水）→ **324 KB（陆地/水域/森林/装饰/HUD 信息条齐全）**
- `17-main-menu-with-save.png`：中部深色提示条 → 无遮挡
- `21/22-hud-*.png`：704 KB / 773 KB（地形可见）
