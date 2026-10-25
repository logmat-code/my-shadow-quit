#!/usr/bin/env python3
"""
Generates pixel art and audio SFX for:
- 3 Unlockable Weapons: Emberblade, Hollow Crossbow, Abyssal Scepter
- 2 Projectiles: Crossbow Bolt, Abyssal Orb
- 1 Collectible: Ember Fragment (heart/crystal)
- 5 Dedicated Enemies:
    1. Ash Crawler (World 1)
    2. Forest Wisp (World 2)
    3. Clockwork Sentinel (World 3)
    4. Drowned Guardian (World 4)
    5. Abyssal Echo (World 5)
- 5 NPCs:
    1. The Archivist (World 1)
    2. The Rootkeeper (World 2)
    3. The Last Clockmaker (World 3)
    4. The Drowned Oracle (World 4)
    5. The Hollow Warden (World 5)
- 5 NPC Portraits (32x32 pixel art dialogues)
- UI Icons: Heart Full, Heart Empty, Weapon icons
- SFX Audio:
    - attack_slash.wav
    - attack_shoot.wav
    - attack_spell.wav
    - enemy_hurt.wav
    - enemy_death.wav
    - dialogue_blip.wav
    - ember_pickup.wav
"""

import os, struct, zlib, math, random, uuid, wave

def write_png(filepath, w, h, pixel_func):
    os.makedirs(os.path.dirname(filepath), exist_ok=True)
    raw = bytearray()
    for y in range(h):
        raw.append(0)  # Filter none
        for x in range(w):
            r, g, b, a = pixel_func(x, y, w, h)
            raw.extend((int(r) & 0xFF, int(g) & 0xFF, int(b) & 0xFF, int(a) & 0xFF))
    def chunk(tag, data):
        return struct.pack('>I', len(data)) + tag + data + struct.pack('>I', zlib.crc32(tag + data) & 0xFFFFFFFF)
    png = b'\x89PNG\r\n\x1a\n' + chunk(b'IHDR', struct.pack('>IIBBBBB', w, h, 8, 6, 0, 0, 0)) + chunk(b'IDAT', zlib.compress(bytes(raw), 9)) + chunk(b'IEND', b'')
    with open(filepath, 'wb') as f:
        f.write(png)
    
    meta_path = filepath + ".meta"
    if not os.path.exists(meta_path):
        guid = uuid.uuid4().hex
        meta_content = """fileFormatVersion: 2
guid: """ + guid + """
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
  doesTextureContainColorSpaceInformation: 0
  originalHPX: """ + str(w) + """
  originalVPX: """ + str(h) + """
  ignorePngGamma: 0
  fileFormatVersion: 100
  size: 2048
  pixelAspectRatio: 1
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 3
    filterMode: 0
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {x: 0.5, y: 0.5}
  spritePixelsToUnits: 32
  spriteBorder: {x: 0, y: 0, z: 0, w: 0}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponentMapping: 0
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 2048
    resizeAlgorithm: 0
    format: 4
    textureFormat: 0
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
"""
        with open(meta_path, 'w') as mf:
            mf.write(meta_content)
    print(f"Generated {filepath} ({w}x{h})")

