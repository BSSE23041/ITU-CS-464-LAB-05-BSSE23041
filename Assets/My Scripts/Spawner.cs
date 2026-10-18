
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject coinPrefab;
    [SerializeField] Transform player;
    [Tooltip("Add floor here")]
    [SerializeField] Transform floor;

    [Header("Normal (gold) coins")]
    [SerializeField] float interval = 1.5f;
    [SerializeField] int maxCoins = 5;
    [Tooltip("Spawn area size if no floor")]
    [SerializeField] float area = 7f;
    [Tooltip("Distance from walls")]
    [SerializeField] float edgeMargin = 1f;
    [SerializeField] float minDistanceFromPlayer = 2f;
    [Tooltip("Coin height")]
    [SerializeField] float heightOffset = 0f;

    [Header("Special coins")]
    [SerializeField] Color redColor = new Color(1f, 0.1f, 0.1f);
    [SerializeField] Color blueColor = new Color(0.1f, 0.35f, 1f);
    [Tooltip("Optional material")]
    [SerializeField] Material redMaterial;
    [SerializeField] Material blueMaterial;
    [SerializeField] float redCoinScale = 1.5f;
    [SerializeField] float bonusRingRadius = 4f;
    [Tooltip("Gap between blue coins")]
    [SerializeField] float bonusCoinSpacing = 0.5f;

    Transform specialParent;
    Material redRuntime, blueRuntime;
    Vector2 arenaMin, arenaMax;

    void Awake()
    {
        specialParent = new GameObject("SpecialCoins").transform;
        ComputeArena();
    }

    void Start() => InvokeRepeating(nameof(SpawnCoin), 1f, interval);

    // spawn normal coins
    void SpawnCoin()
    {
        if (transform.childCount >= maxCoins) return;
        if (!TryGetRandomPoint(out Vector3 pos)) return;
        Create(pos, transform);
    }

    // spawn red coin
    public void SpawnRedCoin()
    {
        if (!TryGetRandomPoint(out Vector3 pos))
            pos = new Vector3((arenaMin.x + arenaMax.x) * 0.5f, SpawnY, (arenaMin.y + arenaMax.y) * 0.5f);

        Coin coin = Create(pos, specialParent);
        coin.name = "RedCoin";
        coin.transform.localScale *= redCoinScale;
        coin.Setup(CoinType.Red, GetMaterial(ref redRuntime, redMaterial, redColor), redColor);
    }

    // spawn blue coins in circle
    public void SpawnBonusRing(int count)
    {
        Vector3 center = player != null ? player.position : transform.position;
        float radius = bonusRingRadius;

        float maxR = Mathf.Min(arenaMax.x - arenaMin.x, arenaMax.y - arenaMin.y) * 0.5f;
        if (maxR > 0.5f)
        {
            radius = Mathf.Min(radius, maxR);
            center.x = Mathf.Clamp(center.x, arenaMin.x + radius, arenaMax.x - radius);
            center.z = Mathf.Clamp(center.z, arenaMin.y + radius, arenaMax.y - radius);
        }

        float y = SpawnY;
        int placed = 0;
        float r = radius;
        float minRing = Mathf.Min(1.5f, radius);

        // make smaller rings if needed
        while (placed < count && r >= minRing)
        {
            int perRing = Mathf.Max(1, Mathf.FloorToInt(2f * Mathf.PI * r / bonusCoinSpacing));
            int n = Mathf.Min(perRing, count - placed);
            float offset = Random.value * Mathf.PI * 2f;
            for (int i = 0; i < n; i++)
            {
                float a = offset + (Mathf.PI * 2f * i / n);
                SpawnBlue(new Vector3(center.x + Mathf.Cos(a) * r, y, center.z + Mathf.Sin(a) * r));
                placed++;
            }
            r -= bonusCoinSpacing;
        }

        while (placed < count)
        {
            Vector2 p = Random.insideUnitCircle * radius;
            SpawnBlue(new Vector3(center.x + p.x, y, center.z + p.y));
            placed++;
        }
    }

    void SpawnBlue(Vector3 pos)
    {
        Coin coin = Create(pos, specialParent);
        coin.name = "BlueCoin";
        coin.Setup(CoinType.Blue, GetMaterial(ref blueRuntime, blueMaterial, blueColor), blueColor);
    }

    float SpawnY => (player != null ? player.position.y : transform.position.y) + heightOffset;

    Coin Create(Vector3 pos, Transform parent)
    {
        GameObject go = Instantiate(coinPrefab, pos, coinPrefab.transform.rotation, parent);
        Coin coin = go.GetComponent<Coin>();
        if (coin == null) coin = go.AddComponent<Coin>();
        return coin;
    }

    bool TryGetRandomPoint(out Vector3 pos)
    {
        for (int i = 0; i < 15; i++)
        {
            float x = Random.Range(arenaMin.x, arenaMax.x);
            float z = Random.Range(arenaMin.y, arenaMax.y);
            pos = new Vector3(x, SpawnY, z);
            if (player == null) return true;
            Vector3 d = pos - player.position;
            d.y = 0f;
            if (d.magnitude >= minDistanceFromPlayer) return true;
        }
        pos = Vector3.zero;
        return false;
    }

    void ComputeArena()
    {
        if (floor != null)
        {
            Renderer[] rends = floor.GetComponentsInChildren<Renderer>();
            if (rends.Length > 0)
            {
                Bounds b = rends[0].bounds;
                for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
                arenaMin = new Vector2(b.min.x + edgeMargin, b.min.z + edgeMargin);
                arenaMax = new Vector2(b.max.x - edgeMargin, b.max.z - edgeMargin);
                if (arenaMax.x < arenaMin.x) arenaMin.x = arenaMax.x = b.center.x;
                if (arenaMax.y < arenaMin.y) arenaMin.y = arenaMax.y = b.center.z;
                return;
            }
        }
        Vector3 c = transform.position;
        arenaMin = new Vector2(c.x - area, c.z - area);
        arenaMax = new Vector2(c.x + area, c.z + area);
    }

    Material GetMaterial(ref Material cache, Material assigned, Color color)
    {
        if (assigned != null) return assigned;
        if (cache != null) return cache;

        string[] shaderNames = { "Universal Render Pipeline/Lit", "Universal Render Pipeline/Simple Lit",
                                 "HDRP/Lit", "Standard", "Sprites/Default" };
        foreach (string n in shaderNames)
        {
            Shader s = Shader.Find(n);
            if (s == null) continue;
            cache = new Material(s);
            cache.color = color;
            if (cache.HasProperty("_BaseColor")) cache.SetColor("_BaseColor", color);
            if (cache.HasProperty("_Color")) cache.SetColor("_Color", color);
            return cache;
        }
        return null;
    }

    // show spawn area in scene
    void OnDrawGizmosSelected()
    {
        ComputeArena();
        Gizmos.color = Color.yellow;
        Vector3 c = new Vector3((arenaMin.x + arenaMax.x) * 0.5f, transform.position.y, (arenaMin.y + arenaMax.y) * 0.5f);
        Gizmos.DrawWireCube(c, new Vector3(arenaMax.x - arenaMin.x, 0.1f, arenaMax.y - arenaMin.y));
    }
}
