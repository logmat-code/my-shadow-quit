using System.Collections.Generic;
using UnityEngine;
using MyShadowQuit.Entities;
using MyShadowQuit.Puzzle;

namespace MyShadowQuit.Core
{
    /// <summary>
    /// Constructs and manages all 8 levels of MY SHADOW QUIT: THE LAST FLAME using original pixel-art assets:
    /// 1. Awakening
    /// 2. The Separation
    /// 3. Follow the Light
    /// 4. The Forgotten Flame
    /// 5. Dark Passage
    /// 6. Two Sides
    /// 7. The Last Light
    /// 8. My Shadow Quit
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        public static readonly string[] LevelTitles =
        {
            "I. AWAKENING",
            "II. THE SEPARATION",
            "III. FOLLOW THE LIGHT",
            "IV. THE FORGOTTEN FLAME",
            "V. DARK PASSAGE",
            "VI. TWO SIDES",
            "VII. THE LAST LIGHT",
            "VIII. MY SHADOW QUIT"
        };

        public static readonly string[] LevelHints =
        {
            "A/D: Move   SPACE: Jump   E: Pick up / Place Torch   TAB: Control Shadow",
            "Stand on the lower stone plate, switch to Shadow [TAB], and Double Jump [SPACE x2 in mid-air] to the upper ledge.",
            "Darkness webs dissolve in Torchlight. Guide your Shadow across the illuminated path.",
            "Ancient solar crystals awaken when bathed in the Torch's golden flame.",
            "Push the carved stone block onto the plate while your Shadow unlocks the inner sanctum.",
            "Two parallel tiers: the Flame walks above, the Shadow walks below.",
            "Transfer the Torch between pedestals to clear Darkness Webs and open the final seal.",
            "One last ascent together. Reconnect Flame and Shadow at the Sanctuary Arch."
        };

        private GameObject _levelRoot;
        private PlayerController _player;
        private ShadowController _shadow;
        private TorchController _torch;

        public PlayerController Player => _player;
        public ShadowController Shadow => _shadow;
        public TorchController Torch => _torch;
        public int TotalLevels => LevelTitles.Length;

        public void BuildLevel(int levelIndex, CameraController cam)
        {
            ClearCurrentLevel();

            _levelRoot = new GameObject($"Level_{levelIndex + 1}");

            if (cam != null)
            {
                cam.BuildParallaxLayers();
                // 16:9 orthographic size 4.5 = 16 units wide x 9 units tall.
                // Clamp camera so outer walls (-9..23) and floor/ceiling (-4..8) fill the 16:9 frame cleanly.
                cam.SetBounds(new Vector2(-1.2f, 0.2f), new Vector2(14.8f, 2.8f));
            }

            // Base outer chamber walls & floor common framing
            BuildChamberFrame(-9, 23, -4, 8);

            switch (levelIndex)
            {
                case 0:
                    BuildLevel1_Awakening();
                    break;
                case 1:
                    BuildLevel2_TheSeparation();
                    break;
                case 2:
                    BuildLevel3_FollowTheLight();
                    break;
                case 3:
                    BuildLevel4_TheForgottenFlame();
                    break;
                case 4:
                    BuildLevel5_DarkPassage();
                    break;
                case 5:
                    BuildLevel6_TwoSides();
                    break;
                case 6:
                    BuildLevel7_TheLastLight();
                    break;
                default:
                    BuildLevel8_MyShadowQuit();
                    break;
            }
        }

