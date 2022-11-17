using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorFader : MonoBehaviour
{
    RawImage obj;
    float duration;
    Color sourceColor;
    Color destColor;
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

        obj.color = new Color(function(sourceColor.r, destColor.r, time), function(sourceColor.g, destColor.g, time), function(sourceColor.b, destColor.b, time));
        if (time >= 1)
        {
            obj.color = destColor;
            Destroy(gameObject);
        }
    }

    public void set(RawImage o, Color dest, float dur)
    {
        obj = o;
        sourceColor = o.color;
        destColor = dest;
        duration = dur;

        if (dur == 0)
        {
            o.color = dest;
            Destroy(gameObject);
        }
    }
}
