using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace SplintIt.EditorTools
{
    /// <summary>
    /// Menu: SplintIt > Build Hiking Demo Scene
    /// Builds the whole demo scene from code: colourful low-poly landscape, GRAY placeholder NPC, items,
    /// placement zones, VR UI boards and all component wiring. Re-run any time to regenerate the scene.
    /// Layout numbers are constants below (1 unit = 1 m): tune them after testing reach in the headset.
    /// </summary>
    public static partial class HikingDemoSceneBuilder
    {
        const string Root = "Assets/SplintItDemo";
        static readonly Vector3 PlayerHead = new Vector3(0f, 1.5f, 0f);

        // ---- layout (world metres; the player stands at the origin facing +Z) ----
        const float TableTop = 0.85f;
        static readonly Vector3 NpcPos = new Vector3(0.26f, 0f, 0.85f);        // NPC faces the player (root is rotated 180 deg)
        static readonly Vector3 ArmLocal = new Vector3(0.26f, 1.0f, 0.15f);    // forearm centre in NPC space
        static readonly Vector2 SuppliesCenter = new Vector2(-0.84f, 0.175f);  // x,z
        static readonly Vector2 SuppliesSize = new Vector2(0.92f, 0.55f);
        static readonly Vector2 PrepCenter = new Vector2(0.62f, 0.30f);
        static readonly Vector2 PrepSize = new Vector2(0.70f, 0.50f);

        static System.Random rng;
        static readonly Dictionary<string, Material> mats = new Dictionary<string, Material>();
        static readonly Dictionary<string, Mesh> meshes = new Dictionary<string, Mesh>();

        // ------------------------------------------------------------------ entry point

        [MenuItem("SplintIt/Build Hiking Demo Scene")]
        public static void Build()
        {
            if (TMP_Settings.instance == null)
            {
                EditorUtility.DisplayDialog("SplintIt",
                    "TextMeshPro Essential Resources are missing.\n\nOpen Window > TextMeshPro > Import TMP Essential Resources, then run this menu item again.", "OK");
                return;
            }

            rng = new System.Random(7);
            mats.Clear();
            meshes.Clear();
            Folder(Root + "/Scenes"); Folder(Root + "/Materials"); Folder(Root + "/Meshes");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var env = new GameObject("Environment").transform;
            BuildLighting(env);
            BuildLandscape(env);
            BuildCamp(env);
            foreach (var t in env.GetComponentsInChildren<Transform>())
                GameObjectUtility.SetStaticEditorFlags(t.gameObject, StaticEditorFlags.BatchingStatic);

            var props = new GameObject("Props").transform;
            BuildTablesAndLog(props);
            var npc = BuildNpc(props);
            var zones = BuildZones(props, npc.root);
            var items = BuildItems(props);

            var rig = EnsureRig();
            var ui = BuildUi(rig);
            WireSystems(npc, zones, items, ui);

            string scenePath = Root + "/Scenes/HikingDemo.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            AddToBuildSettings(scenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[SplintIt] Scene built: " + scenePath + (rig == null ? "  (NO XR ORIGIN FOUND: add one via GameObject > XR > XR Origin (VR), see SETUP.md)" : ""));
            EditorUtility.DisplayDialog("SplintIt", "Hiking demo scene built:\n" + scenePath + "\n\nPress Play (use the XR Device Simulator if you have no headset).", "OK");
        }

        // ------------------------------------------------------------------ helpers

        static void Folder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            Folder(parent);
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(path));
        }

        static float Rf(float a, float b) { return a + (float)rng.NextDouble() * (b - a); }
        static Color H(string hex) { Color c; ColorUtility.TryParseHtmlString(hex, out c); return c; }

        static Shader LitShader { get { return Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"); } }
        static Shader UnlitShader { get { return Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color"); } }

        static Material Mat(string name, Shader shader, Color c, float smooth)
        {
            Material m;
            if (mats.TryGetValue(name, out m)) return m;
            string path = Root + "/Materials/" + name + ".mat";
            m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null) { m = new Material(shader); AssetDatabase.CreateAsset(m, path); }
            m.shader = shader;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            if (m.HasProperty("_Color")) m.SetColor("_Color", c);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smooth);
            if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", smooth);
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", 0f);
            EditorUtility.SetDirty(m);
            mats[name] = m;
            return m;
        }

        /// <summary>Lit material (soft, matte, colourful).</summary>
        static Material L(string name, string hex, float smooth = 0.12f) { return Mat("L_" + name, LitShader, H(hex), smooth); }
        /// <summary>Unlit material for guidance outlines, flames, markers.</summary>
        static Material U(string name, string hex) { return Mat("U_" + name, UnlitShader, H(hex), 0f); }

        static GameObject Prim(PrimitiveType type, string name, Transform parent, Vector3 localPos, Vector3 scale, Material mat, bool collider = true)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            if (!collider) Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        static GameObject MeshObj(string name, Mesh mesh, Transform parent, Vector3 localPos, Vector3 scale, Material mat, Quaternion rot, bool shadows = true)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = rot;
            go.transform.localScale = scale;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            if (!shadows) r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return go;
        }

        // ---- low-poly meshes: flat-shaded icosahedron blobs and cones ----

        static Mesh Flat(Vector3[] src, int[] tris, Vector3 center, float jitter, int seed)
        {
            var r = new System.Random(seed);
            var v = (Vector3[])src.Clone();
            if (jitter > 0f)
                for (int i = 0; i < v.Length; i++)
                    v[i] += new Vector3((float)r.NextDouble() * 2 - 1, (float)r.NextDouble() * 2 - 1, (float)r.NextDouble() * 2 - 1) * jitter;

            var verts = new List<Vector3>();
            var idx = new List<int>();
            for (int t = 0; t < tris.Length; t += 3)
            {
                Vector3 a = v[tris[t]], b = v[tris[t + 1]], c = v[tris[t + 2]];
                Vector3 n = Vector3.Cross(b - a, c - a);
                if (Vector3.Dot(n, (a + b + c) / 3f - center) < 0f) { Vector3 tmp = b; b = c; c = tmp; }   // make faces point outward
                int s = verts.Count;
                verts.Add(a); verts.Add(b); verts.Add(c);
                idx.Add(s); idx.Add(s + 1); idx.Add(s + 2);
            }
            var m = new Mesh();
            m.SetVertices(verts);
            m.SetTriangles(idx, 0);
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }

        static Mesh Save(string key, Mesh m)
        {
            m.name = key;
            string path = Root + "/Meshes/" + key + ".asset";
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(m, path);
            meshes[key] = m;
            return m;
        }

        static Mesh Ico(int variant)
        {
            string key = "Ico" + variant;
            Mesh m;
            if (meshes.TryGetValue(key, out m)) return m;
            float t = (1f + Mathf.Sqrt(5f)) / 2f;
            var v = new[]
            {
                new Vector3(-1, t, 0), new Vector3(1, t, 0), new Vector3(-1, -t, 0), new Vector3(1, -t, 0),
                new Vector3(0, -1, t), new Vector3(0, 1, t), new Vector3(0, -1, -t), new Vector3(0, 1, -t),
                new Vector3(t, 0, -1), new Vector3(t, 0, 1), new Vector3(-t, 0, -1), new Vector3(-t, 0, 1)
            };
            for (int i = 0; i < v.Length; i++) v[i] = v[i].normalized;
            int[] f =
            {
                0,11,5, 0,5,1, 0,1,7, 0,7,10, 0,10,11, 1,5,9, 5,11,4, 11,10,2, 10,7,6, 7,1,8,
                3,9,4, 3,4,2, 3,2,6, 3,6,8, 3,8,9, 4,9,5, 2,4,11, 6,2,10, 8,6,7, 9,8,1
            };
            return Save(key, Flat(v, f, Vector3.zero, variant == 0 ? 0f : 0.12f, 100 + variant));
        }

        /// <summary>Cone with base on y=0 and apex at y=h. segments=4 gives a pyramid (tent).</summary>
        static Mesh Cone(string key, int seg, float r, float h, float jitter)
        {
            Mesh m;
            if (meshes.TryGetValue(key, out m)) return m;
            var v = new Vector3[seg + 2];
            v[0] = new Vector3(0, h, 0);
            v[1] = Vector3.zero;
            for (int i = 0; i < seg; i++)
            {
                float a = i * Mathf.PI * 2f / seg;
                v[2 + i] = new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r);
            }
            var tris = new List<int>();
            for (int i = 0; i < seg; i++)
            {
                int a = 2 + i, b = 2 + (i + 1) % seg;
                tris.AddRange(new[] { 0, a, b });
                tris.AddRange(new[] { 1, b, a });
            }
            return Save(key, Flat(v, tris.ToArray(), new Vector3(0, h * 0.3f, 0), jitter, key.GetHashCode()));
        }

        static void AddToBuildSettings(string path)
        {
            var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (list.Exists(s => s.path == path)) return;
            list.Insert(0, new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = list.ToArray();
        }

        // ------------------------------------------------------------------ lighting & sky

        static void BuildLighting(Transform parent)
        {
            var go = new GameObject("Sun");
            go.transform.SetParent(parent, false);
            go.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
            var l = go.AddComponent<Light>();
            l.type = LightType.Directional;
            l.color = new Color(1f, 0.95f, 0.82f);   // warm sunlight
            l.intensity = 1.4f;
            l.shadows = LightShadows.Soft;
            RenderSettings.sun = l;

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = H("#9CC8FF");
            RenderSettings.ambientEquatorColor = H("#CFE3B0");
            RenderSettings.ambientGroundColor = H("#7A9A5A");
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = H("#BFE0FF");
            RenderSettings.fogStartDistance = 45f;
            RenderSettings.fogEndDistance = 170f;

            var skyShader = Shader.Find("Skybox/Procedural");
            if (skyShader != null)
            {
                string path = Root + "/Materials/Sky.mat";
                var sky = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (sky == null) { sky = new Material(skyShader); AssetDatabase.CreateAsset(sky, path); }
                sky.shader = skyShader;
                sky.SetColor("_SkyTint", H("#5FA8FF"));
                sky.SetColor("_GroundColor", H("#9CD27A"));
                sky.SetFloat("_Exposure", 1.25f);
                sky.SetFloat("_AtmosphereThickness", 0.9f);
                EditorUtility.SetDirty(sky);
                RenderSettings.skybox = sky;
            }
        }

        // ------------------------------------------------------------------ landscape

        static readonly Vector2[] Trail =
        {
            new Vector2(0f, 2.5f), new Vector2(1.2f, 8f), new Vector2(-2.5f, 15f), new Vector2(-6f, 23f),
            new Vector2(-3.5f, 31f), new Vector2(3f, 39f), new Vector2(9f, 48f), new Vector2(16f, 60f)
        };
        static readonly Vector2 PondCenter = new Vector2(10f, 10f);

        static float DistToTrail(Vector2 p)
        {
            float best = float.MaxValue;
            for (int i = 0; i < Trail.Length - 1; i++)
            {
                Vector2 a = Trail[i], b = Trail[i + 1], ab = b - a;
                float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
                best = Mathf.Min(best, (p - (a + ab * t)).magnitude);
            }
            return best;
        }

        static bool FreeSpot(Vector2 p, float trailClear)
        {
            if (p.magnitude < 8f) return false;                         // keep the play area open
            if (DistToTrail(p) < trailClear) return false;
            if ((p - PondCenter).magnitude < 7f) return false;
            if ((p - new Vector2(-3.4f, 5.2f)).magnitude < 3f) return false;   // campfire
            if ((p - new Vector2(5.5f, 6.5f)).magnitude < 3.5f) return false;  // tent
            if ((p - new Vector2(-6f, 9f)).magnitude < 3f) return false;       // 2nd tent
            return true;
        }

        static Vector2 RandomRing(float rMin, float rMax)
        {
            float a = Rf(0f, Mathf.PI * 2f), d = Rf(rMin, rMax);
            return new Vector2(Mathf.Cos(a) * d, Mathf.Sin(a) * d);
        }

        static void BuildLandscape(Transform env)
        {
            Prim(PrimitiveType.Cube, "Ground", env, new Vector3(0, -0.1f, 0), new Vector3(260, 0.2f, 260), L("Grass", "#6FCF4A"));

            // meadow colour patches (flat discs, slightly stacked to avoid z-fighting)
            string[] patch = { "#58C23F", "#9BDB3F", "#43B15A", "#C4E64F" };
            for (int i = 0; i < 20; i++)
            {
                Vector2 p = RandomRing(4f, 50f);
                float s = Rf(4f, 13f);
                Prim(PrimitiveType.Cylinder, "Patch", env, new Vector3(p.x, 0.004f + i * 0.0015f, p.y), new Vector3(s, 0.002f, s), L("Patch" + (i % 4), patch[i % 4]), false);
            }

            // earthy clearing under the player + winding trail
            Prim(PrimitiveType.Cylinder, "Clearing", env, new Vector3(0, 0.025f, 0.45f), new Vector3(5.4f, 0.002f, 5.4f), L("Clearing", "#E8CF9B"), false);
            for (int i = 0; i < Trail.Length - 1; i++)
            {
                Vector2 a = Trail[i], b = Trail[i + 1], d = b - a;
                var seg = Prim(PrimitiveType.Cube, "Trail", env, new Vector3((a.x + b.x) / 2f, 0.03f, (a.y + b.y) / 2f), new Vector3(1.9f, 0.01f, d.magnitude + 0.5f), L("Trail", "#E3B36F"), false);
                seg.transform.rotation = Quaternion.Euler(0, Mathf.Atan2(d.x, d.y) * Mathf.Rad2Deg, 0);
            }

            // pond with sandy shore
            Prim(PrimitiveType.Cylinder, "PondShore", env, new Vector3(PondCenter.x, 0.012f, PondCenter.y), new Vector3(11f, 0.002f, 11f), L("Sand", "#F0DFA0"), false);
            Prim(PrimitiveType.Cylinder, "Pond", env, new Vector3(PondCenter.x, 0.02f, PondCenter.y), new Vector3(9f, 0.002f, 9f), L("Water", "#3DB2F2", 0.85f), false);
            for (int i = 0; i < 6; i++)   // lily pads
            {
                Vector2 o = new Vector2(Rf(-3f, 3f), Rf(-3f, 3f));
                if (o.magnitude > 3.6f) continue;
                Prim(PrimitiveType.Cylinder, "LilyPad", env, new Vector3(PondCenter.x + o.x, 0.03f, PondCenter.y + o.y), new Vector3(0.5f, 0.002f, 0.5f), L("Lily", "#2FBF71"), false);
            }

            // rolling hills (big flat-shaded blobs) in several greens/teals
            string[] hillCols = { "#5DBB63", "#2E9E7A", "#8CCB5E", "#47B36B" };
            for (int i = 0; i < 16; i++)
            {
                Vector2 p = RandomRing(48f, 78f);
                MeshObj("Hill", Ico(1 + i % 3), env, new Vector3(p.x, -2f, p.y), new Vector3(Rf(18, 34), Rf(7, 15), Rf(15, 28)), L("Hill" + (i % 4), hillCols[i % 4], 0.05f), Quaternion.Euler(0, Rf(0, 360), 0));
            }

            // distant violet/blue mountains with snowcaps
            string[] mtCols = { "#7C86D9", "#A07CD0", "#6A9BE0" };
            Mesh cone = Cone("MountainCone", 6, 1f, 1.6f, 0.08f);
            for (int i = 0; i < 9; i++)
            {
                float a = i * Mathf.PI * 2f / 9f + Rf(-0.15f, 0.15f), d = Rf(105f, 125f);
                float H_ = Rf(45f, 75f), R = Rf(24f, 34f);
                var pos = new Vector3(Mathf.Cos(a) * d, -1f, Mathf.Sin(a) * d);
                var mt = new GameObject("Mountain").transform;
                mt.SetParent(env, false); mt.position = pos;
                MeshObj("Rock", cone, mt, Vector3.zero, new Vector3(R, H_ / 1.6f, R), L("Mt" + (i % 3), mtCols[i % 3], 0.05f), Quaternion.identity, false);
                float f = 0.36f;
                MeshObj("Snow", cone, mt, new Vector3(0, H_ * (1f - f) + 0.2f, 0), new Vector3(R * f * 1.03f, H_ / 1.6f * f * 1.03f, R * f * 1.03f), L("Snow", "#FFFFFF", 0.2f), Quaternion.identity, false);
            }

            // fluffy clouds
            for (int i = 0; i < 10; i++)
            {
                Vector2 p = RandomRing(40f, 110f);
                var c = new GameObject("Cloud").transform;
                c.SetParent(env, false); c.position = new Vector3(p.x, Rf(45f, 65f), p.y);
                for (int k = 0; k < 4; k++)
                    MeshObj("Puff", Ico(1 + k % 3), c, new Vector3(Rf(-7, 7), Rf(-1, 2), Rf(-3, 3)), new Vector3(Rf(6, 10), Rf(3, 5), Rf(4, 7)), L("Cloud", "#FFFFFF", 0f), Quaternion.identity, false);
            }

            // trees: pines + round canopies in fresh green, lime, golden, orange and pink
            string[] canopy = { "#9BDB3F", "#56C271", "#FFD84A", "#FF9A2E", "#FF8FB8", "#6FD36B" };
            int placed = 0, guard = 0;
            while (placed < 70 && guard++ < 1000)
            {
                Vector2 p = RandomRing(8f, 42f);
                if (!FreeSpot(p, 2.6f)) continue;
                float s = Rf(0.8f, 1.7f);
                if (rng.NextDouble() < 0.55) Pine(env, new Vector3(p.x, 0, p.y), s, rng.Next(2));
                else RoundTree(env, new Vector3(p.x, 0, p.y), s, canopy[rng.Next(canopy.Length)]);
                placed++;
            }

            // rocks, boulders, mushrooms and flowers
            for (int i = 0; i < 26; i++)
            {
                Vector2 p = RandomRing(6f, 38f);
                if (!FreeSpot(p, 1.8f) && p.magnitude > 8f) continue;
                float s = Rf(0.3f, 1.2f);
                bool moss = rng.NextDouble() < 0.3;
                MeshObj("Rock", Ico(1 + i % 4), env, new Vector3(p.x, s * 0.25f, p.y), new Vector3(s, s * 0.7f, s), L(moss ? "RockMoss" : "RockGray", moss ? "#6E9B6A" : "#A9A5B8", 0.1f), Quaternion.Euler(0, Rf(0, 360), 0));
            }
            for (int i = 0; i < 3; i++)   // big boulders near the clearing
                MeshObj("Boulder", Ico(1 + i), env, new Vector3(-4.5f + i * 0.9f, 0.55f, 3.8f + i * 0.5f), Vector3.one * (1.3f - i * 0.25f), L("RockGray", "#A9A5B8", 0.1f), Quaternion.Euler(0, 40 * i, 0));

            for (int i = 0; i < 14; i++)
            {
                Vector2 p = RandomRing(7f, 30f);
                if (!FreeSpot(p, 1.5f)) continue;
                Prim(PrimitiveType.Cylinder, "MushroomStem", env, new Vector3(p.x, 0.07f, p.y), new Vector3(0.05f, 0.07f, 0.05f), L("MushStem", "#FFF3D6"), false);
                MeshObj("MushroomCap", Ico(1), env, new Vector3(p.x, 0.16f, p.y), new Vector3(0.14f, 0.08f, 0.14f), L("MushCap", "#E8412C"), Quaternion.identity);
            }

            string[] flowerCols = { "#FF4D6D", "#FFD23F", "#FFFFFF", "#B15CFF", "#FF9F1C", "#4CC9F0" };
            for (int i = 0; i < 240; i++)
            {
                Vector2 p = RandomRing(3.2f, 34f);
                if (DistToTrail(p) < 1.1f || (p - PondCenter).magnitude < 5.5f) continue;
                string col = flowerCols[rng.Next(flowerCols.Length)];
                float s = Rf(0.07f, 0.12f);
                MeshObj("Flower", Ico(1), env, new Vector3(p.x, s * 0.8f, p.y), Vector3.one * s, L("Flower" + col, col, 0.2f), Quaternion.identity, false);
            }
        }

        static void Pine(Transform parent, Vector3 pos, float s, int shade)
        {
            var t = new GameObject("Pine").transform;
            t.SetParent(parent, false); t.position = pos;
            Prim(PrimitiveType.Cylinder, "Trunk", t, new Vector3(0, 0.6f * s, 0), new Vector3(0.2f * s, 0.6f * s, 0.2f * s), L("Trunk", "#8B5A33"), false);
            Mesh cone = Cone("PineCone", 7, 1f, 1.6f, 0.05f);
            Material m = L(shade == 0 ? "Pine1" : "Pine2", shade == 0 ? "#1F8A4C" : "#2FA35A", 0.05f);
            MeshObj("Tier1", cone, t, new Vector3(0, 0.9f * s, 0), Vector3.one * 1.15f * s, m, Quaternion.identity);
            MeshObj("Tier2", cone, t, new Vector3(0, 1.7f * s, 0), Vector3.one * 0.85f * s, m, Quaternion.identity);
            MeshObj("Tier3", cone, t, new Vector3(0, 2.4f * s, 0), Vector3.one * 0.6f * s, m, Quaternion.identity);
        }

        static void RoundTree(Transform parent, Vector3 pos, float s, string canopyHex)
        {
            var t = new GameObject("RoundTree").transform;
            t.SetParent(parent, false); t.position = pos;
            Prim(PrimitiveType.Cylinder, "Trunk", t, new Vector3(0, 0.9f * s, 0), new Vector3(0.25f * s, 0.9f * s, 0.25f * s), L("Trunk", "#8B5A33"), false);
            MeshObj("Canopy", Ico(1 + rng.Next(3)), t, new Vector3(0, 2.3f * s, 0), new Vector3(1.5f * s, 1.25f * s, 1.5f * s), L("Canopy" + canopyHex, canopyHex, 0.08f), Quaternion.Euler(0, Rf(0, 360), 0));
        }

        // ------------------------------------------------------------------ camp props

        static void BuildCamp(Transform env)
        {
            // tents
            Mesh pyramid = Cone("TentPyramid", 4, 1f, 1.6f, 0f);
            MeshObj("Tent", pyramid, env, new Vector3(5.5f, 0, 6.5f), new Vector3(1.5f, 0.95f, 2.1f), L("TentOrange", "#FF7A3D"), Quaternion.Euler(0, 20, 0));
            MeshObj("Tent2", pyramid, env, new Vector3(-6f, 0, 9f), new Vector3(1.3f, 0.85f, 1.8f), L("TentTeal", "#2EC4B6"), Quaternion.Euler(0, -35, 0));

            // campfire: stone ring, crossed logs, flames, warm light
            Vector3 fire = new Vector3(-3.4f, 0, 5.2f);
            for (int i = 0; i < 9; i++)
            {
                float a = i * Mathf.PI * 2f / 9f;
                MeshObj("FireStone", Ico(1 + i % 3), env, fire + new Vector3(Mathf.Cos(a) * 0.55f, 0.1f, Mathf.Sin(a) * 0.55f), new Vector3(0.17f, 0.12f, 0.17f), L("RockGray", "#A9A5B8", 0.1f), Quaternion.Euler(0, i * 40, 0));
            }
            var log1 = Prim(PrimitiveType.Cylinder, "FireLog", env, fire + new Vector3(0, 0.12f, 0), new Vector3(0.12f, 0.4f, 0.12f), L("Log", "#7A4A28"), false);
            log1.transform.rotation = Quaternion.Euler(0, 30, 90);
            var log2 = Prim(PrimitiveType.Cylinder, "FireLog", env, fire + new Vector3(0, 0.2f, 0), new Vector3(0.12f, 0.4f, 0.12f), L("Log", "#7A4A28"), false);
            log2.transform.rotation = Quaternion.Euler(0, -40, 90);
            Mesh flame = Cone("FlameCone", 5, 1f, 1.6f, 0.1f);
            MeshObj("Flame", flame, env, fire + new Vector3(0, 0.2f, 0), new Vector3(0.22f, 0.3f, 0.22f), U("FlameOrange", "#FF6A13"), Quaternion.identity, false);
            MeshObj("Flame", flame, env, fire + new Vector3(0.05f, 0.2f, 0.03f), new Vector3(0.13f, 0.22f, 0.13f), U("FlameYellow", "#FFD23F"), Quaternion.identity, false);
            var lightGo = new GameObject("FireLight");
            lightGo.transform.SetParent(env, false); lightGo.transform.position = fire + new Vector3(0, 0.7f, 0);
            var pl = lightGo.AddComponent<Light>();
            pl.type = LightType.Point; pl.color = H("#FFA347"); pl.intensity = 2.2f; pl.range = 7f;

            // tree stumps to sit on
            for (int i = 0; i < 3; i++)
            {
                Vector3 p = fire + new Vector3(-1.2f + i * 1.3f, 0, -1.3f - (i % 2) * 0.4f);
                Prim(PrimitiveType.Cylinder, "Stump", env, p + new Vector3(0, 0.2f, 0), new Vector3(0.5f, 0.2f, 0.5f), L("Stump", "#B97D4B"), false);
                Prim(PrimitiveType.Cylinder, "StumpTop", env, p + new Vector3(0, 0.401f, 0), new Vector3(0.44f, 0.002f, 0.44f), L("StumpTop", "#E6C48F"), false);
            }

            // trail signpost with two coloured arrow boards
            Prim(PrimitiveType.Cylinder, "SignPost", env, new Vector3(3f, 0.85f, 3.4f), new Vector3(0.09f, 0.85f, 0.09f), L("Wood", "#B97D4B"), false);
            var b1 = Prim(PrimitiveType.Cube, "SignBoard", env, new Vector3(3f, 1.5f, 3.4f), new Vector3(0.75f, 0.2f, 0.04f), L("SignRed", "#E8523B"), false);
            b1.transform.rotation = Quaternion.Euler(0, 20, 0);
            var b2 = Prim(PrimitiveType.Cube, "SignBoard", env, new Vector3(3f, 1.2f, 3.4f), new Vector3(0.65f, 0.2f, 0.04f), L("SignBlue", "#2F8DE0"), false);
            b2.transform.rotation = Quaternion.Euler(0, -15, 0);
        }

        // ------------------------------------------------------------------ XR rig

        static XROrigin EnsureRig()
        {
            var rig = Object.FindFirstObjectByType<XROrigin>();
            if (rig == null) { EditorApplication.ExecuteMenuItem("GameObject/XR/XR Origin (VR)"); rig = Object.FindFirstObjectByType<XROrigin>(); }
            if (rig == null) { EditorApplication.ExecuteMenuItem("GameObject/XR/XR Origin (Action-based)"); rig = Object.FindFirstObjectByType<XROrigin>(); }

            if (rig != null) rig.transform.position = Vector3.zero;
            else
            {
                Debug.LogWarning("[SplintIt] Could not create an XR Origin automatically. A fallback camera was added; add GameObject > XR > XR Origin (VR) yourself (see SETUP.md).");
                var cam = new GameObject("Fallback Camera", typeof(Camera), typeof(AudioListener));
                cam.tag = "MainCamera";
                cam.transform.position = new Vector3(0, 1.6f, 0);
            }

            if (Object.FindFirstObjectByType<XRInteractionManager>() == null)
                new GameObject("XR Interaction Manager").AddComponent<XRInteractionManager>();

            if (Object.FindFirstObjectByType<EventSystem>() == null) EditorApplication.ExecuteMenuItem("GameObject/XR/UI EventSystem");
            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<XRUIInputModule>();
            }
            return rig;
        }
    }
}
