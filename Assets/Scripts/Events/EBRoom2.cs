using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EBRoom2 : MonoBehaviour
{
    [SerializeField] private EBInkPipe _left_pipe;
    [SerializeField] private EBInkPipe _right_pipe;
    [SerializeField] private GameObject _left_inkfall;
    [SerializeField] private GameObject _right_inkfall;
    [SerializeField] private GameObject _water;
    [SerializeField] private PhysicsButton _button;
    private Animator _leftAnim;
    private Material _waterMat;
    private BuoyancyEffector2D _waterBuoy;

    void Start()
    {
        _leftAnim = _left_pipe.GetComponent<Animator>();
        _waterMat = _water.GetComponent<MeshRenderer>().material;
        _waterBuoy = _water.GetComponentInChildren<BuoyancyEffector2D>();

        _leftAnim.Play("Pipe_Start");
        _left_inkfall.SetActive(true);

        _waterMat.SetFloat("_WaveSpeed", 4.88f);
        _waterMat.SetFloat("_WaveSize", 0.053f);
        _waterMat.SetFloat("_WaveCount", 1.4f);
        _waterMat.SetFloat("_WaveFlowDirection", -1);

        _waterBuoy.flowMagnitude = 25;

        _button.Deactivate();
    }

    public void SwitchPipes()
    {
        if (_left_pipe.is_busy || _right_pipe.is_busy) return;

        if (_left_pipe.is_active)
        {
            _left_pipe.Deactivate();

            _right_pipe.Activate();
            _right_inkfall.SetActive(true);

            _waterMat.SetFloat("_WaveFlowDirection", 1);
            _waterBuoy.flowMagnitude = -25;
        }

        else if (_right_pipe.is_active)
        {
            _right_pipe.Deactivate();

            _left_pipe.Activate();

            _waterMat.SetFloat("_WaveFloatDirection", -1);
            _waterBuoy.flowMagnitude = 25;
        }
    }
}
