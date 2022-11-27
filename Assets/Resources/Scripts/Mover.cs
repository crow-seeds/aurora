using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Mover : MonoBehaviour
{
    RectTransform obj;
    float duration;
    Vector2 sourceLoc;
    Vector2 destLoc;
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

        obj.localPosition = new Vector3(function(sourceLoc.x, destLoc.x, time), function(sourceLoc.y, destLoc.y, time), 1);
        if(time >= 1)
        {
            obj.localPosition = new Vector3(destLoc.x, destLoc.y, 1);
            Destroy(gameObject);
        }
    }

    public void set(RectTransform o, Vector2 dS, float dur)
    {
        obj = o;
        sourceLoc = o.localScale;
        destLoc = dS;
        
        duration = dur;

        if(dur < .1f)
        {
            o.localPosition = new Vector3(dS.x, dS.y, 1);
            Destroy(gameObject);
        }
    }
}
