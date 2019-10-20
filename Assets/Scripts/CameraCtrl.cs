using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class CameraCtrl : MonoBehaviour
{
    Vector3                     startPos;
    float                       shakeTime;
    float                       shakePower;
    PostProcessProfile          profile;
    ChromaticAberration         chromaticAberration;

    static CameraCtrl instance;

    void Awake()
    {
        instance = this;
        startPos = transform.position;

        PostProcessVolume volume = GetComponentInChildren<PostProcessVolume>();
        profile = volume.profile;

        chromaticAberration = profile.GetSetting<ChromaticAberration>();
    }

    void Update()
    {
        if (shakeTime > 0)
        {
            shakeTime -= Time.deltaTime;
        }
        else
        {
            if (shakePower > 0)
            {
                shakePower = Mathf.Max(0, shakePower - Time.deltaTime * 160.0f);
            }
        }

        if (shakePower > 0)
        {
            float x = Mathf.PerlinNoise(Time.time * 123.1f, 0.0f) * 2.0f - 1.0f;
            float y = Mathf.PerlinNoise(0.0f, Time.time * 134.3f) * 2.0f - 1.0f;
            float p = Mathf.PerlinNoise(0.0f, Time.time * 100.6666f);

            Vector3 shakeDir = new Vector3(x, y, 0);
            shakeDir.Normalize();
            shakeDir = shakeDir + p * shakePower * shakeDir;

            transform.position = startPos + shakeDir;

            chromaticAberration.intensity.value = p;
        }
        else
        {
            transform.position = startPos;
            chromaticAberration.intensity.value = 0.0f;
        }
    }

    public void RunBeat()
    {
        instance.shakeTime = 0.05f;
        instance.shakePower = 10.0f;
    }

    public static void Shake(float time, float power)
    {
        instance.shakeTime = time;
        instance.shakePower = power;
    }
}
