#!/usr/bin/env python3
"""
Generates all pixel art sprites and UI card / level select map assets for:
- WORLD 4: THE DROWNED CATHEDRAL (Submerged Gothic Cathedral)
- WORLD 5: THE ABYSS OF THE LAST FLAME (Fractured Reality / Void Altar)
Pure Python 3 standard library.
"""

import os, struct, zlib, math, random

def write_png(filepath, w, h, pixel_func):
    os.makedirs(os.path.dirname(filepath), exist_ok=True)
    raw = bytearray()
    for y in range(h):
        raw.append(0)  # Filter none
        for x in range(w):
            r, g, b, a = pixel_func(x, y, w, h)
            raw.extend((r, g, b, a))
    def chunk(tag, data):
        return struct.pack('>I', len(data)) + tag + data + struct.pack('>I', zlib.crc32(tag + data) & 0xFFFFFFFF)
    png = b'\x89PNG\r\n\x1a\n' + chunk(b'IHDR', struct.pack('>IIBBBBB', w, h, 8, 6, 0, 0, 0)) + chunk(b'IDAT', zlib.compress(bytes(raw), 9)) + chunk(b'IEND', b'')
    with open(filepath, 'wb') as f:
        f.write(png)
    print(f"Generated {filepath} ({w}x{h})")

