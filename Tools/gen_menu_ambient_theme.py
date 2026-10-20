#!/usr/bin/env python3
"""Procedural synth for menu_ambient_theme.wav - the dedicated MENU soundtrack of MY SHADOW QUIT.

Direction: dark-fantasy ambient, melancholic, minimal. E natural minor (distinct from the D-based ruins
theme, the forest and the clocktower themes). Soft celesta/music-box plucks over a slow airy pad with a
long hall reverb. Deliberately avoids: sub-bass drones (nothing below ~160 Hz), ticking, harsh highs
and short loops (32 s seamless loop). Pure Python, no third-party deps.

usage: gen_menu_ambient_theme.py <out.wav>
"""
import math, random, struct, sys, wave

SR = 44100
DUR = 32.0
N = int(SR * DUR)
rnd = random.Random(7771)


def hz(midi):
    return 440.0 * 2 ** ((midi - 69) / 12.0)


def wrapd(t, start):
    """Smallest circular time distance (t - start) wrapping at DUR, so the loop is seamless."""
    d = t - start
    if d > DUR / 2: d -= DUR
    if d < -DUR / 2: d += DUR
    return d


dry = [0.0] * N
wet = [0.0] * N

# ------------------------------------------------------------------ PAD (4 chords x 8 s, E minor)
# Em9 | Cmaj7(add9) | Am9 | Bm7(sus4)->Em   (voicings start at E3; no deep bass)
chords = [
    [52, 59, 62, 66, 71],      # E3 B3 D4 F#4 B4   (Em9)
    [48 + 12, 55 + 12, 59 + 12, 62 + 12, 64 + 12],  # C4 G4 B4 D5 E5 (Cmaj9)
    [57 - 12 + 12, 60 + 0, 64 + 0, 67 + 0, 71 + 0],  # A3 C4 E4 G4 B4 (Am9)
    [59, 62, 66, 69, 71],      # B3 D4 F#4 A4 B4   (Bm7/sus-ish)
]
CH_LEN = DUR / 4
FADE = 3.0
for ci, notes in enumerate(chords):
    start = ci * CH_LEN
    for ni, m in enumerate(notes):
        f0 = hz(m)
        det = 1.0 + (ni - 2) * 0.0013
        amp = 0.060 / (1 + 0.15 * ni)
        ph = rnd.random() * 6.28
        lfo_f = 0.07 + 0.03 * ni
        lfo_p = rnd.random() * 6.28
        for i in range(N):
            t = i / SR
            d = wrapd(t, start)
            # envelope: fade in over FADE, hold, fade out over FADE (circular)
            if d < -FADE or d > CH_LEN + FADE:
                continue
            if d < 0: e = (d + FADE) / FADE
            elif d < CH_LEN: e = 1.0
            else: e = 1.0 - (d - CH_LEN) / FADE
            e = e * e * (3 - 2 * e)
            trem = 0.82 + 0.18 * math.sin(2 * math.pi * lfo_f * t + lfo_p)
            s = math.sin(2 * math.pi * f0 * det * t + ph)
            s += 0.28 * math.sin(2 * math.pi * f0 * 2.0 * det * t + ph * 1.7)
            s += 0.10 * math.sin(2 * math.pi * f0 * 3.0 * det * t + ph * 0.6)
            wet[i] += amp * e * trem * s * 0.9
            dry[i] += amp * e * trem * s * 0.25

# ------------------------------------------------------------------ AIR (night wind, band-limited noise)
lp1 = lp2 = 0.0
k1 = 1 - math.exp(-2 * math.pi * 1500 / SR)
k2 = 1 - math.exp(-2 * math.pi * 400 / SR)
for i in range(N):
    t = i / SR
    n = rnd.uniform(-1, 1)
    lp1 += k1 * (n - lp1)
    lp2 += k2 * (lp1 - lp2)
    band = lp1 - lp2                                   # ~400..1500 Hz breath, no low rumble
    gust = 0.5 + 0.5 * math.sin(2 * math.pi * (1 / 16.0) * t) * math.sin(2 * math.pi * (1 / 10.67) * t + 1.1)
    wet[i] += 0.050 * band * (0.25 + 0.75 * gust * gust)

