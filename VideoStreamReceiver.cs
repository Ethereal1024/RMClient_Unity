using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class VideoStreamReceiver : MonoBehaviour {
    [Header("UDP配置")]
    [SerializeField] private int udpPort = 3334;
    [SerializeField] private string localIP = "192.168.12.100";

    [Header("数据流设置")]
    [SerializeField] private float packetTimeout = 2.0f;
    [SerializeField] private bool enableAutoCleanup = true;

    [Header("调试配置")]
    [SerializeField] private bool enableLog = true;
    [SerializeField] private int logFrameInterval = 30;

    private UdpClient udpClient;
    private Thread receiveThread;
    private bool isReceiving = false;

    private class VideoFrame {
        public ushort frameNumber;
        public ushort totalPackets;
        public uint totalBytes;
        public Dictionary<ushort, byte[]> fragments = new();
        public uint receivedBytes = 0;
        public float lastPacketTime;

        public bool IsComplete() {
            return fragments.Count == totalPackets;
        }
    }

    private Dictionary<ushort, VideoFrame> frameBuffer = new();
    private ushort expectedFrameNumber = 1;
    private int packetsReceived = 0;

    private class FrameTracker {
        public ushort frameNumber;
        public float startTime;
        public int expectedPackets;
        public int receivedPackets;
        public bool isComplete;
    }

    private Dictionary<ushort, FrameTracker> frameTrackers = new();

    public event Action<byte[]> OnFrameReceived;
    public event Action<float> OnFrameRateUpdated;

    private float fpsUpdateTime = 0f;
    private int frameCounter = 0;
    private float currentFPS = 0f;

    void Start() {
        UnityMainThreadDispatcher.Instance();
        InitializeUDPReceiver();
        StartCoroutine(CleanupCoroutine());
    }

    void InitializeUDPReceiver() {
        try {
            IPAddress localAddress = IPAddress.Parse(localIP);
            IPEndPoint localEndPoint = new IPEndPoint(localAddress, udpPort);

            udpClient = new UdpClient();
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.Client.Bind(localEndPoint);
            udpClient.Client.ReceiveBufferSize = 1024 * 1024;

            isReceiving = true;
            receiveThread = new Thread(new ThreadStart(ReceiveData)) {
                IsBackground = true,
                Priority = System.Threading.ThreadPriority.Highest
            };
            receiveThread.Start();

            Log($"视频流接收器已启动，监听端口：{udpPort}");
        }
        catch (Exception e) {
            LogError($"初始化UDP接收器失败：{e.Message}");
        }
    }

    private void ReceiveData() {
        byte[] buffer = new byte[65535];

        while (isReceiving) {
            try {
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                IAsyncResult result = udpClient.BeginReceive(null, null);
                byte[] receivedData = udpClient.EndReceive(result, ref remoteEndPoint);

                packetsReceived++;
                if (receivedData.Length >= 8) {
                    ProcessVideoPacket(receivedData);
                }
                if (packetsReceived % 1000 == 0 && enableLog) {
                    Log($"已接收 {packetsReceived} 个数据包");
                }
            }
            catch (SocketException e) {
                if (isReceiving) LogError($"Socket异常：{e.Message}");
            }
            catch (Exception e) {
                LogError($"接收数据异常：{e.Message}");
            }
        }
    }

    void ProcessVideoPacket(byte[] packetData) {
        ushort frameNumber = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(packetData, 0));
        ushort fragmentIndex = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(packetData, 2));
        uint totalBytes = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(packetData, 4));

        Log($"帧编号：{frameNumber}，帧内分片序号：{fragmentIndex}，帧总字节数：{totalBytes}");

        float currentTime = Environment.TickCount;

        byte[] videoData = new byte[packetData.Length - 8];
        Buffer.BlockCopy(packetData, 8, videoData, 0, videoData.Length);

        lock (frameBuffer) {
            if (!frameBuffer.ContainsKey(frameNumber)) {
                // 新帧初始化
                frameBuffer[frameNumber] = new VideoFrame {
                    frameNumber = frameNumber,
                    totalPackets = 0,
                    totalBytes = totalBytes,
                    lastPacketTime = currentTime
                };
                frameTrackers[frameNumber] = new FrameTracker {
                    frameNumber = frameNumber,
                    startTime = currentTime,
                    expectedPackets = 0,
                    receivedPackets = 0,
                    isComplete = false
                };
                if (frameNumber % logFrameInterval == 0) {
                    Log($"开始接收新帧 #{frameNumber}");
                }
            }
            VideoFrame currentFrame = frameBuffer[frameNumber];
            FrameTracker tracker = frameTrackers[frameNumber];
            currentFrame.lastPacketTime = currentTime;

            // 若为该帧的首个分片，则计算该帧的总分片数
            if (fragmentIndex == 0) {
                int payloadSize = videoData.Length;
                int totalPackets = (int)Math.Ceiling((double)totalBytes / payloadSize);
                currentFrame.totalPackets = (ushort)totalPackets;
                tracker.expectedPackets = totalPackets;
            }
            if (!currentFrame.fragments.ContainsKey(fragmentIndex)) {
                currentFrame.fragments[fragmentIndex] = videoData;
                currentFrame.receivedBytes += (uint)videoData.Length;
                tracker.receivedPackets++;

                if (currentFrame.IsComplete()) {
                    tracker.isComplete = true;
                    AssembleCompleteFrame(currentFrame);

                    frameBuffer.Remove(frameNumber);
                    frameTrackers.Remove(frameNumber);

                    frameCounter++;

                    if (frameNumber == expectedFrameNumber) {
                        expectedFrameNumber++;
                    }
                }
            }

            if (currentFrame.totalPackets > 0 && tracker.receivedPackets < currentFrame.totalPackets) {
                if (currentTime - currentFrame.lastPacketTime > 0.5f && tracker.receivedPackets > 0) {
                    frameBuffer.Remove(frameNumber);
                    frameTrackers.Remove(frameNumber);
                }
            }
        }
    }

    private void AssembleCompleteFrame(VideoFrame frame) {
        List<ushort> sortedIndices = new(frame.fragments.Keys);
        sortedIndices.Sort();

        int totalSize = 0;
        foreach (var index in sortedIndices) {
            totalSize += frame.fragments[index].Length;
        }

        byte[] completeFrame = new byte[totalSize];
        int offset = 0;
        foreach (var index in sortedIndices) {
            byte[] fragment = frame.fragments[index];
            Buffer.BlockCopy(fragment, 0, completeFrame, offset, fragment.Length);
            offset += fragment.Length;
        }

        if (frame.frameNumber % logFrameInterval == 0) {
            Log($"帧 #{frame.frameNumber} 组装完成， 大小：{completeFrame.Length} 字节");
        }

        UnityMainThreadDispatcher.Instance().Enqueue(() => {
            OnFrameReceived?.Invoke(completeFrame);
        });
    }

    private IEnumerator CleanupCoroutine() {
        while (true) {
            yield return new WaitForSeconds(1.0f);
            if (!enableAutoCleanup) continue;
            lock (frameBuffer) {
                List<ushort> framesToRemove = new();
                float currentTime = Environment.TickCount;

                foreach (var kvp in frameBuffer) {
                    if (currentTime - kvp.Value.lastPacketTime > packetTimeout) {
                        framesToRemove.Add(kvp.Key);
                    }
                }
                foreach (var frameNumber in framesToRemove) {
                    frameBuffer.Remove(frameNumber);
                    frameTrackers.Remove(frameNumber);
                    if (enableLog) {
                        Log($"清理超时帧 #{frameNumber}");
                    }
                }
            }
        }
    }

    void Update() {
        fpsUpdateTime += Time.deltaTime;
        if (fpsUpdateTime >= 1.0f) {
            currentFPS = frameCounter / fpsUpdateTime;
            frameCounter = 0;
            fpsUpdateTime = 0f;

            OnFrameRateUpdated?.Invoke(currentFPS);

            if (Time.time % 5 < Time.deltaTime && enableLog) {
                Log($"状态：FPS={currentFPS:F1}，缓冲帧={frameBuffer.Count}，接收包={packetsReceived}");
            }
        }
    }

    private void Log(string message) {
        if (!enableLog) return;
        UnityMainThreadDispatcher.Instance().Enqueue(() => {
            Debug.Log($"[ VideoStream ] {message}");
        });
    }

    private void LogError(string message) {
        if (!enableLog) return;
        UnityMainThreadDispatcher.Instance().Enqueue(() => {
            Debug.LogError($"[ VideoStream ] {message}");
        });
    }

    void OnDestroy() {
        StopReceiving();
    }

    void OnApplicationQuit() {
        StopReceiving();
    }

    private void StopReceiving() {
        isReceiving = false;
        if (udpClient != null) {
            udpClient.Close();
            udpClient = null;
        }

        if (receiveThread != null && receiveThread.IsAlive) {
            receiveThread.Join(1000);
        }

        Log("视频流接收器已停止");
    }

    public (float fps, int bufferedFrames, int totalPackets) GetStats() {
        lock (frameBuffer) {
            return (currentFPS, frameBuffer.Count, packetsReceived);
        }
    }

    public void ResetStats() {
        lock (frameBuffer) {
            frameBuffer.Clear();
            frameTrackers.Clear();
            packetsReceived = 0;
            frameCounter = 0;
            currentFPS = 0;
            expectedFrameNumber = 1;
        }
    }

    public bool IsReceiving => isReceiving;
    public int BufferFrameCount => frameBuffer.Count;
    public float CurrentFPS => currentFPS;
}
