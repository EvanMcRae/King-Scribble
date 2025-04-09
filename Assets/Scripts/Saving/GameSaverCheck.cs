using UnityEngine;

public class GameSaverCheck : MonoBehaviour
{
    public GameObject gameSaver;

    // Start is called before the first frame update
    void Awake()
    {
        if (!FindFirstObjectByType<GameSaver>())
        {
            Instantiate(gameSaver, transform.position, transform.rotation);
        }
    }
}