# ------------------------------------------------------------------ CELESTA / MUSIC-BOX MELODY (sparse)
# (time s, midi, velocity)  - E minor pentatonic with a gentle F# / C colour
E5, G5, A5, B5, D6, FS5, C6, E6, D5 = 76, 79, 81, 83, 86, 78, 84, 88, 74
melody = [
    (1.2, B5, 0.9), (3.0, G5, 0.7), (5.1, E5, 0.8),
    (8.6, D6, 0.75), (10.4, C6, 0.7), (12.3, G5, 0.8), (14.6, E5, 0.6),
    (16.4, A5, 0.85), (18.2, E5, 0.7), (19.9, C6, 0.6), (22.2, B5, 0.7),
    (24.5, FS5, 0.75), (26.1, B5, 0.85), (27.6, D6, 0.7), (29.3, E6, 0.55), (30.6, B5, 0.5),
]
for (t0, m, vel) in melody:
    f = hz(m)
    dec = 2.6
    i0 = int(t0 * SR)
    L = int(SR * 5.0)
    for j in range(L):
        i = (i0 + j) % N
        t = j / SR
        a = min(1.0, t / 0.004) * math.exp(-t / dec)
        if a < 1e-4:
            break
        s = math.sin(2 * math.pi * f * t)
        s += 0.32 * math.sin(2 * math.pi * f * 2.0 * t) * math.exp(-t / 1.1)
        s += 0.12 * math.sin(2 * math.pi * f * 2.76 * t) * math.exp(-t / 0.5)   # soft bell inharmonic
        v = 0.060 * vel * a * s
        dry[i] += v * 0.55
        wet[i] += v * 0.95

# ------------------------------------------------------------------ echo + Schroeder hall reverb (processed over 2 passes for a seamless loop)
def run_fx(src_wet, src_dry):
    L = N * 2
    x = [src_wet[i % N] for i in range(L)]
    # soft feedback echo (0.52 s, lowpassed)
    de = int(0.52 * SR); fb = 0.38; lp = 0.0; kk = 0.35
    buf = [0.0] * de; bi = 0
    out = [0.0] * L
    for i in range(L):
        d = buf[bi]
        lp += kk * (d - lp)
        y = x[i] + lp * fb
        buf[bi] = y
        bi = (bi + 1) % de
        out[i] = x[i] + lp * 0.55
    x = out
    # 4 combs + 2 allpass
    comb_d = [int(SR * v) for v in (0.0297, 0.0371, 0.0411, 0.0437)]
    comb_g = [0.86, 0.85, 0.84, 0.83]
    rev = [0.0] * L
    for d, g in zip(comb_d, comb_g):
        b = [0.0] * d; p = 0; damp = 0.0
        for i in range(L):
            y = b[p]
            damp += 0.45 * (y - damp)
            b[p] = x[i] + damp * g
            p = (p + 1) % d
            rev[i] += y * 0.25
    for d, g in ((int(SR * 0.0050), 0.7), (int(SR * 0.0017), 0.7)):
        b = [0.0] * d; p = 0
        for i in range(L):
            z = b[p]
            y = -g * rev[i] + z
            b[p] = rev[i] + g * y
            p = (p + 1) % d
            rev[i] = y
    return [rev[N + i] * 1.5 + x[N + i] * 0.35 + src_dry[i] for i in range(N)]


mix = run_fx(wet, dry)

# gentle DC/sub high-pass (~120 Hz) so nothing can rumble
hp_prev_x = hp_prev_y = 0.0
rc = 1 / (2 * math.pi * 120)
a = rc / (rc + 1 / SR)
for i in range(N):
    x = mix[i]
    y = a * (hp_prev_y + x - hp_prev_x)
    hp_prev_x, hp_prev_y = x, y
    mix[i] = y

# loop-edge smoothing is unnecessary (circular processing), normalise to a calm level
peak = max(abs(v) for v in mix)
rms = math.sqrt(sum(v * v for v in mix[::5]) / len(mix[::5]))
scale = min(0.60 / peak, 0.085 / rms)
pcm = bytearray()
for v in mix:
    pcm += struct.pack('<h', max(-32767, min(32767, int(v * scale * 32767))))

with wave.open(sys.argv[1], 'wb') as w:
    w.setnchannels(1); w.setsampwidth(2); w.setframerate(SR)
    w.writeframes(bytes(pcm))
print('written', sys.argv[1], 'peak=%.2f rms=%.3f' % (peak * scale, rms * scale))
