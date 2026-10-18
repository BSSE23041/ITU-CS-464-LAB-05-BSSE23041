using UnityEngine;

public enum CoinType { Normal, Red, Blue }

// Parts 2 + 3 of the lecture: a trigger that checks who touched it, reports to the
// GameManager and removes itself. One script is used for gold, red AND blue coins.
public class Coin : MonoBehaviour
{
    [SerializeField] int value = 1;            // only used by Normal (gold) coins
    [SerializeField] float spinSpeed = 90f;

    public CoinType Type { get; private set; } = CoinType.Normal;
    bool collected;

    void Awake()
    {
        // --- safety net: make sure the coin can actually be picked up -------------
        Collider[] cols = GetComponentsInChildren<Collider>();
        if (cols.Length == 0)
        {
            Debug.LogWarning("Coin prefab has no Collider - add one with Is Trigger ticked. Adding a temporary one.");
            BoxCollider box = gameObject.AddComponent<BoxCollider>();
            Renderer[] rends = GetComponentsInChildren<Renderer>();
            if (rends.Length > 0)
            {
                Bounds b = rends[0].bounds;
                for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
                Vector3 s = transform.lossyScale;
                box.center = transform.InverseTransformPoint(b.center);
                box.size = new Vector3(b.size.x / s.x, b.size.y / s.y, b.size.z / s.z);
            }
            cols = new Collider[] { box };
        }
        foreach (Collider c in cols)
        {
            if (c is MeshCollider mc) mc.convex = true;   // concave mesh colliders can't be triggers
            c.isTrigger = true;
        }

        // A kinematic Rigidbody on the root makes sure the root receives OnTriggerEnter
        // even if the collider sits on a child object.
        if (!TryGetComponent(out Rigidbody rb)) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    void Update() => transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Coin touched by {other.name}");   // before any if: proves the trigger fired
        if (collected) return;
        if (other.CompareTag("Player"))
        {
            collected = true;
            GameManager.Instance.OnCoinCollected(Type, value);
            Destroy(gameObject);                      // gameObject, NOT "this"
        }
    }

    // Called by the Spawner right after Instantiate.
    // 'material' (if given) replaces every material on the coin, so the colour is a pure
    // red/blue instead of being multiplied with the gold texture.
    public void Setup(CoinType type, Material material, Color color)
    {
        Type = type;
        if (type == CoinType.Normal) return;       // gold coins keep the prefab's own look
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            if (material != null)
            {
                Material[] mats = new Material[r.sharedMaterials.Length];
                for (int i = 0; i < mats.Length; i++) mats[i] = material;
                r.sharedMaterials = mats;
            }
            else
            {
                foreach (Material m in r.materials)    // per-renderer copies
                {
                    if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
                    if (m.HasProperty("_Color")) m.SetColor("_Color", color);
                }
            }
        }
    }
}