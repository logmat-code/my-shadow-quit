#!/usr/bin/env python3
"""
Generates rich, bespoke, dark-fantasy pixel art card previews for:
- WORLD 4: THE DROWNED CATHEDRAL (Submerged Gothic Cathedral, underwater bioluminescence, rose window, sunken arches, rays through deep tide)
- WORLD 5: THE ABYSS OF THE LAST FLAME (Fractured Reality, floating obsidian monoliths, abyssal void rift, brilliant golden Last Flame)

Native 248x145 canvas, scaled 2x to standard 496x290 for WORLD SELECT card preview.
Pure Python (zero external dependencies).
"""

import math, random, struct, zlib, os

def create_image(w, h):
    return [[(0, 0, 0, 255) for _ in range(w)] for _ in range(h)]

def clampc(v):
    return max(0, min(255, int(v)))

def blend(img, x, y, c, a=1.0):
    h = len(img)
    w = len(img[0])
    if 0 <= x < w and 0 <= y < h:
        r, g, b, _ = img[y][x]
        img[y][x] = (clampc(r + (c[0] - r) * a), clampc(g + (c[1] - g) * a), clampc(b + (c[2] - b) * a), 255)

def rect(img, x0, y0, x1, y1, c, a=1.0):
    for y in range(max(0, y0), min(len(img), y1 + 1)):
        for x in range(max(0, x0), min(len(img[0]), x1 + 1)):
            blend(img, x, y, c, a)

def circle(img, cx, cy, r, c, a=1.0):
    h = len(img)
    w = len(img[0])
    r2 = r * r
    for y in range(max(0, int(cy - r - 1)), min(h, int(cy + r + 2))):
        for x in range(max(0, int(cx - r - 1)), min(w, int(cx + r + 2))):
            if (x - cx)**2 + (y - cy)**2 <= r2:
                blend(img, x, y, c, a)

def ring(img, cx, cy, r0, r1, c, a=1.0):
    h = len(img)
    w = len(img[0])
    r0_2 = r0 * r0
    r1_2 = r1 * r1
    for y in range(max(0, int(cy - r1 - 1)), min(h, int(cy + r1 + 2))):
        for x in range(max(0, int(cx - r1 - 1)), min(w, int(cx + r1 + 2))):
            d2 = (x - cx)**2 + (y - cy)**2
            if r0_2 <= d2 <= r1_2:
                blend(img, x, y, c, a)

def line(img, x0, y0, x1, y1, c, a=1.0):
    steps = int(max(abs(x1 - x0), abs(y1 - y0))) * 2 + 1
    for i in range(steps + 1):
        t = i / float(steps) if steps > 0 else 0
        blend(img, int(round(x0 + (x1 - x0) * t)), int(round(y0 + (y1 - y0) * t)), c, a)

def lerp(a, b, t):
    return tuple(a[i] + (b[i] - a[i]) * t for i in range(3))

def save_scaled_png(img, out_path, scale=2):
    h = len(img)
    w = len(img[0])
    out_w = w * scale
    out_h = h * scale
    raw = bytearray()
    for y in range(out_h):
        raw.append(0)  # filter none
        src_y = y // scale
        for x in range(out_w):
            src_x = x // scale
            r, g, b, a = img[src_y][src_x]
            raw.extend((r, g, b, a))

    def chunk(tag, data):
        return struct.pack('>I', len(data)) + tag + data + struct.pack('>I', zlib.crc32(tag + data) & 0xFFFFFFFF)

    png = b'\x89PNG\r\n\x1a\n' + chunk(b'IHDR', struct.pack('>IIBBBBB', out_w, out_h, 8, 6, 0, 0, 0)) + chunk(b'IDAT', zlib.compress(bytes(raw), 9)) + chunk(b'IEND', b'')
    os.makedirs(os.path.dirname(out_path), exist_ok=True)
    with open(out_path, 'wb') as f:
        f.write(png)
    print(f"Saved {out_path} ({out_w}x{out_h})")

