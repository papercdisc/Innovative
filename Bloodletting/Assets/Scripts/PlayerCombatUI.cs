using TMPro;
using UnityEngine;

public class PlayerCombatUI : MonoBehaviour
{
    [SerializeField] TMP_Text knivesHeldTxt;

    public void UpdateKnifeCount(int currentKnives)
    {
        knivesHeldTxt.text = $"<u><b>{currentKnives}</b></u><br>Knives Held";
    }
}
