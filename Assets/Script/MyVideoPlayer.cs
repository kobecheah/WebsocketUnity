using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using WebSocketSharp;

public class MyVideoPlayer : MonoBehaviour
{
    WebSocket ws;
    [Header("Settings")]
    [Range(0.1f, 10f)]
    public float playbackSpeed = 1f;

    [Header("UI Controls")]
    public Slider progressBar;

    //Private vars
    VideoPlayer vPlayer;
    public int vidFrameLength;
    bool shouldStartPlaying = false;
    bool reset = false;
    bool sendStatus = false;
    bool IdleWating = false;
    // Use this for initialization
    void Start()
    {
        vPlayer = gameObject.GetComponent<VideoPlayer>();
        vPlayer.playbackSpeed = playbackSpeed;
        vPlayer.Prepare();
        vidFrameLength = (int)vPlayer.frameCount;
        progressBar.maxValue = progressBar.maxValue = vidFrameLength;
        progressBar.onValueChanged.AddListener(JumpToFrame);
        IdleWating = true;
        ws = new WebSocket("wss://quixotic-grey-ceiling.glitch.me/");
        ws.Connect();     
        ws.OnClose += WsOnOnClose;
        ws.OnMessage += (sender, e) =>
        {
            Debug.Log(e.Data);
            if (e.Data == "CurrentState:1")
            {
                shouldStartPlaying = true;
                IdleWating = false;
                Debug.Log("State Start");
            }

            if (e.Data == "CurrentState:0")
            {
                reset = true;
                Debug.Log("Waiting");
            }

        };
    }

    // Update is called once per frame
    void Update()
    {
       
        if (ws == null)
        {

            return;
        }
        if (ws.ReadyState == WebSocketState.Open)
        {
            if (sendStatus == false)
            {
                Debug.LogError("Connected");
                ws.Send("Connect:Screen");
                sendStatus = true;
              
            }

        }
        else if (ws.ReadyState == WebSocketState.Closed)
        {
            sendStatus = false;
        }

        if (shouldStartPlaying && vPlayer.isPrepared)
        {
            Play();
            IdleWating = false;
            shouldStartPlaying = false;
        }
        if (IdleWating && vPlayer.isPrepared)
        {
            JumpToFrame(40);
            Pause();
            IdleWating = false;
        }
        if (reset==true)
        {
            JumpToFrame(40);
            Pause();
            reset = false;
        }
        if (vPlayer.frame == 1800)
        {
            Debug.LogError("VideoPlayer Finish Play");
            ws.Send("ChangeState:2");
      

        }
        if (vPlayer.frame == 2070)
        {
            Pause();
        }
    }

    public void Play()
    {
        vPlayer.Play();
    }

    public void Pause()
    {
        vPlayer.Pause();
    }

    public void Stop()
    {
        vPlayer.Stop();
    }

    //This is called by the slider (Event Trigger: End Drag)
    public void SetFrameBySlider()
    {
        JumpToFrame(progressBar.value);
    }

    public void JumpToFrame(float frame)
    {
        vPlayer.frame = (long)frame;
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
