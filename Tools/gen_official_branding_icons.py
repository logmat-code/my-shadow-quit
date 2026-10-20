#!/usr/bin/env python3
"""
Official Branding & Icon Generator for MY SHADOW QUIT.
Unity 6000.6.2f1 - URP 2D
Generates:
  1. master_app_icon_1024.png (1024x1024 Master Icon)
  2. android_adaptive_bg.png  (1024x1024 Android Adaptive Background)
  3. android_adaptive_fg.png  (1024x1024 Android Adaptive Foreground with strict circular safe-zone)
  4. app_icon_512.png         (512x512 Store / Notification Icon)
  5. favicon.png              (64x64 WebGL / Browser Favicon)
  6. favicon.ico              (Multi-resolution ICO for WebGL template)

Pure Python 3 standard library (no Pillow or NumPy dependencies).
"""

import math
import os
import random
import struct
import sys
import zlib

W, H = 256, 256

def clamp(v, lo=0, hi=255):
    return max(lo, min(hi, int(v)))

def blend_pixel(img, x, y, r, g, b, a):
    if 0 <= x < W and 0 <= y < H:
        cr, cg, cb, ca = img[y][x]
        if a <= 0:
            return
        if a >= 255 and ca == 0:
            img[y][x] = (clamp(r), clamp(g), clamp(b), 255)
            return
        alpha = a / 255.0
        inv_alpha = (1.0 - alpha) * (ca / 255.0)
        out_a = alpha + inv_alpha
        if out_a > 0:
            out_r = int((r * alpha + cr * inv_alpha) / out_a)
            out_g = int((g * alpha + cg * inv_alpha) / out_a)
            out_b = int((b * alpha + cb * inv_alpha) / out_a)
            img[y][x] = (clamp(out_r), clamp(out_g), clamp(out_b), clamp(int(out_a * 255)))

def set_pixel(img, x, y, r, g, b, a=255):
    if 0 <= x < W and 0 <= y < H:
        img[y][x] = (clamp(r), clamp(g), clamp(b), clamp(a))

def draw_circle(img, cx, cy, r, rgb, alpha=255):
    ir = int(math.ceil(r))
    for dy in range(-ir, ir + 1):
        for dx in range(-ir, ir + 1):
            d2 = dx * dx + dy * dy
            if d2 <= r * r:
                blend_pixel(img, int(cx + dx), int(cy + dy), rgb[0], rgb[1], rgb[2], alpha)

def draw_rect(img, x0, y0, x1, y1, rgb, alpha=255):
    for y in range(int(y0), int(y1) + 1):
        for x in range(int(x0), int(x1) + 1):
            blend_pixel(img, x, y, rgb[0], rgb[1], rgb[2], alpha)

def encode_png(pixel_grid, out_w, out_h):
    raw = bytearray()
    in_h = len(pixel_grid)
    in_w = len(pixel_grid[0])
    
    scale_x = out_w / in_w
    scale_y = out_h / in_h

    for y in range(out_h):
        raw.append(0)  # Filter type 0 (None)
        src_y = int(y / scale_y)
        src_y = min(src_y, in_h - 1)
        row = pixel_grid[src_y]
        for x in range(out_w):
            src_x = int(x / scale_x)
            src_x = min(src_x, in_w - 1)
            r, g, b, a = row[src_x]
            raw.extend((r, g, b, a))

    def make_chunk(tag, data):
        c = struct.pack('>I', len(data)) + tag + data
        return c + struct.pack('>I', zlib.crc32(tag + data) & 0xFFFFFFFF)

    png = b'\x89PNG\r\n\x1a\n'
    png += make_chunk(b'IHDR', struct.pack('>IIBBBBB', out_w, out_h, 8, 6, 0, 0, 0))
    png += make_chunk(b'IDAT', zlib.compress(bytes(raw), 9))
    png += make_chunk(b'IEND', b'')
    return png

def encode_ico(png_data_list):
    """Encodes list of PNG byte buffers into an ICO file."""
    header = struct.pack('<HHH', 0, 1, len(png_data_list))
    entries = bytearray()
    image_data = bytearray()
    
    offset = 6 + (16 * len(png_data_list))
    for w, h, data in png_data_list:
        b_w = 0 if w >= 256 else w
        b_h = 0 if h >= 256 else h
        size = len(data)
        entry = struct.pack('<BBBBHHII', b_w, b_h, 0, 0, 1, 32, size, offset)
        entries.extend(entry)
        image_data.extend(data)
        offset += size

    return bytes(header + entries + image_data)