        public void ClearCurrentLevel()
        {
            if (_levelRoot != null)
            {
                DestroyObjectImmediateOrSafe(_levelRoot);
                _levelRoot = null;
            }

            // Clean up any orphaned Level_* containers or stray Protagonist/Shadow/Torch instances
            // (e.g., if left behind by an interrupted EditMode verification or scene reload)
            PlayerController[] strayPlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            foreach (PlayerController p in strayPlayers)
            {
                if (p != null)
                {
                    GameObject rootToDestroy = (p.transform.parent != null && p.transform.parent.name.StartsWith("Level_"))
                        ? p.transform.parent.gameObject
                        : p.gameObject;
                    DestroyObjectImmediateOrSafe(rootToDestroy);
                }
            }

            ShadowController[] strayShadows = FindObjectsByType<ShadowController>(FindObjectsSortMode.None);
            foreach (ShadowController s in strayShadows)
            {
                if (s != null)
                {
                    GameObject rootToDestroy = (s.transform.parent != null && s.transform.parent.name.StartsWith("Level_"))
                        ? s.transform.parent.gameObject
                        : s.gameObject;
                    DestroyObjectImmediateOrSafe(rootToDestroy);
                }
            }

            TorchController[] strayTorches = FindObjectsByType<TorchController>(FindObjectsSortMode.None);
            foreach (TorchController t in strayTorches)
            {
                if (t != null)
                {
                    GameObject rootToDestroy = (t.transform.parent != null && t.transform.parent.name.StartsWith("Level_"))
                        ? t.transform.parent.gameObject
                        : t.gameObject;
                    DestroyObjectImmediateOrSafe(rootToDestroy);
                }
            }

            _player = null;
            _shadow = null;
            _torch = null;
        }

        private static void DestroyObjectImmediateOrSafe(GameObject go)
        {
            if (go == null) return;
            if (Application.isPlaying)
            {
                go.SetActive(false);
                Destroy(go);
            }
            else
            {
                DestroyImmediate(go);
            }
        }

        // =========================================================================
        // LEVEL 1: AWAKENING (Vertical Slice)
        // Wake inside ancient ruins -> pick up burning torch -> shadow separates ->
        // place torch in pedestal -> switch to shadow [TAB] -> pass shadow lattice ->
        // activate shadow obelisk -> stone door opens -> exit sanctuary.
        // =========================================================================
        private void BuildLevel1_Awakening()
        {
            // Accessible stepped stone ledges teaching jump & elevation without blocking the Shadow on the main floor
            CreatePlatformRow(-2, 1, -2, true);
            CreatePlatformRow(3, 7, -1, true);

            // Decorative ancient architecture framed for 16:9 landscape
            CreateProp("Environment/ancient_column", new Vector2(-5.5f, -1.5f), 1);
            CreateProp("Environment/ancient_column", new Vector2(0.5f, -1.5f), 1);
            CreateProp("Environment/ruined_arch", new Vector2(-2.5f, -1.0f), 0);
            CreateProp("Environment/hanging_chain", new Vector2(-3.5f, 4.0f), 2);
            CreateProp("Environment/hanging_chain", new Vector2(4.5f, 4.0f), 2);
            CreateProp("Environment/hanging_chain", new Vector2(12.5f, 4.0f), 2);

            // Spawn Protagonist & Shadow & Nearby Burning Torch
            SpawnCharactersAndTorch(
                playerPos: new Vector2(-6.2f, -2.2f),
                shadowPos: new Vector2(-7.1f, -2.2f),
                torchPos: new Vector2(-3.8f, -2.4f),
                startWithTorch: false
            );

            // Torch Pedestal perched on the central altar platform right before the Shadow Passage
            TorchPedestal pedestal = CreateTorchPedestal(new Vector2(5.5f, -0.25f));

            // Shadow Passage dividing the upper & lower chamber at x = 9.0
            CreateWallColumn(9, 0, 7);
            ShadowPassage passage1 = CreateShadowPassage(new Vector2(9.0f, -2.0f));

            // Shadow-only Obelisk Switch inside the shadow alcove (x = 12.2)
            ShadowSwitch shadowSwitch = CreateShadowSwitch(new Vector2(12.2f, -2.25f));

            // Activating the ShadowSwitch dissolves the ShadowPassage barrier so the Protagonist can walk through!
            passage1.ConfigureUnlockNodes(new List<PuzzleNode> { shadowSwitch }, requireAll: true, latchOpen: true);

            // Ancient Stone Door guarding the Exit Sanctuary (requires Pedestal Lit + Shadow Switch Active)
            // Automatically draws rune conduit lines to both pedestal and shadowSwitch!
            AncientDoor exitDoor = CreateAncientDoor(
                new Vector2(15.5f, -1.5f),
                new List<PuzzleNode> { pedestal, shadowSwitch },
                requireAll: true
            );

            CreateExitPortal(new Vector2(18.8f, -1.75f), exitDoor, false);
        }