def write_wav(filepath, sample_rate, num_samples, gen_func):
    os.makedirs(os.path.dirname(filepath), exist_ok=True)
    with wave.open(filepath, 'w') as wf:
        wf.setnchannels(1)
        wf.setsampwidth(2)
        wf.setframerate(sample_rate)
        data = bytearray()
        for i in range(num_samples):
            t = i / float(sample_rate)
            val = gen_func(t, i, num_samples)
            clamped = max(-1.0, min(1.0, val))
            sample = int(clamped * 32767.0)
            data.extend(struct.pack('<h', sample))
        wf.writeframes(data)
    
    meta_path = filepath + ".meta"
    if not os.path.exists(meta_path):
        guid = uuid.uuid4().hex
        meta_content = f"""fileFormatVersion: 2
guid: {guid}
AudioImporter:
  externalObjects: {{}}
  serializedVersion: 6
  defaultSettings:
    loadType: 0
    sampleRateSetting: 0
    sampleRateOverride: 44100
    compressionFormat: 0
    quality: 1
    conversionMode: 0
  platformSettingOverrides: {{}}
  forceToMono: 1
  normalize: 1
  preloadAudioData: 1
  loadInBackground: 0
  ambisonic: 0
  3D: 0
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""
        with open(meta_path, 'w') as mf:
            mf.write(meta_content)
    print(f"Generated {filepath}")

# ----------------- SPRITE GENERATORS -----------------

# 1. Weapons
def px_emberblade(x, y, w, h):
    # 24x24 glowing flame sword
    dx = x - 12
    dy = y - 12
    # Diagonal blade from bottom-left (4, 4) to top-right (20, 20)
    blade_line = abs(x - y)
    if blade_line <= 1 and x >= 6 and x <= 20 and y >= 6 and y <= 20:
        if blade_line == 0:
            return (255, 235, 170, 255) # white-gold core
        else:
            return (255, 140, 25, 240)  # amber flame edge
    if blade_line == 2 and x >= 8 and x <= 19 and y >= 8 and y <= 19:
        return (230, 60, 20, 160) # outer ember halo
    # Hilt at (5, 5)
    if abs(x - 5) <= 1 and abs(y - 5) <= 1:
        return (90, 80, 70, 255) # dark iron crossguard
    if x == 4 and y == 4 or x == 3 and y == 3:
        return (140, 95, 45, 255) # leather grip
    return (0, 0, 0, 0)

def px_crossbow(x, y, w, h):
    # 24x24 mechanical crossbow
    # Stock along x=12, y from 4 to 18
    if abs(x - 12) == 0 and 4 <= y <= 18:
        return (100, 70, 45, 255) # wood stock
    if abs(x - 12) == 1 and 6 <= y <= 16:
        return (70, 50, 30, 255)
    # Bow arms at y=16, arched
    bow_y = int(16 - 0.25 * (x - 12)**2)
    if abs(y - bow_y) <= 1 and 3 <= x <= 21:
        return (160, 180, 200, 255) # steel bow
    # Cyan hollow glow gem at center
    if abs(x - 12) <= 1 and abs(y - 12) <= 1:
        return (70, 220, 230, 255)
    return (0, 0, 0, 0)

def px_scepter(x, y, w, h):
    # 24x24 abyssal void scepter
    # Shaft from (4, 4) to (16, 16)
    if abs(x - y) == 0 and 4 <= x <= 16:
        return (50, 40, 70, 255) # dark obsidian shaft
    # Void ring head at (18, 18)
    dist = math.sqrt((x - 18)**2 + (y - 18)**2)
    if 2.5 <= dist <= 4.2:
        return (180, 70, 255, 255) # violet void aura
    if dist < 2.2:
        return (255, 130, 255, 255) # bright core
    return (0, 0, 0, 0)

# 2. Projectiles
def px_crossbow_bolt(x, y, w, h):
    # 16x8 projectile arrow
    # Horizontal line
    if y == 3 or y == 4:
        if 2 <= x <= 13:
            return (200, 220, 235, 255) # silver shaft
        if 13 <= x <= 15:
            return (80, 240, 255, 255) # cyan glowing tip
        if 0 <= x <= 2:
            return (180, 60, 60, 255) # red fletching
    return (0, 0, 0, 0)

def px_scepter_orb(x, y, w, h):
    # 16x16 pulsating void orb
    dist = math.sqrt((x - 7.5)**2 + (y - 7.5)**2)
    if dist < 3.0:
        return (255, 240, 255, 255)
    elif dist < 5.0:
        return (190, 70, 250, 230)
    elif dist < 7.0:
        return (90, 20, 160, 120)
    return (0, 0, 0, 0)

# 3. Collectibles & UI
def px_ember_fragment(x, y, w, h):
    # 16x16 glowing crystal heart/fragment
    dx = abs(x - 7.5)
    dy = abs(y - 7.5)
    # Diamond shape
    if dx + dy <= 6.0:
        if dx + dy <= 2.5:
            return (255, 255, 220, 255) # white hot core
        elif dx + dy <= 4.5:
            return (255, 160, 30, 255) # orange flame
        else:
            return (210, 40, 30, 255) # crimson crystal edge
    return (0, 0, 0, 0)

def px_heart_full(x, y, w, h):
    # 16x16 pixel heart (y=0 at top of image, y=15 at bottom)
    # Convert so math works naturally with lobes at top (y_m high) and tip pointing down (y_m=0)
    ym = (h - 1) - y
    d1 = math.sqrt((x - 4.5)**2 + (ym - 10.5)**2)
    d2 = math.sqrt((x - 11.5)**2 + (ym - 10.5)**2)
    in_triangle = (ym <= 10) and (ym >= 2) and (abs(x - 8) <= (ym - 2) * 0.9)
    if d1 <= 3.2 or d2 <= 3.2 or in_triangle:
        if d1 <= 1.5 or (abs(x - 6) <= 1 and abs(ym - 10) <= 1):
            return (255, 200, 200, 255) # specular highlight
        return (230, 35, 60, 255) # rich crimson red
    if d1 <= 4.2 or d2 <= 4.2 or ((ym <= 11) and (ym >= 1) and (abs(x - 8) <= (ym - 1) * 1.05)):
        return (60, 15, 20, 255) # dark outline
    return (0, 0, 0, 0)

def px_heart_empty(x, y, w, h):
    # 16x16 empty heart socket
    ym = (h - 1) - y
    d1 = math.sqrt((x - 4.5)**2 + (ym - 10.5)**2)
    d2 = math.sqrt((x - 11.5)**2 + (ym - 10.5)**2)
    in_triangle = (ym <= 10) and (ym >= 2) and (abs(x - 8) <= (ym - 2) * 0.9)
    if d1 <= 3.2 or d2 <= 3.2 or in_triangle:
        return (35, 25, 30, 200) # dark hollow
    if d1 <= 4.2 or d2 <= 4.2 or ((ym <= 11) and (ym >= 1) and (abs(x - 8) <= (ym - 1) * 1.05)):
        return (80, 70, 80, 255) # stone/iron outline
    return (0, 0, 0, 0)

# 4. Enemies (24x24 or 32x32)
def px_ash_crawler(x, y, w, h):
    # 24x24 bug/crawler made of ash and glowing ember eyes
    # Low body at y=4..12, x=4..20
    dx = (x - 12) / 7.0
    dy = (y - 8) / 4.0
    if dx*dx + dy*dy <= 1.0:
        # Burning core
        if abs(x - 12) <= 3 and abs(y - 8) <= 2:
            return (240, 120, 20, 255)
        return (60, 50, 55, 255) # charred stone carapace
    # Legs (spikes sticking out)
    if y <= 5 and (x in (5, 9, 15, 19)):
        return (40, 35, 40, 255)
    # Glowing ember eyes at front (16, 17)
    if (x == 17 or x == 18) and y == 9:
        return (255, 220, 40, 255)
    return (0, 0, 0, 0)

def px_forest_wisp(x, y, w, h):
    # 24x24 floating ethereal nature wisp
    dist = math.sqrt((x - 12)**2 + (y - 12)**2)
    if dist <= 3.0:
        return (230, 255, 240, 255) # white-green bright core
    elif dist <= 6.5:
        return (60, 200, 120, 220) # glowing emerald body
    elif dist <= 10.0:
        # flickering outer tendrils
        noise = (math.sin(x * 1.7) + math.cos(y * 1.5)) * 1.5
        if dist + noise <= 9.5:
            return (30, 120, 70, 120)
    return (0, 0, 0, 0)

def px_clockwork_sentinel(x, y, w, h):
    # 24x24 brass automaton sentry
    # Circular brass gear body
    dist = math.sqrt((x - 12)**2 + (y - 12)**2)
    if dist <= 7.0:
        # Gear teeth
        angle = math.atan2(y - 12, x - 12)
        tooth = math.cos(angle * 8)
        if dist > 5.5 and tooth < 0.2:
            return (0, 0, 0, 0) # tooth gap
        # Central amber ocular lens
        if dist <= 2.8:
            return (255, 180, 30, 255) # glowing sensor eye
        if dist <= 5.0:
            return (195, 145, 60, 255) # polished brass
        return (140, 95, 35, 255) # gear rim
    return (0, 0, 0, 0)

def px_drowned_guardian(x, y, w, h):
    # 28x28 submerged gargoyle / stone aquatic construct
    # Broad shoulder frame
    if 6 <= y <= 22 and 6 <= x <= 21:
        # Head
        if y >= 17 and 10 <= x <= 17:
            if y == 19 and (x == 12 or x == 15):
                return (100, 240, 255, 255) # cyan glowing eye slit
            return (55, 75, 85, 255) # wet dark stone
        # Torso
        if 9 <= y <= 16 and 7 <= x <= 20:
            # Coral / barnacles
            if (x * 7 + y * 13) % 9 == 0:
                return (40, 160, 150, 255) # bioluminescent patina
            return (45, 60, 70, 255)
        # Legs / pedestal
        if y < 9 and 8 <= x <= 19:
            return (35, 45, 55, 255)
    return (0, 0, 0, 0)

def px_abyssal_echo(x, y, w, h):
    # 28x28 shadowy distorted doppelganger/wraith
    dist = math.sqrt((x - 14)**2 + (y - 14)**2)
    if dist <= 9.0:
        # Swirling dark purple / black mist
        if dist <= 3.5:
            # Void core
            return (20, 10, 30, 255)
        # Violet rim
        if dist >= 6.5:
            if (x + y) % 3 == 0:
                return (170, 60, 240, 200) # violet sparks
            return (110, 35, 160, 180)
        # Twin piercing magenta eyes
        if y == 16 and (x == 11 or x == 16):
            return (255, 90, 255, 255)
        return (40, 20, 60, 240)
    return (0, 0, 0, 0)

# 5. NPCs (24x32 sprites and 32x32 portraits)
def px_npc_archivist(x, y, w, h):
    # 24x32 robed sage with a heavy book & scroll
    # Head with hood
    if 22 <= y <= 30 and 8 <= x <= 15:
        if y <= 25 and 10 <= x <= 13:
            return (210, 180, 150, 255) # beard / face
        if y == 26 and (x == 10 or x == 13):
            return (255, 200, 80, 255) # spectacles
        return (60, 65, 80, 255) # cowl
    # Robe body
    if 4 <= y <= 22 and 6 <= x <= 17:
        if 12 <= y <= 17 and 7 <= x <= 13:
            return (180, 140, 80, 255) # ancient tome in hand
        return (75, 80, 100, 255) # scholar robe
    return (0, 0, 0, 0)

def px_portrait_archivist(x, y, w, h):
    # 32x32 detailed portrait for dialogue modal
    # Border frame at edges
    if x in (0, 31) or y in (0, 31):
        return (140, 115, 65, 255) # gold-leaf frame
    # Dark library backdrop
    if x <= 1 or x >= 30 or y <= 1 or y >= 30:
        return (25, 20, 35, 255)
    # Head & hood
    dist_head = math.sqrt((x - 16)**2 + (y - 18)**2)
    if dist_head <= 9.0:
        if dist_head <= 6.0 and y <= 19:
            # Face & beard
            if y <= 15:
                return (220, 220, 225, 255) # silver beard
            if y == 17 and (x in (13, 18)):
                return (255, 220, 90, 255) # eyes / glasses
            return (215, 180, 155, 255) # weathered skin
        return (60, 65, 85, 255) # hood
    # Robe shoulders
    if y <= 10 and 6 <= x <= 25:
        return (70, 75, 95, 255)
    return (30, 25, 40, 255)

def px_npc_rootkeeper(x, y, w, h):
    # 24x32 ancient dryad / forest hermit covered in moss
    if 20 <= y <= 29 and 8 <= x <= 15:
        if y == 24 and (x == 10 or x == 13):
            return (120, 255, 140, 255) # glowing green eyes
        return (80, 60, 45, 255) # bark mask
    if 4 <= y <= 20 and 5 <= x <= 18:
        if (x + y * 3) % 4 == 0:
            return (40, 150, 70, 255) # hanging moss & leaves
        return (95, 70, 50, 255) # wooden twisted roots
    return (0, 0, 0, 0)

def px_portrait_rootkeeper(x, y, w, h):
    if x in (0, 31) or y in (0, 31):
        return (50, 120, 70, 255)
    # Face made of carved oak with green foliage
    dist = math.sqrt((x - 16)**2 + (y - 17)**2)
    if dist <= 9.0:
        if y == 18 and (x in (12, 19)):
            return (130, 255, 150, 255) # emerald gaze
        if dist <= 6.5:
            return (105, 80, 55, 255) # tree bark face
        return (40, 140, 65, 255) # leaf crown
    if y <= 10 and 5 <= x <= 26:
        return (75, 55, 40, 255)
    return (20, 35, 25, 255)

def px_npc_clockmaker(x, y, w, h):
    # 24x32 mechanical tinkerer with brass goggles
    if 21 <= y <= 29 and 8 <= x <= 15:
        if y == 25 and (x in (10, 13)):
            return (80, 230, 240, 255) # cyan glowing magnifying lens
        if y == 25:
            return (180, 140, 50, 255) # brass goggle frame
        return (215, 175, 145, 255) # skin
    if 4 <= y <= 21 and 6 <= x <= 17:
        if 11 <= y <= 16 and 8 <= x <= 15:
            return (190, 150, 60, 255) # brass gear apron
        return (70, 55, 45, 255) # leather tunic
    return (0, 0, 0, 0)

def px_portrait_clockmaker(x, y, w, h):
    if x in (0, 31) or y in (0, 31):
        return (190, 145, 55, 255)
    dist = math.sqrt((x - 16)**2 + (y - 17)**2)
    if dist <= 9.0:
        # Big brass goggles
        if 17 <= y <= 20 and 9 <= x <= 22:
            if x in (12, 13, 18, 19) and y in (18, 19):
                return (90, 240, 255, 255) # cyan lens reflections
            return (210, 160, 50, 255) # brass rims
        if dist <= 6.5:
            return (215, 175, 145, 255) # face
        return (90, 65, 50, 255) # leather cap
    if y <= 10 and 6 <= x <= 25:
        return (75, 55, 45, 255)
    return (35, 25, 20, 255)

def px_npc_oracle(x, y, w, h):
    # 24x32 aquatic siren / veiled blind seer
    if 21 <= y <= 30 and 8 <= x <= 15:
        if 23 <= y <= 25:
            return (140, 210, 230, 200) # cyan silk veil over eyes
        return (180, 215, 225, 255) # pale aquatic skin
    if 4 <= y <= 21 and 6 <= x <= 17:
        # flowing translucent robes
        return (40, 85, 115, 240)
    return (0, 0, 0, 0)

def px_portrait_oracle(x, y, w, h):
    if x in (0, 31) or y in (0, 31):
        return (60, 160, 190, 255)
    dist = math.sqrt((x - 16)**2 + (y - 17)**2)
    if dist <= 9.0:
        # Silk blindfold with rune
        if 16 <= y <= 19 and 10 <= x <= 21:
            if x in (15, 16) and y in (17, 18):
                return (120, 255, 255, 255) # glowing sea glyph
            return (70, 160, 195, 240) # cyan veil
        if dist <= 6.5:
            return (195, 220, 230, 255) # ethereal pale face
        return (35, 75, 105, 255) # cascading ocean hair
    if y <= 10 and 6 <= x <= 25:
        return (45, 95, 130, 255)
    return (20, 35, 50, 255)

def px_npc_warden(x, y, w, h):
    # 24x32 armored keeper of the final flame
    if 20 <= y <= 29 and 8 <= x <= 15:
        if y == 24 and (x in (10, 13)):
            return (255, 160, 30, 255) # fiery visor slits
        return (40, 35, 50, 255) # abyssal plate helm
    if 4 <= y <= 20 and 5 <= x <= 18:
        # Ashen armor with glowing runic cracks
        if (x == 11 or x == 12) and 8 <= y <= 16:
            return (255, 100, 20, 255) # molten heart slit
        return (50, 45, 60, 255) # dark steel plate
    return (0, 0, 0, 0)

def px_portrait_warden(x, y, w, h):
    if x in (0, 31) or y in (0, 31):
        return (255, 130, 30, 255)
    dist = math.sqrt((x - 16)**2 + (y - 17)**2)
    if dist <= 9.0:
        # Visor slit
        if 17 <= y <= 19 and 11 <= x <= 20:
            return (255, 150, 30, 255) # blazing flame eyes
        if dist <= 7.0:
            return (40, 35, 50, 255) # heavy helm plate
        return (30, 25, 40, 255) # gorget
    if y <= 10 and 5 <= x <= 26:
        return (45, 40, 55, 255)
    return (30, 15, 25, 255)

# ----------------- AUDIO GENERATORS -----------------

def gen_slash(t, i, total):
    # Quick swish with metallic zing (0.18s)
    env = math.exp(-t * 22.0)
    freq = 600.0 - 400.0 * (t / 0.18)
    noise = random.uniform(-0.4, 0.4)
    wave_val = math.sin(2.0 * math.pi * freq * t) * 0.6 + noise * 0.4
    return wave_val * env

def gen_shoot(t, i, total):
    # Crossbow twang & snap (0.22s)
    env = math.exp(-t * 18.0)
    twang = math.sin(2.0 * math.pi * 320.0 * t * (1.0 + math.sin(t * 80.0) * 0.2))
    snap = random.uniform(-0.8, 0.8) if t < 0.03 else 0.0
    return (twang * 0.6 + snap * 0.4) * env

def gen_spell(t, i, total):
    # Abyssal resonance pulse (0.35s)
    env = math.exp(-t * 8.0)
    f1 = 220.0 + math.sin(t * 40.0) * 30.0
    f2 = 440.0 * (1.0 - t * 1.5)
    pulse = math.sin(2.0 * math.pi * f1 * t) * 0.5 + math.sin(2.0 * math.pi * f2 * t) * 0.3
    return pulse * env

def gen_enemy_hurt(t, i, total):
    # Impact thud + crunch (0.16s)
    env = math.exp(-t * 24.0)
    thud = math.sin(2.0 * math.pi * 120.0 * t) * 0.7
    crunch = random.uniform(-0.6, 0.6)
    return (thud + crunch * 0.5) * env

def gen_enemy_death(t, i, total):
    # Dissipating crumble / spectral screech (0.45s)
    env = math.exp(-t * 7.0)
    f = 350.0 * math.exp(-t * 4.0)
    spectral = math.sin(2.0 * math.pi * f * t) * 0.5
    crumble = random.uniform(-0.5, 0.5) * (1.0 - t / 0.45)
    return (spectral + crumble) * env

def gen_dialogue_blip(t, i, total):
    # Soft wooden/chime typewriter blip (0.05s)
    env = math.exp(-t * 50.0)
    f = 520.0
    return math.sin(2.0 * math.pi * f * t) * env * 0.5

def gen_ember_pickup(t, i, total):
    # Uplifting dual harmonic chime (0.30s)
    env = math.exp(-t * 10.0)
    f = 660.0 if t < 0.12 else 880.0
    return math.sin(2.0 * math.pi * f * t) * env * 0.7

def main():
    base_sprites = "Assets/Resources/Sprites"
    base_audio = "Assets/Resources/Audio/SFX"
    
    # 1. Weapons & Projectiles
    write_png(f"{base_sprites}/Combat/weapon_emberblade.png", 24, 24, px_emberblade)
    write_png(f"{base_sprites}/Combat/weapon_crossbow.png", 24, 24, px_crossbow)
    write_png(f"{base_sprites}/Combat/weapon_scepter.png", 24, 24, px_scepter)
    write_png(f"{base_sprites}/Combat/proj_crossbow_bolt.png", 16, 8, px_crossbow_bolt)
    write_png(f"{base_sprites}/Combat/proj_scepter_orb.png", 16, 16, px_scepter_orb)
    
    # 2. Collectibles & HUD Hearts
    write_png(f"{base_sprites}/Combat/ember_fragment.png", 16, 16, px_ember_fragment)
    write_png(f"{base_sprites}/UI/heart_full.png", 16, 16, px_heart_full)
    write_png(f"{base_sprites}/UI/heart_empty.png", 16, 16, px_heart_empty)
    
    # 3. Enemies
    write_png(f"{base_sprites}/Combat/enemy_ash_crawler.png", 24, 24, px_ash_crawler)
    write_png(f"{base_sprites}/Combat/enemy_forest_wisp.png", 24, 24, px_forest_wisp)
    write_png(f"{base_sprites}/Combat/enemy_clockwork_sentinel.png", 24, 24, px_clockwork_sentinel)
    write_png(f"{base_sprites}/Combat/enemy_drowned_guardian.png", 28, 28, px_drowned_guardian)
    write_png(f"{base_sprites}/Combat/enemy_abyssal_echo.png", 28, 28, px_abyssal_echo)
    
    # 4. NPCs & Portraits
    write_png(f"{base_sprites}/Narrative/npc_archivist.png", 24, 32, px_npc_archivist)
    write_png(f"{base_sprites}/Narrative/portrait_archivist.png", 32, 32, px_portrait_archivist)
    
    write_png(f"{base_sprites}/Narrative/npc_rootkeeper.png", 24, 32, px_npc_rootkeeper)
    write_png(f"{base_sprites}/Narrative/portrait_rootkeeper.png", 32, 32, px_portrait_rootkeeper)
    
    write_png(f"{base_sprites}/Narrative/npc_clockmaker.png", 24, 32, px_npc_clockmaker)
    write_png(f"{base_sprites}/Narrative/portrait_clockmaker.png", 32, 32, px_portrait_clockmaker)
    
    write_png(f"{base_sprites}/Narrative/npc_oracle.png", 24, 32, px_npc_oracle)
    write_png(f"{base_sprites}/Narrative/portrait_oracle.png", 32, 32, px_portrait_oracle)
    
    write_png(f"{base_sprites}/Narrative/npc_warden.png", 24, 32, px_npc_warden)
    write_png(f"{base_sprites}/Narrative/portrait_warden.png", 32, 32, px_portrait_warden)
    
    # 5. SFX Audio
    sr = 44100
    write_wav(f"{base_audio}/attack_slash.wav", sr, int(sr * 0.18), gen_slash)
    write_wav(f"{base_audio}/attack_shoot.wav", sr, int(sr * 0.22), gen_shoot)
    write_wav(f"{base_audio}/attack_spell.wav", sr, int(sr * 0.35), gen_spell)
    write_wav(f"{base_audio}/enemy_hurt.wav", sr, int(sr * 0.16), gen_enemy_hurt)
    write_wav(f"{base_audio}/enemy_death.wav", sr, int(sr * 0.45), gen_enemy_death)
    write_wav(f"{base_audio}/dialogue_blip.wav", sr, int(sr * 0.05), gen_dialogue_blip)
    write_wav(f"{base_audio}/ember_pickup.wav", sr, int(sr * 0.30), gen_ember_pickup)
    
    print("ALL COMBAT & NARRATIVE ASSETS SUCCESSFULLY GENERATED!")

if __name__ == "__main__":
    main()
