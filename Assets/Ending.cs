using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class Ending : MonoBehaviour
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
        yield return new WaitForSeconds(4f); //temp time
        drawing2.texture = Resources.Load<Texture>("Sprites/Ending/1");
        dialogue.PlayOneShot(Resources.Load<AudioClip>("SoundEffects/page_turn"), .5f);
        isMoving = true;
        yield return new WaitForSeconds(1f);
        drawing.texture = Resources.Load<Texture>("Sprites/Ending/1");
        page1.Rotate(new Vector3(0, 0, 180));
        page2.Rotate(new Vector3(0, 0, 180));
        paperPos2.localPosition = new Vector2(0, 0);
        yield return new WaitForSeconds(3f);
        PlayerPrefs.SetInt("seenEnding", 1);
        SceneManager.LoadScene("LevelSelect");



    }

    public void skipIntro()
    {
        if(PlayerPrefs.GetInt("seenEnding", 0) == 1)
        {
            SceneManager.LoadScene("LevelSelect");
        }
    }

    [SerializeField] Camera cam;
}
