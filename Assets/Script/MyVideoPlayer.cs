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


    float doubleClickTime = .2f, lastClickTime;
    //Private vars
    VideoPlayer vPlayer;
    public int vidFrameLength;
    bool shouldStartPlaying = false;
    bool sendStatus = false;
    bool IdleWating = false;
    bool loopIdle = false;
    bool end = false;
    bool sendLogo = false;
    bool sendState4 = false;
    bool sendState3 = false;
    bool Islooping = false;
    // Use this for initialization
    void Start()
    {
        Screen.fullScreen = true;
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
                loopIdle = false;
                Islooping = false;
                sendLogo = false;
                sendState3 = false;
                sendState4 = false;
                Debug.Log("State Start");
            }

            if (e.Data == "CurrentState:0")
            {
                IdleWating = true;
                end = false;
                sendLogo = false;
                sendState3 = false;
                sendState4 = false;
                Debug.Log("Waiting");
            }

        };
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            float timeSinceLastClick = Time.time - lastClickTime;

            if (timeSinceLastClick <= doubleClickTime) { fullscreenonoff(); }

            else
                Debug.Log("Normal click");

            lastClickTime = Time.time;
        }
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
            if (!Islooping && end)
            {

                Stop();
            }
            JumpToFrame(485);
            Play();
            IdleWating = false;
            shouldStartPlaying = false;
            loopIdle = false;
            end = false;
        }

        #region idle looping logic
        if (loopIdle == true)
        {
            if (vPlayer.frame == 480)
            {

                Pause();
                JumpToFrame(0);
                Play();
                IdleWating = true;
                loopIdle = false;
                Islooping = true;


            }

        }
        if (IdleWating && vPlayer.isPrepared)
        {


            if (!Islooping)
            {

                Stop();
                Islooping = true;
            }
            JumpToFrame(0);
            Play();
            loopIdle = true;
            Debug.LogError("IdleWating");
            IdleWating = false;
        }
        #endregion

        //send event
        if (!sendLogo && vPlayer.frame == 1920)
        {
            Debug.LogError("VideoPlayer Finish Play");
            ws.Send("ChangeState:2");
            sendLogo = true;

        }
        //end frame
        if (!end && vPlayer.frame == 2570)
        {

            IdleWating = false;
            Pause();
            end = true;
            loopIdle = false;
            Islooping = false;
        }

        if (!sendState4 && vPlayer.frame == 600)
        {
            Debug.LogError("sendState4");
            ws.Send("ChangeState:4");
            sendState4 = true;

        }
        if (!sendState3 && vPlayer.frame == 1200)
        {
            Debug.LogError("sendState3");
            ws.Send("ChangeState:3");
            sendState3 = true;

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
                Thread.Sleep(3000);
                ws.Connect();
            }
        }
    }

    public void fullscreenonoff()
    {
        if (Screen.fullScreen == true)
        {
            Screen.fullScreen = false;
        }
        else
        {
            Screen.fullScreen = true;
        }
    }
}
