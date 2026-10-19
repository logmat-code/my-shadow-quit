#!/usr/bin/env python3
"""Procedural pixel-art illustration for WORLD 3 - THE HOLLOW CLOCKTOWER (WORLD SELECT card).
Pure Python (no Pillow). Native 248x145 canvas, exported at 2x nearest-neighbour = 496x290."""
import math, random, struct, zlib, sys

W, H = 248, 145
img = [[(0, 0, 0, 255) for _ in range(W)] for _ in range(H)]
rnd = random.Random(30303)


def clampc(v):
    return max(0, min(255, int(v)))


def blend(x, y, c, a=1.0):
    if 0 <= x < W and 0 <= y < H:
        r, g, b, _ = img[y][x]
        img[y][x] = (clampc(r + (c[0] - r) * a), clampc(g + (c[1] - g) * a), clampc(b + (c[2] - b) * a), 255)


def put(x, y, c):
    blend(x, y, c, 1.0)


def rect(x0, y0, x1, y1, c, a=1.0):
    for y in range(y0, y1 + 1):
        for x in range(x0, x1 + 1):
            blend(x, y, c, a)


def circle(cx, cy, r, c, a=1.0):
    for y in range(int(cy - r - 1), int(cy + r + 2)):
        for x in range(int(cx - r - 1), int(cx + r + 2)):
            if (x - cx) ** 2 + (y - cy) ** 2 <= r * r:
                blend(x, y, c, a)


def ring(cx, cy, r0, r1, c, a=1.0):
    for y in range(int(cy - r1 - 1), int(cy + r1 + 2)):
        for x in range(int(cx - r1 - 1), int(cx + r1 + 2)):
            d2 = (x - cx) ** 2 + (y - cy) ** 2
            if r0 * r0 <= d2 <= r1 * r1:
                blend(x, y, c, a)


def poly(pts, c, a=1.0, shade=None):
    ys = [p[1] for p in pts]
    for y in range(int(min(ys)), int(max(ys)) + 1):
        xs = []
        n = len(pts)
        for i in range(n):
            x1, y1 = pts[i]
            x2, y2 = pts[(i + 1) % n]
            if (y1 <= y < y2) or (y2 <= y < y1):
                xs.append(x1 + (y - y1) * (x2 - x1) / (y2 - y1))
        xs.sort()
        for i in range(0, len(xs) - 1, 2):
            for x in range(int(math.ceil(xs[i])), int(xs[i + 1]) + 1):
                col = shade(x, y) if shade else c
                blend(x, y, col, a)


def line(x0, y0, x1, y1, c):
    steps = int(max(abs(x1 - x0), abs(y1 - y0))) * 2 + 1
    for i in range(steps + 1):
        t = i / steps
        put(int(round(x0 + (x1 - x0) * t)), int(round(y0 + (y1 - y0) * t)), c)


def lerp(a, b, t):
    return tuple(a[i] + (b[i] - a[i]) * t for i in range(3))


# ---------------------------------------------------------------- night sky
top, mid, low = (7, 10, 28), (20, 30, 62), (46, 52, 92)
for y in range(H):
    t = y / (H - 1)
    c = lerp(top, mid, t / 0.55) if t < 0.55 else lerp(mid, low, (t - 0.55) / 0.45)
    for x in range(W):
        # subtle ordered dither for a pixel-art gradient feel
        d = ((x + y) % 2) * 1.6 - 0.8
        img[y][x] = (clampc(c[0] + d), clampc(c[1] + d), clampc(c[2] + d), 255)

# stars
for _ in range(95):
    x, y = rnd.randrange(W), rnd.randrange(int(H * 0.62))
    b = rnd.choice([120, 160, 200, 235])
    put(x, y, (b, b, min(255, b + 20)))
for _ in range(7):
    x, y = rnd.randrange(8, W - 8), rnd.randrange(4, 60)
    for dx, dy in ((0, 0), (1, 0), (-1, 0), (0, 1), (0, -1)):
        put(x + dx, y + dy, (235, 240, 255))

