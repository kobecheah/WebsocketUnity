using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine;
using UnityEngine.Video;
using WebSocketSharp;
public class WebsoketController : MonoBehaviour
{
    public GameObject QrcodePanel;
    public GameObject VideoPanel;
    public GameObject LogoPanel;
    public VideoPlayer videoPlayer;
    public VideoPlayer logoPlayer;
    WebSocket ws;
    bool playvideoFlag = false;
    bool qrpanelFlag = false;
    private void Start()
    {
        QrcodePanel.SetActive(true);
        VideoPanel.SetActive(false);
        LogoPanel.SetActive(false);
        videoPlayer.Stop();
        logoPlayer.Stop();

        ws = new WebSocket("wss://demo.piesocket.com/v3/channel_1?api_key=VCXCEuvhGcBDP7XhiJJUDvR1e1D3eiVjgZ9VRiaV&notify_self");
        ws.Connect();
        ws.OnClose += WsOnOnClose;
        ws.OnMessage += (sender, e) =>
        {
            if (e.Data == "Start")
            {
                playvideoFlag = true;
                Debug.Log("State Start");
            }
            if (e.Data == "Waiting")
            {
                qrpanelFlag = true;
                Debug.Log("Waiting");
            }
         
        };
        videoPlayer.loopPointReached += EndReached;

    }
    void EndReached(UnityEngine.Video.VideoPlayer vp)
    {
        Debug.Log("VideoPlayer Finish Play");
        ws.Send("RevealLogo");
        videoPlayer.Stop();
        LogoPanel.SetActive(true);
        logoPlayer.Play();
        logoPlayer.isLooping = true;
    }
    private void Update()
    {
        if (qrpanelFlag == true)
        {
            QrcodePanel.SetActive(true);
            videoPlayer.Stop();
            logoPlayer.Stop();
            VideoPanel.SetActive(false);
            LogoPanel.SetActive(false);
            qrpanelFlag = false;
        }
        if (playvideoFlag == true) {
            QrcodePanel.SetActive(false);
            VideoPanel.SetActive(true);
            videoPlayer.Play();
            playvideoFlag = false;
        }
        if (ws == null)
        {
         
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ws.Send("Hello");
        }
    }
    private void WsOnOnClose(object sender, CloseEventArgs closeEventArgs)
    {
        if (!closeEventArgs.WasClean)
        {
            if (!ws.IsAlive)
            {
                Thread.Sleep(10000);
                ws.Connect();
            }
        }
    }

}
