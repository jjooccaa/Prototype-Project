using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSpell : Card
{
    

    private void Awake()
    {
        cardName = "Spell Card";
        description = "Give health boost to all teammates";
    }

    public override void Effect(ref List<Unit> units)
    {

    }
}
