using System;
using UnityEngine;

public class Clock : MonoBehaviour
{
    [SerializeField]
    Transform hoursPivot = default;

    [SerializeField]
    Transform minutesPivot = default;

    [SerializeField]
    Transform secondsPivot = default;

    private void Update()
    {
        hoursPivot.localRotation = Quaternion.Euler(0, 0, -30 * DateTime.Now.Hour);
        minutesPivot.localRotation = Quaternion.Euler(0, 0, -6 * DateTime.Now.Minute);
        secondsPivot.localRotation = Quaternion.Euler(0, 0, -6 * DateTime.Now.Second);
    }
}