# ---------------------------------------------------------------- moon
MX, MY = 196, 30
for r in range(46, 14, -1):
    blend(0, 0, (0, 0, 0), 0)  # no-op keeps linter calm
    for y in range(MY - r, MY + r + 1):
        for x in range(MX - r, MX + r + 1):
            d = math.hypot(x - MX, y - MY)
            if r - 1 <= d <= r:
                blend(x, y, (150, 175, 230), 0.012 + (46 - r) * 0.0016)
circle(MX, MY, 14, (222, 232, 255))
circle(MX + 3, MY + 2, 12, (206, 219, 248), 0.8)
for cx, cy, r in ((MX - 5, MY - 4, 3), (MX + 4, MY + 5, 2), (MX + 6, MY - 5, 2), (MX - 3, MY + 6, 2)):
    circle(cx, cy, r, (176, 194, 232), 0.85)

# ---------------------------------------------------------------- distant gothic skyline
far = (17, 22, 46)
for cx, h, w in ((14, 58, 12), (34, 40, 9), (222, 52, 11), (240, 36, 9), (64, 28, 8)):
    base = 130
    rect(cx - w // 2, base - h, cx + w // 2, base, far)
    poly([(cx - w // 2 - 1, base - h), (cx, base - h - w * 1.7), (cx + w // 2 + 1, base - h)], far)
    rect(cx, base - h - int(w * 1.7) - 4, cx, base - h - int(w * 1.7), far)

# ---------------------------------------------------------------- giant gears (behind tower)
BRONZE_D, BRONZE_M, BRONZE_L = (62, 40, 26), (110, 70, 38), (176, 118, 58)


def gear(cx, cy, R, teeth, rot, spokes, alpha=1.0, depth=0.0):
    T = max(3, R * 0.11)
    inner = R * 0.62
    hub = R * 0.18
    for y in range(int(cy - R - T - 2), int(cy + R + T + 3)):
        for x in range(int(cx - R - T - 2), int(cx + R + T + 3)):
            dx, dy = x - cx, y - cy
            d = math.hypot(dx, dy)
            if d > R + T + 1:
                continue
            a = math.atan2(dy, dx)
            tooth = (math.sin(teeth * a + rot) > -0.15)
            edge = R + (T if tooth else 0)
            if d > edge:
                continue
            inring = d >= inner
            spoke = False
            if hub <= d < inner:
                s = abs(math.sin(spokes * (a + rot / teeth) / 2.0))
                spoke = s * d < 3.0
            if not (inring or spoke or d < hub):
                continue
            # lighting: top-left lit
            lit = (-dx - dy) / (d + 1e-6)
            col = BRONZE_M
            if lit > 0.35:
                col = BRONZE_L
            elif lit < -0.35:
                col = BRONZE_D
            if d > edge - 1.2:
                col = lerp(col, (30, 20, 14), 0.55)
            if d < hub:
                col = lerp(col, (200, 160, 90), 0.35) if d > hub * 0.45 else (40, 28, 18)
            if inner - 1 <= d <= inner + 0.5 and inring:
                col = lerp(col, (30, 20, 14), 0.5)
            blend(x, y, lerp(col, (20, 26, 52), depth), alpha)


gear(42, 100, 42, 14, 0.0, 6, depth=0.35)
gear(216, 98, 48, 16, 0.2, 8, depth=0.30)
gear(150, 14, 18, 9, 0.5, 5, depth=0.55)
gear(196, 140, 20, 9, 0.9, 6, depth=0.45)

# ---------------------------------------------------------------- main clocktower
STONE, STONE_D, STONE_L = (40, 42, 56), (28, 30, 42), (64, 68, 90)


def stone_shade(x, y):
    row = (y // 6)
    off = 0 if row % 2 == 0 else 5
    col = STONE
    if y % 6 == 0 or (x + off) % 10 == 0:
        col = STONE_D
    return col


def tower_block(x0, x1, y0, y1):
    for y in range(y0, y1 + 1):
        for x in range(x0, x1 + 1):
            col = stone_shade(x, y)
            t = (x - x0) / max(1, (x1 - x0))
            if t < 0.12:
                col = lerp(col, STONE_L, 0.55)
            elif t > 0.88:
                col = lerp(col, (12, 14, 24), 0.55)
            put(x, y, col)


ROOF, ROOF_L, ROOF_D = (44, 58, 96), (78, 100, 150), (24, 32, 58)


def roof(x0, x1, ybase, apex):
    cx = (x0 + x1) / 2

    def sh(x, y):
        return ROOF_L if x < cx - (cx - x0) * 0.45 else (ROOF_D if x > cx + (x1 - cx) * 0.35 else ROOF)

    poly([(x0 - 2, ybase), (cx, apex), (x1 + 2, ybase)], ROOF, shade=sh)
    line(x0 - 2, ybase, int(cx), apex, (200, 140, 70))
    line(x1 + 2, ybase, int(cx), apex, (90, 60, 34))


# side turrets
tower_block(78, 93, 62, 134)
tower_block(155, 170, 62, 134)
roof(78, 93, 62, 38)
roof(155, 170, 62, 38)
# main body
tower_block(92, 156, 48, 134)
roof(94, 154, 49, 2)
# finial & trim
rect(123, 0, 125, 6, (190, 130, 62))
rect(90, 48, 158, 51, (150, 98, 50))
rect(90, 48, 158, 48, (214, 156, 84))
rect(90, 51, 158, 51, (70, 44, 26))
for x in range(92, 157, 6):  # crenel notches
    rect(x, 52, x + 2, 53, (24, 26, 38))

# clock glow + face
CX, CY = 124, 71
for r in range(30, 19, -1):
    ring(CX, CY, r - 1, r, (150, 190, 255), 0.02 + (30 - r) * 0.004)
circle(CX, CY, 20, (176, 112, 50))
ring(CX, CY, 17.5, 20, (230, 170, 90))
ring(CX, CY, 16, 17.5, (62, 40, 24))
circle(CX, CY, 16, (204, 219, 242))
circle(CX, CY, 16, (170, 190, 224), 0.25)
for i in range(12):
    a = i * math.pi / 6
    r0 = 12.5 if i % 3 == 0 else 13.8
    line(CX + math.cos(a) * r0, CY + math.sin(a) * r0, CX + math.cos(a) * 15, CY + math.sin(a) * 15, (36, 40, 62))
# hands (showing 10:08 for a stopped, haunted clock)
ang_h = math.radians(-90 - 60)
ang_m = math.radians(-90 + 48)
line(CX, CY, CX + math.cos(ang_h) * 8, CY + math.sin(ang_h) * 8, (26, 30, 48))
line(CX + 1, CY, CX + 1 + math.cos(ang_h) * 8, CY + math.sin(ang_h) * 8, (26, 30, 48))
line(CX, CY, CX + math.cos(ang_m) * 12, CY + math.sin(ang_m) * 12, (26, 30, 48))
circle(CX, CY, 2, (200, 140, 66))

# bronze bands
rect(92, 92, 156, 93, (150, 98, 50))
rect(92, 92, 156, 92, (214, 156, 84))
rect(92, 130, 156, 133, (120, 78, 40))
rect(92, 130, 156, 130, (200, 144, 76))
rect(78, 110, 93, 111, (140, 92, 46))
rect(155, 110, 170, 111, (140, 92, 46))

# glowing gothic windows
def gothic_window(cx, y0, y1, w=4):
    for y in range(y0, y1 + 1):
        for x in range(cx - w, cx + w + 1):
            inside = True
            if y < y0 + w + 1:
                inside = (x - cx) ** 2 + (y - (y0 + w + 1)) ** 2 <= (w + 0.6) ** 2
            if inside:
                t = (y - y0) / max(1, (y1 - y0))
                put(x, y, lerp((255, 214, 120), (255, 132, 38), t))
    # frame
    for y in range(y0 - 1, y1 + 2):
        put(cx - w - 1, y, (96, 62, 34))
        put(cx + w + 1, y, (60, 38, 22))
    # mullions
    rect(cx, y0 + w, cx, y1, (70, 44, 26))
    rect(cx - w, (y0 + y1) // 2, cx + w, (y0 + y1) // 2, (70, 44, 26))
    # glow
    for r in range(6, 12):
        ring(cx, (y0 + y1) / 2, r, r + 1, (255, 170, 70), 0.025)


for wx in (106, 124, 142):
    gothic_window(wx, 98, 122)
# turret slit windows
for tx in (86, 163):
    gothic_window(tx, 74, 90, w=2)
    gothic_window(tx, 104, 120, w=2)

# entrance arch
for y in range(120, 135):
    for x in range(114, 135):
        inside = True
        if y < 128:
            inside = (x - 124) ** 2 + (y - 128) ** 2 <= 10.6 ** 2
        if inside:
            put(x, y, (10, 12, 22))
line(124, 130, 124, 134, (255, 170, 70))

# ---------------------------------------------------------------- foreground gear + ground
gear(24, 142, 22, 10, 0.3, 6, depth=0.15)
for y in range(132, H):
    t = (y - 132) / (H - 132)
    c = lerp((26, 28, 44), (10, 12, 22), t)
    for x in range(W):
        n = ((x * 7 + y * 13) % 5) * 1.2
        blend(x, y, (c[0] + n, c[1] + n, c[2] + n + 2), 1.0)
for x in range(0, W, 12):  # cobbles
    blend(x, 133, (70, 74, 96), 0.5)

# mist
for _ in range(14):
    mx, my = rnd.randrange(W), rnd.randrange(120, 143)
    rw, rh = rnd.randrange(14, 40), rnd.randrange(2, 5)
    for y in range(my - rh, my + rh + 1):
        for x in range(mx - rw, mx + rw + 1):
            d = ((x - mx) / rw) ** 2 + ((y - my) / rh) ** 2
            if d <= 1:
                blend(x, y, (150, 170, 220), 0.10 * (1 - d))

# moonbeams
for off in (0, 14):
    poly([(MX - 12 - off // 4, MY + 6), (MX + 2 - off // 4, MY + 10), (MX - 80 - off, H), (MX - 118 - off, H)], (200, 220, 255), a=0.045)

# vignette
for y in range(H):
    for x in range(W):
        d = max(abs(x - W / 2) / (W / 2), abs(y - H / 2) / (H / 2))
        if d > 0.8:
            blend(x, y, (4, 6, 14), (d - 0.8) * 1.4)

# ---------------------------------------------------------------- export 2x nearest PNG
SC = 2


def write_png(path):
    raw = bytearray()
    for y in range(H * SC):
        raw.append(0)
        row = img[y // SC]
        for x in range(W * SC):
            r, g, b, a = row[x // SC]
            raw += bytes((r, g, b, a))

    def chunk(tag, data):
        c = struct.pack('>I', len(data)) + tag + data
        return c + struct.pack('>I', zlib.crc32(tag + data) & 0xFFFFFFFF)

    png = b'\x89PNG\r\n\x1a\n'
    png += chunk(b'IHDR', struct.pack('>IIBBBBB', W * SC, H * SC, 8, 6, 0, 0, 0))
    png += chunk(b'IDAT', zlib.compress(bytes(raw), 9))
    png += chunk(b'IEND', b'')
    with open(path, 'wb') as f:
        f.write(png)


write_png(sys.argv[1])
print('written', sys.argv[1], W * SC, H * SC)
