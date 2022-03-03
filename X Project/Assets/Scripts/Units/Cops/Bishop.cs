using System.Collections.Generic;
using UnityEngine;

public class Bishop : Unit
{
    private const int MOVES = 1;
    private const int RANGE = 1;
    public const int startingHealth = 3;
    public const int startingDamage = 15;

    private void Awake()
    {
        health = startingHealth;
        healthBar.SetMaxHealth(health);

        damage = startingDamage;
    }

    public override List<Vector2Int> GetAvailableMoves(ref Unit[,] board, int tileCountX, int tileCountY)
    {
        moves = MOVES;
        return base.GetAvailableMoves(ref board, tileCountX, tileCountY);
    }
    public override List<Vector2Int> GetAvailableRange(ref Unit[,] board, int tileCountX, int tileCountY)
    {
        range = RANGE;
        return base.GetAvailableRange(ref board, tileCountX, tileCountY);
    }
}
