using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;


public class Intro : MonoBehaviour
{
    EasingFunction.Function function;
    bool isMoving = false;
    float timer = 0;

    [SerializeField] Transform paperPos1;
    [SerializeField] Transform paperPos2;
    [SerializeField] RawImage drawing;
    [SerializeField] RawImage drawing2;
    [SerializeField] AudioSource dialogue;

    [SerializeField] Transform page1;
    [SerializeField] Transform page2;


    // Start is called before the first frame update
    void Start()
    {
        EasingFunction.Ease movement = EasingFunction.Ease.EaseOutBack;
        function = EasingFunction.GetEasingFunction(movement);
        StartCoroutine(displayIntro());
    }

    // Update is called once per frame
    void Update()
    {
        if (isMoving)
        {
            timer += Time.deltaTime;
            paperPos2.localPosition = new Vector2(0, function(0, 900, timer));
            if(timer >= 1)
            {
                timer = 0;
                isMoving = false;
            }
        }
        
        if((Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Space)))
        {
            skipIntro();
        }
    }

    IEnumerator displayIntro()
    {
        yield return new WaitWhile(() => !(dialogue.clip.loadState.Equals(AudioDataLoadState.Loaded)));
        yield return new WaitForSeconds(3f); //temp time
        drawing.texture = Resources.Load<Texture>("Sprites/Intro/1");
        dialogue.PlayOneShot(Resources.Load<AudioClip>("SoundEffects/page_write"), .5f);
        yield return new WaitForSeconds(2f); //temp time
        drawing.texture = Resources.Load<Texture>("Sprites/Intro/2");
        dialogue.PlayOneShot(Resources.Load<AudioClip>("SoundEffects/page_write"), .5f);
        yield return new WaitForSeconds(1f); //temp time
        drawing2.texture = Resources.Load<Texture>("Sprites/Intro/3");
        dialogue.PlayOneShot(Resources.Load<AudioClip>("SoundEffects/page_turn"), .5f);
        isMoving = true;
        yield return new WaitForSeconds(1f);
        drawing.texture = Resources.Load<Texture>("Sprites/Intro/3");
        page1.Rotate(new Vector3(0, 0, 180));
        page2.Rotate(new Vector3(0, 0, 180));
        paperPos2.localPosition = new Vector2(0, 0);
        yield return new WaitForSeconds(1f); //temp time
        drawing.texture = Resources.Load<Texture>("Sprites/Intro/4");
        dialogue.PlayOneShot(Resources.Load<AudioClip>("SoundEffects/page_write"), .5f);
        yield return new WaitForSeconds(1.5f); //temp time
        drawing2.texture = Resources.Load<Texture>("Sprites/Intro/5");
        dialogue.PlayOneShot(Resources.Load<AudioClip>("SoundEffects/page_turn"), .5f);
        isMoving = true;
        yield return new WaitForSeconds(1f);
        page1.Rotate(new Vector3(0, 0, 180));
        page2.Rotate(new Vector3(0, 0, 180));
        drawing.texture = Resources.Load<Texture>("Sprites/Intro/5");
        paperPos2.localPosition = new Vector2(0, 0);
        yield return new WaitForSeconds(1.5f); //temp time
        drawing.texture = Resources.Load<Texture>("Sprites/Intro/6");
        dialogue.PlayOneShot(Resources.Load<AudioClip>("SoundEffects/page_write"), .5f);
        yield return new WaitForSeconds(6f); //temp time
        drawing2.texture = Resources.Load<Texture>("Sprites/Intro/7");
        dialogue.PlayOneShot(Resources.Load<AudioClip>("SoundEffects/page_turn"), .5f);
        isMoving = true;
        yield return new WaitForSeconds(2f);
        page1.Rotate(new Vector3(0, 0, 180));
        page2.Rotate(new Vector3(0, 0, 180));
        drawing.texture = Resources.Load<Texture>("Sprites/Intro/7");
        paperPos2.localPosition = new Vector2(0, 0);
        yield return new WaitForSeconds(3f); //temp time
        drawing2.texture = Resources.Load<Texture>("Sprites/Intro/8");
        dialogue.PlayOneShot(Resources.Load<AudioClip>("SoundEffects/page_turn"), .5f);
        isMoving = true;
        yield return new WaitForSeconds(2f);
        page1.Rotate(new Vector3(0, 0, 180));
        page2.Rotate(new Vector3(0, 0, 180));
        drawing.texture = Resources.Load<Texture>("Sprites/Intro/8");
        paperPos2.localPosition = new Vector2(0, 0);
        yield return new WaitForSeconds(3f); //temp time
        drawing2.texture = Resources.Load<Texture>("Sprites/Intro/9");
        dialogue.PlayOneShot(Resources.Load<AudioClip>("SoundEffects/page_turn"), .5f);
        isMoving = true;
        yield return new WaitForSeconds(2f);
        page1.Rotate(new Vector3(0, 0, 180));
        page2.Rotate(new Vector3(0, 0, 180));
        drawing.texture = Resources.Load<Texture>("Sprites/Intro/9");
        paperPos2.localPosition = new Vector2(0, 0);
        yield return new WaitForSeconds(4f); //temp time
        drawing.texture = Resources.Load<Texture>("Sprites/Intro/10");
        dialogue.PlayOneShot(Resources.Load<AudioClip>("SoundEffects/page_write"), .5f);
        yield return new WaitForSeconds(4f); //temp time
        page2.GetComponent<RawImage>().texture = Resources.Load<Texture>("Sprites/paper_invert");
        drawing2.texture = Resources.Load<Texture>("Sprites/Intro/11");
        dialogue.PlayOneShot(Resources.Load<AudioClip>("SoundEffects/page_turn"), .5f);
        isMoving = true;
        yield return new WaitForSeconds(2f);
        page1.Rotate(new Vector3(0, 0, 180));
        page2.Rotate(new Vector3(0, 0, 180));
        drawing.texture = Resources.Load<Texture>("Sprites/Intro/11");
        page1.GetComponent<RawImage>().texture = Resources.Load<Texture>("Sprites/paper_invert");
        paperPos2.localPosition = new Vector2(0, 0);
        yield return new WaitForSeconds(4.5f);
        cam.backgroundColor = Color.black;
        paperPos1.gameObject.SetActive(false);
        drawing.gameObject.SetActive(false);
        yield return new WaitWhile(() => gameObject.GetComponent<AudioSource>().isPlaying);
        PlayerPrefs.SetInt("seenIntro", 1);
        SceneManager.LoadScene("LevelSelect");



    }

    public void skipIntro()
    {
        if(PlayerPrefs.GetInt("seenIntro", 0) == 1)
        {
            SceneManager.LoadScene("LevelSelect");
        }
    }

    [SerializeField] Camera cam;
}
