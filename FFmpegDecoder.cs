using UnityEngine;
using System;
using UnityEngine.UI;
using System.Threading;
using System.Collections.Concurrent;
using System.IO;
using System.Diagnostics;

public class FFmpegDecoder : MonoBehaviour {
    public VideoStreamReceiver streamReceiver;
    public RawImage displayImage;

    private Process ffmpegProcess;
    private Thread decodeOutputThread;
    private bool isDecoding = false;

    private ConcurrentQueue<byte[]> frameDataQueue = new();
    private Texture2D videoTexture;
    private int frameWidth = 1920;
    private int frameHeight = 1080;
    private int rgbFrameSize; // = width * height * 3

    void Start() {
        rgbFrameSize = frameWidth * frameHeight * 3;
        videoTexture = new Texture2D(frameWidth, frameHeight, TextureFormat.RGB24, false);
        displayImage.texture = videoTexture;

        StartFFmpegProcess();
        if (streamReceiver != null) {
            streamReceiver.OnFrameReceived += OnH265FrameReceived;
        }
    }

    void StartFFmpegProcess() {
        string ffmpegPath = Path.Combine(Application.streamingAssetsPath, "ffmpeg/ffmpeg.exe");

        ProcessStartInfo startInfo = new ProcessStartInfo {
            FileName = ffmpegPath,
            Arguments = "-f hevc -framerate 30 -i pipe:0 -f rawvideo -pix_fmt rgb24 -s 1920x1080 pipe:1 -loglevel quiet",
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        ffmpegProcess = new Process { StartInfo = startInfo };
        ffmpegProcess.ErrorDataReceived += (sender, e) => {
            if (!string.IsNullOrEmpty(e.Data))
                UnityEngine.Debug.LogError($"FFmpeg Error: {e.Data}");
        };

        if (ffmpegProcess.Start()) {
            ffmpegProcess.BeginErrorReadLine();
            isDecoding = true;
            decodeOutputThread = new Thread(DecodeOutputThread) { IsBackground = true };
            decodeOutputThread.Start();
        }
    }

    private void OnH265FrameReceived(byte[] h265FrameData) {
        try {
            ffmpegProcess.StandardInput.BaseStream.Write(h265FrameData, 0, h265FrameData.Length);
            ffmpegProcess.StandardInput.BaseStream.Flush();
        }
        catch (Exception e) {
            UnityEngine.Debug.LogError($"写入 FFmpeg 失败: {e.Message}");
        }
    }

    private void DecodeOutputThread() {
        byte[] buffer = new byte[rgbFrameSize];
        Stream outputStream = ffmpegProcess.StandardOutput.BaseStream;

        while (isDecoding && !ffmpegProcess.HasExited) {
            try {
                int totalRead = 0;
                while (totalRead < rgbFrameSize) {
                    int bytesRead = outputStream.Read(buffer, totalRead, rgbFrameSize - totalRead);
                    if (bytesRead <= 0) break;
                    totalRead += bytesRead;
                }

                if (totalRead == rgbFrameSize) {
                    // 成功读取一帧 RGB 数据
                    byte[] frameCopy = new byte[rgbFrameSize];
                    Buffer.BlockCopy(buffer, 0, frameCopy, 0, rgbFrameSize);
                    frameDataQueue.Enqueue(frameCopy); // 入队，等待主线程处理
                }
            }
            catch (Exception e) {
                UnityEngine.Debug.LogError($"解码输出线程错误: {e.Message}");
                break;
            }
        }
    }

    void Update() {
        if (frameDataQueue.TryDequeue(out byte[] rgbFrame)) {
            videoTexture.LoadRawTextureData(rgbFrame);
            videoTexture.Apply();
        }
    }

    void OnDestroy() {
        isDecoding = false;
        if (streamReceiver != null) streamReceiver.OnFrameReceived -= OnH265FrameReceived;

        if (ffmpegProcess != null && !ffmpegProcess.HasExited) {
            ffmpegProcess.StandardInput.Close();
            ffmpegProcess.WaitForExit(1000);
            ffmpegProcess.Kill();
            ffmpegProcess.Dispose();
        }
        decodeOutputThread?.Join(1000);
    }
}