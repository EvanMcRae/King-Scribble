using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class PenIntroLevelPickupEvent : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds1 = new(1);
    private static WaitForSeconds _waitForSeconds3 = new(3);
    private static WaitForSeconds _waitForSeconds2 = new(2f);
    private static WaitForSeconds _waitForSeconds0_5 = new(0.5f);

    public CinemachineCamera cam, followCam, initialFollowCam, sourceCam;
    public PolygonCollider2D followCamBounds;
    public GameObject inkFlow_L;
    public GameObject inkFlow_R;
    public UnityEvent startFlood;
    public UnityEvent closeDoor;
    public UnityEvent onLoadCheckpoint;
    private bool doorClosed;
    private bool isAnimating;
    public SoundPlayer rumblePlayer, soundPlayer;
    public GameObject skipButton;
    [SerializeField] Animator anim_L;
    [SerializeField] Animator anim_R;
    [SerializeField] public SwitchMusicOnLoad introMusic, crazyMusic;
    private static bool lateStart = false;

    public void Awake()
    {
        // Attempt to load from save data
        try
        {
            SceneSerialization scene = GameSaver.GetScene(GameSaver.currData.scene);
            if (scene.unlockPoints.Contains("inkRises"))
            {
                lateStart = true;
                initialFollowCam.gameObject.SetActive(false);
                followCam.gameObject.SetActive(true);
                followCam.Follow = PlayerVars.instance.transform;
                followCam.ForceCameraPosition(PlayerVars.instance.transform.position + Vector3.forward * -10f, Quaternion.identity);
                Camera.main.transform.position = PlayerVars.instance.transform.position + Vector3.forward * -10f;
            }
            else
            {
                if (AudioManager.instance.currentArea == AudioManager.GameArea.TEMPLE)
                {
                    introMusic.duration = 1.0f;
                }
                introMusic.enabled = true;
                GameManager.ResetAction += ResetLevel;
                Checkpoint.ActivatedAction += CheckpointHit;
            }
        }
        catch (System.Exception)
        {
            introMusic.enabled = true;
            GameManager.ResetAction += ResetLevel;
            Checkpoint.ActivatedAction += CheckpointHit;
        }
    }

    void CheckpointHit()
    {
        Checkpoint.ActivatedAction -= CheckpointHit;
        GameManager.ResetAction -= ResetLevel;
    }

    void ResetLevel(bool shouldFade)
    {
        GameManager.ResetAction -= ResetLevel;
        try
        {
            SceneSerialization scene = GameSaver.GetScene(GameSaver.currData.scene);
            if (shouldFade && !scene.unlockPoints.Contains("inkRises"))
            {
                AudioManager.instance.FadeOutCurrent();
            }
        }
        catch (System.Exception)
        {
            if (shouldFade)
                AudioManager.instance.FadeOutCurrent();
        }
    }

    public void LateUpdate()
    {
        if (lateStart)
        {
            isAnimating = true;
            doorClosed = true;
            SkipCutscene();
            onLoadCheckpoint.Invoke();
            isAnimating = false;
            lateStart = false;
        }
    }

    public void StartEvent()
    {
        StartCoroutine(Start_Event());

        if (GameSaver.GetScene("Level5") != null && GameSaver.GetScene("Level5").permaUnlockPoints.Contains("cutsceneWatched"))
        {
            skipButton.SetActive(true);
        }
    }

    public void Update()
    {
        if (isAnimating && PlayerVars.instance.cheatMode)
        {
            skipButton.SetActive(true);
        }
    }

    public void StopEvent()
    {
        // DOTween Ink flows
        inkFlow_L.transform.DOMoveY(-330f, 5f);
        inkFlow_R.transform.DOMoveY(-330f, 5f);
        // Stop anim
        anim_L.Play("Pipe_Stop");
        anim_R.Play("Pipe_Stop");
    }

    void PlayCrazyMusic()
    {
        crazyMusic.enabled = true;
    }

    IEnumerator Start_Event()
    {
        isAnimating = true;
        AudioManager.instance.FadeOutCurrent();
        GameManager.canMove = false;
        yield return _waitForSeconds0_5;
        cam.gameObject.SetActive(true);
        rumblePlayer.PlaySound("Ink.Rumble", 1, false);
        cam.TryGetComponent(out CinemachineBasicMultiChannelPerlin noise);
        noise.AmplitudeGain = 0.125f;
        DOTween.To(() => noise.AmplitudeGain, x => noise.AmplitudeGain = x, 0.5f, 4f);
        Invoke(nameof(PlayCrazyMusic), 2.97f);
        yield return _waitForSeconds3;
        anim_L.Play("Pipe_Start");
        anim_R.Play("Pipe_Start");
        yield return _waitForSeconds1; // For the Pipe animation to transition from start to flowing
        inkFlow_L.transform.DOLocalMoveY(-118, 2.5f);
        inkFlow_R.transform.DOLocalMoveY(-118, 2.5f);
        soundPlayer.PlaySound("Ink.Flood", 1, true);
        noise.AmplitudeGain = 0.25f;
        DOTween.To(() => noise.AmplitudeGain, x => noise.AmplitudeGain = x, 0f, 3f);
        yield return _waitForSeconds3;
        cam.gameObject.SetActive(false);
        yield return _waitForSeconds0_5;
        closeDoor.Invoke();
        doorClosed = true;
        startFlood.Invoke();
        followCam.gameObject.SetActive(true);
        followCam.Follow = PlayerVars.instance.transform;
        GameManager.canMove = true;
        isAnimating = false;
        SceneSerialization s = GameSaver.GetScene("Level5");
        if (s == null)
        {
            s = new("Level5", PlayerVars.instance.GetSpawnPos())
            {
                spawnpoint = new Vector3Serialization(PlayerVars.instance.GetSpawnPos()),
                unlockPoints = new(),
                permaUnlockPoints = new()
            };
            GameSaver.currData.scenes.Add(s);
        }
        GameSaver.UnlockPointPermanent("Level5", "cutsceneWatched");
        if (!s.permaUnlockPoints.Contains("cutsceneWatched"))
            s.permaUnlockPoints.Add("cutsceneWatched");
        GameSaver.instance.SaveGame();
        skipButton.GetComponent<HUDButtonCursorHandler>().OnPointerExit(null);
        skipButton.SetActive(false);
        GameSaver.UnlockPoint("Level5", "inkRises");
        yield return _waitForSeconds2;
        initialFollowCam.gameObject.SetActive(false);
    }

    public void SkipCutscene()
    {
        if (isAnimating)
        {
            CancelInvoke();
            crazyMusic.enabled = true;
            StopAllCoroutines();
            StartCoroutine(SkipCutsceneRoutine());
            rumblePlayer.EndAllSounds();
            anim_L.Play("Pipe_Flowing");
            anim_R.Play("Pipe_Flowing");
            inkFlow_L.transform.localPosition = new Vector3(inkFlow_L.transform.localPosition.x, -118f, 0f);
            inkFlow_R.transform.localPosition = new Vector3(inkFlow_R.transform.localPosition.x, -118f, 0f);
            GameManager.canMove = true;
            if (!doorClosed)
            {
                closeDoor.Invoke();
                doorClosed = true;
            }
            startFlood.Invoke();
            isAnimating = false;
            skipButton.GetComponent<HUDButtonCursorHandler>().OnPointerExit(null);
            skipButton.SetActive(false);
            GameSaver.UnlockPoint("Level5", "inkRises");
        }
    }

    IEnumerator SkipCutsceneRoutine()
    {
        cam.ForceCameraPosition(PlayerVars.instance.transform.position + Vector3.up * 10f + Vector3.forward * -10f, Quaternion.identity);
        initialFollowCam.ForceCameraPosition(PlayerVars.instance.transform.position + Vector3.up * 10f + Vector3.forward * -10f, Quaternion.identity);
        Camera.main.transform.position = PlayerVars.instance.transform.position + Vector3.up * 10f + Vector3.forward * -10f;
        Camera.main.GetComponent<CinemachineBrain>().DefaultBlend.Time = 0;
        yield return new WaitForEndOfFrame();
        cam.gameObject.SetActive(false);
        initialFollowCam.gameObject.SetActive(false);
        followCam.gameObject.SetActive(true);
        followCam.ForceCameraPosition(PlayerVars.instance.transform.position + Vector3.up * 10f + Vector3.forward * -10f, Quaternion.identity);
        Camera.main.transform.position = PlayerVars.instance.transform.position + Vector3.up * 10f + Vector3.forward * -10f;
        yield return new WaitForEndOfFrame();
        Camera.main.GetComponent<CinemachineBrain>().DefaultBlend.Time = 2;
        followCam.Follow = PlayerVars.instance.transform;
    }
}
