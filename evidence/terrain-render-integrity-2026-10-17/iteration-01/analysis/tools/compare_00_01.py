# iteration-00 vs iteration-01 逐字节对照：同名 PNG 完全一致或有差异（含差异像素数与 bbox）
import os, hashlib

BASE = r"D:\data\UnityProjects\WorldSandboxPrototype\docs\impl\evidence\terrain-render-integrity-2026-10-17"
I0 = os.path.join(BASE, "iteration-00-baseline")
I1 = os.path.join(BASE, "iteration-01")

def sha(path):
    h = hashlib.sha256()
    with open(path, "rb") as f:
        for chunk in iter(lambda: f.read(1 << 16), b""):
            h.update(chunk)
    return h.hexdigest()

try:
    from PIL import Image
    has_pil = True
except ImportError:
    has_pil = False

pairs = []
for seed in ("seed-20261013", "seed-20261014", "seed-20261015"):
    d0 = os.path.join(I0, seed)
    d1 = os.path.join(I1, seed)
    if not os.path.isdir(d1):
        print(f"!! {d1} 不存在")
        continue
    names = sorted(f for f in os.listdir(d1) if f.endswith(".png"))
    for n in names:
        p0 = os.path.join(d0, n)
        p1 = os.path.join(d1, n)
        if not os.path.exists(p0):
            print(f"[{seed}/{n}] 仅存在于 01（00 未命中项）")
            continue
        s0, s1 = sha(p0), sha(p1)
        if s0 == s1:
            pairs.append((seed, n, "IDENTICAL", 0, None))
            continue
        # 有差异 → 用 PIL 量化差异
        if has_pil:
            a = Image.open(p0).convert("RGB")
            b = Image.open(p1).convert("RGB")
            if a.size != b.size:
                pairs.append((seed, n, f"DIFF-SIZE {a.size} vs {b.size}", -1, None))
                continue
            pa, pb = a.load(), b.load()
            w, h = a.size
            cnt = 0
            minx, miny, maxx, maxy = w, h, -1, -1
            for y in range(0, h, 2):          # 隔行采样加速（差异定位足够）
                for x in range(0, w, 2):
                    if pa[x, y] != pb[x, y]:
                        cnt += 1
                        if x < minx: minx = x
                        if x > maxx: maxx = x
                        if y < miny: miny = y
                        if y > maxy: maxy = y
            bbox = f"({minx},{miny})-({maxx},{maxy})" if cnt else "-"
            pairs.append((seed, n, f"DIFF(采样 {cnt}px)", cnt, bbox))
        else:
            pairs.append((seed, n, "DIFF-BYTES", -1, None))

ident = [p for p in pairs if p[2] == "IDENTICAL"]
diff = [p for p in pairs if p[2] != "IDENTICAL"]
print(f"== 00 vs 01 对照：共 {len(pairs)} 对；完全一致 {len(ident)}；有差异 {len(diff)} ==")
print("-- 差异清单 --")
for seed, n, kind, cnt, bbox in diff:
    print(f"  {seed}/{n}: {kind} bbox={bbox}")
print("-- 一致清单 --")
for seed, n, kind, cnt, bbox in ident:
    print(f"  {seed}/{n}")
