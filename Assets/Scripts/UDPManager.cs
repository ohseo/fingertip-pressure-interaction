using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using static System.Console;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class UDPManager : MonoBehaviour
{
    private UdpClient socket;
    private IPEndPoint ipep;
    private string message = "";
    public string lastMessage { get; set; }

    public class UDPEvent : UnityEvent<string> { }
    public UDPEvent UDPReceiveHandler = new UDPEvent();
    private string ip;
    private int port;
    private TextMeshProUGUI udpText;

    void Start()
    {
        // udpText.text = "UDPManager: Starting...";
    }

    public void Init()
    {
        lastMessage = "";
        socket = new UdpClient(port);
        socket.BeginReceive(OnReceive, null);
        ipep = new IPEndPoint(IPAddress.Parse(ip), port);
        Debug.Log("UDPManager: Listening on port " + port);
        UDPReceiveHandler.AddListener(MessageReceiver);
    }

    void OnReceive(IAsyncResult ar)
    {
        try{
            byte[] bytes = socket.EndReceive(ar, ref ipep);
            message = Encoding.ASCII.GetString(bytes);
            WriteLine(message);
        }
        catch (SocketException e) {}
        
        socket.BeginReceive(OnReceive, null);
    }

    void MessageReceiver(string msg)
    {
        // Debug.Log(msg);
        // udpText.text = msg.Split('\t')[0];
        // udpText.text = msg;
        lastMessage = msg;
    }

    void Update()
    {
        UDPReceiveHandler.Invoke(message);
    }

    private void OnDestroy()
    {
        UDPReceiveHandler.RemoveAllListeners();
        socket.Close();
    }

    public void SetIPAddress(string ip)
    {
        this.ip = ip;
    }

    public void SetPort(int port)
    {
        this.port = port;
    }

    public void SetTextMesh(TextMeshProUGUI udpText)
    {
        this.udpText = udpText;
    }
}
