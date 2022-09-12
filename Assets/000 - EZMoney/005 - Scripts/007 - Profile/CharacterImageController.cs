using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterImageController : MonoBehaviour
{
    //================================================================
    [field: Header("CHARACTER DATA")]
    [field: SerializeField] public string CharacterID { get; set; }
    [field: SerializeField] public CharacterData CharacterData { get; set; }

    [Header("ANIMAL DATA")]
    [SerializeField] private Image AnimalImage;
    [SerializeField] private TextMeshProUGUI StrengthTMP;
    [SerializeField] private TextMeshProUGUI SpeedTMP;
    [SerializeField] public TextMeshProUGUI StaminaTMP;
    [SerializeField] private TMP_Dropdown RoleDropdown;
    //================================================================

    public void SetCharacterImageData()
    {
        AnimalImage.sprite = CharacterData.characterPanelSprite;
        StrengthTMP.text = CharacterData.strength.ToString(); ;
        SpeedTMP.text = CharacterData.speed.ToString();
    }
}