        // =========================================================================
        // LEVEL 2: THE SEPARATION
        // Requires simultaneous cooperation on dual pressure plates.
        // =========================================================================
        private void BuildLevel2_TheSeparation()
        {
            CreateProp("Environment/ancient_column", new Vector2(-4f, -1.5f), 1);
            CreateProp("Environment/ruined_arch", new Vector2(3f, -1.0f), 0);

            SpawnCharactersAndTorch(
                playerPos: new Vector2(-6.5f, -2.2f),
                shadowPos: new Vector2(-7.3f, -2.2f),
                torchPos: new Vector2(-5.2f, -2.4f),
                startWithTorch: true
            );

            // First plate on lower floor
            PressurePlate plateA = CreatePressurePlate(new Vector2(1.5f, -2.85f), false);

            // Upper ledge reachable by jumping or passing a shadow barrier
            CreatePlatformRow(5, 11, 0, true);
            CreateShadowPassage(new Vector2(7.0f, 1.5f));
            PressurePlate plateB = CreatePressurePlate(new Vector2(9.5f, 0.65f), false);

            TorchPedestal pedestal = CreateTorchPedestal(new Vector2(3.5f, -2.25f));
            _ = pedestal;

            AncientDoor door = CreateAncientDoor(
                new Vector2(15.5f, -1.5f),
                new List<PuzzleNode> { plateA, plateB },
                requireAll: true,
                latchOpen: true
            );

            CreateExitPortal(new Vector2(19.0f, -1.75f), door, false);
        }

        // =========================================================================
        // LEVEL 3: FOLLOW THE LIGHT
        // Darkness Barriers block the path unless illuminated by the protagonist's Torch.
        // =========================================================================
        private void BuildLevel3_FollowTheLight()
        {
            CreateProp("Environment/hanging_chain", new Vector2(-1f, 4.0f), 2);
            CreateProp("Environment/hanging_chain", new Vector2(8f, 4.0f), 2);

            SpawnCharactersAndTorch(
                playerPos: new Vector2(-6.5f, -2.2f),
                shadowPos: new Vector2(-7.2f, -2.2f),
                torchPos: new Vector2(-5.5f, -2.4f),
                startWithTorch: true
            );

            // Upper walkway for Protagonist carrying the Torch
            CreatePlatformRow(-2, 12, 1, true);
            CreatePlatformRow(-4, -2, -1, true);

            // Darkness Webs on the lower path blocking the Shadow unless Player stands above with the Torch
            CreateDarknessBarrier(new Vector2(2.0f, -2.0f), dissolvesWhenLit: true);
            CreateDarknessBarrier(new Vector2(7.5f, -2.0f), dissolvesWhenLit: true);

            // Shadow Passage at the end of the lower tunnel
            ShadowPassage passage3 = CreateShadowPassage(new Vector2(11.5f, -2.0f));
            ShadowSwitch sw = CreateShadowSwitch(new Vector2(13.5f, -2.25f));
            passage3.ConfigureUnlockNodes(new List<PuzzleNode> { sw }, requireAll: true, latchOpen: true);

            AncientDoor door = CreateAncientDoor(
                new Vector2(16.5f, -1.5f),
                new List<PuzzleNode> { sw },
                requireAll: true
            );

            CreateExitPortal(new Vector2(19.5f, -1.75f), door, false);
        }