def generate_branding_art():
    bg = [[(0, 0, 0, 255) for _ in range(W)] for _ in range(H)]
    fg = [[(0, 0, 0, 0) for _ in range(W)] for _ in range(H)]

    torch_x, torch_y = 168, 84  # Torch focal position

    # -------------------------------------------------------------
    # 1. BACKGROUND CANVAS (Ruins, Gothic Arch, Torch Glow, Vignette)
    # -------------------------------------------------------------
    # Sky gradient: deep nocturnal atmosphere (midnight indigo to dark slate)
    for y in range(H):
        t = y / (H - 1)
        r = int(6 + 8 * t)
        g = int(8 + 10 * t)
        b = int(16 + 20 * t)
        for x in range(W):
            bg[y][x] = (r, g, b, 255)

    # Distant gothic stars / magical dust
    rnd_stars = random.Random(202)
    for _ in range(45):
        sx = rnd_stars.randint(12, W - 12)
        sy = rnd_stars.randint(12, 135)
        dist_to_center = math.hypot(sx - 128, sy - 128)
        if dist_to_center > 42:
            br = rnd_stars.randint(65, 155)
            bg[sy][sx] = (br, int(br * 0.92), int(br * 1.15), 255)

    # Distant ruins silhouettes (broken spires & crenellations)
    for x in range(W):
        h_ruin = 132 + int(7 * math.sin(x * 0.08) + 4 * math.cos(x * 0.16))
        if 30 <= x <= 60:
            h_ruin = 72 + int(x % 6 < 3) * 6
        elif 196 <= x <= 226:
            h_ruin = 82 + int(x % 6 < 3) * 6
        for y in range(h_ruin, H):
            t_depth = (y - h_ruin) / max(1, H - h_ruin)
            r_val = int(10 + 6 * t_depth)
            g_val = int(12 + 8 * t_depth)
            b_val = int(22 + 10 * t_depth)
            bg[y][x] = (r_val, g_val, b_val, 255)

    # Massive Gothic Archway framing the scene
    for y in range(12, H):
        for x in range(W):
            is_left_pillar = (12 <= x <= 46) and (y >= 40)
            is_right_pillar = (210 <= x <= 244) and (y >= 40)
            dx = x - 128
            arch_top = 30 + int((dx * dx) / 130.0)
            is_arch = (arch_top - 20 <= y <= arch_top + 10) and (42 <= x <= 214)

            if is_left_pillar or is_right_pillar or is_arch:
                brick_x = x % 16
                brick_y = y % 8
                is_mortar = (brick_x == 0 or brick_y == 0)
                base_c = 14 if is_mortar else (22 + (x * 7 + y * 13) % 9)
                bg[y][x] = (base_c, base_c + 2, base_c + 8, 255)

    # Radial Torch Glow on Background (warm amber glow cast on ruins)
    for y in range(H):
        for x in range(W):
            d_torch = math.hypot(x - torch_x, y - torch_y)
            if d_torch < 145:
                glow_factor = max(0.0, 1.0 - (d_torch / 145.0)) ** 1.7
                blend_pixel(bg, x, y, 255, 140, 30, int(glow_factor * 130))
                if d_torch < 52:
                    core_f = max(0.0, 1.0 - (d_torch / 52.0)) ** 1.3
                    blend_pixel(bg, x, y, 255, 215, 95, int(core_f * 95))

            # Left side: mysterious void/violet aura where Shadow is located
            d_shadow = math.hypot(x - 104, y - 125)
            if d_shadow < 90:
                violet_f = max(0.0, 1.0 - (d_shadow / 90.0)) ** 1.5
                blend_pixel(bg, x, y, 130, 55, 225, int(violet_f * 70))

    # Base stone floor in background
    for y in range(178, H):
        t_floor = (y - 178) / (H - 178)
        for x in range(W):
            brick_x = (x + 8 * int(y / 6)) % 14
            brick_y = y % 6
            is_gap = (brick_x == 0 or brick_y == 0)
            lum = 12 if is_gap else int(18 + 10 * (1.0 - t_floor) + (x * 3 + y * 7) % 6)
            blend_pixel(bg, x, y, lum, lum + 2, lum + 7, 255)

    # Vignette darkening towards edges for master background
    for y in range(H):
        for x in range(W):
            r_norm = math.hypot((x - 128) / 128.0, (y - 128) / 128.0)
            if r_norm > 0.70:
                dark_f = min(1.0, (r_norm - 0.70) / 0.30) ** 1.5
                blend_pixel(bg, x, y, 4, 4, 8, int(dark_f * 185))

    # -------------------------------------------------------------
    # 2. FOREGROUND CANVAS (Protagonist, Torch, Shadow, Plinth)
    # STRICT CIRCULAR SAFE ZONE: Everything within radius 80 px of (128, 128)!
    # -------------------------------------------------------------

    # A. Stone Pedestal / Plinth (y = 170..190, x = 68..188)
    for py in range(170, 191):
        t_p = (py - 170) / 21.0
        for px in range(70, 186):
            edge_cut = 0
            if px < 80:
                edge_cut = 80 - px
            elif px > 176:
                edge_cut = px - 176
            if py >= 170 + edge_cut:
                d_torch_p = math.hypot(px - torch_x, py - torch_y)
                light = max(0.0, 1.0 - d_torch_p / 90.0)
                sr = int(36 + 100 * light * (1.0 - t_p * 0.45))
                sg = int(38 + 75 * light * (1.0 - t_p * 0.45))
                sb = int(50 + 30 * light * (1.0 - t_p * 0.45))

                if px < 118:
                    sr = int(sr * 0.7 + 28)
                    sg = int(sg * 0.7 + 18)
                    sb = int(sb * 0.7 + 55)

                fg[py][px] = (clamp(sr), clamp(sg), clamp(sb), 255)

    # Carved border highlight along plinth top
    for px in range(74, 182):
        d_torch_p = math.hypot(px - torch_x, 170 - torch_y)
        light = max(0.0, 1.0 - d_torch_p / 75.0)
        fg[170][px] = (clamp(int(85 + 140 * light)), clamp(int(80 + 120 * light)), clamp(int(95 + 45 * light)), 255)
        fg[171][px] = (clamp(int(55 + 95 * light)), clamp(int(52 + 80 * light)), clamp(int(65 + 30 * light)), 255)

    # B. THE SHADOW: UNIFIED SOLID PHANTOM SILHOUETTE SEPARATING FROM PROTAGONIST
    # Cowl Head from Y=94, Torso & Cowl to Y=158, Dissolving Smoky Base to Y=174
    for sy in range(94, 175):
        # Calculate silhouette width and center at this Y
        if sy < 108:
            # Rounded cowl head
            dy = 108 - sy
            w_shape = math.sqrt(max(0.0, 12.0 * 12.0 - dy * dy * 0.75))
            cx_shape = 104.0
        elif sy < 156:
            # Torso / flowing cloak, leaning back to the left
            t_b = (sy - 108) / 48.0
            cx_shape = 104.0 - t_b * 4.0
            w_shape = 12.0 + t_b * 3.5
        else:
            # Dissolving smoky tendrils
            t_s = (sy - 156) / 19.0
            wisp = math.sin(sy * 0.45) * 4.0
            cx_shape = 100.0 + wisp - (sy - 156) * 0.6
            w_shape = 15.0 * (1.0 - t_s * 0.7)

        x_start = int(cx_shape - w_shape)
        x_end = int(cx_shape + w_shape)

        for sx in range(x_start, x_end + 1):
            dist_from_cx = abs(sx - cx_shape)
            is_outer_edge = dist_from_cx >= w_shape - 1.8
            is_mid_rim = dist_from_cx >= w_shape - 3.2

            if sy >= 156:
                # Smoky base fading into ground
                t_fade = (sy - 156) / 19.0
                alpha_tendril = int(255 * (1.0 - t_fade * 0.75))
                if is_outer_edge:
                    blend_pixel(fg, sx, sy, 185, 105, 255, alpha_tendril)
                else:
                    blend_pixel(fg, sx, sy, 38, 22, 60, alpha_tendril)
            else:
                # Solid wraith body
                if is_outer_edge:
                    fg[sy][sx] = (195, 115, 255, 255)  # Vivid luminous neon violet rim
                elif is_mid_rim:
                    fg[sy][sx] = (135, 70, 215, 255)  # Ethereal purple transition
                else:
                    # Solid obsidian purple body with rich visible contrast
                    fg[sy][sx] = (45, 26, 75, 255)

    # Shadow Eye Slit: Bright horizontal luminous violet-white visor aperture
    # Rendered seamlessly across the cowl face (y = 102..106, x = 98..110)
    for ey in range(102, 107):
        for ex in range(98, 111):
            fg[ey][ex] = (255, 255, 255, 255)  # Brilliant white core
    # Surrounding neon violet glow halo
    for ey in range(100, 109):
        for ex in range(95, 114):
            if fg[ey][ex][0] < 220:
                d_eye = math.hypot(ex - 104, ey - 104)
                if d_eye <= 6.5:
                    alpha_g = int(235 * (1.0 - d_eye / 6.5))
                    blend_pixel(fg, ex, ey, 210, 130, 255, alpha_g)

    # Ethereal floating phantom motes curling from the shadow into the void
    rnd_motes = random.Random(404)
    for _ in range(22):
        mx = rnd_motes.randint(76, 96)
        my = rnd_motes.randint(84, 155)
        d_sh = math.hypot(mx - 104, my - 120)
        if 8 <= d_sh <= 32:
            rad = rnd_motes.uniform(1.0, 2.4)
            draw_circle(fg, mx, my, rad, (195, 115, 255), rnd_motes.randint(160, 250))

    # C. THE PROTAGONIST (Right-Center Character, bold chunky pixel art)
    # Stance: Facing right-three-quarters, holding torch high
    # Sturdy Boots (y = 166..172)
    draw_rect(fg, 137, 166, 144, 171, (22, 20, 30))
    draw_rect(fg, 136, 169, 145, 172, (38, 34, 50))
    draw_rect(fg, 148, 166, 155, 171, (26, 24, 36))
    draw_rect(fg, 147, 169, 156, 172, (44, 40, 58))

    # Protagonist Cloak Body (y = 136..166, x = 132..160)
    for cy in range(136, 166):
        t_c = (cy - 136) / 30.0
        w_left = 140 - int(t_c * 8)
        w_right = 152 + int(t_c * 8)
        for cx in range(w_left, w_right + 1):
            dist_torch = math.hypot(cx - torch_x, cy - torch_y)
            light_t = max(0.0, 1.0 - dist_torch / 75.0)

            # Deep indigo cloak fabric
            cr = int(42 + 80 * light_t)
            cg = int(40 + 65 * light_t)
            cb = int(66 + 30 * light_t)

            if cx < 138:
                cr = int(cr * 0.6)
                cg = int(cg * 0.6)
                cb = int(cb * 0.7)

            fg[cy][cx] = (clamp(cr), clamp(cg), clamp(cb), 255)

    # Ornate Gold Hem Stripe along cloak bottom (y = 163..165, x = 133..159)
    for cx in range(133, 160):
        light_g = max(0.0, 1.0 - math.hypot(cx - torch_x, 164 - torch_y) / 65.0)
        fg[163][cx] = (clamp(int(205 + 50 * light_g)), clamp(int(150 + 70 * light_g)), 38, 255)
        fg[164][cx] = (clamp(int(240 + 15 * light_g)), clamp(int(180 + 60 * light_g)), 48, 255)
        fg[165][cx] = (clamp(int(180 + 45 * light_g)), clamp(int(130 + 55 * light_g)), 32, 255)

    # Protagonist Hood & Head (y = 110..136, x = 136..158)
    for hy in range(110, 137):
        for hx in range(136, 159):
            d_hood = math.hypot(hx - 146, hy - 122)
            if d_hood <= 12.0:
                dist_torch = math.hypot(hx - torch_x, hy - torch_y)
                light_h = max(0.0, 1.0 - dist_torch / 60.0)
                hr = int(46 + 100 * light_h)
                hg = int(42 + 85 * light_h)
                hb = int(72 + 45 * light_h)
                if hx < 142:
                    hr = int(hr * 0.58)
                    hg = int(hg * 0.58)
                    hb = int(hb * 0.68)
                fg[hy][hx] = (clamp(hr), clamp(hg), clamp(hb), 255)

    # Gold shoulder / chest mantle trim (y = 135..137, x = 136..156)
    for mx in range(136, 157):
        light_m = max(0.0, 1.0 - math.hypot(mx - torch_x, 136 - torch_y) / 55.0)
        fg[135][mx] = (clamp(int(220 + 35 * light_m)), clamp(int(160 + 60 * light_m)), 42, 255)
        fg[136][mx] = (clamp(int(185 + 40 * light_m)), clamp(int(135 + 50 * light_m)), 34, 255)

    # Face Opening in Hood (y = 116..130, x = 146..156)
    for fy in range(116, 131):
        for fx in range(146, 157):
            if math.hypot(fx - 150, fy - 123) <= 6.8:
                light_f = max(0.0, 1.0 - math.hypot(fx - torch_x, fy - torch_y) / 48.0)
                sr = int(225 + 30 * light_f)
                sg = int(175 + 50 * light_f)
                sb = int(140 + 25 * light_f)
                fg[fy][fx] = (clamp(sr), clamp(sg), clamp(sb), 255)

    # Dark hair / brow fringe
    draw_rect(fg, 146, 117, 151, 118, (48, 32, 24))

    # Big Expressive Pixel Eye (y = 120..124, x = 150..154)
    draw_rect(fg, 150, 120, 153, 123, (255, 255, 255))
    draw_rect(fg, 152, 120, 154, 123, (28, 22, 34))
    fg[120][151] = (255, 250, 225, 255)  # Catchlight glint

    # Arm Raised Holding Torch (y = 114..136, x = 156..170)
    for ay in range(114, 136):
        t_a = (ay - 114) / 22.0
        ax = int(166 - t_a * 10)
        draw_rect(fg, ax - 2, ay, ax + 3, ay + 1, (int(56 + 75 * (1.0 - t_a)), int(50 + 60 * (1.0 - t_a)), 85))
    # Hand clasping wooden torch shaft (y = 112..118, x = 164..170)
    draw_rect(fg, 164, 112, 170, 118, (250, 190, 150))

    # D. THE TORCH & AMBER FLAME (Focal Point of Warmth & Hope)
    # Wooden shaft with iron bands (y = 102..138, x = 166..170)
    for ty in range(102, 138):
        w_col = (135, 85, 50) if (ty % 4 != 0) else (100, 60, 36)
        fg[ty][166] = (w_col[0] - 25, w_col[1] - 20, w_col[2] - 15, 255)
        fg[ty][167] = (w_col[0], w_col[1], w_col[2], 255)
        fg[ty][168] = (w_col[0] + 35, w_col[1] + 25, w_col[2] + 15, 255)
        fg[ty][169] = (w_col[0] + 55, w_col[1] + 40, w_col[2] + 25, 255)

    # Iron brazier head (y = 96..103, x = 162..174)
    draw_rect(fg, 162, 98, 174, 103, (72, 68, 78))
    draw_rect(fg, 163, 99, 173, 102, (98, 95, 106))
    fg[100][163] = (185, 180, 195, 255)
    fg[100][173] = (185, 180, 195, 255)

    # Glorious Multi-Tiered Pixel Torch Flame (y = 62..98, x = 154..184)
    flame_x, flame_y = 168, 84
    # Layer 1: Outer Scarlet & Crimson Fire Tendrils
    for fy in range(62, 99):
        t_f = (fy - 62) / 37.0
        w_outer = 11.0 * math.sin(t_f * math.pi) + (1.0 - t_f) * 3.0
        curl = math.sin(fy * 0.4) * 2.2
        for fx in range(int(flame_x + curl - w_outer), int(flame_x + curl + w_outer) + 1):
            df = math.hypot(fx - flame_x, fy - flame_y)
            if df <= 17.5:
                fg[fy][fx] = (255, 65, 10, 255)

    # Layer 2: Vibrant Golden-Amber Flame Body
    for fy in range(66, 97):
        t_f = (fy - 66) / 31.0
        w_mid = 8.0 * math.sin(t_f * math.pi)
        curl_m = math.sin(fy * 0.4) * 1.5
        for fx in range(int(flame_x + curl_m - w_mid), int(flame_x + curl_m + w_mid) + 1):
            df = math.hypot(fx - flame_x, fy - (flame_y + 1))
            if df <= 13.0:
                fg[fy][fx] = (255, 175, 20, 255)

    # Layer 3: Incandescent Yellow Core
    for fy in range(72, 95):
        t_f = (fy - 72) / 23.0
        w_in = 5.5 * math.sin(t_f * math.pi)
        for fx in range(int(flame_x - w_in), int(flame_x + w_in) + 1):
            df = math.hypot(fx - flame_x, fy - (flame_y + 2))
            if df <= 8.5:
                fg[fy][fx] = (255, 235, 75, 255)

    # Layer 4: Pure White-Hot Hearth
    for fy in range(78, 92):
        for fx in range(flame_x - 3, flame_x + 4):
            if math.hypot(fx - flame_x, fy - 85) <= 4.8:
                fg[fy][fx] = (255, 255, 235, 255)

    # Ascending Glowing Sparks & Embers
    rnd_sparks = random.Random(888)
    for _ in range(25):
        sx = flame_x + rnd_sparks.randint(-16, 20)
        sy = flame_y - rnd_sparks.randint(14, 48)
        d_top = flame_y - sy
        if d_top > 0:
            sp_r = rnd_sparks.uniform(0.9, 2.0)
            alpha_sp = int(255 * max(0.2, 1.0 - d_top / 52.0))
            draw_circle(fg, sx, sy, sp_r, (255, 205, 45), alpha_sp)

    # -------------------------------------------------------------
    # 3. MASTER COMPOSITE ICON (Background + Foreground)
    # -------------------------------------------------------------
    master = [[bg[y][x] for x in range(W)] for y in range(H)]
    for y in range(H):
        for x in range(W):
            fr, fg_c, fb, fa = fg[y][x]
            if fa > 0:
                blend_pixel(master, x, y, fr, fg_c, fb, fa)

    return master, bg, fg

