using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using static System.Console;
using UnityEngine;
using UnityEngine.Events;

public class UDPManager : MonoBehaviour
{
    UdpClient socket;
    IPEndPoint ipep;
    string message = "";

    public class UDPEvent : UnityEvent<string> { }
    public UDPEvent OnReceiveEvent = new UDPEvent();
    public string ip;
    public int port;

    void Start()
    {
        OnReceiveEvent.AddListener(ReceiveMessage);
    }

    public void Init()
    {
        socket = new UdpClient(port);
        socket.BeginReceive(OnReceive, null);
        ipep = new IPEndPoint(IPAddress.Parse(ip), port);
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

    void ReceiveMessage(string msg)
    {
        Debug.Log(msg);
    }

    void Update()
    {
        OnReceiveEvent.Invoke(message);
    }

    private void OnDestroy()
    {
        OnReceiveEvent.RemoveAllListeners();
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
}
