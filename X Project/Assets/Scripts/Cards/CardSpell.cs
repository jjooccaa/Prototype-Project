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
        foreach (var u in units)
        {
            u.health += 20;
            Debug.Log("New health for: " + u.ToString() + " " + u.health);
        }
    }
}