        // =========================================================================
        // LEVEL 4: THE FORGOTTEN FLAME
        // Light-sensitive solar crystals + Torch Pedestal placement.
        // =========================================================================
        private void BuildLevel4_TheForgottenFlame()
        {
            CreateProp("Environment/ruined_arch", new Vector2(-1f, -1.0f), 0);
            CreateProp("Environment/ancient_column", new Vector2(10f, -1.5f), 1);

            SpawnCharactersAndTorch(
                playerPos: new Vector2(-6.5f, -2.2f),
                shadowPos: new Vector2(-7.3f, -2.2f),
                torchPos: new Vector2(-5.5f, -2.4f),
                startWithTorch: true
            );

            // Pedestal near high LightSwitch
            CreatePlatformRow(0, 6, 0, true);
            TorchPedestal pedestal = CreateTorchPedestal(new Vector2(3.0f, 0.75f));
            LightSwitch lightCrystal = CreateLightSwitch(new Vector2(4.5f, 0.75f), latch: false);
            _ = pedestal;

            // Shadow passage leading to second switch
            ShadowPassage passage4 = CreateShadowPassage(new Vector2(9.0f, -2.0f));
            ShadowSwitch shadowSw = CreateShadowSwitch(new Vector2(12.0f, -2.25f));
            passage4.ConfigureUnlockNodes(new List<PuzzleNode> { shadowSw }, requireAll: true, latchOpen: true);

            AncientDoor door = CreateAncientDoor(
                new Vector2(16.0f, -1.5f),
                new List<PuzzleNode> { lightCrystal, shadowSw },
                requireAll: true
            );

            CreateExitPortal(new Vector2(19.5f, -1.75f), door, false);
        }

        // =========================================================================
        // LEVEL 5: DARK PASSAGE
        // Introduces Movable Stone Block + Pressure Plate + Shadow Switch.
        // =========================================================================
        private void BuildLevel5_DarkPassage()
        {
            CreateProp("Environment/ancient_column", new Vector2(-3f, -1.5f), 1);
            CreateProp("Environment/ancient_column", new Vector2(6f, -1.5f), 1);

            SpawnCharactersAndTorch(
                playerPos: new Vector2(-6.5f, -2.2f),
                shadowPos: new Vector2(-7.3f, -2.2f),
                torchPos: new Vector2(-5.5f, -2.4f),
                startWithTorch: true
            );

            CreateMovableBlock(new Vector2(-1.5f, -2.1f));
            PressurePlate plate = CreatePressurePlate(new Vector2(2.5f, -2.85f), false);

            CreateCheckpoint(new Vector2(5.0f, -2.25f));

            CreatePlatformRow(7, 13, 0, true);
            CreateShadowPassage(new Vector2(9.5f, 1.5f));
            ShadowSwitch shadowSw = CreateShadowSwitch(new Vector2(12.0f, 0.75f));

            AncientDoor door = CreateAncientDoor(
                new Vector2(16.5f, -1.5f),
                new List<PuzzleNode> { plate, shadowSw },
                requireAll: true
            );

            CreateExitPortal(new Vector2(19.5f, -1.75f), door, false);
        }

        // =========================================================================
        // LEVEL 6: TWO SIDES
        // Divided upper and lower sanctums requiring synchronized switches.
        // =========================================================================
        private void BuildLevel6_TwoSides()
        {
            SpawnCharactersAndTorch(
                playerPos: new Vector2(-6.5f, -2.2f),
                shadowPos: new Vector2(-6.5f, 1.8f),
                torchPos: new Vector2(-5.5f, -2.4f),
                startWithTorch: true
            );

            // Dividing stone deck separating Upper Shadow Realm and Lower Torch Realm
            CreatePlatformRow(-8, 12, 0, false);

            // Lower path: Torch dissolves darkness barrier & lights solar crystal
            CreateDarknessBarrier(new Vector2(1.0f, -2.0f), dissolvesWhenLit: true);
            TorchPedestal lowerPedestal = CreateTorchPedestal(new Vector2(6.5f, -2.25f));
            LightSwitch lowerCrystal = CreateLightSwitch(new Vector2(8.0f, -2.25f), latch: false);
            _ = lowerPedestal;

            // Upper path: Shadow navigates shadow passage and activates obelisk
            CreateShadowPassage(new Vector2(3.5f, 2.0f));
            ShadowSwitch upperShadowSw = CreateShadowSwitch(new Vector2(9.5f, 1.75f));

            AncientDoor door = CreateAncientDoor(
                new Vector2(16.0f, -1.5f),
                new List<PuzzleNode> { lowerCrystal, upperShadowSw },
                requireAll: true
            );

            CreateExitPortal(new Vector2(19.5f, -1.75f), door, false);
        }

