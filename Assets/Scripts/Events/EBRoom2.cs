using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EBRoom2 : MonoBehaviour
{
    [SerializeField] private GameObject _left_pipe;
    [SerializeField] private GameObject _right_pipe;
    [SerializeField] private GameObject _left_inkfall;
    [SerializeField] private GameObject _right_inkfall;
    [SerializeField] private GameObject _water;
    [SerializeField] private PhysicsButton _button;
    private Animator _leftAnim;
    private Animator _rightAnim;
    private bool _leftActive, _rightActive = false;
    private Material _waterMat;
    private BuoyancyEffector2D _waterBuoy;

    void Start()
    {
        _leftAnim = _left_pipe.GetComponent<Animator>();
        _rightAnim = _right_pipe.GetComponent<Animator>();
        _waterMat = _water.GetComponent<MeshRenderer>().material;
        _waterBuoy = _water.GetComponentInChildren<BuoyancyEffector2D>();

        _leftAnim.Play("Pipe_Start");
        _left_inkfall.SetActive(true);
        _leftActive = true;

        _waterMat.SetFloat("_WaveSpeed", 4.88f);
        _waterMat.SetFloat("_WaveSize", 0.053f);
        _waterMat.SetFloat("_WaveCount", 1.4f);
        _waterMat.SetFloat("_WaveFlowDirection", -1);

        _waterBuoy.flowMagnitude = 25;

        _button.Deactivate();
    }

    public void SwitchPipes()
    {
        if (_leftActive)
        {
            _leftAnim.Play("Pipe_Stop");
            _left_inkfall.SetActive(false);
            _leftActive = false;

            _rightAnim.Play("Pipe_Start");
            _right_inkfall.SetActive(true);
            _rightActive = true;

            _waterMat.SetFloat("_WaveFlowDirection", 1);
            _waterBuoy.flowMagnitude = -25;
        }

        else if (_rightActive)
        {
            _rightAnim.Play("Pipe_Stop");
            _right_inkfall.SetActive(false);
            _rightActive = false;

            _leftAnim.Play("Pipe_Start");
            _left_inkfall.SetActive(true);
            _leftActive = true;

            _waterMat.SetFloat("_WaveFloatDirection", -1);
            _waterBuoy.flowMagnitude = 25;
        }
    }
}
