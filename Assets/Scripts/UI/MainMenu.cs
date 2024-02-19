using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    [SerializeField] private TMP_InputField joinCodeField;

    public async void StartHost()
    {
        await HostSingleton.Instance.GameManger.StartHostAsync();
    }

    public async void StartClient()
    {
        await ClientSingleton.Instance.GameManger.StartClientAsync(joinCodeField.text);
    }
    public void GabrielButtonSelectedHost(string character)
    {
        GameManager.Instance.SetPlayerCharacter(character);
    }

    public void KhafreButtonSelectedHost(string character)
    {
        GameManager.Instance.SetPlayerCharacter(character);
    }

    public void GabrielButtonSelectedClient(string character)
    {
        GameManager.Instance.SetPlayerCharacter(character);
    }

    public void KhafreButtonSelectedClient(string character)
    {
        GameManager.Instance.SetPlayerCharacter(character);
    }
}
