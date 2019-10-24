using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneButton
{
    enum Type { Key, Gamepad };

    Type    type;
    KeyCode key;
    bool    currentPress;
    float   timeLastPress;
    float   timeLastRelease;
    bool    isTapped;

    public void Update()
    {
        bool prevPress = currentPress;

        isTapped = false;

        switch (type)
        {
            case Type.Key:
                currentPress = Input.GetKey(key);
                if ((!prevPress) && (currentPress)) timeLastPress = Time.realtimeSinceStartup;
                if ((prevPress) && (!currentPress))
                {
                    timeLastRelease = Time.realtimeSinceStartup;
                    if ((timeLastRelease - timeLastPress) < 0.5f) isTapped = true;
                }
                break;
            case Type.Gamepad:
                break;
            default:
                break;
        }
    }

    public bool IsTapped()
    {
        return isTapped;
    }

    public bool IsPressed()
    {
        return currentPress;
    }

    public float GetTimeSincePress()
    {
        return Time.realtimeSinceStartup - timeLastPress;
    }

    static List<OneButton>  currentButtons = new List<OneButton>();

    static public void UpdateButtons()
    {
        foreach (var b in currentButtons)
        {
            b.Update();
        }
    }

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
                string keyName = vKey.ToString();

                if (keyName.StartsWith("JoystickButton"))
                {
                    // Ignore these, because they are also mapped as Joystick1Button*
                    continue;
                }

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
