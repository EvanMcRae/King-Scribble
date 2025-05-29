using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WaterFall : MonoBehaviour
{
    [Header("Collision Layers")]
    [Tooltip("The layers that will be treated as liquid. Force will be applied on these objects by the waterfall.")]
    [SerializeField] LayerMask _waterLayers;
    [Tooltip("The layers that will be treated as solid objects. These objects will block the flow of water from the waterfall.")]
    [SerializeField] LayerMask _colliders;

    [Header("Particles")]
    [SerializeField] private ParticleSystem _part;
    [Tooltip("Determines how many raycasts will be used in particle placement. A higher number will yield more \"precise\" behavior, but may decrease performance. Multiplied by the waterfall's width.")]
    [SerializeField] private float _castMult = 10f;
    [Tooltip("The rate at which particles will be spawned. Increasing this will decrease performance.")]
    [SerializeField] private float _spawnRate = 1f;
    [Tooltip("The offset at which raycasts will begin from the top of the waterfall - adjust to avoid colliding with any overlapping foreground elements.")]
    [SerializeField] private float _offset = 1f;
    [Tooltip("The radius of the particles spawned from the bottom of the waterfall")]
    [SerializeField] private float _particleRadius = 0.5f;
    [Tooltip("Determines how much the particles offsets itself from the bottom, as a scale factor on radius")]
    [SerializeField] private float _landOffset = 0.9f;
    [Tooltip("Determines whether the water cutoff mask attempts to clip itself halfway inside particles")]
    [SerializeField] private bool _clipWaterInParticles = true;

    [Header("Force")]
    [Tooltip("The force that the waterfall will apply (in the upward direction) on the affected water objects at every interval")]
    public float _force = 5f;
    [Tooltip("The interval at which force will be applied on the affected water objects. A negative value will cause force to be applied at every FixedUpdate interval (this is bad).")]
    public float _tickTime = 0.5f;

    private Collider2D _col;
    private float _timer;
    private InteractableWater _water;
    private Material _mat;
    private int _numCasts; // How many raycasts we will use to determine particle spawn positions - determined by waterfall width and _castMult
    private ParticleSystem[] _parts;

    private void Start()
    {
        _col = GetComponent<Collider2D>();
        _timer = 0f;
        _mat = transform.parent.GetComponent<SpriteRenderer>().material; // TODO: should probably amend this at some point to allow for use of alternate renderers (?)
        _numCasts = (int)(_castMult * _col.bounds.extents.x);
        _parts = new ParticleSystem[_numCasts];
    }

    private void FixedUpdate()
    {
        // Apply force to the water if it is unblocked
        _timer -= Time.fixedDeltaTime; // Timer is used to ensure that force does not grow out of control - must be applied at a rate close to that at which gravity counteracts it
        if (_timer > 0f) { return; }
        Vector2 top = new(_col.bounds.center.x, _col.bounds.center.y + _col.bounds.extents.y);
        RaycastHit2D hit = Physics2D.Raycast(top, Vector2.down, -3 * (_col.bounds.center.y - _col.bounds.extents.y), _waterLayers | _colliders); // Don't ask about the -3. Please.
        if (hit)
        {
            if (hit.collider.gameObject.TryGetComponent(out _water) || (hit.collider.transform.parent != null && hit.collider.transform.parent.TryGetComponent(out _water)))
            {
                _col.transform.position = hit.point; // This could probably be made less obtuse - the current implementation of the water takes the collider itself and extracts its position
                float vel = _force * _water.ForceMultiplier; // Therefore, the only way to ensure that the force is applied where we want it is to move the waterfall's actual collider to that position
                vel = Mathf.Clamp(Mathf.Abs(vel), 0f, _water.MaxForce); // The water itself could likely be rewritten so that this is not necessary, but for now it is a non-issue
                _water.Splash(_col, vel);
            }
        }
        _timer = _tickTime;
    }

    public void Update()
    {
        SpawnParticles(); // Particle/shader stuff
    }

    private void SpawnParticles()
    {
        float interval = _col.bounds.extents.x * 2 / _numCasts;
        float startX = _col.bounds.center.x - _col.bounds.extents.x;
        float yTop = _col.bounds.center.y + _col.bounds.extents.y;
        float yBot = _col.bounds.center.y - _col.bounds.extents.y;

        float oldNumCasts = _numCasts;
        _numCasts = (int)(_castMult * _col.bounds.extents.x);
        if (_numCasts != oldNumCasts)
        {
            _parts = new ParticleSystem[_numCasts];
        }

        // Define texture for water crop data
        Texture2D objects = new(width: Mathf.Max(1,_numCasts+1), height: 2, textureFormat: TextureFormat.RGBAFloat, mipCount: 0, false);

        // Raycast _numCasts times, evenly distributed among the top of the waterfall, and spawn the particle system at each hit
        for (int i = 0; i < _numCasts + 1; i++)
        {
            // Raycast downward from the current point on the top
            Vector2 start = new(startX + interval * i, yTop);
            RaycastHit2D hit = Physics2D.Raycast(start, Vector2.down, -3*yBot, _waterLayers | _colliders);
            if (hit)
            {
                // Only spawn if the timer is up and it's below the offset point
                if (i != _numCasts && _parts[i] == null && hit.point.y < yTop - _offset)
                {
                    _parts[i] = Instantiate(_part, hit.point, Quaternion.identity, gameObject.transform);
                    ParticleSystem.MainModule main = _parts[i].GetComponent<ParticleSystem>().main;
                    main.startSizeMultiplier = _particleRadius * 2;
                    _parts[i].transform.position += _landOffset * _particleRadius * Vector3.up;
                }

                // Reuse raycast values for water crop positions
                Vector3 yPos = (Vector3)(hit.point) - transform.position;
                if (_clipWaterInParticles && hit.point.y < yTop - _offset)
                    yPos += _landOffset * _particleRadius * Vector3.up;

                // Encode y position as color channels in pixels of a Texture2D
                // R = center.y integer, G = center.y decimal, B = center.y sign
                Color obj_info = new(Mathf.Floor(Mathf.Abs(yPos.y)) / 255, Mathf.Abs(yPos.y) % 1, Mathf.Clamp01(Mathf.Sign((yPos.y) / 255f)));
                objects.SetPixel(i, 0, obj_info);
            }
        }

        // Update water crop
        objects.Apply();
        _mat.SetTexture("_ObjArray", objects);
        _mat.SetFloat("_NumObjs", _numCasts);
        _mat.SetFloat("_StartX", startX);
        _mat.SetFloat("_Interval", interval);
        _mat.SetFloat("_WorldY", transform.position.y);
    }
}
