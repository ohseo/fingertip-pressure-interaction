using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ForceLevelManager : MonoBehaviour
{
    public UDPManager udpManager;
    // Start is called before the first frame update
    [SerializeField]
    public string ip = "127.0.0.1";
    public int port = 5005;
    public TextMeshProUGUI udpTextMesh;

    public string forceLevel { get; set;}

    void Start()
    {
        forceLevel = "";
        udpManager.SetIPAddress(ip);
        udpManager.SetPort(port);
        udpManager.SetTextMesh(udpTextMesh);
        udpManager.Init();
    }

    // Update is called once per frame
    void Update()
    {
        forceLevel = udpManager.lastMessage;
    }
}