# =========================================================================
# WORLD 4: THE DROWNED CATHEDRAL
# =========================================================================
def generate_world4_preview():
    W, H = 248, 145
    img = create_image(W, H)
    rnd = random.Random(40404)

    # 1. Deep abyssal ocean gradient with greenish-cyan bioluminescence
    c_deep = (4, 10, 22)
    c_mid = (12, 38, 55)
    c_top = (25, 75, 95)
    c_aqua = (45, 140, 160)

    for y in range(H):
        t = y / float(H - 1)
        bg = lerp(c_deep, c_mid, t / 0.6) if t < 0.6 else lerp(c_mid, c_top, (t - 0.6) / 0.4)
        for x in range(W):
            noise = (rnd.random() - 0.5) * 4.0
            col = (clampc(bg[0] + noise), clampc(bg[1] + noise), clampc(bg[2] + noise))
            blend(img, x, y, col, 1.0)

    # 2. Light shafts / rays piercing deep water from above (top-left to bottom-right)
    for ray_x in range(30, W - 20, 24):
        ray_alpha = 0.08 + rnd.random() * 0.06
        for y in range(H):
            rx = int(ray_x + (y * 0.45) + math.sin(y * 0.1) * 2.0)
            rect(img, rx - 6, y, rx + 6, y, c_aqua, ray_alpha * (1.0 - y / float(H) * 0.6))

    # 3. Distant cathedral silhouette & sunken buttresses
    col_dist_stone = (8, 24, 38)
    for bx in range(20, W - 20, 32):
        rect(img, bx, 25, bx + 16, H, col_dist_stone, 0.85)
        # Arch apex
        for y in range(12, 26):
            w_arch = int(8 * (1.0 - (25 - y) / 14.0))
            rect(img, bx + 8 - w_arch, y, bx + 8 + w_arch, y, col_dist_stone, 0.85)

    # 4. Central Grand Rose Window (submerged Gothic wheel window)
    cx, cy = 124, 60
    # Outer glow
    circle(img, cx, cy, 38, (50, 180, 205), 0.25)
    circle(img, cx, cy, 32, (80, 225, 235), 0.45)
    circle(img, cx, cy, 26, (140, 245, 255), 0.65)
    circle(img, cx, cy, 14, (210, 255, 255), 0.85)

    # Stone tracery spokes & rings
    ring(img, cx, cy, 28, 31, (18, 42, 58), 0.95)
    ring(img, cx, cy, 18, 20, (18, 42, 58), 0.95)
    ring(img, cx, cy, 8, 10, (18, 42, 58), 0.95)
    for i in range(12):
        ang = i * (math.pi / 6.0)
        x2 = int(cx + math.cos(ang) * 30)
        y2 = int(cy + math.sin(ang) * 30)
        line(img, cx, cy, x2, y2, (18, 42, 58), 0.95)

    # 5. Foreground Gothic Submerged Pillars & Ruined Flooded Nave
    c_fore_stone = (12, 28, 40)
    c_highlight = (35, 90, 115)
    # Left Massive Pillar
    rect(img, 0, 0, 42, H, c_fore_stone)
    rect(img, 40, 0, 42, H, c_highlight, 0.7)
    # Right Massive Pillar
    rect(img, W - 42, 0, W, H, c_fore_stone)
    rect(img, W - 42, 0, W - 40, H, c_highlight, 0.7)

    # Overhead Gothic Arch connecting pillars
    for y in range(0, 36):
        span = int(100 * (1.0 - y / 36.0)**1.5)
        rect(img, 0, y, 42 + span, y, c_fore_stone)
        rect(img, W - 42 - span, y, W, y, c_fore_stone)

    # 6. Sunken Cathedral Floor & Steppes
    rect(img, 0, H - 28, W, H, c_fore_stone)
    rect(img, 0, H - 28, W, H - 26, c_highlight, 0.9)
    rect(img, 30, H - 20, W - 30, H, (10, 22, 34))

    # 7. Protagonist & Shadow at the Submerged Altar
    px, py = 112, H - 29
    # Protagonist (warm amber torch in hand)
    rect(img, px, py - 14, px + 6, py, (22, 20, 28))
    rect(img, px + 2, py - 18, px + 5, py - 14, (180, 150, 130))
    # Torch flame glow underwater (amber halo)
    circle(img, px + 7, py - 15, 12, (255, 140, 30), 0.35)
    circle(img, px + 7, py - 15, 6, (255, 200, 70), 0.75)
    circle(img, px + 7, py - 15, 2, (255, 255, 200), 1.0)

    # Shadow (ethereal violet beside him)
    sx = px + 16
    rect(img, sx, py - 15, sx + 6, py, (60, 25, 95), 0.85)
    circle(img, sx + 3, py - 9, 8, (140, 60, 220), 0.4)

    # 8. Bioluminescent particles & sea bubbles
    for _ in range(45):
        bx = rnd.randint(15, W - 15)
        by = rnd.randint(15, H - 15)
        circle(img, bx, by, 1, (80, 230, 240), 0.65 + rnd.random() * 0.35)

    save_scaled_png(img, "Assets/Resources/Sprites/UI/world4_card_art.png", scale=2)

