using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Rotator : MonoBehaviour
{
    RectTransform obj;
    float duration;
    float sourceRot;
    float destRot;
    float time = 0;
    EasingFunction.Function function;


    // Start is called before the first frame update
    void Start()
    {
        EasingFunction.Ease movement = EasingFunction.Ease.EaseOutBack;
        function = EasingFunction.GetEasingFunction(movement);
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime / duration;

        obj.localRotation = Quaternion.Euler(0, 0, function(sourceRot, destRot, time));
        if(time >= 1)
        {
            obj.localRotation = Quaternion.Euler(0, 0, destRot);
            Destroy(gameObject);
        }
    }

    public void set(RectTransform o, float rotAmount, float dur)
    {
        obj = o;
        sourceRot = o.localRotation.eulerAngles.z;
        destRot = sourceRot + rotAmount;
        duration = dur;

        if(dur < .1f)
        {
            o.localRotation = Quaternion.Euler(0, 0, destRot);
            Destroy(gameObject);
        }
    }
}