def write_meta(meta_path, guid, ppu=32):
    content = """fileFormatVersion: 2
guid: """ + str(guid) + """
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 1
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMasterTextureLimit: 0
  doesTextureContainColorSpaceAttribute: 0
  ignoreReadWrite: 0
  platforms: []
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {x: 0.5, y: 0.5}
  spritePixelsToUnits: """ + str(ppu) + """
  spriteBorder: {x: 0, y: 0, z: 0, w: 0}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationMethod: 0
  spriteTessellationDetail: -1
  spriteGeometrySubdivision: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  swizzle: 50462976
  cookieLightType: 0
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 2048
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 0
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    ignorePlatformSupport: 0
    minimumSharedRuntimeVersion: 0
    androidETC2FallbackOverride: 0
    forceRowByRowCompressionSet: 0
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    physicsShape: []
    bones: []
    spriteID: 
    internalID: 0
    vertices: []
    indices: 
    edges: []
    weights: []
  spritePackingTag: 
  pSDRemoveMatte: 0
  pSDShowRemoveMatteOption: 0
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""
    with open(meta_path, 'w') as f:
        f.write(content)

# ----------------- WORLD 4 SPRITES -----------------
# 1. Floor: 32x32 Submerged wet dark stone block with sea moss & barnacles
def w4_floor(x, y, w, h):
    base = 18 + ((x * 7 + y * 13) % 9)
    # Teal/marine tint: (14, 28, 32)
    r = int(base * 0.7)
    g = int(base * 1.3)
    b = int(base * 1.5)
    # Top edge wet highlight
    if y == 0:
        return (r + 40, g + 60, b + 70, 255)
    if y == 1:
        return (r + 20, g + 35, b + 45, 255)
    # Bevel/joint
    if x == 0 or x == w - 1 or y == h - 1:
        return (max(0, r - 8), max(0, g - 6), max(0, b - 5), 255)
    # Sea-moss flecks
    if (x + y * 3) % 11 == 0 and y < 8:
        return (35, 75, 60, 255)
    return (r, g, b, 255)

# 2. Wall: 32x32 Submerged gothic carved pillar block
def w4_wall(x, y, w, h):
    base = 14 + ((x * 11 + y * 17) % 8)
    r = int(base * 0.6)
    g = int(base * 1.1)
    b = int(base * 1.4)
    # Vertical stone fluting
    if x in (4, 12, 20, 28):
        return (r + 15, g + 25, b + 35, 255)
    if x in (5, 13, 21, 29):
        return (max(0, r - 6), max(0, g - 6), max(0, b - 6), 255)
    return (r, g, b, 255)

# 3. Platform: 64x32 Gothic stone floating slab
def w4_platform(x, y, w, h):
    if y >= 20: return (0, 0, 0, 0)
    base = 22 + ((x * 5 + y * 11) % 9)
    r = int(base * 0.65)
    g = int(base * 1.25)
    b = int(base * 1.45)
    if y == 0: return (r + 45, g + 70, b + 80, 255)
    if y == 1: return (r + 25, g + 40, b + 50, 255)
    if y == 19: return (r - 10, g - 10, b - 10, 255)
    if x < 4 or x > w - 5: return (r - 8, g - 5, b - 5, 255)
    return (r, g, b, 255)

# 4. Parallax Far: 256x144 Cathedral under dark sea
def w4_parallax_far(x, y, w, h):
    t = y / float(h)
    # Deep abyssal sea gradient
    r = int(4 + 8 * t)
    g = int(10 + 16 * t)
    b = int(18 + 26 * t)
    # Distant gothic cathedral silhouettes (vaults & spires)
    spire1 = 50 + int(40 * math.exp(-((x - 70) / 14)**2))
    spire2 = 40 + int(60 * math.exp(-((x - 130) / 18)**2))
    spire3 = 55 + int(35 * math.exp(-((x - 195) / 15)**2))
    cathedral_h = max(spire1, spire2, spire3, 75)
    if y >= cathedral_h:
        r = int(r * 0.5)
        g = int(g * 0.65)
        b = int(b * 0.8)
    return (r, g, b, 255)

# 5. Parallax Mid: 256x144 Submerged stone pillars & fractured rose window
def w4_parallax_mid(x, y, w, h):
    # Transparent top, arches and rose window in middle
    if (x % 64 < 16) and y > 25:
        # Columns
        col = 22 + (x % 5)
        return (int(col * 0.7), int(col * 1.3), int(col * 1.6), 220)
    # Rose window at x=128, y=60
    dist_rose = math.hypot(x - 128, y - 60)
    if 18 <= dist_rose <= 24:
        return (40, 95, 115, 200)
    if dist_rose < 18 and (int(dist_rose) % 4 == 0 or (x + y) % 6 == 0):
        return (30, 80, 95, 180)
    return (0, 0, 0, 0)

# 6. Parallax Near: 256x144 Near chains, seaweed fronds, broken buttresses
def w4_parallax_near(x, y, w, h):
    # Hanging heavy chains
    if x in (42, 43, 178, 179) and y < 100 and (y % 6 < 4):
        return (28, 48, 55, 230)
    # Seaweed strands drifting from bottom
    if y > 80:
        wave = int(3 * math.sin(y * 0.15 + x * 0.2))
        if abs((x % 48) - 20 - wave) <= 1:
            return (20, 60, 48, 210)
    return (0, 0, 0, 0)

# 7. Water Surface Overlay: 64x16 animated tile
def w4_water_surface(x, y, w, h):
    wave = int(2 * math.sin(x * 0.3))
    if y < 4 + wave:
        return (0, 0, 0, 0)
    if y == 4 + wave:
        return (140, 230, 245, 230)
    if y == 5 + wave:
        return (60, 160, 190, 190)
    return (20, 75, 110, 140)

# 8. Buoyant Platform: 64x32 wooden pontoon with bronze fittings
def w4_buoyant_platform(x, y, w, h):
    if y >= 22: return (0, 0, 0, 0)
    if y < 3:
        # Planks
        c = 45 + (x % 16 < 2 and 15 or 0)
        return (c + 15, int(c * 1.1), int(c * 0.8), 255)
    # Pontoon body
    r = 30 + (x % 8)
    return (r, r + 20, r + 35, 255)

# 9. Tide Switch Off / On: 32x32
def w4_tide_switch_off(x, y, w, h):
    d = math.hypot(x - 16, y - 16)
    if d <= 12:
        if d <= 8:
            return (20, 50, 65, 255)
        return (40, 85, 105, 255)
    return (0, 0, 0, 0)

def w4_tide_switch_on(x, y, w, h):
    d = math.hypot(x - 16, y - 16)
    if d <= 12:
        if d <= 8:
            return (60, 220, 245, 255)  # Glowing aquamarine
        return (80, 160, 190, 255)
    return (0, 0, 0, 0)

# 10. Refraction Crystal Off / Lit: 32x32
def w4_crystal_off(x, y, w, h):
    # Diamond shape
    dx, dy = abs(x - 16), abs(y - 16)
    if dx + dy <= 12:
        val = 40 + (12 - (dx + dy)) * 5
        return (int(val * 0.6), int(val * 1.1), int(val * 1.3), 255)
    return (0, 0, 0, 0)

def w4_crystal_lit(x, y, w, h):
    dx, dy = abs(x - 16), abs(y - 16)
    if dx + dy <= 12:
        val = 140 + (12 - (dx + dy)) * 9
        return (val, int(val * 0.85), int(val * 0.4), 255)  # Radiant amber refractive light
    return (0, 0, 0, 0)

# 11. Submerged Grate: 32x64
def w4_submerged_grate(x, y, w, h):
    if x in (0, 1, w-2, w-1) or y in (0, 1, h-2, h-1):
        return (35, 60, 75, 255)
    if x % 6 in (0, 1) or y % 8 in (0, 1):
        return (45, 80, 95, 230)
    return (0, 0, 0, 0)

# 12. Cathedral Bell Hazard: 48x48
def w4_bell_hazard(x, y, w, h):
    d = math.hypot(x - 24, y - 20)
    if d <= 16:
        return (60, 75, 65, 255)  # Verdigris bronze bell
    if y >= 20 and y <= 34 and abs(x - 24) <= 18:
        return (70, 85, 75, 255)
    return (0, 0, 0, 0)

# ----------------- WORLD 5 SPRITES -----------------
# 1. Floor: 32x32 Floating fractured void stone with glowing amber fissure
def w5_floor(x, y, w, h):
    base = 16 + ((x * 13 + y * 7) % 8)
    r = int(base * 1.2)
    g = int(base * 0.9)
    b = int(base * 1.5)
    # Glowing fissure in center
    if abs(x - 16) + (y % 4) == 2:
        return (255, 170, 30, 255)  # Amber energy fissure
    if y == 0:
        return (r + 35, g + 25, b + 45, 255)
    if x == 0 or x == w - 1 or y == h - 1:
        return (max(0, r - 6), max(0, g - 6), max(0, b - 4), 255)
    return (r, g, b, 255)

# 2. Wall: 32x32 Void monument pillar
def w5_wall(x, y, w, h):
    base = 12 + ((x * 9 + y * 19) % 7)
    r = int(base * 1.1)
    g = int(base * 0.8)
    b = int(base * 1.6)
    if x in (6, 15, 24):
        return (r + 15, g + 8, b + 25, 255)
    return (r, g, b, 255)

# 3. Platform: 64x32 Suspended abyssal stone platform
def w5_platform(x, y, w, h):
    if y >= 20: return (0, 0, 0, 0)
    base = 18 + ((x * 7 + y * 13) % 9)
    r = int(base * 1.2)
    g = int(base * 0.9)
    b = int(base * 1.6)
    if y == 0: return (r + 45, g + 30, b + 60, 255)
    if y == 19: return (r - 8, g - 8, b - 8, 255)
    if (x + y * 2) % 19 == 0: return (255, 180, 40, 255)
    return (r, g, b, 255)

# 4. Parallax Far: 256x144 Cosmic Abyss of the Last Flame
def w5_parallax_far(x, y, w, h):
    t = y / float(h)
    # Deep void obsidian-indigo to spectral violet
    r = int(8 + 14 * t)
    g = int(4 + 8 * t)
    b = int(16 + 28 * t)
    # Colossal vertical rift of the Last Flame at x=128
    dist_rift = abs(x - 128)
    if dist_rift < 30:
        glow = math.exp(-(dist_rift / 10.0)**2)
        r = min(255, int(r + 240 * glow))
        g = min(255, int(g + 160 * glow))
        b = min(255, int(b + 50 * glow))
    return (r, g, b, 255)

# 5. Parallax Mid: 256x144 Floating shattered arches and monolithic pillars
def w5_parallax_mid(x, y, w, h):
    # Shattered floating islands
    island1 = 70 + int(15 * math.sin(x * 0.1))
    if 40 <= x <= 100 and y >= island1 and y <= island1 + 25:
        c = 28 + (x % 7)
        return (int(c * 1.2), int(c * 0.8), int(c * 1.6), 220)
    island2 = 55 + int(12 * math.cos(x * 0.12))
    if 160 <= x <= 220 and y >= island2 and y <= island2 + 28:
        c = 30 + (x % 7)
        return (int(c * 1.2), int(c * 0.8), int(c * 1.6), 220)
    return (0, 0, 0, 0)

# 6. Parallax Near: 256x144 Monolithic chains disappearing into void
def w5_parallax_near(x, y, w, h):
    if x in (55, 56, 195, 196) and (y % 8 < 6):
        return (35, 25, 50, 240)
    # Floating golden ember motes
    if (x * 17 + y * 31) % 197 == 0:
        return (255, 200, 60, 220)
    return (0, 0, 0, 0)

# 7. Dual Reality Platform (Active Light / Active Shadow): 64x32
def w5_dual_plat_light(x, y, w, h):
    if y >= 20: return (0, 0, 0, 0)
    # Brilliant radiant gold / amber crystal slab
    c = 180 + ((x * 7 + y * 11) % 35)
    if y == 0: return (255, 240, 180, 255)
    return (c, int(c * 0.78), int(c * 0.25), 255)

def w5_dual_plat_shadow(x, y, w, h):
    if y >= 20: return (0, 0, 0, 0)
    # Deep violet spectral plate
    c = 140 + ((x * 7 + y * 11) % 30)
    if y == 0: return (220, 180, 255, 255)
    return (int(c * 0.65), int(c * 0.35), c, 255)

# 8. Flame Anchor Pedestal: 32x48
def w5_flame_anchor(x, y, w, h):
    if y < 12:
        # Golden altar bowl
        if abs(x - 16) <= 12 - y:
            return (220, 160, 40, 255)
    # Altar pillar
    if abs(x - 16) <= 8:
        return (35, 25, 48, 255)
    return (0, 0, 0, 0)

# 9. Shadow Echo Switch: 32x32
def w5_echo_switch_off(x, y, w, h):
    d = math.hypot(x - 16, y - 16)
    if d <= 12:
        if d <= 6: return (30, 18, 48, 255)
        return (80, 45, 120, 255)
    return (0, 0, 0, 0)

def w5_echo_switch_on(x, y, w, h):
    d = math.hypot(x - 16, y - 16)
    if d <= 12:
        if d <= 6: return (240, 180, 255, 255)
        return (170, 90, 245, 255)
    return (0, 0, 0, 0)

# 10. Abyssal Bridge: 64x16
def w5_abyssal_bridge(x, y, w, h):
    if y >= 10: return (0, 0, 0, 0)
    # Luminous golden-violet woven energy bridge
    t = x / float(w)
    r = int(240 * (1 - t) + 160 * t)
    g = int(180 * (1 - t) + 80 * t)
    b = int(60 * (1 - t) + 240 * t)
    return (r, g, b, 230)

# 11. Final Altar of the Last Flame: 64x64
def w5_final_flame_altar(x, y, w, h):
    d = math.hypot(x - 32, y - 32)
    if d <= 26:
        if d <= 14:
            return (255, 240, 190, 255)  # Core golden sanctuary
        return (80, 50, 110, 255)
    return (0, 0, 0, 0)

def main():
    print("Generating World 4 & World 5 sprites...")
    base_dir = "Assets/Resources/Sprites"
    
    # World 4
    w4_sprites = [
        ("World4/cathedral_floor.png", 32, 32, w4_floor, "a401a89045b8431ebdc8901234567890"),
        ("World4/cathedral_wall.png", 32, 32, w4_wall, "a402a89045b8431ebdc8901234567891"),
        ("World4/cathedral_platform.png", 64, 32, w4_platform, "a403a89045b8431ebdc8901234567892"),
        ("World4/cathedral_platform_left.png", 32, 32, w4_floor, "a404a89045b8431ebdc8901234567893"),
        ("World4/cathedral_platform_right.png", 32, 32, w4_floor, "a405a89045b8431ebdc8901234567894"),
        ("World4/cathedral_parallax_far.png", 256, 144, w4_parallax_far, "a406a89045b8431ebdc8901234567895"),
        ("World4/cathedral_parallax_mid.png", 256, 144, w4_parallax_mid, "a407a89045b8431ebdc8901234567896"),
        ("World4/cathedral_parallax_near.png", 256, 144, w4_parallax_near, "a408a89045b8431ebdc8901234567897"),
        ("World4/water_surface.png", 64, 16, w4_water_surface, "a409a89045b8431ebdc8901234567898"),
        ("World4/buoyant_platform.png", 64, 32, w4_buoyant_platform, "a410a89045b8431ebdc8901234567899"),
        ("World4/tide_switch_off.png", 32, 32, w4_tide_switch_off, "a411a89045b8431ebdc890123456789a"),
        ("World4/tide_switch_on.png", 32, 32, w4_tide_switch_on, "a412a89045b8431ebdc890123456789b"),
        ("World4/crystal_off.png", 32, 32, w4_crystal_off, "a413a89045b8431ebdc890123456789c"),
        ("World4/crystal_lit.png", 32, 32, w4_crystal_lit, "a414a89045b8431ebdc890123456789d"),
        ("World4/submerged_grate.png", 32, 64, w4_submerged_grate, "a415a89045b8431ebdc890123456789e"),
        ("World4/bell_hazard.png", 48, 48, w4_bell_hazard, "a416a89045b8431ebdc890123456789f"),
    ]
    for rel_path, w, h, fn, guid in w4_sprites:
        full_p = os.path.join(base_dir, rel_path)
        write_png(full_p, w, h, fn)
        write_meta(full_p + ".meta", guid, 32 if "parallax" not in rel_path else 16)

    # World 5
    w5_sprites = [
        ("World5/abyss_floor.png", 32, 32, w5_floor, "a501a89045b8431ebdc8901234567890"),
        ("World5/abyss_wall.png", 32, 32, w5_wall, "a502a89045b8431ebdc8901234567891"),
        ("World5/abyss_platform.png", 64, 32, w5_platform, "a503a89045b8431ebdc8901234567892"),
        ("World5/abyss_platform_left.png", 32, 32, w5_floor, "a504a89045b8431ebdc8901234567893"),
        ("World5/abyss_platform_right.png", 32, 32, w5_floor, "a505a89045b8431ebdc8901234567894"),
        ("World5/abyss_parallax_far.png", 256, 144, w5_parallax_far, "a506a89045b8431ebdc8901234567895"),
        ("World5/abyss_parallax_mid.png", 256, 144, w5_parallax_mid, "a507a89045b8431ebdc8901234567896"),
        ("World5/abyss_parallax_near.png", 256, 144, w5_parallax_near, "a508a89045b8431ebdc8901234567897"),
        ("World5/dual_plat_light.png", 64, 32, w5_dual_plat_light, "a509a89045b8431ebdc8901234567898"),
        ("World5/dual_plat_shadow.png", 64, 32, w5_dual_plat_shadow, "a510a89045b8431ebdc8901234567899"),
        ("World5/flame_anchor.png", 32, 48, w5_flame_anchor, "a511a89045b8431ebdc890123456789a"),
        ("World5/echo_switch_off.png", 32, 32, w5_echo_switch_off, "a512a89045b8431ebdc890123456789b"),
        ("World5/echo_switch_on.png", 32, 32, w5_echo_switch_on, "a513a89045b8431ebdc890123456789c"),
        ("World5/abyssal_bridge.png", 64, 16, w5_abyssal_bridge, "a514a89045b8431ebdc890123456789d"),
        ("World5/final_flame_altar.png", 64, 64, w5_final_flame_altar, "a515a89045b8431ebdc890123456789e"),
    ]
    for rel_path, w, h, fn, guid in w5_sprites:
        full_p = os.path.join(base_dir, rel_path)
        write_png(full_p, w, h, fn)
        write_meta(full_p + ".meta", guid, 32 if "parallax" not in rel_path else 16)

    # UI Card Art & Level Select Maps for World 4 and World 5
    ui_dir = "Assets/Resources/Sprites/UI"
    # World 4 Card Art (496x290)
    def w4_card_art(x, y, w, h):
        # Submerged Cathedral Rose Window & Sunken Spire
        dx, dy = x - 248, y - 145
        d = math.hypot(dx, dy)
        if d < 70:
            return (40, 120, 140, 255)
        # Deep ocean blue gradient
        t = y / float(h)
        return (int(10 + 15 * t), int(25 + 35 * t), int(45 + 50 * t), 255)
    write_png(os.path.join(ui_dir, "world4_card_art.png"), 496, 290, w4_card_art)
    write_meta(os.path.join(ui_dir, "world4_card_art.png.meta"), "b401a89045b8431ebdc8901234567890", 32)

    # World 5 Card Art (496x290)
    def w5_card_art(x, y, w, h):
        # Void Rift & The Last Flame golden bloom
        dx = abs(x - 248)
        glow = math.exp(-(dx / 35.0)**2) * (1.0 - abs(y - 145) / 200.0)
        r = min(255, int(15 + 240 * glow))
        g = min(255, int(8 + 170 * glow))
        b = min(255, int(25 + 60 * glow))
        return (r, g, b, 255)
    write_png(os.path.join(ui_dir, "world5_card_art.png"), 496, 290, w5_card_art)
    write_meta(os.path.join(ui_dir, "world5_card_art.png.meta"), "b501a89045b8431ebdc8901234567890", 32)

    # World 4 Map (480x270)
    def w4_map(x, y, w, h):
        t = y / float(h)
        return (int(8 + 12 * t), int(20 + 28 * t), int(35 + 40 * t), 255)
    write_png(os.path.join(ui_dir, "world4_cathedral_map.png"), 480, 270, w4_map)
    write_meta(os.path.join(ui_dir, "world4_cathedral_map.png.meta"), "b402a89045b8431ebdc8901234567891", 32)

    # World 5 Map (480x270)
    def w5_map(x, y, w, h):
        t = y / float(h)
        return (int(12 + 18 * t), int(6 + 10 * t), int(22 + 30 * t), 255)
    write_png(os.path.join(ui_dir, "world5_abyss_map.png"), 480, 270, w5_map)
    write_meta(os.path.join(ui_dir, "world5_abyss_map.png.meta"), "b502a89045b8431ebdc8901234567891", 32)

    print("All sprites and UI cards generated successfully!")

if __name__ == "__main__":
    main()
