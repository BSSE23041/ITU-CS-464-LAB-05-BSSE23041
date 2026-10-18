
using UnityEngine;

public enum CoinType { Normal, Red, Blue }

public class Coin : MonoBehaviour
{
    [SerializeField] int value = 1;
    [SerializeField] float spinSpeed = 90f;

    public CoinType Type { get; private set; } = CoinType.Normal;
    bool collected;

    void Awake()
    {
        // check coin collider
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
            if (c is MeshCollider mc) mc.convex = true;
            c.isTrigger = true;
        }

        if (!TryGetComponent(out Rigidbody rb)) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    void Update() => transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Coin touched by {other.name}");
        if (collected) return;
        if (other.CompareTag("Player"))
        {
            collected = true;
            GameManager.Instance.OnCoinCollected(Type, value);
            Destroy(gameObject);
        }
    }

    // set coin type and color
    public void Setup(CoinType type, Material material, Color color)
    {
        Type = type;
        if (type == CoinType.Normal) return;
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
                foreach (Material m in r.materials)
                {
                    if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
                    if (m.HasProperty("_Color")) m.SetColor("_Color", color);
                }
            }
        }
    }
}
