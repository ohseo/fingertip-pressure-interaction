using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceLevelManager : MonoBehaviour
{
    public UDPManager udpManager;
    // Start is called before the first frame update
    [SerializeField]
    public string ip = "127.0.0.1";
    public int port = 5005;

    void Start()
    {
        udpManager.SetIPAddress(ip);
        udpManager.SetPort(port);
        udpManager.Init();
        udpManager.UDPReceiveHandler.AddListener(ReceiveForceLevel);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ReceiveForceLevel(string message)
    {
        Debug.Log(message);
    }

}
