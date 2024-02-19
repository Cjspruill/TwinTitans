using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class HostSingleton : MonoBehaviour
{

    private static HostSingleton instance;

    public HostGameManager GameManger { get; private set; }

    public static HostSingleton Instance
    {
        get
        {
            if (instance != null) { return instance; }

            instance = FindObjectOfType<HostSingleton>();

            if (instance == null)
            {
                Debug.LogError("No Host Singleton in the scene!");
                return null;
            }
            return instance;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void CreateHost()
    {
        GameManger = new HostGameManager();
    }

    private void OnDestroy()
    {
        GameManger?.Dispose();
    }
}
