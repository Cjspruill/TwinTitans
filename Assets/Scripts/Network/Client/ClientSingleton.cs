using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ClientSingleton : MonoBehaviour
{

    private static ClientSingleton instance;
    public ClientGameManager GameManger { get; private set; }

    public static ClientSingleton Instance
    {
        get
        {
            if (instance != null) { return instance; }

            instance = FindObjectOfType<ClientSingleton>();

            if (instance == null)
            {
                Debug.LogError("No Client Singleton in the scene!");
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

    public async Task<bool> CreateClient()
    {
        GameManger = new ClientGameManager();

        return await GameManger.InitAsync();
    }

    private void OnDestroy()
    {
        GameManger?.Dispose();
    }
}
