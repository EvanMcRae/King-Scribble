using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.Rendering;

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

    [Header("Force")]
    [Tooltip("The force that the waterfall will apply (in the upward direction) on the affected water objects at every interval")]
    public float _force = 5f;
    [Tooltip("The interval at which force will be applied on the affected water objects. A negative value will cause force to be applied at every FixedUpdate interval (this is bad).")]
    public float _tickTime = 0.5f;

    private Collider2D _col;
    private float _timer;
    private InteractableWater _water;
    private Material _mat;
    private ParticleSystem _curPart;
    private int _obj_counter = 0;
    private List<Collider2D> _objects;
    private int _numCasts; // How many raycasts we will use to determine particle spawn positions - determined by waterfall width and _castMult
    private float[] _castTimers; // Array of timers for each raycast - particles may only be spawned if the raycast's corresponding timer is at 0
    private ParticleSystem[] _parts;

    private void Start()
    {
        _col = GetComponent<Collider2D>();
        _timer = 0f;
        _mat = transform.parent.GetComponent<SpriteRenderer>().material; // TODO: should probably amend this at some point to allow for use of alternate renderers (?)
        _objects = new List<Collider2D>();
        _numCasts = (int)(_castMult * _col.bounds.extents.x);
        _castTimers = new float[_numCasts];
        _parts = new ParticleSystem[_numCasts];
        for (int i = 0; i < _numCasts; i++)
        {
            _castTimers[i] = 0f;
        }
    }

    private void FixedUpdate()
    {
        // Apply force to the water if it is unblocked
        _timer -= Time.fixedDeltaTime; // Timer is used to ensure that force does not grow out of control - must be applied at a rate close to that at which gravity counteracts it
        if (_timer > 0f) { return; }
        Vector2 top = new Vector2(_col.bounds.center.x, _col.bounds.center.y + _col.bounds.extents.y);
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
        UpdateCrop(); // Shader stuff
        SpawnParticles(); // Particle stuff
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((_colliders.value & (1 << collision.gameObject.layer)) > 0)
        {
            _obj_counter++;
            _objects.Add(collision);
        }

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if ((_colliders.value & (1 << collision.gameObject.layer)) > 0)
        {
            _obj_counter--;
            _objects.Remove(collision);
        }

        if (_obj_counter == 0)
        {
            Texture2D objects = new(width: 1, height: 3, textureFormat: TextureFormat.RGBAFloat, mipCount: 0, false);
            _mat.SetTexture("_ObjArray", objects);
            _mat.SetFloat("_NumObjs", _obj_counter);
        }
    }

    private void UpdateCrop()
    {
        if (_obj_counter == 0) return;
        Texture2D objects = new(width: _obj_counter, height: 3, textureFormat: TextureFormat.RGBAFloat, mipCount: 0, false);

        for (int i = 0; i < _obj_counter; i++)
        {
            // Pack the collider info for each present object into a Texture2D so it may be sent to the shader
            Collider2D cur = _objects[i];
            Vector3 cen = cur.bounds.center - transform.position;
            Vector3 ext = cur.bounds.extents;
            Color obj_info = new(Mathf.Floor(Mathf.Abs(cen.y + ext.y)) / 255, Mathf.Floor(Mathf.Abs(cen.x - ext.x)) / 255, Mathf.Floor(Mathf.Abs(cen.x + ext.x)) / 255);
            Color obj_deci = new(Mathf.Abs(cen.y + ext.y) % 1, Mathf.Abs(cen.x - ext.x) % 1, Mathf.Abs(cen.x + ext.x) % 1);
            Color obj_sign = new(Mathf.Clamp01(Mathf.Sign((cen.y + ext.y) / 255f)), Mathf.Clamp01(Mathf.Sign((cen.x - ext.x) / 255f)), Mathf.Clamp01(Mathf.Sign((cen.x + ext.x) / 255f)));
            objects.SetPixel(i, 0, obj_info);
            objects.SetPixel(i, 1, obj_deci);
            objects.SetPixel(i, 2, obj_sign);
        }

        objects.Apply();
        _mat.SetVector("_WorldPos", transform.position);
        _mat.SetTexture("_ObjArray", objects);
        _mat.SetFloat("_NumObjs", _obj_counter);
    }

    private void SpawnParticles()
    {
        float interval = _col.bounds.extents.x * 2 / _numCasts;
        float startX = _col.bounds.center.x - _col.bounds.extents.x;
        float yTop = _col.bounds.center.y + _col.bounds.extents.y - _offset;
        float yBot = _col.bounds.center.y - _col.bounds.extents.y;
        // Raycast _numCasts times, evenly distributed among the top of the waterfall, and spawn the particle system at each hit
        for (int i = 0; i < _numCasts; i++)
        {
            if (_parts[i] == null) // Only raycast and spawn if the timer is up
            {
                // Raycast downward from the current point on the top
                Vector2 start = new(startX + interval * i, yTop);
                RaycastHit2D hit = Physics2D.Raycast(start, Vector2.down, -3*yBot, _waterLayers | _colliders);
                if (hit)
                {
                    _parts[i] = Instantiate(_part, hit.point, Quaternion.identity, gameObject.transform);
                }
                
            }
            
        }
    }
}
