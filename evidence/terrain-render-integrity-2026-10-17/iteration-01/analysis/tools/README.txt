# analysis/tools —— D-065 iter-01 审计工具（自建脚本，非项目生产代码）

全部为一次性审计工具（Python 3 + Pillow；运行加 -I 隔离模式）。随批发布以供独立复验。

| 脚本 | 用途 | 运行示例 |
| --- | --- | --- |
| `render_diff_publish.py <iteration-NN 目录>` | 逐格渲染一致性对照——数据水格 mask（../water-mask-seed-*.txt）vs 该批 TS1b 截图水色（extra/missing + 覆盖分类） | `python -I render_diff_publish.py ../../` |
| `stability_report.py` | iteration-00 vs iteration-01 全像素稳定性对照（逐图差异像素数 + 动画色系归因） | `python -I stability_report.py > out.txt` |
| `make_lake_grids.py <iteration-01 目录> <输出目录>` | 内湖放大 + 格线（可读格坐标）图与树冠/水色对比图（本轮已产出的 PNG 即由它生成） | `python -I make_lake_grids.py ../../ ../..` |
| `compare_00_01.py` | 00/01 同名文件哈希与差异速览（stability_report 的轻量版） | `python -I compare_00_01.py` |
| `offline-recompute/` | 离线逐位复算（Roslyn csc + Unity Mono 运行真实源文件）——water-audit-stats / water-mask 即其输出 | 见其 README.txt |

注：脚本内的绝对路径常量（BASE/OFF 等）为生成时环境路径；独立复验时按本批目录结构调整即可。
（坐标换算约定：TS1a ortho40 = 9px/格、TS1b ortho64 = 5.625px/格、相机中心 (64,64) = 屏幕 (640,360)。）
