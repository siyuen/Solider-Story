using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public class PublicRoleData
{
    public string ID;
    public string Name;
}

public class HeroData : PublicRoleData
{
    public static HeroData GetCharacterData(CharacterData data)
    {
        HeroData characterData = new HeroData();
        characterData.ID = data.id;
        characterData.Name = data.name;
        return characterData;
    }
}

public class PublicData : MonoBehaviour
{

}
