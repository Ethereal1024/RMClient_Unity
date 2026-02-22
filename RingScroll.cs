using UnityEngine;
using UnityEngine.UI;

public class RingScroll : MonoBehaviour {

    [Range(0, 100)] public float progress;

    [SerializeField] private float beginAngle;
    [SerializeField] private float totalRange;
    [SerializeField] private float minProgress = 0;

    private GameObject ring;

    void Awake() {
        ring = transform.GetChild(0).gameObject;
    }

    void Update() {
        Image img = ring.GetComponent<Image>();
        img.fillAmount = progress * totalRange * 0.01f / 360;
    }

    void OnValidate() {
        ring = transform.GetChild(0).gameObject;
        ring.transform.localRotation = Quaternion.Euler(0, -180, -beginAngle);
        Image img = ring.GetComponent<Image>();
        img.fillAmount = progress * totalRange * 0.01f / 360;
    }
}
