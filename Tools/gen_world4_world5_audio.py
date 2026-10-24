#!/usr/bin/env python3
"""Procedural synth for:
- cathedral_ambient_theme.wav (World 4: The Drowned Cathedral)
  Gothic submerged cathedral: deep resonant organ chords in F minor / Ab major, echoing choir/reedy pads,
  subtle distant dripping/water bell reverberations. Melancholic, sacred, underwater majesty.
- abyss_ambient_theme.wav (World 5: The Abyss of the Last Flame)
  The final trial: C# minor / E major, ethereal dark cosmos, shimmering celestial harmonics,
  warm lingering flame pulse, dramatic finality and sorrowful beauty.

Pure Python wave generation, seamless 32-second loop, no external dependencies.
"""
import math, os, random, struct, wave

SR = 44100
DUR = 32.0
N = int(SR * DUR)

def hz(midi):
    return 440.0 * (2.0 ** ((midi - 69) / 12.0))

def wrapd(t, start):
    d = t - start
    if d > DUR / 2: d -= DUR
    if d < -DUR / 2: d += DUR
    return d

def write_wav(filepath, left, right):
    # Find peak
    peak = 1e-6
    for i in range(N):
        peak = max(peak, abs(left[i]), abs(right[i]))
    gain = 0.88 / peak
    frames = bytearray()
    for i in range(N):
        l = max(-32767, min(32767, int(left[i] * gain * 32767)))
        r = max(-32767, min(32767, int(right[i] * gain * 32767)))
        frames.extend(struct.pack('<hh', l, r))
    with wave.open(filepath, 'wb') as wf:
        wf.setnchannels(2)
        wf.setsampwidth(2)
        wf.setframerate(SR)
        wf.writeframes(frames)
    print(f"Generated {filepath} ({DUR}s loop, peak normalized)")

