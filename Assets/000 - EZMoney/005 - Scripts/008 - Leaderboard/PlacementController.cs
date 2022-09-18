using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlacementController : MonoBehaviour
{
    [field: SerializeField] public CanvasGroup PlacementCG { get; set; }
    [field: SerializeField] public TextMeshProUGUI NameTMP { get; set; }
    [field: SerializeField] public TextMeshProUGUI GemTMP { get; set; }
}
