using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Input_Field_Allergen : MonoBehaviour
{
    public InputField inputField;
    public Text allergenText;

    private List<string> allergens = new List<string>();


    public void addAlergen() 
    {
        allergens.Add(inputField.text);
        inputField.text = "Please enter a allergen";
    }

    public List<string> getAllergens()
    {
        return allergens;
    }
}

/*public class Input_Field_Allergen : MonoBehaviour
{
    public InputField inputField;
    public List<string> allergens = new List<string>();
    [SerializeField]
    private TouchScreenKeyboard keyboard;

    public void OpenSystemKeyboard()
    {
        keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default, false, false, false, false);
    }

    private void Update()
    {
        if (keyboard != null)
        {
            string keyboardText = keyboard.text;
            if (keyboard.done)
            {
                if (!string.IsNullOrEmpty(keyboardText))
                {
                    allergens.Add(keyboardText);
                }
                keyboard = null;
            }
        }
    }
}
*/