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