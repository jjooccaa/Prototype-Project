using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CopsUnitType
{
    None = 0,
    Aurelia = 1,
    Bishop = 2,
    Bradley = 3,
    Phoenix = 4,
}

public enum RebelsUnitType
{
    None = 0,
    Kira = 1,
    Roco = 2,
    Ben = 3,
    John = 4,
}

public enum SpecialOpsUnitType
{
    None = 0,
    Blaze = 1,
    Ember = 2,
    Karmak = 3,
    Nova = 4,
}
public enum OldOnesUnitType
{

}
public class Unit : MonoBehaviour
{    
    public int currentX;
    public int currentY;
    public int team;
    public int moves { get; set; }
    public int range { get; set; }
    public int health { get; set; }
    public int damage { get; set; }

    public CopsUnitType copsType;
    public RebelsUnitType rebelType;
    public SpecialOpsUnitType sOType;
    public OldOnesUnitType oldOnesType;

    public Vector3 desiredPosition;
    public Vector3 desiredScale = Vector3.one;

    // UI

    public HealthBar healthBar;
    public ParticleSystem fireDeathParticle;

    private void Start()
    {
        // rotate unit depending on team
        transform.rotation = Quaternion.Euler((team == 0) ? Vector3.zero : new Vector3(0, 180, 0));

        
    }

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 5);
        transform.localScale = Vector3.Lerp(transform.localScale, desiredScale, Time.deltaTime * 5);

        healthBar.SetHealth(health);
        if(health < 10)
        {
            fireDeathParticle.Play();
        }

    }

    public virtual void Passive()
    {
        return;
    }
    public virtual List<Vector2Int>GetAvailableMoves(ref Unit[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        int countingMoves = 0;

        // check if team is white or black, if it's white it goes +1, else -1
        //int direction = (team == 0) ? 1 : -1;

        //  front
        for (int i = currentY + 1; i < tileCountY; i++)
        {
            if (countingMoves < moves)
            {
                if (board[currentX, i] == null)
                {
                    r.Add(new Vector2Int(currentX, i));
                }

                if (board[currentX, i] != null)
                {

                    break;
                }
            }
            countingMoves++;
        }
        countingMoves = 0;

        //  back
        for (int i = currentY - 1; i >= 0; i--)
        {
            if (countingMoves < moves)
            {
                if (board[currentX, i] == null)
                {
                    r.Add(new Vector2Int(currentX, i));
                }

                if (board[currentX, i] != null)
                {

                    break;
                }
            }
            countingMoves++;

        }
        countingMoves = 0;

        //Left
        for (int i = currentX - 1; i >= 0; i--)
        {
            if (countingMoves < moves)
            {
                if (board[i, currentY] == null)
                {
                    r.Add(new Vector2Int(i, currentY));
                }

                if (board[i, currentY] != null)
                {

                    break;
                }
            }
            countingMoves++;

        }
        countingMoves = 0;

        //Right
        for (int i = currentX + 1; i < tileCountX; i++)
        {
            if (countingMoves < moves)
            {
                if (board[i, currentY] == null)
                {
                    r.Add(new Vector2Int(i, currentY));
                }

                if (board[i, currentY] != null)
                {

                    break;
                }
            }
            countingMoves++;

        }
        countingMoves = 0;

        //Top right
        for (int x = currentX + 1, y = currentY + 1; x < tileCountX && y < tileCountY; x++, y++)
        {
            if (countingMoves < moves)
            {
                if (board[x, y] == null)
                {
                    r.Add(new Vector2Int(x, y));
                }
                if (board[x, y] != null)
                {
                    break;
                }
            }
            countingMoves++;
        }
        countingMoves = 0;

        //Top left
        for (int x = currentX - 1, y = currentY + 1; x >= 0 && y < tileCountY; x--, y++)
        {
            if (countingMoves < moves)
            {
                if (board[x, y] == null)
                {
                    r.Add(new Vector2Int(x, y));
                }
                if (board[x, y] != null)
                {
                    break;
                }

            }
            countingMoves++;
        }
        countingMoves = 0;

        //bottom right
        for (int x = currentX + 1, y = currentY - 1; x < tileCountX && y >= 0; x++, y--)
        {
            if (countingMoves < moves)
            {
                if (board[x, y] == null)
                {
                    r.Add(new Vector2Int(x, y));
                }
                if (board[x, y] != null)
                {
                    break;
                }
            }
            countingMoves++;
        }
        countingMoves = 0;

        //bottom left
        for (int x = currentX - 1, y = currentY - 1; x >= 0 && y >= 0; x--, y--)
        {
            if (countingMoves < moves)
            {
                if (board[x, y] == null)
                {
                    r.Add(new Vector2Int(x, y));
                }
                if (board[x, y] != null)
                {
                    break;
                }
            }
            countingMoves++;
        }


        return r;
    }

    public virtual List<Vector2Int> GetAvailableRange(ref Unit[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        //  front
        for (int i = currentY + 1, countingR = 0; countingR < range && i < tileCountY; i++, countingR++)
        {
            r.Add(new Vector2Int(currentX, i));

        }


        //  back
        for (int i = currentY - 1, countingR = 0; countingR < range && i >= 0; i--, countingR++)
        {

            r.Add(new Vector2Int(currentX, i));

        }


        //Left
        for (int i = currentX - 1, countingR = 0; countingR < range && i >= 0; i--, countingR++)
        {
            r.Add(new Vector2Int(i, currentY));

        }


        //Right
        for (int i = currentX + 1, countingR = 0; countingR < range && i < tileCountX; i++, countingR++)
        {

            r.Add(new Vector2Int(i, currentY));

        }


        //Top right
        for (int x = currentX + 1, y = currentY + 1, countingR = 0; (x < tileCountX && y < tileCountY) && countingR < range; x++, y++, countingR++)
        {

            r.Add(new Vector2Int(x, y));

        }


        //Top left
        for (int x = currentX - 1, y = currentY + 1, countingR = 0; (x >= 0 && y < tileCountY) && countingR < range; x--, y++, countingR++)
        {

            r.Add(new Vector2Int(x, y));


        }


        //bottom right
        for (int x = currentX + 1, y = currentY - 1, countingR = 0; (x < tileCountX && y >= 0) && countingR < range; x++, y--, countingR++)
        {

            r.Add(new Vector2Int(x, y));


        }


        //bottom left
        for (int x = currentX - 1, y = currentY - 1, countingR = 0; (x >= 0 && y >= 0) && countingR < range; x--, y--, countingR++)
        {

            r.Add(new Vector2Int(x, y));

        }


        return r;
    }

    public virtual void SetPosition(Vector3 position, bool force = false)
    {
        desiredPosition = position;
        if(force)
        {
            transform.position = desiredPosition;
        }
    }

    public virtual void SetScale(Vector3 scale, bool force = false)
    {
        desiredScale = scale;
        if(force)
        {
            transform.localScale = scale;
        }
    }

}
