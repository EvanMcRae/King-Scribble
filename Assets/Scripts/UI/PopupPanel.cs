using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PopupPanel : MonoBehaviour
{
    [SerializeField] private GameObject PrimaryButton;
    public bool Closable = true, ClosableOverride = false;
    public float animProgress;
    private GameObject PreviousButton;
    public static bool open = false;
    public static int numPopups = 0;
    public bool visible = false;
    public static int mouseNeverMoved = 0;
    private Animator anim;
    private GameObject currentSelection;
    [SerializeField] private Image ScreenDarkener;
    [SerializeField] private bool darkensScreen = true, selectsPrevious = true;
    public static bool closedThisFrame = false;
    [SerializeField] private Image blocker;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        open = true;

        if (visible)
        {
            if (EventSystem.current.currentSelectedGameObject != null)
            {
                currentSelection = EventSystem.current.currentSelectedGameObject;
            }
            else
            {
                MenuButton.globalNoSound = true;
                EventSystem.current.SetSelectedGameObject(currentSelection);
                MenuButton.globalNoSound = false;
            }
        }
        else if (EventSystem.current.currentSelectedGameObject == PrimaryButton)
        {
            EventSystem.current.SetSelectedGameObject(PreviousButton);
        }

        if (Input.GetKeyDown(KeyCode.Tab) && Closable)
        {
            if (ClosableOverride)
                ClosableOverride = false;
            else
                Close();
        }

        if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
        {
            mouseNeverMoved = 0;
        }

        if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.0 && anim.GetFloat("Speed") < 0 && open)
        {
            Disable();
        }

        if (darkensScreen)
        {
            Color c = ScreenDarkener.color;
            c.a = animProgress * 0.5f;
            ScreenDarkener.color = c;
        }
    }

    // Start is called before the first frame update
    private void OnEnable()
    {
        anim.SetFloat("Speed", 1);
        open = true;
        mouseNeverMoved = 2;
        numPopups++;
        if (blocker != null)
            blocker.enabled = true;
        foreach (MenuButton c in GetComponentsInChildren<MenuButton>())
        {
            c.popupID = numPopups;
        }
        visible = true;
        PreviousButton = EventSystem.current.currentSelectedGameObject;
        MenuButton.globalNoSound = true;
        EventSystem.current.SetSelectedGameObject(PrimaryButton);
        PrimaryButton.GetComponent<MenuButton>().OnSelect(null);
        MenuButton.globalNoSound = false;
        if (darkensScreen)
        {
            ScreenDarkener.gameObject.SetActive(true);
            ScreenDarkener.raycastTarget = true;
        }
    }

    public void Close()
    {
        if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.93)
            anim.SetFloat("Speed", -2);
        else
            anim.SetFloat("Speed", -10);
        if (darkensScreen)
            ScreenDarkener.raycastTarget = false;
        numPopups--;
        if (numPopups < 0) numPopups = 0;
        visible = false;
        if (selectsPrevious)
        {
            closedThisFrame = true;
            EventSystem.current.SetSelectedGameObject(PreviousButton);
            closedThisFrame = false;
        }
    }

    public void Disable()
    {
        if (anim.GetFloat("Speed") < 0)
        {
            gameObject.SetActive(false);
            ScreenDarkener.gameObject.SetActive(false);
            anim.SetFloat("Speed", 0);
            open = false;
        }
    }

    public void NormalSpeed()
    {
        if (anim.GetFloat("Speed") < 0)
        {
            anim.SetFloat("Speed", -2);
        }
        else if (anim.GetFloat("Speed") > 0)
        {
            if (blocker != null)
                blocker.enabled = false;
        }
    }

    public void ResetSpeed()
    {
        if (anim.GetFloat("Speed") > 0)
        {
            anim.SetFloat("Speed", 0);
        }
    }
}