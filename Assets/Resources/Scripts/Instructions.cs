using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Instructions : MonoBehaviour
{
    int[] instructions = new int[64];
    [SerializeField] List<Hexagon> hexagons;
    List<int> breakpoints = new List<int>();
    float timeBetweenInstructions = .3f;
    int currentInstruction = 0;
    int lastInstruction = 63;

    [SerializeField] ScrollRect scroll;
    EasingFunction.Function function;
    float timer = 0;
    bool isMoving = false;


    // Start is called before the first frame update
    void Start()
    {
        EasingFunction.Ease movement = EasingFunction.Ease.EaseOutBack;
        function = EasingFunction.GetEasingFunction(movement);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            runProgram();
        }

        if (isMoving)
        {
            if(timeBetweenInstructions > 0)
            {

            }
            else
            {
                
            }
        }
    }

    public void pressButton(ButtonParameters b)
    {
        int r = b.row;
        int c = b.col;
        b.active = !b.active;

        instructions[r] = (instructions[r] ^ (1 << c));

        if (b.active)
        {
            b.GetComponent<RawImage>().color = Color.gray;
        }
        else
        {
            b.GetComponent<RawImage>().color = Color.white;
        }
    }

    bool isRunning = false;

    public void runProgram()
    {
        if (!isRunning)
        {
            isRunning = true;
            lastInstruction = -1;
            currentInstruction = 0;
            for (int i = 63; i >= 0; i--)
            {
                if (instructions[i] != 0)
                {
                    lastInstruction = i;
                    break;
                }
            }
            Debug.Log(lastInstruction);
            StartCoroutine(runProgramHelper());
        }
        
    }

    IEnumerator runProgramHelper()
    {
        if(currentInstruction <= lastInstruction && currentInstruction <= 63)
        {
            int instruction = instructions[currentInstruction];
            int opcode = (instruction >> 7) & 0b111;
            int hex = ((instruction >> 3) & 0b1111) - 1;
            int sign = (instruction >> 2) & 0b1;
            int amt = instruction & 0b11;
            int dir = instruction & 0b111;
            switch (opcode)
            {
                case 0: //rotate
                    if(hex == -1)
                    {
                        for(int i = 0; i < hexagons.Count; i++)
                        {
                            Hexagon h = hexagons[i];
                            if (h.isActiveAndEnabled)
                            {
                                StartCoroutine(rotate(i, sign, amt));
                            }
                        }
                    }
                    else if(hex >= 0 && hex < hexagons.Count)
                    {
                        StartCoroutine(rotate(hex, sign, amt));
                    }
                    break;
                case 1: //move
                    if(hex == -1)
                    {
                        for (int i = 0; i < hexagons.Count; i++)
                        {
                            Hexagon h = hexagons[i];
                            if (h.isActiveAndEnabled)
                            {
                                StartCoroutine(move(i, dir));
                            }
                        }
                    }
                    else if(hex >= 0 && hex < hexagons.Count)
                    {
                        StartCoroutine(move(hex, dir));
                    }
                    break;
                case 2: //add-subtract
                    if(sign == 0)
                    {
                        if(hex == -1)
                        {
                            for (int i = 0; i < hexagons.Count; i++)
                            {
                                Hexagon h = hexagons[i];
                                if (h.isActiveAndEnabled)
                                {
                                    add(i, amt);
                                }
                            }
                        }
                        else
                        {
                            add(hex, amt);
                        }
                        
                    }
                    else
                    {
                        if (hex == -1)
                        {
                            for (int i = 0; i < hexagons.Count; i++)
                            {
                                Hexagon h = hexagons[i];
                                if (h.isActiveAndEnabled)
                                {
                                    sub(i, amt);
                                }
                            }
                        }
                        else
                        {
                            sub(hex, amt);
                        }
                    }
                    break;

            }
            currentInstruction++;
            yield return new WaitForSeconds(timeBetweenInstructions);


            StartCoroutine(runProgramHelper());
        }
        else
        {
            isRunning = false;
        }
        
    }

    public IEnumerator rotate(int hex, int sign, int amount)
    {
        Hexagon h = hexagons[hex];
        float rotAmount = (sign == 0 ? 1 : -1) * 60 * amount;
        Instantiate(Resources.Load<Rotator>("Prefabs/Rotator")).set(h.GetComponent<RectTransform>(), rotAmount, timeBetweenInstructions);
        yield return new WaitForSeconds(timeBetweenInstructions);
        h.rotate(sign, amount);
    }

    public IEnumerator move(int hex, int dir)
    {
        if(dir == 6)
        {
            StartCoroutine(move(hex, 0));
            StartCoroutine(move(hex, 1));
            StartCoroutine(move(hex, 2));
            StartCoroutine(move(hex, 3));
            StartCoroutine(move(hex, 4));
            StartCoroutine(move(hex, 5));
            yield break;
        }

        Hexagon h = hexagons[hex];
        Color c = h.getColor(dir);
        

        switch (dir)
        {
            case 0:
                if(hex > 3)
                {
                    hexagons[hex - 4].give(3, timeBetweenInstructions, c);
                }
                break;
            case 1:
                if(hex % 4 != 3 && hex != 0 && hex != 2)
                {
                    if(hex % 2 == 1)
                    {
                        hexagons[hex + 1].give(4, timeBetweenInstructions, c);
                    }
                    else
                    {
                        hexagons[hex - 3].give(4, timeBetweenInstructions, c);
                    }
                }
                break;
            case 2:
                if (hex % 4 != 3 && hex != 13 && hex != 14)
                {
                    if (hex % 2 == 1)
                    {
                        hexagons[hex + 5].give(5, timeBetweenInstructions, c);
                    }
                    else
                    {
                        hexagons[hex + 1].give(5, timeBetweenInstructions, c);
                    }
                }
                break;
            case 3:
                if (hex < 11)
                {
                    hexagons[hex + 4].give(0, timeBetweenInstructions, c);
                }
                break;
            case 4:
                if (hex % 4 != 0 && hex != 13)
                {
                    if (hex % 2 == 1)
                    {
                        hexagons[hex + 3].give(1, timeBetweenInstructions, c);
                    }
                    else
                    {
                        hexagons[hex - 1].give(1, timeBetweenInstructions, c);
                    }
                }
                break;
            case 5:
                if (hex % 4 != 0 && hex != 2)
                {
                    if (hex % 2 == 1)
                    {
                        hexagons[hex - 1].give(2, timeBetweenInstructions, c);
                    }
                    else
                    {
                        hexagons[hex - 5].give(2, timeBetweenInstructions, c);
                    }
                }
                break;
        }
        h.take(dir, timeBetweenInstructions, Color.white);
        yield return new WaitForSeconds(timeBetweenInstructions);
    }

    public void add(int hex, int color)
    {
        Color c = Color.white;
        switch (color)
        {
            case 0:
                c = Color.red;
                break;
            case 1:
                c = Color.green;
                break;
            case 2:
                c = Color.blue;
                break;
        }
        hexagons[hex].giveAll(timeBetweenInstructions, c);
    }

    public void sub(int hex, int color)
    {
        Color c = Color.white;
        switch (color)
        {
            case 0:
                c = Color.red;
                break;
            case 1:
                c = Color.green;
                break;
            case 2:
                c = Color.blue;
                break;
        }
        hexagons[hex].takeAll(timeBetweenInstructions, c);
    }


    public void setBreakpoint(int i)
    {
        if (breakpoints.Contains(i))
        {
            breakpoints.Remove(i);
        }
        else
        {
            breakpoints.Add(i);
        }
    }
}
