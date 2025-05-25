using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Collider2D))]
public class WaterFall : MonoBehaviour
{
    private Collider2D _col;
    public float _force = 5f;
    public float _tickTime = 0.5f;
    private float _timer;
    private InteractableWater _water;
    [SerializeField] LayerMask _waterLayers;
    [SerializeField] LayerMask _colliders;
    private Material _mat;
    private float _cur_max_height;
    private float _base_cull_height;
    private Collider2D _cur_blocking_obj;
    [SerializeField] private ParticleSystem _part;
    private ParticleSystem _curPart;
    private int _obj_counter = 0;
    private void Start()
    {
        _col = GetComponent<Collider2D>();
        _timer = 0f;
        _mat = transform.parent.GetComponent<SpriteRenderer>().material;
        _base_cull_height = _cur_max_height = _mat.GetFloat("_ObjTop");
    }
    private void FixedUpdate()
    {
        // Apply force to the water if colliding
        _timer -= Time.fixedDeltaTime;
        if (_timer > 0f) { return; }
        Vector2 top = new Vector2(_col.bounds.center.x, _col.bounds.center.y + _col.bounds.extents.y);
        RaycastHit2D hit = Physics2D.Raycast(top, Vector2.down, -3*_cur_max_height, _waterLayers); // Don't ask about the -3. Please.
        if (hit)
        {
            if (hit.collider.transform.parent.TryGetComponent<InteractableWater>(out _water))
            {
                if (!_curPart)
                    _curPart = Instantiate(_part, gameObject.transform.position, Quaternion.identity);
                else
                    _curPart.transform.position = gameObject.transform.position;
                _col.transform.position = hit.point;
                _base_cull_height = hit.point.y - 2f;
                float vel = _force * _water.ForceMultiplier;
                vel = Mathf.Clamp(Mathf.Abs(vel), 0f, _water.MaxForce);
                _water.Splash(_col, vel);
            }
        }
        _timer = _tickTime;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((_colliders.value & (1 << collision.gameObject.layer)) > 0)
        {
            _obj_counter++;
        }
            
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if ((_colliders.value & (1 << collision.gameObject.layer)) > 0)
        {
            float top = collision.bounds.center.y + collision.bounds.extents.y;
            if (top > _cur_max_height) // Object is the highest - block the waterfall
            {
                _cur_blocking_obj = collision;
                _mat.SetFloat("_ObjTop", top);
                _cur_max_height = top;
                _curPart.transform.position = new Vector3(_curPart.transform.position.x, top, 0f);
            }
            else if (top < _cur_max_height && (_cur_blocking_obj == collision) && (top > _base_cull_height + 2f)) // Highest object has moved downwards (but not below the water's surface)
            {
                _mat.SetFloat("_ObjTop", top);
                _cur_max_height = top;
                _curPart.transform.position = new Vector3(_curPart.transform.position.x, top, 0f);
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if ((_colliders.value & (1 << collision.gameObject.layer)) > 0)
        {
            _obj_counter--;
        }
            
        
        if (collision == _cur_blocking_obj)
            _cur_blocking_obj = null;
        
        _cur_max_height = _base_cull_height;
        
        if (_obj_counter == 0)
        {
            // Reset position to water surface
            _mat.SetFloat("_ObjTop", _cur_max_height);
            _curPart.transform.position = gameObject.transform.position;
        }
    }
}