# =========================================================================
# WORLD 5: THE ABYSS OF THE LAST FLAME
# =========================================================================
def generate_world5_preview():
    W, H = 248, 145
    img = create_image(W, H)
    rnd = random.Random(50505)

    # 1. Void sky: deep obsidian purple into dark magenta fracture
    c_void_deep = (6, 3, 12)
    c_void_mid = (18, 6, 32)
    c_void_high = (45, 12, 65)

    for y in range(H):
        t = y / float(H - 1)
        bg = lerp(c_void_deep, c_void_mid, t / 0.7) if t < 0.7 else lerp(c_void_mid, c_void_high, (t - 0.7) / 0.3)
        for x in range(W):
            noise = (rnd.random() - 0.5) * 3.0
            col = (clampc(bg[0] + noise), clampc(bg[1] + noise), clampc(bg[2] + noise))
            blend(img, x, y, col, 1.0)

    # 2. Celestial Void Rift (Central spatial tear with violet lightning tendrils)
    cx, cy = 124, 55
    for ang_deg in range(0, 360, 15):
        ang = math.radians(ang_deg)
        r_dist = 50 + rnd.randint(-12, 18)
        x2 = int(cx + math.cos(ang) * r_dist)
        y2 = int(cy + math.sin(ang) * r_dist * 0.6)
        line(img, cx, cy, x2, y2, (160, 50, 240), 0.45)

    circle(img, cx, cy, 45, (80, 20, 130), 0.35)
    circle(img, cx, cy, 25, (180, 60, 255), 0.55)
    circle(img, cx, cy, 10, (230, 160, 255), 0.85)

    # 3. Floating fractured obsidian monoliths / shattered reality islands
    c_obsidian = (14, 10, 22)
    c_rim_light = (130, 55, 190)

    # Distant floating shards
    shards = [
        (40, 40, 24, 10),
        (200, 35, 28, 12),
        (65, 75, 30, 8),
        (185, 78, 34, 10),
        (25, 95, 22, 9),
        (215, 98, 20, 8),
    ]
    for sx, sy, sw, sh in shards:
        rect(img, sx - sw//2, sy, sx + sw//2, sy + sh, c_obsidian)
        rect(img, sx - sw//2, sy, sx + sw//2, sy + 1, c_rim_light, 0.8)

    # 4. Central Altar Floating Island
    ax, ay = 124, H - 32
    aw = 90
    rect(img, ax - aw//2, ay, ax + aw//2, H, c_obsidian)
    rect(img, ax - aw//2, ay, ax + aw//2, ay + 2, (190, 80, 255), 0.9)
    # Underside jagged spikes crumbling into void
    for x in range(ax - aw//2 + 4, ax + aw//2 - 4, 8):
        h_spike = rnd.randint(6, 18)
        line(img, x, ay + 6, x, ay + 6 + h_spike, (25, 14, 38))

    # 5. Dual Reality Bridge beams connecting the altar (spectral cyan/purple)
    for bx in range(15, ax - aw//2 + 5):
        by = ay + int(math.sin(bx * 0.2) * 1.5)
        rect(img, bx, by, bx + 1, by + 2, (70, 220, 240), 0.6)
    for bx in range(ax + aw//2 - 5, W - 15):
        by = ay + int(math.sin(bx * 0.2) * 1.5)
        rect(img, bx, by, bx + 1, by + 2, (210, 70, 255), 0.6)

    # 6. The Altar of the Last Flame (Final Crucible)
    altar_x = ax
    altar_y = ay - 6
    rect(img, altar_x - 14, altar_y, altar_x + 14, ay, (35, 22, 50))
    rect(img, altar_x - 10, altar_y - 8, altar_x + 10, altar_y, (45, 30, 65))
    rect(img, altar_x - 10, altar_y - 8, altar_x + 10, altar_y - 6, (255, 180, 50), 0.8)

    # THE LAST FLAME (Glorious radiant golden-white bloom contrasting the void)
    fx = altar_x
    fy = altar_y - 12
    circle(img, fx, fy, 32, (255, 120, 20), 0.25)
    circle(img, fx, fy, 22, (255, 180, 40), 0.45)
    circle(img, fx, fy, 14, (255, 230, 100), 0.70)
    circle(img, fx, fy, 7, (255, 255, 230), 0.95)
    circle(img, fx, fy, 3, (255, 255, 255), 1.0)

    # 7. Protagonist (holding the spark) and Shadow (manifest as equal companion)
    # Protagonist standing to the left of the altar
    px = altar_x - 24
    py = ay
    rect(img, px, py - 14, px + 5, py, (22, 20, 28))
    rect(img, px + 1, py - 18, px + 4, py - 14, (210, 180, 160))

    # Shadow standing to the right of the altar
    sx = altar_x + 20
    sy = ay
    rect(img, sx, sy - 15, sx + 5, sy, (60, 20, 95), 0.9)
    circle(img, sx + 2, sy - 10, 7, (170, 70, 255), 0.45)

    # 8. Floating golden embers & void crystal sparks
    for _ in range(40):
        ex = rnd.randint(fx - 40, fx + 40)
        ey = rnd.randint(fy - 35, fy + 35)
        circle(img, ex, ey, 1, (255, 210, 80), 0.8)
    for _ in range(30):
        vx = rnd.randint(10, W - 10)
        vy = rnd.randint(10, H - 20)
        circle(img, vx, vy, 1, (180, 80, 255), 0.6)

    save_scaled_png(img, "Assets/Resources/Sprites/UI/world5_card_art.png", scale=2)

def main():
    generate_world4_preview()
    generate_world5_preview()
    print("SUCCESS: Rich bespoke card previews generated for World 4 and World 5!")

if __name__ == "__main__":
    main()
