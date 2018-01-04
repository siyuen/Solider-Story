using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public class CharacterData
{
    public readonly string id;
    public readonly string name;
    public float Value(string m)
    {
        float intA;
        float.TryParse(m, out intA);
        return intA;
    }
}

public class StaticData : MonoBehaviour {
    public Dictionary<string, CharacterData> characterDictionary;
	// Use this for initialization
	void Start () {
        characterDictionary = Load<CharacterData>("Data/test");
        var Data = characterDictionary["1"];
        var characterData = HeroData.GetCharacterData(Data);
        Debug.Log(characterData.ID + "," + characterData.Name);
        var Data1 = characterDictionary["2"];
        var characterData1 = HeroData.GetCharacterData(Data1);
        Debug.Log(characterData1.ID + "," + characterData1.Name);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public Dictionary<string, T> Load<T>(string rName)
    {
        string str;
        {
            TextAsset textAsset = Resources.Load<TextAsset>(rName);
            if (textAsset == null)
            {
                return null;
            }
            str = textAsset.text;
        }
        Dictionary<string, T> data = JsonMapper.ToObject<Dictionary<string, T>>(str);
        return data;
    }
}
