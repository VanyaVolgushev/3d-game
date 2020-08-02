using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class GameMath
{
    public static RaycastHit RaycastFromCenterOfScreen(float maxDistance)
    {
        RaycastHit rayHit;
        Ray ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));
        Physics.Raycast(ray, out rayHit, maxDistance);
        return rayHit;
    }
    public static List<string> DivideSentenceIntoWords(string sentence)
    {
        List<string> words = new List<string>();
        char[] separators = {' '};

        foreach (string word in sentence.Split(separators))
        {
            if (word.ToCharArray().Length != 0) {
                char[] characters = word.ToCharArray();

                if (characters[characters.Length - 1] == '.' || characters[characters.Length - 1] == '!' || characters[characters.Length - 1] == '?' || characters[characters.Length - 1] == ':' || characters[characters.Length - 1] == ',')
                {
                    words.Add(word.Remove(characters.Length - 1,1));
                    words.Add(characters[characters.Length - 1].ToString());
                }
                else
                {
                    words.Add(word);
                }
            }
        }
        return words;
    }
}
