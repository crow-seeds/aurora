using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Manual : MonoBehaviour
{
    // Start is called before the first frame update
    int pageNum = 1;
    [SerializeField] GameObject leftArrow;
    [SerializeField] GameObject rightArrow;
    [SerializeField] TextMeshProUGUI pageText;
    [SerializeField] RawImage pageImage;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void pageLeft()
    {
        if(pageNum > 1)
        {
            GetComponent<AudioSource>().PlayOneShot(Resources.Load<AudioClip>("SoundEffects/beep" + UnityEngine.Random.Range(0, 4).ToString()), .5f);
            pageNum--;
            pageText.text = "PAGE " + pageNum.ToString();
            pageImage.texture = Resources.Load<Texture>("Sprites/Manual/manual-" + pageNum.ToString());
            rightArrow.SetActive(true);
            if(pageNum == 1)
            {
                leftArrow.SetActive(false);
            }
        }
    }

    public void pageRight()
    {
        if (pageNum < 13)
        {
            GetComponent<AudioSource>().PlayOneShot(Resources.Load<AudioClip>("SoundEffects/beep" + UnityEngine.Random.Range(0, 4).ToString()), .5f);
            pageNum++;
            pageText.text = "PAGE " + pageNum.ToString();
            pageImage.texture = Resources.Load<Texture>("Sprites/Manual/manual-" + pageNum.ToString());
            leftArrow.SetActive(true);
            if (pageNum == 13)
            {
                rightArrow.SetActive(false);
            }
        }
    }

    public void viewPDF()
    {
        GetComponent<AudioSource>().PlayOneShot(Resources.Load<AudioClip>("SoundEffects/beep" + UnityEngine.Random.Range(0, 4).ToString()), .5f);
        Application.OpenURL("https://crowseeds.com/aurora/manual.pdf");
    }
}
