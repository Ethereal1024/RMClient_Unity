using System.IO;
using ProtoBuf;
using UnityEngine;

public abstract class MsgProcessorBase : MonoBehaviour {
    public readonly string topic;
    public static MsgProcessorBase instance;
    public abstract void Process(byte[] payload);

    protected MsgProcessorBase(string topic) {
        this.topic = topic;
    }

    void Awake() {
        if (instance == null) instance = this;
        else Destroy(instance);
        MsgManager.instance.Register(instance);
    }
}

abstract class MsgProcessor<T> : MsgProcessorBase {
    protected abstract void Callback(T data);

    public MsgProcessor(string topic) : base(topic) {}

    public override void Process(byte[] payload) {
        using (var stream = new MemoryStream(payload)) {
            T data = Serializer.Deserialize<T>(stream);
            Callback(data);
        }
    }
}
