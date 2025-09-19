using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WaterFall : MonoBehaviour
{
    [Header("Collision Layers")]
    [Tooltip("The layers that will be treated as liquid. Force will be applied on these objects by the waterfall.")]
    [SerializeField] LayerMask _waterLayers;
    [Tooltip("The layers that will be treated as solid objects. These objects will block the flow of water from the waterfall.")]
    [SerializeField] LayerMask _colliders;
    [Tooltip("The amount to offset the collider (in the +Y direction) of the waterfall once its water has been found - sometimes necessary due to the collider's downwards movement to meet the water's surface")]
    [SerializeField] private float _colOffset = 0f;

    [Header("Particles")]
    [SerializeField] private ParticleSystem _part;
    [Tooltip("Determines how many raycasts will be used in particle placement. A higher number will yield more \"precise\" behavior, but may decrease performance. Multiplied by the waterfall's width.")]
    [SerializeField] private float _castMult = 20f;
    [Tooltip("The offset at which raycasts will begin from the top of the waterfall - adjust to avoid colliding with any overlapping foreground elements.")]
    [SerializeField] private float _offset = 1f;
    [Tooltip("The radius of the particles spawned from the bottom of the waterfall")]
    [SerializeField] private float _particleRadius = 0.5f;
    [Tooltip("Determines how much the particles offsets itself from the bottom, as a scale factor on radius")]
    [SerializeField] private float _landOffset = 0.9f;
    [Tooltip("Determines how many particles spawn per raycast")]
    [SerializeField] private int _particleInterval = 2;

    [Header("Force")]
    [Tooltip("The force that the waterfall will apply (in the upward direction) on the affected water objects at every interval")]
    public float _force = 5f;
    [Tooltip("The interval at which force will be applied on the affected water objects. A negative value will cause force to be applied at every FixedUpdate interval (this is bad).")]
    public float _tickTime = 0.5f;

    [Header("Water")]
    [Tooltip("Determines whether the water cutoff mask smooths itself over height points")]
    [SerializeField] private bool _smooths = true;
    [Tooltip("Determines whether the water cutoff mask attempts to clip itself halfway inside particles")]
    [SerializeField] private bool _clipsInParticles = true;

    [Header("Miscellaneous")]
    [Tooltip("Threshold for when the player should be killed - increase to allow more liquid to hit the player without killing them")]
    [SerializeField] private int _killThreshold = 5;

    private Collider2D _col;
    private float _timer;
    private InteractableWater _water;
    private Material _mat;
    private int _numCasts; // How many raycasts we will use to determine particle spawn positions - determined by waterfall width and _castMult
    private ParticleSystem[] _parts;
    private Vector2 _offsetVector; // To prevent repeatedly assigning memory for the same vector
    public bool isAnimating = false;
    public float minHeight = float.NegativeInfinity;

    private void Start()
    {
        _col = GetComponent<Collider2D>();
        _timer = 0f;
        _mat = transform.parent.GetComponent<SpriteRenderer>().material; // TODO: should probably amend this at some point to allow for use of alternate renderers (?)
        _numCasts = (int)(_castMult * _col.bounds.extents.x);
        _parts = new ParticleSystem[_numCasts];
        _offsetVector = new Vector2(0f, _colOffset);
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
                _col.offset = _offsetVector;
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
        float startX = _col.bounds.center.x - _col.bounds.extents.x - interval;
        float yTop = _col.bounds.center.y + _col.bounds.extents.y;
        if (isAnimating)
            yTop = Mathf.Max(minHeight, yTop);
        float yBot = _col.bounds.center.y - _col.bounds.extents.y;

        _numCasts = (int)(_castMult * _col.bounds.extents.x);
        _particleInterval = Mathf.Max(_particleInterval, 1);
        if (_parts.Length != Mathf.FloorToInt(_numCasts / _particleInterval))
        {
            _parts = new ParticleSystem[Mathf.FloorToInt(_numCasts / _particleInterval)];
        }

        // Define texture for water crop data
        Texture2D objects = new(width: Mathf.Max(1,_numCasts+3), height: 2, textureFormat: TextureFormat.RGBAFloat, mipCount: 0, false);
        
        int hitCounter = 0;
        // Raycast _numCasts times, evenly distributed among the top of the waterfall, and spawn the particle system at each hit
        for (int i = 0, p = 0; i < _numCasts + 3; i++)
        {
            // Raycast downward from the current point on the top
            Vector2 start = new(startX + interval * i, yTop);
            RaycastHit2D hit = Physics2D.Raycast(start, Vector2.down, -3 * yBot, _waterLayers | _colliders);
            if (hit)
            {
                // Waterfalls kill the player on collision - if raycast hits the player, kill them >:)
                if (hit.collider.gameObject.CompareTag("Player") && hit.collider.gameObject.name != "LandCheck" && !PlayerVars.instance.cheatMode)
                {
                    hitCounter++;
                    if (hitCounter >= _killThreshold && !GameManager.resetting)
                        GameManager.instance.ResetGame();
                    hit = Physics2D.Raycast(start, Vector2.down, -3 * yBot, (_waterLayers | _colliders) & ~(1 << LayerMask.NameToLayer("Player")));
                }

                // Increase separate counter for particles
                if (i % Mathf.Max(1, _particleInterval) == 0)
                {
                    // Only spawn if the previous particle has died and it's below the offset point
                    if (p < _parts.Length && _parts[p] == null && hit.point.y < yTop - _offset)
                    {
                        _parts[p] = Instantiate(_part, hit.point, Quaternion.identity);
                        ParticleSystem.MainModule main = _parts[p].GetComponent<ParticleSystem>().main;
                        main.startSizeMultiplier = _particleRadius * 2;
                        if ((_waterLayers & (1 << hit.collider.gameObject.layer)) == 0) // Only offset if not in water layer
                            _parts[p].transform.position += _landOffset * _particleRadius * Vector3.up;
                    }
                    p++;
                }

                // Reuse raycast values for water crop positions
                Vector3 yPos = (Vector3)(hit.point) - transform.position;
                if (_clipsInParticles && hit.point.y < yTop - _offset && (_waterLayers & (1 << hit.collider.gameObject.layer)) == 0)
                    yPos += _landOffset * _particleRadius * Vector3.up;

                // Encode y position as color channels in pixels of a Texture2D
                // R = integer, G = decimal, B = sign
                Color obj_info = new(Mathf.Floor(Mathf.Abs(yPos.y)) / 255, Mathf.Abs(yPos.y) % 1, Mathf.Clamp01(Mathf.Sign((yPos.y) / 255f)));
                objects.SetPixel(i, 0, obj_info);
            }
            else if (isAnimating)
            {
                Color obj_info = new(Mathf.Floor(Mathf.Abs(yBot)) / 255, Mathf.Abs(yBot) % 1, Mathf.Clamp01(Mathf.Sign((yBot) / 255f)));
                objects.SetPixel(i, 0, obj_info);
            }
        }

        // Update water crop
        objects.Apply();
        _mat.SetTexture("_ObjArray", objects);
        _mat.SetFloat("_NumObjs", _numCasts+1);
        _mat.SetFloat("_StartX", startX);
        _mat.SetFloat("_Interval", interval);
        _mat.SetFloat("_WorldY", transform.position.y);
        _mat.SetInt("_Smooths", _smooths ? 1 : 0);
    }
}
