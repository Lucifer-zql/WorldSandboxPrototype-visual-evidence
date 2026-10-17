# iteration-01 版：内湖放大 + 格线（可读格坐标）图 + 树冠色 vs 水色对比图
# 用法：python -I make_lake_grids.py <iteration-01 目录> <输出目录>
import sys, os
from PIL import Image, ImageDraw

I1 = sys.argv[1]
OUT = sys.argv[2]
os.makedirs(OUT, exist_ok=True)
S = 5.625; OX, OY = 640.0, 360.0  # TS1b

def cell_px(gx0, gy0, gx1, gy1, img):
    x0 = OX + (gx0 - 64) * S; x1 = OX + (gx1 + 1 - 64) * S
    y1p = OY - (gy0 - 64) * S; y0p = OY - (gy1 + 1 - 64) * S
    return (max(0, int(x0) - 3), max(36, int(y0p) - 3), min(img.width, int(x1) + 3), min(img.height, int(y1p) + 3))

# (seed, 名称, gx0, gy0, gx1, gy1)  —— 来自 iter-01 审计的内湖清单（含 1 格外扩）
lakes = [
    ("20261013", "lake-A-seed13-vertical", 17, 55, 31, 85),
    ("20261013", "lake-B-seed13-north", 45, 96, 62, 117),
    ("20261014", "lake-C-seed14-north", 43, 95, 67, 113),
    ("20261015", "lake-D-seed15-north", 70, 11, 116, 35),
    ("20261015", "lake-E-seed15-round", 19, 76, 32, 88),
    ("20261015", "lake-F-seed15-small", 48, 47, 57, 57),
    ("20261013", "edge-strip-seed13-left", 0, 15, 10, 62),   # 边缘 1 格窄水道
]

for seed, name, gx0, gy0, gx1, gy1 in lakes:
    img = Image.open(os.path.join(I1, f"seed-{seed}", "TS1b-fullmap-ortho64.png")).convert("RGB")
    box = cell_px(gx0, gy0, gx1, gy1, img)
    crop = img.crop(box)
    w, h = crop.size
    s = max(2, min(10, 900 // max(w, h)))
    big = crop.resize((w * s, h * s), Image.NEAREST)
    d = ImageDraw.Draw(big)
    d.rectangle([0, 0, w * s - 1, 30], fill=(0, 0, 0))
    d.text((8, 5), f"{name} seed-{seed} cells({gx0},{gy0})-({gx1},{gy1}) TS1b", fill=(255, 255, 0))
    for gx in range(gx0, gx1 + 2):
        px = (OX + (gx - 64) * S - box[0]) * s
        d.line([(px, 30), (px, h * s)], fill=(255, 0, 255), width=2 if gx % 10 == 0 else 1)
    for gy in range(gy0, gy1 + 2):
        py = (OY - (gy - 64) * S - box[1]) * s
        d.line([(0, py), (w * s, py)], fill=(255, 0, 255), width=2 if gy % 10 == 0 else 1)
    p = os.path.join(OUT, f"{name}.png")
    big.save(p)
    print(p, big.size)

# 树冠色 vs 水色对比图（seed13：树群近景 vs 水面近景 + 色标）
img13 = Image.open(os.path.join(I1, "seed-20261013", "TS1b-fullmap-ortho64.png")).convert("RGB")
img15 = Image.open(os.path.join(I1, "seed-20261015", "TS1b-fullmap-ortho64.png")).convert("RGB")

def crop_cells(img, gx0, gy0, gx1, gy1, scale):
    box = cell_px(gx0, gy0, gx1, gy1, img)
    c = img.crop(box)
    return c.resize((c.width * scale, c.height * scale), Image.NEAREST)

# 树群（seed13 图 825,69 附近的树丛 → cell ~(97,86)；用数据无关的目测点）
trees = crop_cells(img13, 95, 83, 105, 93, 6)
water = crop_cells(img15, 22, 79, 30, 87, 6)

W = trees.width + water.width + 40
Hh = max(trees.height, water.height) + 120
comp = Image.new("RGB", (W, Hh), (24, 24, 28))
comp.paste(trees, (0, 0))
comp.paste(water, (trees.width + 40, 0))
d = ImageDraw.Draw(comp)
d.rectangle([0, 0, W - 1, 26], fill=(0, 0, 0))
d.text((8, 5), "LEFT: conifer crowns (seed13)   RIGHT: lake water (seed15)", fill=(255, 255, 0))
y = max(trees.height, water.height) + 10
swatches = [
    ("water 71,171,168", (71, 171, 168)),
    ("crown dark 45,131,129", (45, 131, 129)),
    ("crown mid 55,174,122", (55, 174, 122)),
    ("crown outline 22,28,46", (22, 28, 46)),
]
x = 8
for label, col in swatches:
    d.rectangle([x, y, x + 34, y + 34], fill=col, outline=(200, 200, 200))
    d.text((x + 40, y + 10), label, fill=(230, 230, 230))
    x += 240
p = os.path.join(OUT, "tree-vs-water-color-contrast.png")
comp.save(p)
print(p, comp.size)
