# RELEASE QA REPORT — AI BROWSER GAME JAM 5
**Project:** MY SHADOW QUIT: THE LAST FLAME  
**Date:** October 28, 2026  
**Version:** Release 1.0 (WebGL / HTML5)  
**Target Platform:** WebGL (itch.io)  
**Engine:** Unity 6000.6.2f1 (URP 2D)  
**Status:** PASS — PRODUCTION READY FOR RELEASE

---

## 1. RELEASE ARTIFACT VERIFICATION

| Metric | Result / Value | Verification Status |
| :--- | :--- | :--- |
| **ZIP File Location** | `Releases/AI-Browser-Game-Jam-5/MY-SHADOW-QUIT-THE-LAST-FLAME-AI-JAM-5.zip` | Confirmed |
| **ZIP File Size** | 13,382,624 bytes (~12.8 MB) | PASS (Limit < 500 MB) |
| **Total Files Count in Archive** | 8 files | PASS (Limit < 1,000 files) |
| **Max Single File Size** | 7.5 MB (`WebGL.wasm.br`) | PASS (Limit < 200 MB) |
| **Root Structure** | `index.html` located at archive root (no enclosing folder) | PASS |
| **SHA-256 Checksum** | `7ad024b2bf2b4b57162a937d8452b7e391391aae30afc834e28ab9c237bf3c7a` | Confirmed |

### Archive Contents List:
```
  Length      Date    Time    Name
---------  ---------- -----   ----
     9927  10-28-2026 09:07   favicon.ico
     3407  10-28-2026 09:07   index.html
     7038  10-28-2026 09:07   favicon.png
        0  10-28-2026 09:08   Build/
    72386  10-28-2026 09:07   Build/WebGL.framework.js.br
  6338050  10-28-2026 09:07   Build/WebGL.data.br
  7522320  10-28-2026 09:07   Build/WebGL.wasm.br
    27675  10-28-2026 09:07   Build/WebGL.loader.js
---------                     -------
 13980803                     8 files
```

---

## 2. AUTOMATED PRE-BUILD & VERIFICATION SUITE RESULTS

The automated verification suite (`ProjectSetupBuilder.RunAutomatedVerification()`) executed prior to compilation and passed all checks:

- [x] **Player Prefab & Rigidbody Physics:** Checked mass, linear drag, collision layers, and gravity scale.
- [x] **Shadow Entity & Controller:** Verified detached shadow mechanics, roost detection, and independent trigger interactions.
- [x] **Save System & Progress:** Tested all 10 levels in World 1 (`world_1`), World 2 (`world_2`), World 3 (`world_3`), World 4 (`world_4`), and World 5 (`world_5`). Confirmed `HasSavedProgress`, `GetContinueLevelIndex`, and completion bitmasks.
- [x] **Level Select Map & Ruin Nodes:** Verified 10-node progression graph, lock/unlock states, and launch handlers.
- [x] **UI Navigation & Menus:** Verified Home, World Select, Level Select, Pause, and Shop panels.
- [x] **Combat & Weapon System:** Verified Emberblade, Hollow Crossbow, and Abyssal Scepter instantiation, projectile pooling, and cooldown timers.
- [x] **Lethal Water & Buoyancy (World 4):** Validated discrete water hazard height calculations with foot contacts to ensure zero false deaths on floating buoyant platforms.
- [x] **Audio System Single Lifecycle:** Guarded audio source initialization and safe teardown during scene and menu transitions.

---

## 3. CONFIRMED HOTFIXES INCORPORATED IN BUILD

1. **Level 33 False Water Death & Restart on Jump:** Resolved. Contact checking distinguishes between safe platform contacts and actual submersion below the lethal water surface line.
2. **Level 35 Cooperative Puzzle Design:** Rebalanced puzzle flow requiring dual simultaneous cooperation between Protagonist and Shadow.
3. **Level 36 Water Height Alignment:** Platform heights correctly placed relative to water level hazard zones.
4. **World 4 Completion Panel Persist Bug:** Fixed in `UIManager.HideAllMenuPanels()`. The `CONTINUE TO WORLD 5` panel now cleanly closes upon transitioning to World 5.
5. **Level 41 HUD Numbering ("Level 1" bug):** Tutorial director constrained strictly to World 1. Level numbering correctly indexes from 41 to 50 in World 5.
6. **Level 43 Abyssal Redesign:** Replaced confusing layout with a clean 5-phase cooperative challenge utilizing the Abyssal Scepter, shadow roosts, abyssal bridge, and void echo switches.
7. **Control Redesign:** Distinct dedicated attack key (`J` / Left Click) and cycle weapon key (`F`). Separate dedicated touch buttons on mobile canvas.
8. **UI Cleanup:** Removed overlapping weapon sprites from HUD right-side interface.

---

## 4. BROWSER & COMPATIBILITY CHECKS

- **Canvas Aspect Ratio:** Standard 16:9 (`1280x720`). Viewport wrapper automatically clamps to maximum aspect size within browser window while preserving pixel-perfect integer scaling where supported.
- **Canvas Focus Handling:** Custom template listens to pointerdown / click events on `#unity-canvas` to guarantee keyboard input focus immediately upon clicking anywhere on the game canvas.
- **Compression:** Brotli compression applied with standard Unity WebGL loader decoding.
- **WebGL Version:** WebGL 2.0 (GLES 3.0 compatible) with WebGPU compatibility warnings mitigated for upcoming browsers.

---

## 5. CONCLUSION

The WebGL release package is fully compiled, verified, and packaged into the standard submission format for itch.io and AI Browser Game Jam 5. All criteria are met.
