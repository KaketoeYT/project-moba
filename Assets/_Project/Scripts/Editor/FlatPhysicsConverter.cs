using System.Collections.Generic;
using System.Text;
using _Project.Scripts.FlatPhysics;
using _Project.Scripts.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Editor-only, run once: MOBA > Convert Unity Physics To Flat Colliders.
// Swaps every Unity collider in the project's prefabs and the open scene for a FlatCollider of the
// same size (spheres/capsules/cylinders -> circles, boxes -> boxes), removes Rigidbodies and
// CharacterControllers, drops ground colliders (nothing needs them now) and turns off Unity's
// physics simulation. Safe to run again: objects without Unity colliders are left alone.
namespace _Project.Scripts.EditorTools
{
    public static class FlatPhysicsConverter
    {
        private const string PrefabFolder = "Assets/_Project";

        [MenuItem("MOBA/Convert Unity Physics To Flat Colliders")]
        private static void Convert()
        {
            if (!EditorUtility.DisplayDialog("Flat physics",
                    "This replaces every Unity collider, Rigidbody and CharacterController in the prefabs under " +
                    PrefabFolder + " and in the open scene with FlatColliders, and turns off Unity's physics.\n\n" +
                    "Prefab changes can't be undone with Ctrl+Z, so commit to git first.",
                    "Convert", "Cancel"))
            {
                return;
            }

            var log = new StringBuilder();

            // Prefabs first, so prefab instances in the scene are already converted when we get there
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { PrefabFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject root = PrefabUtility.LoadPrefabContents(path);

                if (ConvertHierarchy(root, path, log, useUndo: false))
                {
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }

                PrefabUtility.UnloadPrefabContents(root);
            }

            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                var scene = EditorSceneManager.GetSceneAt(i);
                bool changed = false;

                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    changed |= ConvertHierarchy(root, scene.name, log, useUndo: true);
                }

