using UnityEngine;
using UnityEditor;

public class AddParticleColliders
{
    [MenuItem("Tools/Add Particle Colliders To Children")]
    static void AddColliders()
    {
        GameObject root = Selection.activeGameObject;

        if (root == null)
        {
            Debug.LogError("Select the parent ground object first.");
            return;
        }

        int count = 0;

        foreach (Transform t in root.GetComponentsInChildren<Transform>())
        {
            var col2d = t.GetComponent<BoxCollider2D>();

            if (col2d != null)
            {
                // create child object
                GameObject child = new GameObject("ParticleCollider");
                child.transform.SetParent(t);
                child.transform.localPosition = Vector3.zero;
                child.transform.localRotation = Quaternion.identity;
                child.transform.localScale = Vector3.one;

                var col3d = child.AddComponent<BoxCollider>();

                col3d.size = new Vector3(col2d.size.x, col2d.size.y, 1f);
                col3d.center = new Vector3(col2d.offset.x, col2d.offset.y, 0f);

                child.layer = LayerMask.NameToLayer("ParticleGround");

                count++;
            }
        }

        Debug.Log($"Added {count} particle colliders.");
    }
}