def write_audio_meta(filepath, guid):
    meta = f"""fileFormatVersion: 2
guid: {guid}
AudioImporter:
  defaultSettings:
    loadType: 0
    sampleRateSetting: 0
    sampleRateOverride: 44100
    compressionFormat: 1
    quality: 1
    conversionMode: 0
  customSampleRateSetting: 44100
  forceToMono: 0
  normalize: 1
  preloadAudioData: 1
  loadInBackground: 0
  ambisonic: 0
  3D: 0
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""
    with open(filepath + ".meta", 'w') as f:
        f.write(meta)

# =========================================================================
# WORLD 4: THE DROWNED CATHEDRAL
# =========================================================================
def generate_cathedral_theme():
    rnd = random.Random(4041)
    dry_l = [0.0] * N
    dry_r = [0.0] * N
    wet_l = [0.0] * N
    wet_r = [0.0] * N

    # Chords in F minor:
    # Fm9 -> Dbmaj7 -> Bbm9 -> C7sus4 / Edim
    chords = [
        [53, 60, 65, 68, 72], # F3 C4 F4 Ab4 C5 (Fm)
        [49, 56, 60, 65, 68], # Db3 Ab3 C4 F4 Ab4 (Dbmaj7)
        [46, 53, 58, 61, 65], # Bb2 F3 Bb3 Db4 F4 (Bbm7)
        [48, 55, 60, 63, 67], # C3 G3 C4 Eb4 G4 (Cm7 / Csus)
    ]
    CH_LEN = DUR / 4
    FADE = 3.2

    for ci, notes in enumerate(chords):
        start = ci * CH_LEN
        for ni, m in enumerate(notes):
            f0 = hz(m)
            amp = 0.055 / (1.0 + 0.12 * ni)
            det1 = 1.0 + (ni - 2) * 0.0011
            det2 = 1.0 - (ni - 2) * 0.0009
            ph1 = rnd.random() * 6.28
            ph2 = rnd.random() * 6.28
            for i in range(N):
                t = i / SR
                d = wrapd(t, start)
                if d < -FADE or d > CH_LEN + FADE:
                    continue
                if d < 0: e = (d + FADE) / FADE
                elif d < CH_LEN: e = 1.0
                else: e = 1.0 - (d - CH_LEN) / FADE
                e = e * e * (3 - 2 * e)
                # Cathedral reed / organ harmonics
                s1 = math.sin(2 * math.pi * f0 * det1 * t + ph1)
                s1 += 0.45 * math.sin(4 * math.pi * f0 * det1 * t + ph1 * 1.5)
                s1 += 0.20 * math.sin(6 * math.pi * f0 * det1 * t + ph1 * 0.7)
                s1 += 0.08 * math.sin(8 * math.pi * f0 * det1 * t)

                s2 = math.sin(2 * math.pi * f0 * det2 * t + ph2)
                s2 += 0.45 * math.sin(4 * math.pi * f0 * det2 * t + ph2 * 1.5)
                s2 += 0.20 * math.sin(6 * math.pi * f0 * det2 * t + ph2 * 0.7)
                s2 += 0.08 * math.sin(8 * math.pi * f0 * det2 * t)

                dry_l[i] += amp * e * s1 * 0.7
                dry_r[i] += amp * e * s2 * 0.7

    # Submerged acoustic dripping & distant bell tones
    bell_times = [1.2, 5.0, 9.5, 13.0, 17.2, 21.0, 25.5, 29.2]
    bell_pitches = [72, 75, 77, 80, 75, 72, 68, 70] # high ethereal chimes
    for bt, bp in zip(bell_times, bell_pitches):
        bf0 = hz(bp)
        pan = rnd.uniform(0.2, 0.8)
        for i in range(N):
            t = i / SR
            d = wrapd(t, bt)
            if d < 0 or d > 4.5:
                continue
            env = math.exp(-d * 2.2) * (1.0 - math.exp(-d * 80.0))
            # bell harmonics
            tone = math.sin(2 * math.pi * bf0 * d) * 0.6
            tone += math.sin(2 * math.pi * bf0 * 2.76 * d) * 0.3
            tone += math.sin(2 * math.pi * bf0 * 5.4 * d) * 0.15
            amp = 0.045 * env * tone
            wet_l[i] += amp * (1.0 - pan)
            wet_r[i] += amp * pan

    # Cavernous / flooded hall delay (echo filter)
    dly_samples = int(SR * 0.45)
    feed = 0.42
    out_l = [0.0] * N
    out_r = [0.0] * N
    for i in range(N):
        prev = (i - dly_samples) % N
        out_l[i] = dry_l[i] + wet_l[i] + out_r[prev] * feed
        out_r[i] = dry_r[i] + wet_r[i] + out_l[prev] * feed

    out_path = "Assets/Resources/Audio/Music/cathedral_ambient_theme.wav"
    write_wav(out_path, out_l, out_r)
    write_audio_meta(out_path, "c4a40001000200030004000500060007")

# =========================================================================
# WORLD 5: THE ABYSS OF THE LAST FLAME
# =========================================================================
def generate_abyss_theme():
    rnd = random.Random(5052)
    dry_l = [0.0] * N
    dry_r = [0.0] * N
    wet_l = [0.0] * N
    wet_r = [0.0] * N

    # Chords in C# minor:
    # C#m9 -> Amaj7 -> F#m9 -> G#7sus4 -> C#m
    chords = [
        [49, 56, 61, 64, 68], # C#3 G#3 C#4 E4 G#4 (C#m)
        [45, 52, 57, 61, 64], # A2 E3 A3 C#4 E4 (Amaj7)
        [50, 57, 62, 65, 69], # D3 / F#m / resolving voicing
        [44, 51, 56, 59, 63], # G#2 D#3 G#3 B3 D#4 (G#m7)
    ]
    CH_LEN = DUR / 4
    FADE = 3.5

    for ci, notes in enumerate(chords):
        start = ci * CH_LEN
        for ni, m in enumerate(notes):
            f0 = hz(m)
            amp = 0.052 / (1.0 + 0.1 * ni)
            det1 = 1.0 + (ni - 2) * 0.0014
            det2 = 1.0 - (ni - 2) * 0.0014
            ph1 = rnd.random() * 6.28
            ph2 = rnd.random() * 6.28
            for i in range(N):
                t = i / SR
                d = wrapd(t, start)
                if d < -FADE or d > CH_LEN + FADE:
                    continue
                if d < 0: e = (d + FADE) / FADE
                elif d < CH_LEN: e = 1.0
                else: e = 1.0 - (d - CH_LEN) / FADE
                e = e * e * (3 - 2 * e)
                # Shimmering celestial pad
                s1 = math.sin(2 * math.pi * f0 * det1 * t + ph1)
                s1 += 0.35 * math.sin(4 * math.pi * f0 * det1 * t + ph1 * 1.8)
                s1 += 0.15 * math.sin(8 * math.pi * f0 * det1 * t + ph1 * 0.5)

                s2 = math.sin(2 * math.pi * f0 * det2 * t + ph2)
                s2 += 0.35 * math.sin(4 * math.pi * f0 * det2 * t + ph2 * 1.8)
                s2 += 0.15 * math.sin(8 * math.pi * f0 * det2 * t + ph2 * 0.5)

                dry_l[i] += amp * e * s1 * 0.75
                dry_r[i] += amp * e * s2 * 0.75

    # Cosmic crystalline flame pulses (warm glass harmonics)
    flame_arpeggios = [
        (0.5, 73), (2.0, 76), (4.0, 80), (6.0, 85),
        (8.5, 73), (10.0, 76), (12.0, 81), (14.0, 88),
        (16.5, 74), (18.0, 77), (20.0, 81), (22.0, 86),
        (24.5, 71), (26.0, 75), (28.0, 80), (30.0, 83)
    ]
    for pt, pm in flame_arpeggios:
        pf0 = hz(pm)
        pan = rnd.uniform(0.3, 0.7)
        for i in range(N):
            t = i / SR
            d = wrapd(t, pt)
            if d < 0 or d > 3.0:
                continue
            env = math.exp(-d * 3.2) * (1.0 - math.exp(-d * 60.0))
            tone = math.sin(2 * math.pi * pf0 * d)
            tone += 0.25 * math.sin(4 * math.pi * pf0 * d)
            val = 0.038 * env * tone
            wet_l[i] += val * (1.0 - pan)
            wet_r[i] += val * pan

    # Cosmic abyss stereo ping-pong reverb
    dly_samples = int(SR * 0.55)
    feed = 0.44
    out_l = [0.0] * N
    out_r = [0.0] * N
    for i in range(N):
        prev = (i - dly_samples) % N
        out_l[i] = dry_l[i] + wet_l[i] + out_r[prev] * feed
        out_r[i] = dry_r[i] + wet_r[i] + out_l[prev] * feed

    out_path = "Assets/Resources/Audio/Music/abyss_ambient_theme.wav"
    write_wav(out_path, out_l, out_r)
    write_audio_meta(out_path, "c5a50001000200030004000500060008")

if __name__ == "__main__":
    print("Generating World 4 & World 5 ambient music themes...")
    os.makedirs("Assets/Resources/Audio/Music", exist_ok=True)
    generate_cathedral_theme()
    generate_abyss_theme()
    print("Audio themes generated successfully!")
