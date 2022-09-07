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
    bool sendStatus = false;
    bool IdleWating = false;
    bool loopIdle = false;
    bool end = false;
    bool sendLogo = false;
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
                IdleWating = true;
                end = false;
                sendLogo = false;
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
        //start press
        if (shouldStartPlaying && vPlayer.isPrepared)
        {
            JumpToFrame(556);
            Play();
            IdleWating = false;
            shouldStartPlaying = false;
        }

        #region idle looping logic
        if (loopIdle == true) {
            if (vPlayer.frame == 555)
            {
                Pause();
                JumpToFrame(0);
                Play();
                IdleWating = true;
                loopIdle = false;
            }
       
        }
        if ( IdleWating && vPlayer.isPrepared)
        {
          
            JumpToFrame(0);
            Stop();
            Play();    
            loopIdle = true;
            Debug.LogError("IdleWating");
            IdleWating = false;
        }
        #endregion

         //send event
        if (!sendLogo && vPlayer.frame == 1800)
        {
            Debug.LogError("VideoPlayer Finish Play");
            ws.Send("ChangeState:2");
            sendLogo = true;

        }
        //end frame
        if (!end && vPlayer.frame == 2040)
        {

            IdleWating = false;
            Pause();
            end = true;
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
