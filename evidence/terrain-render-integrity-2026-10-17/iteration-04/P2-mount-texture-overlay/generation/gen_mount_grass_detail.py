# -*- coding: utf-8 -*-
# D-065 iter-04 P2 派生透明草纹理生成器（确定性、可追溯）。
# 源：Assets/_Project/Art/Terrain/Tilemap_color1.png 切片 _10（c1r1，64×64，实测 100% 不透明、
#     浅青白 0%、无边框线（边缘 3px 带亮度 160.5≈全体 161.7）、自身 wrap 接缝 左右 2.4/上下 5.2 通道差）。
# 规则：
#   1) low = 源图 GaussianBlur(radius=3)；d = max_c |pixel_c - low_c|（高频残差）
#   2) alpha = clamp((d - 4) * 0.024, 0, 0.6)   # 阈值 4 过滤平区噪声；p90(d)=25 → α≈0.50；上限 0.6
#   3) RGB：亮细节（像素亮度 ≥ 模糊亮度）→ 原色 ×1.35 提亮；暗细节 → 原色 ×0.62 压暗（保色相）
#   4) 4 变体 = 对 (1)-(3) 输出做 wrap 平移 (0,0)/(21,13)/(43,37)/(11,52)——保持点状内容、消除 64px 周期感
#   5) 不含底色、不含格边线——纯点缀 alpha；每格由渲染侧按 (x,y) 哈希选变体。
# 输出：MountGrassDetail_1..4.png（64×64 RGBA）+ 预览拼贴 tile_preview.png（8×8 格自检接缝）。
import io, sys, os
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
from PIL import Image, ImageFilter

ROOT = r'D:/data/UnityProjects/WorldSandboxPrototype'
SRC = os.path.join(ROOT, 'Assets/_Project/Art/Terrain/Tilemap_color1.png')
OUT = os.path.join(os.environ['TEMP'], 'iter01-scan', 'iter04-p2', 'out')
os.makedirs(OUT, exist_ok=True)

src = Image.open(SRC).convert('RGB').crop((64, 64, 128, 128))   # _10
blur = src.filter(ImageFilter.GaussianBlur(3))
sp, bp = src.load(), blur.load()

base = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
ob = base.load()
for y in range(64):
    for x in range(64):
        r, g, b = sp[x, y]
        lr, lg, lb = bp[x, y]
        d = max(abs(r - lr), abs(g - lg), abs(b - lb))
        a = (d - 4) * 0.024
        a = 0.0 if a < 0 else (0.6 if a > 0.6 else a)
        if a <= 0: continue
        lum_p = 0.299 * r + 0.587 * g + 0.114 * b
        lum_b = 0.299 * lr + 0.587 * lg + 0.114 * lb
        k = 1.35 if lum_p >= lum_b else 0.62
        rr = min(255, int(r * k)); gg = min(255, int(g * k)); bb = min(255, int(b * k))
        ob[x, y] = (rr, gg, bb, int(round(a * 255)))

def wrap_shift(img, dx, dy):
    out = Image.new('RGBA', (64, 64))
    for y in range(64):
        for x in range(64):
            out.putpixel(((x + dx) % 64, (y + dy) % 64), img.getpixel((x, y)))
    return out

shifts = [(0, 0), (21, 13), (43, 37), (11, 52)]
variants = [wrap_shift(base, dx, dy) for dx, dy in shifts]
for i, v in enumerate(variants, 1):
    p = os.path.join(OUT, f'MountGrassDetail_{i}.png')
    v.save(p)
    print('saved', p)

# 自检预览：8×8 格，每格按 (x*73856093 ^ y*19349663) % 4 选变体——检查接缝/方格线
prev = Image.new('RGBA', (64 * 8, 64 * 8), (0, 0, 0, 0))
for gy in range(8):
    for gx in range(8):
        h = (gx * 73856093 ^ gy * 19349663) % 4
        prev.alpha_composite(variants[h], (gx * 64, gy * 64))
prev.save(os.path.join(OUT, 'tile_preview.png'))
print('saved tile_preview.png（8×8 平铺自检）')

# 统计：alpha 直方图（透明度方案依据）
hist = [0] * 7
for y in range(64):
    for x in range(64):
        a = base.getpixel((x, y))[3]
        hist[min(6, a * 7 // 256)] += 1
print('alpha 分桶(0..6 of 0-255):', hist)
