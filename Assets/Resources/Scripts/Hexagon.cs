using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Hexagon : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] List<RawImage> images;
    [SerializeField] TextMeshProUGUI number;
    [SerializeField] Instructions ins;

    public int isInput = -1;
    public int isOutput = -1;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        number.rectTransform.rotation = Quaternion.identity;
    }

    public void rotate(int sign, int amount)
    {
        //clockwise
        if(sign == 0)
        {
            for(int i = 0; i < amount; i++)
            {
                RawImage temp = images[5];
                images.RemoveAt(5);
                images.Insert(0, temp);
            }
        }
        else
        {
            for (int i = 0; i < amount; i++)
            {
                RawImage temp = images[0];
                images.RemoveAt(0);
                images.Add(temp);
            }
        }
    }

    public void take(int dir, float time, Color c)
    {
        if (!gameObject.activeSelf)
        {
            return;
        }

        if (colorDifference(c, Color.white) < .1f)
        {
            Instantiate(Resources.Load<ColorFader>("Prefabs/ColorFader")).set(images[dir], Color.gray, time);
            return;
        }

        if (colorDifference(images[dir].color, Color.gray) < .1f)
        {
            return;
        }


        Color sub = images[dir].color - c;
        sub = new Color(Mathf.Round(Mathf.Clamp01(sub.r)), Mathf.Round(Mathf.Clamp01(sub.g)), Mathf.Round(Mathf.Clamp01(sub.b)));
        if (colorDifference(sub, Color.black) < .1f)
        {
            sub = Color.gray;
        }
        Instantiate(Resources.Load<ColorFader>("Prefabs/ColorFader")).set(images[dir], sub, time);
    }

    public Color getColor(int dir)
    {
        return images[dir-1].color;
    }

    public float colorDifference(Color c1, Color c2)
    {
        return Mathf.Abs(Mathf.Clamp01(c1.r) - Mathf.Clamp01(c2.r)) + Mathf.Abs(Mathf.Clamp01(c1.g) - Mathf.Clamp01(c2.g)) + Mathf.Abs(Mathf.Clamp01(c1.b) - Mathf.Clamp01(c2.b));
    }

    public void give(int dir, float time, Color c)
    {
        if (!gameObject.activeSelf)
        {
            return;
        }

        if(colorDifference(c, Color.gray) < .1f)
        {
            return;
        }

        if(isOutput >= 0)
        {
            giveAllForce(time, c);
            return;
        }

        if(colorDifference(images[dir].color, Color.gray) < .1f)
        {
            Instantiate(Resources.Load<ColorFader>("Prefabs/ColorFader")).set(images[dir], c, time);
            return;
        }


        Color add = c + images[dir].color;
        add = new Color(Mathf.Round(Mathf.Clamp01(add.r)), Mathf.Round(Mathf.Clamp01(add.g)), Mathf.Round(Mathf.Clamp01(add.b)));
        Instantiate(Resources.Load<ColorFader>("Prefabs/ColorFader")).set(images[dir], add, time);
    }

    public void giveAll(float time, Color c)
    {
        for(int i = 0; i < 6; i++)
        {
            give(i, time, c);
        }
    }

    public void giveAllForce(float time, Color c)
    {
        Instantiate(Resources.Load<ColorFader>("Prefabs/ColorFader")).set(images[0], c, time);
        Instantiate(Resources.Load<ColorFader>("Prefabs/ColorFader")).set(images[1], c, time);
        Instantiate(Resources.Load<ColorFader>("Prefabs/ColorFader")).set(images[2], c, time);
        Instantiate(Resources.Load<ColorFader>("Prefabs/ColorFader")).set(images[3], c, time);
        Instantiate(Resources.Load<ColorFader>("Prefabs/ColorFader")).set(images[4], c, time);
        Instantiate(Resources.Load<ColorFader>("Prefabs/ColorFader")).set(images[5], c, time);
    }

    public void takeAll(float time, Color c)
    {
        for (int i = 0; i < 6; i++)
        {
            take(i, time, c);
        }
    }

    public bool hasAny(Color c)
    {
        Debug.Log(colorDifference(c, Color.green));

        for(int i = 0; i < 6; i++)
        {
            if(colorDifference(c, Color.red) < .1f && images[i].color.r > .8f)
            {
                return true;
            }

            if (colorDifference(c, Color.green) < .1f && images[i].color.g > .8f)
            {
                Debug.Log("true");
                return true;
            }

            if (colorDifference(c, Color.blue) < .1f && images[i].color.b > .8f)
            {
                return true;
            }
        }
        Debug.Log("false");
        return false;
    }
}
