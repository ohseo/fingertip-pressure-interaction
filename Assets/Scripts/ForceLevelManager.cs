using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Globalization;
using UnityEditor.PackageManager;

public class ForceLevelManager : MonoBehaviour
{
    public UDPManager udpManager;
    // Start is called before the first frame update
    [SerializeField]
    public string ip = "127.0.0.1";
    public int port = 5005;
    public TextMeshProUGUI udpTextMesh;

    public string forceLevel { get; set; }
    private string receivedForceLevel;
    public int historyLength { get; set; }
    public int[] historyCount { get; set; }

    void Start()
    {
        forceLevel = "";
        receivedForceLevel = "";
        udpManager.SetIPAddress(ip);
        udpManager.SetPort(port);
        // udpManager.SetTextMesh(udpTextMesh);
        udpManager.Init();
        historyCount = new int[3];
    }

    // Update is called once per frame
    void Update()
    {
        // forceLevel = udpManager.lastForceLevel;
        Parser(udpManager.lastMessage);
        // udpTextMesh.text = forceLevel;
    }

    private void Parser(string message)
    {
        string[] splitText = message.Split('\t');
        if(splitText.Length < 5)
        {
            return;
        }
        receivedForceLevel = splitText[0];
        historyLength = Int32.Parse(splitText[1]);
        historyCount[0] = Int32.Parse(splitText[2]);
        historyCount[1] = Int32.Parse(splitText[3]);
        historyCount[2] = Int32.Parse(splitText[4]);
        FilterForceLevel();
    }
    
    private void FilterForceLevel()
    {
        int level;
        if(Int32.TryParse(receivedForceLevel, out level))
        {
            if(historyCount[level] < historyLength)
            {
                forceLevel = "uncertain";
            } else
            {
                forceLevel = receivedForceLevel;
            }
        }
    }
}
