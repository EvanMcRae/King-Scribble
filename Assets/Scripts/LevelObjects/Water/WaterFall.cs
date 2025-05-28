using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private float _cur_min_x;
    private float _cur_max_x;
    private float _base_cull_height;
    private float _base_min_x;
    private float _base_max_x;
    private Collider2D _cur_top_blocking_obj;
    private Collider2D _cur_left_blocking_obj;
    private Collider2D _cur_right_blocking_obj;
    [SerializeField] private ParticleSystem _part;
    private ParticleSystem _curPart;
    private int _obj_counter = 0;
    private List<Collider2D> _objects;
    private void Start()
    {
        _col = GetComponent<Collider2D>();
        _timer = 0f;
        _mat = transform.parent.GetComponent<SpriteRenderer>().material;
        _base_cull_height = _cur_max_height = _mat.GetFloat("_ObjTop");
        _base_min_x = _col.bounds.center.x - _col.bounds.extents.x;
        _base_max_x = _col.bounds.center.x + _col.bounds.extents.x;
        _cur_min_x = _base_max_x;
        _cur_max_x = _base_min_x;
        _mat.SetFloat("_ObjBoundL", _cur_min_x);
        _mat.SetFloat("_ObjBoundR", _cur_max_x);
        _objects = new List<Collider2D>();
    }
    private void FixedUpdate()
    {
        // Apply force to the water if colliding
        _timer -= Time.fixedDeltaTime;
        if (_timer > 0f) { return; }
        Vector2 top = new Vector2(_col.bounds.center.x, _col.bounds.center.y + _col.bounds.extents.y);
        RaycastHit2D hit = Physics2D.Raycast(top, Vector2.down, -3 * _cur_max_height, _waterLayers); // Don't ask about the -3. Please.
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

    public void Update()
    {
        UpdateCrop();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((_colliders.value & (1 << collision.gameObject.layer)) > 0)
        {
            _obj_counter++;
            _objects.Add(collision);
        }

    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if ((_colliders.value & (1 << collision.gameObject.layer)) > 0)
        {
            float top = collision.bounds.center.y + collision.bounds.extents.y;
            float leftBound = collision.bounds.center.x - collision.bounds.extents.x;
            float rightBound = collision.bounds.center.x + collision.bounds.extents.x;

            // Crop based on height
            if (top > _cur_max_height) // Object is the highest - block the waterfall
            {
                _cur_top_blocking_obj = collision;
                _mat.SetFloat("_ObjTop", top);
                _cur_max_height = top;
                _curPart.transform.position = new Vector3(_curPart.transform.position.x, top, 0f);
            }

            else if (top < _cur_max_height && (_cur_top_blocking_obj == collision) && (top > _base_cull_height + 2f)) // Highest object has moved downwards (but not below the water's surface)
            {
                _mat.SetFloat("_ObjTop", top);
                _cur_max_height = top;
                _curPart.transform.position = new Vector3(_curPart.transform.position.x, top, 0f);
            }
            if (leftBound < _base_min_x && rightBound > _base_min_x && _cur_top_blocking_obj != collision) rightBound = leftBound;
            // Crop based on left bound
            if (leftBound <= _cur_min_x) // Object is the leftmost - block the waterfall
            {
                _cur_left_blocking_obj = collision;
                _mat.SetFloat("_ObjBoundL", leftBound);
                _cur_min_x = leftBound;
            }

            else if (_cur_left_blocking_obj == collision) // Leftmost object has moved and is still leftmost
            {
                _mat.SetFloat("_ObjBoundL", leftBound);
                _cur_min_x = leftBound;
            }

            // Crop based on right bound
            if (rightBound >= _cur_max_x)
            {
                _cur_right_blocking_obj = collision;
                _mat.SetFloat("_ObjBoundR", rightBound);
                _cur_max_x = rightBound;
            }

            else if (_cur_right_blocking_obj == collision) // Rightmost object has moved and is still rightmost
            {
                _mat.SetFloat("_ObjBoundR", rightBound);
                _cur_max_x = rightBound;
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if ((_colliders.value & (1 << collision.gameObject.layer)) > 0)
        {
            _obj_counter--;
            _objects.Remove(collision);
        }

        if (collision == _cur_top_blocking_obj) _cur_top_blocking_obj = null;
        if (collision == _cur_left_blocking_obj) _cur_left_blocking_obj = null;
        if (collision == _cur_right_blocking_obj) _cur_right_blocking_obj = null;

        _cur_max_height = _base_cull_height;
        _cur_min_x = _base_max_x;
        _cur_max_x = _base_min_x;

        if (_obj_counter == 0)
        {
            // Reset position to water surface
            _mat.SetFloat("_ObjTop", _cur_max_height);
            _mat.SetFloat("_ObjBoundL", _cur_min_x);
            _mat.SetFloat("_ObjBoundR", _cur_max_x);
            if (_curPart != null)
                _curPart.transform.position = gameObject.transform.position;

            Texture2D objects = new(width: 1, height: 3, textureFormat: TextureFormat.RGBAFloat, mipCount: 0, false);
            _mat.SetTexture("_ObjArray", objects);
            _mat.SetFloat("_NumObjs", _obj_counter);
        }
    }
    private void UpdateCrop()
    {
        if (_obj_counter == 0) return;
        Texture2D objects = new(width: _obj_counter, height: 3, textureFormat:TextureFormat.RGBAFloat, mipCount:0, false);
        for (int i = 0; i < _obj_counter; i++)
        {
            // Pack the collider info for each present object into a Texture2D so it may be sent to the shader
            Collider2D cur = _objects[i];
            Vector3 cen = cur.bounds.center;
            Vector3 ext = cur.bounds.extents;
            Color obj_info = new(Mathf.Floor(Mathf.Abs(cen.y + ext.y)) / 255, Mathf.Floor(Mathf.Abs(cen.x - ext.x)) / 255, Mathf.Floor(Mathf.Abs(cen.x + ext.x)) / 255);
            Color obj_deci = new(Mathf.Abs(cen.y + ext.y) % 1, Mathf.Abs(cen.x - ext.x) % 1, Mathf.Abs(cen.x + ext.x) % 1);
            Color obj_sign = new(Mathf.Clamp01(Mathf.Sign((cen.y + ext.y) / 255f)), Mathf.Clamp01(Mathf.Sign((cen.x - ext.x) / 255f)), Mathf.Clamp01(Mathf.Sign((cen.x + ext.x) / 255f)));
            objects.SetPixel(i, 0, obj_info);
            objects.SetPixel(i, 1, obj_deci);
            objects.SetPixel(i, 2, obj_sign);
        }
        objects.Apply();
        _mat.SetTexture("_ObjArray", objects);
        _mat.SetFloat("_NumObjs", _obj_counter);
    }
}
