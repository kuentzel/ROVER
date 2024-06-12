using UnityEngine;

public class TrackedObjectManager : MonoBehaviour
{
    public Transform tracker;
    public Transform headset;

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(tracker.position.x, 0, tracker.position.z);
        transform.LookAt(new Vector3(headset.position.x, 0, headset.position.z));
    }

    public void ToggleObject()
    {
        gameObject.SetActive(!gameObject.activeInHierarchy);
    }
    }
