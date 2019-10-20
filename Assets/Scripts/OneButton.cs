using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneButton
{
    enum Type { Key, Mouse, Gamepad };

    Type    type;
    KeyCode key;

    static List<OneButton>  currentButtons = new List<OneButton>();

    static public void ClearButtons()
    {
        currentButtons.Clear();
    }
    static public OneButton GetButtonPress()
    {
        if (Input.anyKeyDown)
        {
            foreach (KeyCode vKey in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKey(vKey))
                {
                    // Check if this key is already in use
                    bool inUse = false;

                    foreach (var button in currentButtons)
                    {
                        if ((button.type == Type.Key) && (button.key == vKey))
                        {
                            inUse = true;
                            break;
                        }
                    }

                    if (!inUse)
                    {
                        OneButton b = new OneButton();
                        b.type = Type.Key;
                        b.key = vKey;

                        currentButtons.Add(b);

                        return b;
                    }
                }
            }
        }

        return null;
    }
}
