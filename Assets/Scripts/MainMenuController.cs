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

public class MainMenuController : MonoBehaviour
{
    [SerializeField] GameObject connectingPanel;
    [SerializeField] GameObject menuPanel;
    [SerializeField] TMP_InputField joinCodeInputField;
}
