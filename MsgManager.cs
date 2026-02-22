using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Packets;
using MQTTnet.Protocol;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

using UnityEngine;

using ProtoBuf;

public class MsgManager : MonoBehaviour {
    [Header("MQTT配置")]
    [SerializeField] private string serverIP = "192.168.12.1";
    [SerializeField] private int serverPort = 3333;
    [SerializeField] private string clientId = "RMQianliClient";

    private IMqttClient mqttClient;
    private bool isConnected = false;

    [Header("调试配置")]
    [SerializeField] private bool enableLogging = true;

    private Dictionary<string, MsgProcessorBase> processors;

    public event Action<string, byte[]> OnMessageReceived;
    public event Action<bool> OnConnectionStatusChanged;

    public static MsgManager instance;

    void Awake() {
        if (instance == null) instance = this;
        else Destroy(instance);
        processors = new Dictionary<string, MsgProcessorBase>();
    }

    void Start() {
        InitialzeMqttClient();
    }

    async void InitialzeMqttClient() {
        try {
            var factory = new MqttFactory();
            mqttClient = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(serverIP, serverPort)
                .WithClientId(clientId)
                .WithCleanSession()
                .WithTimeout(TimeSpan.FromSeconds(5))
                .Build();

            mqttClient.ConnectedAsync += OnConnectedAsync;
            mqttClient.DisconnectedAsync += OnDisconnectedAsync;
            mqttClient.ApplicationMessageReceivedAsync += OnMessageReceiveAsync;

            Log("正在连接MQTT服务器...");
            var result = await mqttClient.ConnectAsync(options, CancellationToken.None);

            if (result.ResultCode != MqttClientConnectResultCode.Success) {
                LogError($"连接失败：{result.ResultCode}");
                return;
            }
        }
        catch (Exception ex) {
            LogError($"初始化失败：{ex.Message}");
        }
    }

    private Task OnConnectedAsync(MqttClientConnectedEventArgs arg) {
        isConnected = true;
        Log("MQTT连接成功");
        OnConnectionStatusChanged?.Invoke(true);
        SubscribeTopics();
        return Task.CompletedTask;
    }

    private Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs arg) {
        isConnected = false;
        Log("MQTT连接断开");
        OnConnectionStatusChanged?.Invoke(false);
        return Task.CompletedTask;
    }

    private Task OnMessageReceiveAsync(MqttApplicationMessageReceivedEventArgs arg) {
        try {
            var topic = arg.ApplicationMessage.Topic;
            var payload = arg.ApplicationMessage.PayloadSegment.ToArray();

            Log($"收到消息 - 主题：{topic}，长度：{payload.Length}字节");
            OnMessageReceived?.Invoke(topic, payload);
            ProcessMessage(topic, payload);
        }
        catch (Exception ex) {
            LogError($"消息处理错误：{ex.Message}");
        }
        return Task.CompletedTask;
    }

    private void SubscribeTopics() {
        foreach (var (topic, _) in processors) {
            Subscribe(topic);
        }
    }

    public async void Subscribe(string topic) {
        if (mqttClient == null || !isConnected) return;
        try {
            var result = await mqttClient.SubscribeAsync(new MqttTopicFilter {
                Topic = topic,
                QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce
            });

            Log($"订阅主题成功：{topic}");
        }
        catch (Exception ex) {
            LogError($"订阅失败 {topic}: {ex.Message}");
        }
    }

    public async void Publish(string topic, byte[] payload) {
        if (mqttClient == null || !isConnected) return;
        try {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag(false)
                .Build();

            await mqttClient.PublishAsync(message);
            Log($"发布消息成功 - 主题：{topic}，长度：{payload.Length}字节");
        }
        catch (Exception ex) {
            LogError($"发布失败 {topic}: {ex.Message}");
        }
    }

    public async void Publish<T>(string topic, T message) {
        using (var stream = new MemoryStream()) {
            Serializer.Serialize(stream, message);
            var payload = stream.ToArray();
            Publish(topic, payload);
        }
    }

    public void Register(MsgProcessorBase processor) {
        processors[processor.topic] = processor;
        Log($"注册成功：{processor.topic}");
    }

    private void ProcessMessage(string topic, byte[] payload) {
        try {
            if (processors.TryGetValue(topic, out MsgProcessorBase processor)) {
                processor.Process(payload);
            }
        }
        catch (Exception ex) {
            LogError($"消息处理错误 {topic}: {ex.Message}");
        }
    }

    async void OnDestroy() {
        if (mqttClient != null && isConnected) {
            await mqttClient.DisconnectAsync();
            mqttClient.Dispose();
        }
    }

    private void Log(string message) {
        if (enableLogging) Debug.Log($"[ MsgManager ] {message}");
    }

    private void LogError(string message) {
        Debug.LogError($"[ MsgManager ] {message}");
    }
}
