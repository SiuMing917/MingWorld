using GDE.GenericSelectionUI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActionSelectionUI : SelectionUI<TextSlot>
{
    // Start is called before the first frame update
    void Start()
    {
        SetSelectionSettings(SelectionType.Grid, 2);
        SetItems(GetComponentsInChildren<TextSlot>().ToList());
    }
}