        // =========================================================================
        // LEVEL 7: THE LAST LIGHT
        // Multi-stage torch relay with latching solar crystal, block, and shadow gate.
        // =========================================================================
        private void BuildLevel7_TheLastLight()
        {
            CreateProp("Environment/ruined_arch", new Vector2(0f, -1.0f), 0);
            CreateProp("Environment/ruined_arch", new Vector2(10f, -1.0f), 0);

            SpawnCharactersAndTorch(
                playerPos: new Vector2(-6.5f, -2.2f),
                shadowPos: new Vector2(-7.3f, -2.2f),
                torchPos: new Vector2(-5.5f, -2.4f),
                startWithTorch: true
            );

            // Stage 1: Latching solar crystal on high ledge
            CreatePlatformRow(-3, 2, 0, true);
            LightSwitch crystal1 = CreateLightSwitch(new Vector2(-0.5f, 0.75f), latch: true);

            // Stage 2: Darkness barrier & movable block
            CreateDarknessBarrier(new Vector2(4.0f, -2.0f), dissolvesWhenLit: true);
            CreateMovableBlock(new Vector2(6.0f, -2.1f));
            PressurePlate plate = CreatePressurePlate(new Vector2(8.5f, -2.85f), false);

            // Stage 3: Shadow passage & obelisk
            ShadowPassage passage7 = CreateShadowPassage(new Vector2(11.5f, -2.0f));
            ShadowSwitch shadowSw = CreateShadowSwitch(new Vector2(13.8f, -2.25f));
            passage7.ConfigureUnlockNodes(new List<PuzzleNode> { shadowSw }, requireAll: true, latchOpen: true);

            AncientDoor door = CreateAncientDoor(
                new Vector2(16.5f, -1.5f),
                new List<PuzzleNode> { crystal1, plate, shadowSw },
                requireAll: true
            );

            CreateExitPortal(new Vector2(19.5f, -1.75f), door, false);
        }

        // =========================================================================
        // LEVEL 8: MY SHADOW QUIT (Grand Finale)
        // Full synthesis of Torch, Pedestal, Light Crystal, Shadow Passage, Pressure Plate,
        // and final Reconnection at the Sanctuary Arch.
        // =========================================================================
        private void BuildLevel8_MyShadowQuit()
        {
            CreateProp("Environment/ancient_column", new Vector2(-4f, -1.5f), 1);
            CreateProp("Environment/ancient_column", new Vector2(4f, -1.5f), 1);
            CreateProp("Environment/ancient_column", new Vector2(12f, -1.5f), 1);
            CreateProp("Environment/ruined_arch", new Vector2(8f, -1.0f), 0);

            SpawnCharactersAndTorch(
                playerPos: new Vector2(-6.5f, -2.2f),
                shadowPos: new Vector2(-7.3f, -2.2f),
                torchPos: new Vector2(-5.5f, -2.4f),
                startWithTorch: true
            );

            // Tier 1: Movable block onto ancient pressure plate
            CreateMovableBlock(new Vector2(-2.5f, -2.1f));
            PressurePlate plate = CreatePressurePlate(new Vector2(0.5f, -2.85f), false);

            // Tier 2: Upper altar where Torch is placed in Pedestal to ignite Solar Crystal
            CreatePlatformRow(2, 8, 0, true);
            TorchPedestal pedestal = CreateTorchPedestal(new Vector2(5.0f, 0.75f));
            LightSwitch crystal = CreateLightSwitch(new Vector2(6.5f, 0.75f), latch: true);
            _ = pedestal;

            // Tier 3: Shadow passes barrier to trigger final Obelisk, then reunites with Protagonist
            ShadowPassage passage8 = CreateShadowPassage(new Vector2(10.5f, -2.0f));
            ShadowSwitch shadowSw = CreateShadowSwitch(new Vector2(13.0f, -2.25f));
            passage8.ConfigureUnlockNodes(new List<PuzzleNode> { shadowSw }, requireAll: true, latchOpen: true);

            AncientDoor finalDoor = CreateAncientDoor(
                new Vector2(16.0f, -1.5f),
                new List<PuzzleNode> { plate, crystal, shadowSw },
                requireAll: true
            );

            // Final Sanctuary requires both Protagonist and Shadow to reunite at the exit!
            CreateExitPortal(new Vector2(19.5f, -1.75f), finalDoor, requireShadowNear: true);
        }

