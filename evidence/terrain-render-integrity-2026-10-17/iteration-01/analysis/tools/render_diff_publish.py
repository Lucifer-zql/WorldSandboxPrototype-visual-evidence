# 逐格渲染一致性对照（发布版）：数据水格 mask vs TS1b 截图水色 mask
# 用法：python -I render_diff_publish.py <iteration-NN 目录> （mask 恒用离线复算的 mask-seed-*.txt）
import sys, os
from PIL import Image
from collections import Counter

I = sys.argv[1]
OFF = r"C:\Users\18871\AppData\Local\Temp\iter01-scan\offline"
S = 5.625; OX, OY = 640.0, 360.0

def is_water_px(r, g, b):
    return abs(r - 71) < 14 and abs(g - 171) < 14 and abs(b - 168) < 14

def near(c, ref, tol=10):
    return all(abs(c[i] - ref[i]) <= tol for i in range(3))

def classify(col):
    if near(col, (101, 157, 157), 12): return "wall-water-piece(半透明崖壁入水件系色)"
    if near(col, (229, 233, 205), 8) or near(col, (218, 215, 187), 8): return "cloud/ui(云或UI提示条米色)"
    if near(col, (45, 131, 129)) or near(col, (55, 174, 122)) or near(col, (22, 28, 46)) or \
       near(col, (103, 211, 102)) or near(col, (95, 171, 98)): return "tree-crown(针叶树冠系色)"
    if near(col, (233, 244, 250), 10) or near(col, (198, 240, 218), 12) or near(col, (140, 194, 195), 10) or \
       near(col, (61, 144, 143), 8): return "foam-or-light(泡沫/贴岸深色带系)"
    return f"OTHER({col[0]},{col[1]},{col[2]})"

print("## 逐格渲染一致性对照（D-065 iter-01 审计 · 发布版）")
print("## 方法：离线复算数据水格 mask（128×128）vs TS1b 截图像素水色（每格 5.625px 区域、纯水色像素占比>50% 判为画面水）")
print("## 口径：extra=画面水∧数据非水（多画/露底候选）；missing=数据水∧画面非水（被覆盖候选）")
print("## UI 排除：格像素范围与顶栏(py<50)或底部提示条(py>575)交叠则跳过")
print()

for seed in ("20261013", "20261014", "20261015"):
    lines = open(rf"{OFF}\mask-seed-{seed}.txt").read().strip().split("\n")
    data = set()
    for i, line in enumerate(lines):
        gy = 127 - i
        for gx, ch in enumerate(line):
            if ch == "~":
                data.add((gx, gy))

    img = Image.open(os.path.join(I, f"seed-{seed}", "TS1b-fullmap-ortho64.png")).convert("RGB")
    px = img.load()

    def cell_px_range(gx, gy):
        x0 = int(OX + (gx - 64) * S); x1 = int(OX + (gx + 1 - 64) * S)
        y0 = int(OY - (gy + 1 - 64) * S); y1 = int(OY - (gy - 64) * S)
        return x0, y0, x1, y1

    extra, missing = [], []
    checked = 0
    for gy in range(128):
        for gx in range(128):
            x0, y0, x1, y1 = cell_px_range(gx, gy)
            if y1 <= 50 or y0 >= 575:
                continue
            total = wcnt = 0
            for yy in range(max(0, y0), min(720, y1)):
                for xx in range(x0, x1):
                    r, g, b = px[xx, yy]
                    total += 1
                    if is_water_px(r, g, b): wcnt += 1
            if total == 0: continue
            checked += 1
            sw = wcnt / total > 0.5
            dw = (gx, gy) in data
            if sw and not dw: extra.append((gx, gy))
            elif dw and not sw: missing.append((gx, gy))

    print(f"== seed-{seed}: 对照格 {checked}；extra={len(extra)}；missing={len(missing)} ==")
    if extra:
        print("  EXTRA（画面水∧数据非水——如非空则是渲染多画/露底，须即报）:")
        for gx, gy in extra[:30]:
            x0, y0, x1, y1 = cell_px_range(gx, gy)
            c = px[(x0 + x1) // 2, (y0 + y1) // 2]
            print(f"    ({gx},{gy}) center_color={c}")
    cats = Counter()
    samples = {}
    for gx, gy in missing:
        x0, y0, x1, y1 = cell_px_range(gx, gy)
        cols = Counter()
        for yy in range(max(0, y0), min(720, y1)):
            for xx in range(x0, x1):
                cols[px[xx, yy]] += 1
        top = cols.most_common(1)[0][0]
        k = classify(top)
        cats[k] += 1
        samples.setdefault(k, []).append(f"({gx},{gy})top={top}")
    for k, n in cats.most_common():
        print(f"  missing-{n:>3} 类[{k}]  样例: {'; '.join(samples[k][:4])}")
    print()
