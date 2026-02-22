using System;
using UnityEngine;

[Serializable]
public class KeyCodePair {
    public KeyCode keyCode;
    [Range(0, 31)]
    public int bitPos;
}

public class KeyboardController : MonoBehaviour {

    public float mouseDPI = 100f;
    public KeyCodePair[] keyCodeMap;
    public byte[] dataBuffer;

    [SerializeField] private string controlTopic = "RemoteControl";

    void Update() {
        HandleInput();
    }

    void HandleInput() {
        uint keyboardState = 0;

        foreach (var pair in keyCodeMap) {
            if (pair.bitPos < 0 || pair.bitPos >= 32) continue;
            if (Input.GetKey(pair.keyCode)) keyboardState |= 1U << pair.bitPos;
        }

        float mouseX = Input.GetAxis("Mouse X") * mouseDPI;
        float mouseY = Input.GetAxis("Mouse Y") * mouseDPI;
        float mouseZ = Input.GetAxis("Mouse ScrollWheel") * mouseDPI;

        bool leftButtonDown = Input.GetMouseButton(0);
        bool rightButtonDown = Input.GetMouseButton(1);

        var message = new RMMsgs.RemoteControlData {
            mouse_x = (int)mouseX,
            mouse_y = (int)mouseY,
            mouse_z = (int)mouseZ,
            left_button_down = leftButtonDown,
            right_button_down = rightButtonDown,
            keyboard_value = keyboardState,
            mid_button_down = false,
            data = dataBuffer
        };
        MsgManager.instance.Publish(controlTopic, message);
    }
}