        // =========================================================================
        // BUILDER HELPERS
        // =========================================================================
        private void BuildChamberFrame(int minX, int maxX, int minY, int maxY)
        {
            Sprite floorSp = AssetRegistry.GetSprite("Environment/stone_floor");
            Sprite wallSp = AssetRegistry.GetSprite("Environment/stone_wall");

            // Solid Floor (2 tiles thick)
            for (int x = minX; x <= maxX; x++)
            {
                CreateTileSprite("Floor_" + x, floorSp, new Vector2(x, minY + 0.5f), 2);
                CreateTileSprite("SubFloor_" + x, wallSp, new Vector2(x, minY - 0.5f), 2);
            }
            CreateBoxCollider("FloorCollider", new Vector2((minX + maxX) * 0.5f, minY), new Vector2(maxX - minX + 1f, 2f));

            // Ceiling
            for (int x = minX; x <= maxX; x++)
            {
                CreateTileSprite("Ceiling_" + x, wallSp, new Vector2(x, maxY - 0.5f), 2);
            }
            CreateBoxCollider("CeilingCollider", new Vector2((minX + maxX) * 0.5f, maxY - 0.5f), new Vector2(maxX - minX + 1f, 1f));

            // Left & Right Outer Walls
            for (int y = minY + 1; y < maxY; y++)
            {
                CreateTileSprite("WallL_" + y, wallSp, new Vector2(minX, y + 0.5f), 2);
                CreateTileSprite("WallR_" + y, wallSp, new Vector2(maxX, y + 0.5f), 2);
            }
            CreateBoxCollider("LeftWallCollider", new Vector2(minX, (minY + maxY) * 0.5f), new Vector2(1f, maxY - minY));
            CreateBoxCollider("RightWallCollider", new Vector2(maxX, (minY + maxY) * 0.5f), new Vector2(1f, maxY - minY));
        }

        private void CreatePlatformRow(int startX, int endX, int y, bool useBevelEdges)
        {
            Sprite midSp = AssetRegistry.GetSprite("Environment/stone_floor");
            Sprite leftSp = AssetRegistry.GetSprite("Environment/platform_edge_left");
            Sprite rightSp = AssetRegistry.GetSprite("Environment/platform_edge_right");

            for (int x = startX; x <= endX; x++)
            {
                Sprite sp = midSp;
                if (useBevelEdges && x == startX) sp = leftSp;
                else if (useBevelEdges && x == endX) sp = rightSp;
                CreateTileSprite($"Plat_{startX}_{y}_{x}", sp, new Vector2(x, y), 2);
            }

            float width = (endX - startX + 1);
            float centerX = (startX + endX) * 0.5f;
            CreateBoxCollider($"PlatCol_{startX}_{y}", new Vector2(centerX, y + 0.08f), new Vector2(width, 0.78f));
        }

        private void CreateWallColumn(int x, int startY, int endY)
        {
            Sprite wallSp = AssetRegistry.GetSprite("Environment/stone_wall");
            for (int y = startY; y <= endY; y++)
            {
                CreateTileSprite($"WallCol_{x}_{y}", wallSp, new Vector2(x, y), 2);
            }
            float height = (endY - startY + 1);
            float centerY = (startY + endY) * 0.5f;
            CreateBoxCollider($"WallColCollider_{x}", new Vector2(x, centerY), new Vector2(0.95f, height));
        }

