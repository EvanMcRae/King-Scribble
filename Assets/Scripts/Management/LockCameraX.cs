using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// An add-on module for Cinemachine Virtual Camera that locks the camera's X co-ordinate
/// Source: https://discussions.unity.com/t/how-do-i-lock-a-certain-axis-when-using-a-cinemachine-follow-camera/1604529
/// </summary>
[AddComponentMenu("")] // Hide in menu
public class LockCameraX : CinemachineExtension
{
    private float m_XPosition = 0;

    protected override void Awake()
    {
        base.Awake();
        m_XPosition = transform.position.x;
    }

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (enabled && stage == CinemachineCore.Stage.Finalize)
        {
            var pos = state.GetCorrectedPosition();
            pos.x = m_XPosition;
            state.RawPosition = pos;
            state.PositionCorrection = Vector3.zero;
        }
    }
}