def main():
    print("[BrandingGen] Generating upgraded official Dark Fantasy Pixel Art icon assets for MY SHADOW QUIT...")
    master_grid, bg_grid, fg_grid = generate_branding_art()

    target_dir = "Assets/Art/Branding"
    webgl_template_dir = "Assets/WebGLTemplates/Landscape16x9"
    os.makedirs(target_dir, exist_ok=True)
    os.makedirs(webgl_template_dir, exist_ok=True)

    # 1. Master App Icon 1024x1024
    master_1024_bytes = encode_png(master_grid, 1024, 1024)
    master_path = os.path.join(target_dir, "master_app_icon_1024.png")
    with open(master_path, "wb") as f:
        f.write(master_1024_bytes)
    print(f"  -> Generated {master_path} (1024x1024, {len(master_1024_bytes):,} bytes)")

    # 2. Android Adaptive Background 1024x1024
    bg_1024_bytes = encode_png(bg_grid, 1024, 1024)
    bg_path = os.path.join(target_dir, "android_adaptive_bg.png")
    with open(bg_path, "wb") as f:
        f.write(bg_1024_bytes)
    print(f"  -> Generated {bg_path} (1024x1024, {len(bg_1024_bytes):,} bytes)")

    # 3. Android Adaptive Foreground 1024x1024 (with transparent BG, circular safe zone checked)
    fg_1024_bytes = encode_png(fg_grid, 1024, 1024)
    fg_path = os.path.join(target_dir, "android_adaptive_fg.png")
    with open(fg_path, "wb") as f:
        f.write(fg_1024_bytes)
    print(f"  -> Generated {fg_path} (1024x1024, {len(fg_1024_bytes):,} bytes)")

    # 4. App Icon 512x512 (Google Play / Store)
    app_512_bytes = encode_png(master_grid, 512, 512)
    app_512_path = os.path.join(target_dir, "app_icon_512.png")
    with open(app_512_path, "wb") as f:
        f.write(app_512_bytes)
    print(f"  -> Generated {app_512_path} (512x512, {len(app_512_bytes):,} bytes)")

    # 5. Favicon PNG 64x64
    fav_64_bytes = encode_png(master_grid, 64, 64)
    fav_path = os.path.join(target_dir, "favicon.png")
    with open(fav_path, "wb") as f:
        f.write(fav_64_bytes)
    print(f"  -> Generated {fav_path} (64x64, {len(fav_64_bytes):,} bytes)")

    # Also sync favicon to WebGL template
    webgl_fav_png = os.path.join(webgl_template_dir, "favicon.png")
    with open(webgl_fav_png, "wb") as f:
        f.write(fav_64_bytes)
    print(f"  -> Synced WebGL favicon {webgl_fav_png}")

    # 6. Favicon ICO (16x16, 32x32, 64x64)
    fav_16_bytes = encode_png(master_grid, 16, 16)
    fav_32_bytes = encode_png(master_grid, 32, 32)
    ico_bytes = encode_ico([
        (16, 16, fav_16_bytes),
        (32, 32, fav_32_bytes),
        (64, 64, fav_64_bytes)
    ])
    fav_ico_path = os.path.join(target_dir, "favicon.ico")
    with open(fav_ico_path, "wb") as f:
        f.write(ico_bytes)
    print(f"  -> Generated {fav_ico_path} (multi-size ICO, {len(ico_bytes):,} bytes)")

    webgl_fav_ico = os.path.join(webgl_template_dir, "favicon.ico")
    with open(webgl_fav_ico, "wb") as f:
        f.write(ico_bytes)
    print(f"  -> Synced WebGL favicon {webgl_fav_ico}")

    print("[BrandingGen] SUCCESS: All official branding assets created!")

if __name__ == "__main__":
    main()
