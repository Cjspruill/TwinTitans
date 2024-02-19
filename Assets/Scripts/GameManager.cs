using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    [SerializeField] string playerCharacter;
    [SerializeField] int playerChar;

    [SerializeField] NetworkObject gabrielNetworkPlayer;
    [SerializeField] NetworkObject khafreNetworkPlayer;
    [SerializeField] NetworkObject enemyPrefab;

    [SerializeField] float countDownTimer;
    [SerializeField] float readyTime;
    public static GameManager Instance { get; private set; }

    public EnemySpawnPoint[] enemySpawnPoints;


    private void OnEnable()
    {
       SceneManager.sceneLoaded += LevelLoaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= LevelLoaded;
    }

    private void LevelLoaded(Scene arg0, LoadSceneMode arg1)
    {
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Game"))
        {

            Debug.Log("SceneLoaded");


            Invoke("ChangeCharacter", .1f);
          //  Invoke("SpawnEnemies", .5f);
        }
    }

    void ChangeCharacter()
    {
        if (IsServer)
        { 
            if (GetPlayerCharacter() == "Gabriel")
            {        
                //Do nothing. We are already set as gabriel
            }
            else if (GetPlayerCharacter() == "Khafre")
            {
                NetworkObject[] netobjs = FindObjectsOfType<NetworkObject>();

                for (int i = 0; i < netobjs.Length; i++)
                {
                    if (netobjs[i].OwnerClientId == NetworkManager.Singleton.LocalClient.ClientId)
                    {
                        netobjs[i].Despawn();
                    }
                }

                NetworkObject newObject = Instantiate(khafreNetworkPlayer, Vector3.zero, Quaternion.identity);
                newObject.SpawnAsPlayerObject(NetworkManager.Singleton.LocalClient.ClientId);
            }
            SpawnEnemies();
        }
        else if (IsClient)
        {
            ulong clientId = NetworkManager.Singleton.LocalClient.ClientId;
            ChangeCharacterServerRpc(clientId);
        }
    }
    [ServerRpc(RequireOwnership =false)]
   void ChangeCharacterServerRpc(ulong clientId)
    {
        if (GetPlayerCharacter() == "Gabriel")
        {
            return;
        }
        else if (GetPlayerCharacter() == "Khafre")
        {
            NetworkObject[] netobjs = FindObjectsOfType<NetworkObject>();

            for (int i = 0; i < netobjs.Length; i++)
            {
                if (netobjs[i].OwnerClientId == clientId)
                {
                    //netobjs[i].RemoveOwnership();

                    netobjs[i].Despawn();
                }
            }

            NetworkObject newObject = Instantiate(khafreNetworkPlayer, Vector3.zero, Quaternion.identity);
            newObject.SpawnAsPlayerObject(clientId);
        }
    }

    void SpawnEnemies()
    {
        if (!IsServer) return;

        enemySpawnPoints = FindObjectsOfType<EnemySpawnPoint>();

        for (int i = 0; i < 3; i++)
        {
            int enemySpawnPoint = UnityEngine.Random.Range(0, enemySpawnPoints.Length);
            NetworkObject newObject = Instantiate(enemyPrefab, enemySpawnPoints[enemySpawnPoint].transform.position, enemySpawnPoints[enemySpawnPoint].transform.rotation);
            newObject.Spawn();
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void CountDown()
    {
        countDownTimer += Time.deltaTime;

        Debug.Log(Mathf.RoundToInt(countDownTimer));

        if (countDownTimer >= readyTime)
        {
            countDownTimer = 0;
        }
    }

    public void SetPlayerCharacter(string character)
    {
        playerCharacter = character;

        if (character == "Gabriel")
            playerChar = 0;
        else if (character == "Khafre")
            playerChar = 1;

    }
    public string GetPlayerCharacter()
    {
        return playerCharacter;
    }


}