using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine.SceneManagement;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System;
using UnityEngine.UI;

public class MainMenuController : NetworkBehaviour
{
    [SerializeField] GameObject connectingPanel;
    [SerializeField] GameObject hostMenuPanel;
    [SerializeField] GameObject clientMenuPanel;
    [SerializeField] TMP_InputField joinCodeInputField;
    [SerializeField] TextMeshProUGUI joinCodeText;

    public async void StartHost()
    {
    //  joinCodeText.text = "Join Code: " + await HostManager.Instance.StartHostWithRelay(HostManager.Instance.maxConnections);
   
      if(joinCodeText.text != "Join Code: ")
        {
            connectingPanel.SetActive(false);
            hostMenuPanel.SetActive(true);
        }

    }

    public void Update()
    {
        if (IsServer) return;

        bool clientConnected = NetworkManager.Singleton.IsConnectedClient;

        if (clientConnected && IsClient)
        {
            connectingPanel.SetActive(false);
            clientMenuPanel.SetActive(true);
        }
    }
    public async void StartClient()
    {
    //    await HostManager.Instance.StartClientWithRelay(joinCodeInputField.text);

        connectingPanel.SetActive(true);
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
