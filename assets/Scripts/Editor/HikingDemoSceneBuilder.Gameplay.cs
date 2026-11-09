using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace SplintIt.EditorTools
{
    public static partial class HikingDemoSceneBuilder
    {
        class NpcRefs
        {
            public Transform root; public NpcReaction reaction;
        }

        class ZoneRefs
        {
            public PlacementZone prep, support, padding, strapUpper, strapLower;
        }

        class UiRefs
        {
            public GameObject systems;
            public GameObject menuPanel, roundPanel, resultPanel, pausePanel;
            public TMP_Text menuBriefing, menuNotice, objectiveText, checklistText, statusText, toastText;
            public TMP_Text resultTitle, resultBody, otherModeLabel, pauseNotice, restartLabel, quitLabel, seatedLabel, boardTimer, wristTimer;
            public Button guidedButton, evaluationButton, hintButton, checkButton, pauseButton, returnItemsButton;
            public Button retryButton, otherModeButton, resultMenuButton, resumeButton, restartButton, quitButton, seatedButton, heightUpButton, heightDownButton;
        }

        // ------------------------------------------------------------------ tables, log, labels

        static void Table(Transform parent, string name, Vector2 center, Vector2 size, string topHex)
        {
            var t = new GameObject(name).transform;
            t.SetParent(parent, false);
            t.position = new Vector3(center.x, 0f, center.y);
            Prim(PrimitiveType.Cube, "Top", t, new Vector3(0, TableTop - 0.025f, 0), new Vector3(size.x, 0.05f, size.y), L(name + "Top", topHex));
            float legH = TableTop - 0.05f;
            for (int sx = -1; sx <= 1; sx += 2)
                for (int sz = -1; sz <= 1; sz += 2)
                    Prim(PrimitiveType.Cylinder, "Leg", t, new Vector3(sx * (size.x / 2f - 0.05f), legH / 2f, sz * (size.y / 2f - 0.05f)), new Vector3(0.06f, legH / 2f, 0.06f), L("Wood", "#B97D4B"));
        }

        static void BuildTablesAndLog(Transform props)
        {
            Table(props, "SuppliesTable", SuppliesCenter, SuppliesSize, "#F5E6B8");
            Table(props, "PrepTable", PrepCenter, PrepSize, "#46C2CF");

            // flat "PREP AREA" label on the prep table, readable from above
            var label = NewCanvas("PrepLabel", props, new Vector3(PrepCenter.x, TableTop + 0.004f, PrepCenter.y - 0.17f), Quaternion.Euler(90, 0, 0), new Vector2(400, 100), 0.001f, false);
            Txt(label.transform, "Text", "PREP AREA", 60, TextAlignmentOptions.Center, 0, 0, 1, 1, FontStyles.Bold, "#FFFFFF");
            var sup = NewCanvas("SuppliesLabel", props, new Vector3(SuppliesCenter.x, TableTop + 0.004f, SuppliesCenter.y + SuppliesSize.y / 2f - 0.03f), Quaternion.Euler(90, 0, 0), new Vector2(400, 80), 0.001f, false);
            Txt(sup.transform, "Text", "SUPPLIES", 48, TextAlignmentOptions.Center, 0, 0, 1, 1, FontStyles.Bold, "#8A5A2B");

            // log the injured friend sits on
            var log = Prim(PrimitiveType.Cylinder, "Log", props, new Vector3(NpcPos.x, 0.22f, NpcPos.z + 0.05f), new Vector3(0.44f, 0.8f, 0.44f), L("Bark", "#9A6A3A"));
            log.transform.rotation = Quaternion.Euler(0, 0, 90);
        }

        // ------------------------------------------------------------------ NPC (gray placeholder)

        static NpcRefs BuildNpc(Transform props)
        {
            var root = new GameObject("NPC_InjuredHiker").transform;
            root.SetParent(props, false);
            root.position = NpcPos;
            root.rotation = Quaternion.Euler(0, 180, 0);   // local +Z points at the player

            var gray = L("CharacterGray", "#A8A8AC", 0.2f);
            var visual = new GameObject("Visual_REPLACE_WITH_YOUR_MODEL").transform;
            visual.SetParent(root, false);

            Vector3 hip = new Vector3(0, 0.5f, 0);
            var sway = new GameObject("Sway").transform;
            sway.SetParent(visual, false);
            sway.localPosition = hip;

            // torso, head and the relaxed arm sway gently; legs and the injured arm stay still so placed items never drift
            Prim(PrimitiveType.Capsule, "Torso", sway, new Vector3(0, 0.98f, -0.05f) - hip, new Vector3(0.34f, 0.30f, 0.22f), gray, false);
            Prim(PrimitiveType.Sphere, "Head", sway, new Vector3(0, 1.50f, -0.03f) - hip, Vector3.one * 0.22f, gray, false);
            Prim(PrimitiveType.Capsule, "L_UpperArm", sway, new Vector3(-0.24f, 1.12f, -0.03f) - hip, new Vector3(0.09f, 0.17f, 0.09f), gray, false);
            var lf = Prim(PrimitiveType.Capsule, "L_Forearm", sway, new Vector3(-0.24f, 0.98f, 0.14f) - hip, new Vector3(0.08f, 0.14f, 0.08f), gray, false);
            lf.transform.localRotation = Quaternion.Euler(90, 0, 0);

            for (int s = -1; s <= 1; s += 2)
            {
                var th = Prim(PrimitiveType.Capsule, "Thigh", visual, new Vector3(s * 0.10f, 0.50f, 0.17f), new Vector3(0.16f, 0.20f, 0.16f), gray, false);
                th.transform.localRotation = Quaternion.Euler(90, 0, 0);
                Prim(PrimitiveType.Capsule, "Shin", visual, new Vector3(s * 0.10f, 0.24f, 0.40f), new Vector3(0.14f, 0.24f, 0.14f), gray, false);
                Prim(PrimitiveType.Cube, "Foot", visual, new Vector3(s * 0.10f, 0.04f, 0.48f), new Vector3(0.10f, 0.08f, 0.22f), gray, false);
            }

            // injured (right) arm: forearm rests forward at a comfortable height
            Prim(PrimitiveType.Capsule, "R_UpperArm", visual, new Vector3(0.26f, 1.15f, -0.02f), new Vector3(0.09f, 0.16f, 0.09f), gray, false);
            var fa = Prim(PrimitiveType.Capsule, "R_Forearm", visual, ArmLocal, new Vector3(0.08f, 0.15f, 0.08f), gray, false);
            fa.transform.localRotation = Quaternion.Euler(90, 0, 0);
            Prim(PrimitiveType.Sphere, "R_Hand", visual, ArmLocal + new Vector3(0, 0, 0.19f), Vector3.one * 0.075f, gray, false);

            // injury indicator: a simple band + "!" sign. No blood, no distorted limbs.
            var band = Prim(PrimitiveType.Cylinder, "InjuryBand", visual, ArmLocal, new Vector3(0.11f, 0.008f, 0.11f), U("InjuryBand", "#FF8A00"), false);
            band.transform.localRotation = Quaternion.Euler(90, 0, 0);

            var badge = NewCanvas("InjuryBadge", root, root.TransformPoint(ArmLocal + new Vector3(0, 0.20f, 0)), Quaternion.identity, new Vector2(140, 140), 0.001f, false);
            badge.gameObject.AddComponent<Billboard>();
            var badgeBg = Bg(badge.transform, "Bg", H("#FF8A00"));
            var badgeText = Txt(badge.transform, "Symbol", "!", 110, TextAlignmentOptions.Center, 0, 0, 1, 1, FontStyles.Bold, "#FFFFFF");

            var bubble = NewCanvas("SpeechBubble", root, root.TransformPoint(new Vector3(0, 1.95f, 0)), Quaternion.identity, new Vector2(760, 170), 0.001f, false);
            bubble.gameObject.AddComponent<Billboard>();
            Bg(bubble.transform, "Bg", new Color(0.08f, 0.1f, 0.16f, 0.9f));
            var speech = Txt(bubble.transform, "Text", "", 46, TextAlignmentOptions.Center, 0, 0, 1, 1, FontStyles.Normal, "#FFFFFF");

            var reaction = root.gameObject.AddComponent<NpcReaction>();
            reaction.swayRoot = sway;
            reaction.speechBubble = bubble.gameObject;
            reaction.speechText = speech;
            reaction.badgeText = badgeText;
            reaction.badgeBackground = badgeBg;
            reaction.bandRenderer = band.GetComponent<Renderer>();
            reaction.bandHurtMaterial = U("InjuryBand", "#FF8A00");
            reaction.bandReliefMaterial = U("InjuryOk", "#2ED47A");

            return new NpcRefs { root = root, reaction = reaction };
        }

        // ------------------------------------------------------------------ placement zones

        static PlacementZone Zone(Transform parent, string name, ZoneKind kind, Vector3 pos, bool localSpace, float radius, GameObject ghost)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            if (localSpace) go.transform.localPosition = pos; else go.transform.position = pos;
            var z = go.AddComponent<PlacementZone>();
            z.kind = kind;
            z.radius = radius;
            if (ghost != null) { ghost.transform.SetParent(go.transform, false); ghost.SetActive(false); z.ghost = ghost; }
            return z;
        }

        static GameObject GhostCylinder(Vector3 scale)
        {
            var g = Prim(PrimitiveType.Cylinder, "Ghost", null, Vector3.zero, scale, U("Ghost", "#25F4EE"), false);
            g.transform.localRotation = Quaternion.Euler(90, 0, 0);
            return g;
        }

        static ZoneRefs BuildZones(Transform props, Transform npcRoot)
        {
            var z = new ZoneRefs();

            // guided-mode outlines (unlit cyan so they read in any light; text/markers back them up)
            var prepGhost = new GameObject("Ghost");
            Vector2 half = PrepSize / 2f - new Vector2(0.03f, 0.03f);
            Material cyan = U("Ghost", "#25F4EE");
            for (int s = -1; s <= 1; s += 2)
            {
                Prim(PrimitiveType.Cube, "EdgeX", prepGhost.transform, new Vector3(0, 0, s * half.y), new Vector3(half.x * 2, 0.006f, 0.014f), cyan, false);
                Prim(PrimitiveType.Cube, "EdgeZ", prepGhost.transform, new Vector3(s * half.x, 0, 0), new Vector3(0.014f, 0.006f, half.y * 2), cyan, false);
            }
            z.prep = Zone(props, "Zone_Prep", ZoneKind.Prep, new Vector3(PrepCenter.x, TableTop + 0.06f, PrepCenter.y), false, 0.42f, prepGhost);
            prepGhost.transform.localPosition = new Vector3(0, -0.053f, 0);
            prepGhost.SetActive(false);

            // arm zones live in NPC space (local +Z runs along the forearm toward the player)
            z.support = Zone(npcRoot, "Zone_Support", ZoneKind.Support, ArmLocal + new Vector3(0, 0.065f, 0), true, 0.22f, GhostCylinder(new Vector3(0.06f, 0.26f, 0.06f)));
            z.padding = Zone(npcRoot, "Zone_Padding", ZoneKind.Padding, ArmLocal + new Vector3(0, 0.115f, 0), true, 0.22f, Prim(PrimitiveType.Cube, "Ghost", null, Vector3.zero, new Vector3(0.30f, 0.012f, 0.27f), cyan, false));
            z.strapUpper = Zone(npcRoot, "Zone_StrapUpper", ZoneKind.StrapUpper, ArmLocal + new Vector3(0, 0.045f, -0.09f), true, 0.13f, GhostCylinder(new Vector3(0.21f, 0.01f, 0.21f)));
            z.strapLower = Zone(npcRoot, "Zone_StrapLower", ZoneKind.StrapLower, ArmLocal + new Vector3(0, 0.045f, 0.09f), true, 0.13f, GhostCylinder(new Vector3(0.21f, 0.01f, 0.21f)));
            return z;
        }

        // ------------------------------------------------------------------ items

        static SplintItem NewItem(Transform parent, string name, string display, ItemRole role, Vector3 pos, float yaw, float length, string good, string bad)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.SetPositionAndRotation(pos, Quaternion.Euler(0, yaw, 0));
            var rb = root.AddComponent<Rigidbody>();
            rb.mass = 0.3f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            root.AddComponent<XRGrabInteractable>();
            var item = root.AddComponent<SplintItem>();
            item.displayName = display; item.role = role; item.length = length; item.lengthAxis = Vector3.forward;
            item.whyGood = good; item.whyNotSuitable = bad;

            var marker = MeshObj("HintMarker", Cone("MarkerCone", 5, 1f, 1.6f, 0f), root.transform, new Vector3(0, 0.30f, 0), new Vector3(0.05f, 0.06f, 0.05f), U("Hint", "#FFE600"), Quaternion.Euler(180, 0, 0), false);
            marker.SetActive(false);
            item.hintMarker = marker;
            return item;
        }

        static GameObject Band(SplintItem item, string hex)
        {
            var band = Prim(PrimitiveType.Cylinder, "WrappedBand", item.transform, Vector3.zero, new Vector3(0.20f, 0.014f, 0.20f), L("Band" + hex, hex), false);
            band.transform.localRotation = Quaternion.Euler(90, 0, 0);
            band.SetActive(false);
            item.wrappedVisual = band;
            return band;
        }

        static void CollectLoose(SplintItem item)
        {
            var list = new System.Collections.Generic.List<Renderer>();
            foreach (var r in item.GetComponentsInChildren<Renderer>(true))
                if (r.gameObject != item.hintMarker && r.gameObject != item.wrappedVisual) list.Add(r);
            item.looseRenderers = list.ToArray();
        }

        static SplintItem[] BuildItems(Transform props)
        {
            var parent = new GameObject("Items").transform;
            parent.SetParent(props, false);
            float T = TableTop;

            // --- useful items (3 or 4 are required by the MVP; here: support, padding, 2 straps) ---
            var branch = NewItem(parent, "Item_Branch", "Branch", ItemRole.RigidSupport, new Vector3(-0.80f, T + 0.03f, 0.38f), 90f, 0.5f,
                "stiff and long enough to support the forearm.", "");
            var b = Prim(PrimitiveType.Cylinder, "Body", branch.transform, Vector3.zero, new Vector3(0.045f, 0.25f, 0.045f), L("Branch", "#8A5A2B"));
            b.transform.localRotation = Quaternion.Euler(90, 0, 0);
            var twig = Prim(PrimitiveType.Cylinder, "Twig", branch.transform, new Vector3(0.03f, 0.02f, 0.05f), new Vector3(0.012f, 0.04f, 0.012f), L("Branch", "#8A5A2B"), false);
            twig.transform.localRotation = Quaternion.Euler(0, 0, 60);

            var jacket = NewItem(parent, "Item_Jacket", "Jacket", ItemRole.Padding, new Vector3(-0.58f, T + 0.03f, 0.10f), 0f, 0.26f,
                "soft, so it can pad the arm against the hard support.", "");
            Material jm = L("Jacket", "#FF5A36");
            Prim(PrimitiveType.Cube, "Body", jacket.transform, Vector3.zero, new Vector3(0.26f, 0.05f, 0.24f), jm);
            Prim(PrimitiveType.Cube, "SleeveL", jacket.transform, new Vector3(-0.175f, 0, 0.02f), new Vector3(0.09f, 0.04f, 0.05f), jm, false);
            Prim(PrimitiveType.Cube, "SleeveR", jacket.transform, new Vector3(0.175f, 0, 0.02f), new Vector3(0.09f, 0.04f, 0.05f), jm, false);
            Prim(PrimitiveType.Cube, "Collar", jacket.transform, new Vector3(0, 0.004f, -0.12f), new Vector3(0.12f, 0.056f, 0.03f), L("JacketDark", "#C93A1C"), false);
            Prim(PrimitiveType.Cube, "Zipper", jacket.transform, Vector3.zero + new Vector3(0, 0.026f, 0), new Vector3(0.02f, 0.004f, 0.24f), L("Gold", "#FFD23F"), false);
            CollectLoose(jacket);

            var belt = NewItem(parent, "Item_Belt", "Belt", ItemRole.Strap, new Vector3(-0.95f, T + 0.03f, -0.05f), 0f, 0.45f,
                "strong enough to hold the splint firmly.", "");
            Prim(PrimitiveType.Cube, "Body", belt.transform, Vector3.zero, new Vector3(0.45f, 0.014f, 0.035f), L("Belt", "#5B3A1E"), false);
            Prim(PrimitiveType.Cube, "Buckle", belt.transform, new Vector3(0.2f, 0.004f, 0), new Vector3(0.05f, 0.02f, 0.045f), L("Gold", "#FFD23F"), false);
            var bc = belt.gameObject.AddComponent<BoxCollider>(); bc.size = new Vector3(0.45f, 0.04f, 0.06f);
            CollectLoose(belt); Band(belt, "#5B3A1E");

            var scarf = NewItem(parent, "Item_Scarf", "Scarf", ItemRole.Strap, new Vector3(-0.78f, T + 0.03f, 0.27f), 0f, 0.55f,
                "long and flexible, so it can tie the splint in place.", "");
            Prim(PrimitiveType.Cube, "Body", scarf.transform, Vector3.zero, new Vector3(0.55f, 0.012f, 0.09f), L("Scarf", "#C03FE0"), false);
            Prim(PrimitiveType.Cube, "Stripe", scarf.transform, new Vector3(0, 0.0005f, 0), new Vector3(0.55f, 0.0125f, 0.025f), L("ScarfStripe", "#FFFFFF"), false);
            var sc = scarf.gameObject.AddComponent<BoxCollider>(); sc.size = new Vector3(0.55f, 0.04f, 0.10f);
            CollectLoose(scarf); Band(scarf, "#C03FE0");

            // --- distractors ---
            var bottle = NewItem(parent, "Item_Bottle", "Bottle", ItemRole.Distractor, new Vector3(-1.18f, T + 0.10f, 0.12f), 0f, 0.2f,
                "", "it is short and bends, so it cannot support the arm.");
            Material bm = L("Bottle", "#4FC3F7", 0.7f);
            Prim(PrimitiveType.Cylinder, "Body", bottle.transform, Vector3.zero, new Vector3(0.07f, 0.09f, 0.07f), bm);
            Prim(PrimitiveType.Cylinder, "Neck", bottle.transform, new Vector3(0, 0.12f, 0), new Vector3(0.035f, 0.03f, 0.035f), bm, false);
            Prim(PrimitiveType.Cylinder, "Cap", bottle.transform, new Vector3(0, 0.152f, 0), new Vector3(0.04f, 0.012f, 0.04f), L("White", "#FFFFFF"), false);

            var rock = NewItem(parent, "Item_Rock", "Rock", ItemRole.Distractor, new Vector3(-1.02f, T + 0.06f, 0.12f), 0f, 0.12f,
                "", "it is heavy and hard, and it cannot hold or protect the arm.");
            var rg = MeshObj("Body", Ico(2), rock.transform, Vector3.zero, new Vector3(0.06f, 0.05f, 0.06f), L("RockItem", "#7A7A84", 0.1f), Quaternion.identity);
            rg.AddComponent<SphereCollider>().radius = 1f;

            var phone = NewItem(parent, "Item_Phone", "Phone", ItemRole.Distractor, new Vector3(-0.86f, T + 0.01f, 0.12f), 15f, 0.14f,
                "", "it is too small, and it is not meant for this.");
            Prim(PrimitiveType.Cube, "Body", phone.transform, Vector3.zero, new Vector3(0.07f, 0.012f, 0.14f), L("Phone", "#2B2B33"));
            Prim(PrimitiveType.Cube, "Screen", phone.transform, new Vector3(0, 0.0065f, 0), new Vector3(0.062f, 0.002f, 0.125f), L("Screen", "#5A7BFF"), false);

            var pack = NewItem(parent, "Item_Backpack", "Backpack", ItemRole.Distractor, new Vector3(-1.20f, T + 0.15f, 0.30f), 0f, 0.3f,
                "", "it is bulky and cannot be shaped around the arm.");
            Prim(PrimitiveType.Cube, "Body", pack.transform, Vector3.zero, new Vector3(0.20f, 0.28f, 0.12f), L("Backpack", "#2F8F83"));
            Prim(PrimitiveType.Cube, "Pocket", pack.transform, new Vector3(0, -0.05f, 0.08f), new Vector3(0.15f, 0.10f, 0.04f), L("Pocket", "#F2B134"), false);

            return new[] { branch, jacket, belt, scarf, bottle, rock, phone, pack };
        }

        // ------------------------------------------------------------------ UI (world-space uGUI)

        static Canvas NewCanvas(string name, Transform parent, Vector3 worldPos, Quaternion rot, Vector2 px, float scale, bool interactive)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(worldPos, rot);
            go.transform.localScale = Vector3.one * scale;
            var c = go.AddComponent<Canvas>();
            c.renderMode = RenderMode.WorldSpace;
            go.GetComponent<RectTransform>().sizeDelta = px;
            if (interactive) go.AddComponent<TrackedDeviceGraphicRaycaster>();
            return c;
        }

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }

        static Image Bg(Transform parent, string name, Color c)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            Stretch(go.GetComponent<RectTransform>());
            var im = go.GetComponent<Image>();
            im.color = c;
            im.raycastTarget = false;
            return im;
        }

        static GameObject Panel(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            Stretch(go.GetComponent<RectTransform>());
            return go;
        }

        static TMP_Text Txt(Transform parent, string name, string text, float size, TextAlignmentOptions align, float x0, float y0, float x1, float y1, FontStyles style, string color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(x0, y0); rt.anchorMax = new Vector2(x1, y1);
            rt.offsetMin = new Vector2(14, 6); rt.offsetMax = new Vector2(-14, -6);
            var t = go.AddComponent<TextMeshProUGUI>();
            t.text = text; t.fontSize = size; t.alignment = align; t.fontStyle = style; t.color = H(color);
            t.enableAutoSizing = true; t.fontSizeMax = size; t.fontSizeMin = size * 0.5f;
            t.raycastTarget = false;
            return t;
        }

        static Button Btn(Transform parent, string name, string label, float x0, float y0, float x1, float y1, string hex, float size, out TMP_Text labelText)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(x0, y0); rt.anchorMax = new Vector2(x1, y1);
            rt.offsetMin = new Vector2(5, 5); rt.offsetMax = new Vector2(-5, -5);
            var img = go.GetComponent<Image>();
            img.color = H(hex);
            var b = go.GetComponent<Button>();
            b.targetGraphic = img;
            var cb = b.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(0.82f, 0.95f, 1f);
            cb.pressedColor = new Color(0.65f, 0.65f, 0.65f);
            cb.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.7f);
            b.colors = cb;
            labelText = Txt(go.transform, "Label", label, size, TextAlignmentOptions.Center, 0, 0, 1, 1, FontStyles.Bold, "#FFFFFF");
            return b;
        }

        static Transform FindLeftController(XROrigin rig)
        {
            if (rig == null) return null;
            Transform fallback = null;
            foreach (var t in rig.GetComponentsInChildren<Transform>(true))
            {
                string n = t.name.ToLowerInvariant();
                if (n == "left controller") return t;
                if (fallback == null && n.Contains("left") && (n.Contains("controller") || n.Contains("hand")) && !n.Contains("interactor") && !n.Contains("visual"))
                    fallback = t;
            }
            return fallback;
        }

        static UiRefs BuildUi(XROrigin rig)
        {
            var ui = new UiRefs();
            var flow = new GameObject("GameSystems");   // component host; the real DemoFlow is added in WireSystems
            var uiRoot = new GameObject("VR_UI").transform;

            // ---------- main board: menu / result / pause share one slot in front of the player ----------
            Vector3 mainPos = new Vector3(0f, 1.5f, 0.62f);
            var main = NewCanvas("MainBoard", uiRoot, mainPos, Quaternion.LookRotation(mainPos - PlayerHead), new Vector2(1000, 680), 0.0008f, true);
            Bg(main.transform, "Bg", new Color(0.07f, 0.10f, 0.17f, 0.94f));

            var menu = Panel(main.transform, "MenuPanel");
            Txt(menu.transform, "Title", "SplintIt VR  -  Hiking Demo", 52, TextAlignmentOptions.Center, 0, 0.86f, 1, 1, FontStyles.Bold, "#FFFFFF");
            var briefing = Txt(menu.transform, "Briefing", "", 30, TextAlignmentOptions.TopLeft, 0, 0.40f, 1, 0.86f, FontStyles.Normal, "#EAF2FF");
            var notice = Txt(menu.transform, "Notice", "", 24, TextAlignmentOptions.Center, 0, 0.30f, 1, 0.40f, FontStyles.Normal, "#FFE08A");
            TMP_Text l1, l2;
            var guided = Btn(menu.transform, "GuidedButton", "GUIDED\nlearn step by step", 0.03f, 0.10f, 0.49f, 0.30f, "#2E86DE", 30, out l1);
            var eval = Btn(menu.transform, "EvaluationButton", "EVALUATION\ntimed, no hints", 0.51f, 0.10f, 0.97f, 0.30f, "#E67E22", 30, out l2);
            Txt(menu.transform, "Footer", "Educational demo. Not professional medical training.", 20, TextAlignmentOptions.Center, 0, 0, 1, 0.09f, FontStyles.Italic, "#9AA7BD");

            var result = Panel(main.transform, "ResultPanel");
            var rTitle = Txt(result.transform, "Title", "", 52, TextAlignmentOptions.Center, 0, 0.86f, 1, 1, FontStyles.Bold, "#FFFFFF");
            var rBody = Txt(result.transform, "Body", "", 28, TextAlignmentOptions.TopLeft, 0, 0.26f, 1, 0.86f, FontStyles.Normal, "#EAF2FF");
            TMP_Text rl1, rl2, rl3;
            var retry = Btn(result.transform, "RetryButton", "RETRY", 0.02f, 0.03f, 0.33f, 0.23f, "#2E86DE", 32, out rl1);
            var other = Btn(result.transform, "OtherModeButton", "TRY EVALUATION", 0.345f, 0.03f, 0.665f, 0.23f, "#E67E22", 28, out rl2);
            var rmenu = Btn(result.transform, "MenuButton", "MENU", 0.69f, 0.03f, 0.98f, 0.23f, "#5D6D7E", 32, out rl3);

            var pause = Panel(main.transform, "PausePanel");
            Txt(pause.transform, "Title", "|| PAUSED", 56, TextAlignmentOptions.Center, 0, 0.86f, 1, 1, FontStyles.Bold, "#FFFFFF");
            var pNotice = Txt(pause.transform, "Notice", "", 28, TextAlignmentOptions.Center, 0, 0.66f, 1, 0.86f, FontStyles.Normal, "#FFE08A");
            TMP_Text pl1, pl2, pl3, pl4, pl5, pl6;
            var resume = Btn(pause.transform, "ResumeButton", "RESUME", 0.2f, 0.46f, 0.8f, 0.64f, "#27AE60", 40, out pl1);
            var restart = Btn(pause.transform, "RestartButton", "RESTART", 0.03f, 0.27f, 0.49f, 0.43f, "#C0392B", 30, out pl2);
            var quit = Btn(pause.transform, "QuitButton", "MAIN MENU", 0.51f, 0.27f, 0.97f, 0.43f, "#5D6D7E", 30, out pl3);
            var seated = Btn(pause.transform, "SeatedButton", "Standing", 0.03f, 0.05f, 0.49f, 0.22f, "#2E86DE", 26, out pl4);
            var hDown = Btn(pause.transform, "HeightDown", "HEIGHT -", 0.52f, 0.05f, 0.73f, 0.22f, "#34495E", 24, out pl5);
            var hUp = Btn(pause.transform, "HeightUp", "HEIGHT +", 0.76f, 0.05f, 0.97f, 0.22f, "#34495E", 24, out pl6);

            // ---------- round board (checklist, timer, actions) to the right ----------
            Vector3 roundPos = new Vector3(1.35f, 1.40f, 0.50f);
            var round = NewCanvas("RoundBoard", uiRoot, roundPos, Quaternion.LookRotation(roundPos - PlayerHead), new Vector2(760, 860), 0.001f, true);
            Bg(round.transform, "Bg", new Color(0.07f, 0.10f, 0.17f, 0.94f));
            var roundPanel = round.gameObject;
            var timerText = Txt(round.transform, "Timer", "TIME --:--", 52, TextAlignmentOptions.Center, 0, 0.90f, 1, 1, FontStyles.Bold, "#FFFFFF");
            var objective = Txt(round.transform, "Objective", "", 32, TextAlignmentOptions.TopLeft, 0, 0.73f, 1, 0.90f, FontStyles.Bold, "#FFE08A");
            var checklist = Txt(round.transform, "Checklist", "", 32, TextAlignmentOptions.TopLeft, 0, 0.40f, 1, 0.73f, FontStyles.Normal, "#FFFFFF");
            var status = Txt(round.transform, "Status", "", 26, TextAlignmentOptions.Left, 0, 0.33f, 1, 0.40f, FontStyles.Normal, "#B8C4D8");
            var toast = Txt(round.transform, "Toast", "", 28, TextAlignmentOptions.TopLeft, 0, 0.17f, 1, 0.33f, FontStyles.Normal, "#9BE7FF");
            TMP_Text bl1, bl2, bl3, bl4;
            var hint = Btn(round.transform, "HintButton", "HINT", 0.01f, 0.02f, 0.22f, 0.16f, "#8E44AD", 26, out bl1);
            var check = Btn(round.transform, "CheckButton", "CHECK SPLINT", 0.23f, 0.02f, 0.62f, 0.16f, "#27AE60", 30, out bl2);
            var pauseBtn = Btn(round.transform, "PauseButton", "|| PAUSE", 0.63f, 0.02f, 0.80f, 0.16f, "#5D6D7E", 22, out bl3);
            var ret = Btn(round.transform, "ReturnButton", "RETURN\nITEMS", 0.81f, 0.02f, 0.99f, 0.16f, "#34495E", 20, out bl4);

            // ---------- wrist timer on the left controller (or floating if none found) ----------
            Transform left = FindLeftController(rig);
            Canvas wrist = left != null
                ? NewCanvas("WristTimer", left, left.position, left.rotation, new Vector2(260, 100), 0.0005f, false)
                : NewCanvas("WristTimer", uiRoot, new Vector3(-0.35f, 1.0f, 0.25f), Quaternion.LookRotation(new Vector3(-0.35f, 1.0f, 0.25f) - PlayerHead), new Vector2(260, 100), 0.0005f, false);
            if (left != null)
            {
                wrist.transform.localPosition = new Vector3(0f, 0.03f, -0.05f);
                wrist.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);   // lies on top of the controller, readable when you look down
            }
            Bg(wrist.transform, "Bg", new Color(0.07f, 0.10f, 0.17f, 0.9f));
            var wristText = Txt(wrist.transform, "Text", "TIME --:--", 34, TextAlignmentOptions.Center, 0, 0, 1, 1, FontStyles.Bold, "#FFFFFF");

            // ---------- hand everything to a holder component set up in WireSystems ----------
            var holder = ui;
            holder.systems = flow;
            holder.menuPanel = menu; holder.roundPanel = roundPanel; holder.resultPanel = result; holder.pausePanel = pause;
            holder.menuBriefing = briefing; holder.menuNotice = notice; holder.guidedButton = guided; holder.evaluationButton = eval;
            holder.objectiveText = objective; holder.checklistText = checklist; holder.statusText = status; holder.toastText = toast;
            holder.hintButton = hint; holder.checkButton = check; holder.pauseButton = pauseBtn; holder.returnItemsButton = ret;
            holder.resultTitle = rTitle; holder.resultBody = rBody; holder.retryButton = retry; holder.otherModeButton = other; holder.otherModeLabel = rl2; holder.resultMenuButton = rmenu;
            holder.pauseNotice = pNotice; holder.resumeButton = resume; holder.restartButton = restart; holder.restartLabel = pl2;
            holder.quitButton = quit; holder.quitLabel = pl3; holder.seatedButton = seated; holder.seatedLabel = pl4;
            holder.heightUpButton = hUp; holder.heightDownButton = hDown;
            holder.boardTimer = timerText; holder.wristTimer = wristText;
            return ui;
        }

        // ------------------------------------------------------------------ wiring

        static void WireSystems(NpcRefs npc, ZoneRefs zones, SplintItem[] items, UiRefs ui)
        {
            var sys = ui.systems;
            var h = ui;

            var timer = sys.AddComponent<RoundTimer>();
            timer.outputs = new[] { h.boardTimer, h.wristTimer };

            var build = sys.AddComponent<SplintBuildController>();
            build.items = items;
            build.prepZone = zones.prep; build.supportZone = zones.support; build.paddingZone = zones.padding;
            build.strapUpperZone = zones.strapUpper; build.strapLowerZone = zones.strapLower;
            build.timer = timer;

            var tracking = sys.AddComponent<TrackingGuard>();
            var comfort = sys.AddComponent<ComfortSettings>();

            var flow = sys.AddComponent<DemoFlow>();
            flow.build = build; flow.timer = timer; flow.npc = npc.reaction; flow.tracking = tracking; flow.comfort = comfort;
            flow.menuPanel = h.menuPanel; flow.roundPanel = h.roundPanel; flow.resultPanel = h.resultPanel; flow.pausePanel = h.pausePanel;
            flow.menuBriefing = h.menuBriefing; flow.menuNotice = h.menuNotice; flow.guidedButton = h.guidedButton; flow.evaluationButton = h.evaluationButton;
            flow.objectiveText = h.objectiveText; flow.checklistText = h.checklistText; flow.statusText = h.statusText; flow.toastText = h.toastText;
            flow.hintButton = h.hintButton; flow.checkButton = h.checkButton; flow.pauseButton = h.pauseButton; flow.returnItemsButton = h.returnItemsButton;
            flow.resultTitle = h.resultTitle; flow.resultBody = h.resultBody; flow.retryButton = h.retryButton;
            flow.otherModeButton = h.otherModeButton; flow.otherModeLabel = h.otherModeLabel; flow.resultMenuButton = h.resultMenuButton;
            flow.pauseNotice = h.pauseNotice; flow.resumeButton = h.resumeButton; flow.restartButton = h.restartButton; flow.restartLabel = h.restartLabel;
            flow.quitButton = h.quitButton; flow.quitLabel = h.quitLabel; flow.seatedButton = h.seatedButton; flow.seatedLabel = h.seatedLabel;
            flow.heightUpButton = h.heightUpButton; flow.heightDownButton = h.heightDownButton;

        }

    }
}