                if (changed)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                }
            }

            // Nothing uses Unity physics any more: stop it from simulating every FixedUpdate
            Physics.simulationMode = SimulationMode.Script;
            AssetDatabase.SaveAssets();

            string summary = log.Length > 0 ? log.ToString() : "Nothing to convert.";
            Debug.Log("Flat physics conversion:\n" + summary);
            EditorUtility.DisplayDialog("Flat physics",
                "Done. Unity physics simulation is now off (Project Settings > Physics > Simulation Mode: Script).\n\n" +
                "Save the scene to keep its changes. Details are in the Console.", "OK");
        }

        private static bool ConvertHierarchy(GameObject root, string source, StringBuilder log, bool useUndo)
        {
            bool changed = false;
            var objects = new HashSet<GameObject>();

            foreach (Collider collider in root.GetComponentsInChildren<Collider>(true))
            {
                objects.Add(collider.gameObject);
            }

            foreach (Rigidbody body in root.GetComponentsInChildren<Rigidbody>(true))
            {
                objects.Add(body.gameObject);
            }

            foreach (GameObject obj in objects)
            {
                changed |= ConvertObject(obj, source, log, useUndo);
            }

            return changed;
        }

        private static bool ConvertObject(GameObject obj, string source, StringBuilder log, bool useUndo)
        {
            Collider[] colliders = obj.GetComponents<Collider>();
            Rigidbody[] bodies = obj.GetComponents<Rigidbody>();

            // Parts of a prefab instance are converted through their prefab, not here
            foreach (Component component in colliders)
            {
                if (IsFromPrefab(component))
                {
                    return false;
                }
            }

            // Characters (things that walk or have health) get a body that doesn't block others,
            // so characters can pass each other like in most MOBAs
            bool isCharacter = obj.GetComponent<CharacterController>() != null ||
                               obj.GetComponent<PlayerMovement>() != null ||
                               obj.GetComponent<PlayerStats>() != null;

            // CharacterController first: it is the real body of a character
            Collider mainCollider = obj.GetComponent<CharacterController>();
            if (mainCollider == null && colliders.Length > 0)
            {
                mainCollider = colliders[0];
            }

            string what = $"{source}: {GetPath(obj)}";

            if (mainCollider != null && obj.GetComponent<FlatCollider>() == null)
            {
                if (TryDescribe(mainCollider, out FlatShape shape, out float radius, out Vector2 size, out Vector2 offset))
                {
                    bool trigger = mainCollider.isTrigger;
                    FlatCollider flat = useUndo ? Undo.AddComponent<FlatCollider>(obj) : obj.AddComponent<FlatCollider>();
                    flat.Configure(shape, radius, size, offset, trigger, blocks: !trigger && !isCharacter);

                    string kind = trigger ? "zone" : isCharacter ? "character" : "solid";
                    string dims = shape == FlatShape.Circle ? $"radius {radius:0.##}" : $"size {size.x:0.##} x {size.y:0.##}";
                    log.AppendLine($"  {what}: {mainCollider.GetType().Name} -> {shape} {dims} ({kind})");
                }
                else
                {
                    log.AppendLine($"  {what}: {mainCollider.GetType().Name} removed (flat ground, not needed)");
                }
            }

            foreach (Collider collider in colliders)
            {
                Remove(collider, useUndo);
            }

            foreach (Rigidbody body in bodies)
            {
                if (!IsFromPrefab(body))
                {
                    Remove(body, useUndo);
                    log.AppendLine($"  {what}: Rigidbody removed");
                }
            }

            return true;
        }

        // The flat shape matching a Unity collider. False for flat ground (planes), which needs nothing.
        private static bool TryDescribe(Collider collider, out FlatShape shape, out float radius, out Vector2 size, out Vector2 offset)
        {
            shape = FlatShape.Circle;
            radius = 0.5f;
            size = Vector2.one;
            offset = Vector2.zero;

            switch (collider)
            {
                case CharacterController controller:
                    radius = controller.radius;
                    offset = Flat(controller.center);
                    return true;

                case SphereCollider sphere:
                    radius = sphere.radius;
                    offset = Flat(sphere.center);
                    return true;

                case CapsuleCollider capsule:
                    radius = capsule.radius;
                    offset = Flat(capsule.center);
                    return true;

                case BoxCollider box:
                    shape = FlatShape.Box;
                    size = Flat(box.size);
                    offset = Flat(box.center);
                    return true;

                case MeshCollider meshCollider:
                    Mesh mesh = meshCollider.sharedMesh;
                    if (mesh == null || mesh.bounds.size.y < 0.01f)
                    {
                        return false;
                    }

                    Bounds bounds = mesh.bounds;
                    offset = Flat(bounds.center);
                    string meshName = mesh.name.ToLowerInvariant();

                    if (meshName.Contains("cylinder") || meshName.Contains("sphere") || meshName.Contains("capsule"))
                    {
                        radius = Mathf.Max(bounds.extents.x, bounds.extents.z);
                    }
                    else
                    {
                        shape = FlatShape.Box;
                        size = Flat(bounds.size);
                    }

                    return true;

                default:
                    // Terrain and anything else: treat as ground
                    return false;
            }
        }

        private static Vector2 Flat(Vector3 value)
        {
            return new Vector2(value.x, value.z);
        }

        private static bool IsFromPrefab(Component component)
        {
            return PrefabUtility.IsPartOfPrefabInstance(component) && !PrefabUtility.IsAddedComponentOverride(component);
        }

        private static void Remove(Component component, bool useUndo)
        {
            if (useUndo)
            {
                Undo.DestroyObjectImmediate(component);
            }
            else
            {
                Object.DestroyImmediate(component, true);
            }
        }

        private static string GetPath(GameObject obj)
        {
            string path = obj.name;
            for (Transform parent = obj.transform.parent; parent != null; parent = parent.parent)
            {
                path = parent.name + "/" + path;
            }

            return path;
        }
    }
}