        private void CreateTileSprite(string name, Sprite sp, Vector2 pos, int sortOrder)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sp;
            sr.sortingOrder = sortOrder;
        }

        private void CreateBoxCollider(string name, Vector2 pos, Vector2 size)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = size;
        }

        private void CreateProp(string spritePath, Vector2 pos, int sortOrder)
        {
            GameObject go = new GameObject("Prop_" + spritePath);
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetRegistry.GetSprite(spritePath);
            sr.sortingOrder = sortOrder;
        }

        private void SpawnCharactersAndTorch(Vector2 playerPos, Vector2 shadowPos, Vector2 torchPos, bool startWithTorch)
        {
            GameObject torchObj = new GameObject("Torch");
            torchObj.transform.SetParent(_levelRoot.transform, false);
            torchObj.transform.position = torchPos;
            _torch = torchObj.AddComponent<TorchController>();

            GameObject playerObj = new GameObject("Protagonist");
            playerObj.transform.SetParent(_levelRoot.transform, false);
            playerObj.transform.position = playerPos;
            _player = playerObj.AddComponent<PlayerController>();
            _player.EnsureInitialized();

            GameObject shadowObj = new GameObject("Shadow");
            shadowObj.transform.SetParent(_levelRoot.transform, false);
            shadowObj.transform.position = shadowPos;
            _shadow = shadowObj.AddComponent<ShadowController>();
            _shadow.Initialize(_player, _torch, startSeparated: true);

            if (startWithTorch)
            {
                _player.ForceEquipTorch(_torch);
            }
        }

        private TorchPedestal CreateTorchPedestal(Vector2 pos)
        {
            GameObject go = new GameObject("TorchPedestal");
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            return go.AddComponent<TorchPedestal>();
        }

        private PressurePlate CreatePressurePlate(Vector2 pos, bool latch)
        {
            GameObject go = new GameObject("PressurePlate");
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            PressurePlate pp = go.AddComponent<PressurePlate>();
            pp.EnsureInitialized();
            return pp;
        }

        private LightSwitch CreateLightSwitch(Vector2 pos, bool latch)
        {
            GameObject go = new GameObject("LightSwitch");
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            LightSwitch ls = go.AddComponent<LightSwitch>();
            ls.Configure(latch);
            return ls;
        }

        private ShadowSwitch CreateShadowSwitch(Vector2 pos)
        {
            GameObject go = new GameObject("ShadowSwitch");
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            return go.AddComponent<ShadowSwitch>();
        }

        private ShadowPassage CreateShadowPassage(Vector2 pos)
        {
            GameObject go = new GameObject("ShadowPassage");
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            ShadowPassage sp = go.AddComponent<ShadowPassage>();
            sp.EnsureInitialized();
            return sp;
        }

        private DarknessBarrier CreateDarknessBarrier(Vector2 pos, bool dissolvesWhenLit)
        {
            GameObject go = new GameObject("DarknessBarrier");
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            DarknessBarrier db = go.AddComponent<DarknessBarrier>();
            db.Configure(dissolvesWhenLit, new Vector2(0.9f, 1.95f));
            return db;
        }

        private MovableBlock CreateMovableBlock(Vector2 pos)
        {
            GameObject go = new GameObject("MovableBlock");
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            return go.AddComponent<MovableBlock>();
        }

        private Checkpoint CreateCheckpoint(Vector2 pos)
        {
            GameObject go = new GameObject("Checkpoint");
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            return go.AddComponent<Checkpoint>();
        }

        private AncientDoor CreateAncientDoor(Vector2 pos, List<PuzzleNode> nodes, bool requireAll = true, bool latchOpen = true)
        {
            GameObject go = new GameObject("AncientDoor");
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            AncientDoor door = go.AddComponent<AncientDoor>();
            door.Configure(nodes, requireAll, latchOpen);
            return door;
        }

        private LevelExitPortal CreateExitPortal(Vector2 pos, AncientDoor door, bool requireShadowNear)
        {
            GameObject go = new GameObject("ExitSanctuary");
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            LevelExitPortal portal = go.AddComponent<LevelExitPortal>();
            portal.Configure(door, requireShadowNear);
            return portal;
        }
    }
}
