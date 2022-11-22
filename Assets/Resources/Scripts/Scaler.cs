using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scaler : MonoBehaviour
{
    RectTransform obj;
    float duration;
    Vector2 sourceScale;
    Vector2 destScale;
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

        obj.localScale = new Vector3(function(sourceScale.x, destScale.x, time), function(sourceScale.y, destScale.y, time), 1);
        if(time >= 1)
        {
            obj.localScale = new Vector3(destScale.x, destScale.y, 1);
            Destroy(gameObject);
        }
    }

    public void set(RectTransform o, Vector2 dS, float dur)
    {
        obj = o;
        sourceScale = o.localScale;
        destScale = dS;
        
        duration = dur;

        if(dur < .1f)
        {
            o.localScale = new Vector3(dS.x, dS.y, 1);
            Destroy(gameObject);
        }
    }
}
