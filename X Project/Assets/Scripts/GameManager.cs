using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    [SerializeField] GameObject boardObject;

    private BoardManager board;

    // Prefabs
    [Header("Cards")]
    [SerializeField] private GameObject prefabWhiteTeamCard;
    [SerializeField] private GameObject prefabBlackTeamCard;


    // Cards and decks
    private Deck<Card> whiteTeamDeck = new Deck<Card>(new List<Card>());
    private Deck<Card> blackTeamDeck = new Deck<Card>(new List<Card>());
    private List<Card> cardsInWhiteTeamHand = new List<Card>();
    private List<Card> cardsInBlackTeamHand = new List<Card>();


    private int countingSmndCards;


    void Awake()
    {
        board = boardObject.GetComponent<BoardManager>();

        countingSmndCards = 0;


        SpawnDeck(0); // spawn white team deck
        SpawnDeck(1); // spawn black team deck

        // draw 5 cards for white team and 4 for black at the beginning
        cardsInWhiteTeamHand = whiteTeamDeck.Draw(5); 
        cardsInBlackTeamHand = blackTeamDeck.Draw(4);
    }

    private void Start()
    {
        PositionCardsInHand(0); // position cards in hand for white team
        PositionCardsInHand(1); // for black team
    }

    // Update is called once per frame
    void Update()
    {
        // if white team card is clicked, summon it and remove it
        if (board.IsWhiteTurn)
        {
            PlayCard(0); // play and summon card for white team
        }
        // if black team card is clicked, summon it and remove it
        if (!board.IsWhiteTurn)
        {
            PlayCard(1); // play and summon card for black team
        }
    }


    // when player presses end turn button, reset everything and opponent draws another card
    public void EndWhiteTurn()
    { 
        board.IsWhiteTurn = false;
        board.CountingMoves = 0;
        board.CountingAttacks = 0;
        countingSmndCards = 0;

        // opponent draws card when turn is over
        List<Card> drawnCards = blackTeamDeck.Draw(1);
        if (drawnCards != null)
        {
            Card c = drawnCards[0];
            cardsInBlackTeamHand.Add(c);
        }
        if (drawnCards == null)
        {
            Debug.Log("No more cards");
        }
        PositionCardsInHand(1);

    }
    public void EndBlackTurn()
    {
        board.IsWhiteTurn = true;
        board.CountingMoves = 0;
        board.CountingAttacks = 0;
        countingSmndCards = 0;

        // opponent draws card when turn is over
        List<Card> drawnCards = whiteTeamDeck.Draw(1);
        if (drawnCards != null)
        {
            Card c = drawnCards[0];
            cardsInWhiteTeamHand.Add(c);
        }
        if (drawnCards == null)
        {
            Debug.Log("No more cards");
        }
        PositionCardsInHand(0); // position cards for white team
    }



    // Cards
    private Card SpawnSingleCard(int team)
    {
        if (team == 0) // white team card
        {
            Card c = Instantiate(prefabWhiteTeamCard, transform).GetComponent<Card>();
            c.team = 0;
            return c;
        }
        if (team == 1) // black team card
        {

            Card c = Instantiate(prefabBlackTeamCard, transform).GetComponent<Card>();
            c.team = 1;
            return c;
        }

        return null;
    }
    private void SpawnDeck(int team)
    {
        if (team == 0) // white team deck
        {
            for (int i = 0; i < 20; i++)
            {
                whiteTeamDeck.Cards().Add(SpawnSingleCard(team));
                whiteTeamDeck.Cards()[i].SetPosition(new Vector3(6.52f, 0.52f, -3.67f), true);
                whiteTeamDeck.Cards()[i].SetRotation(new Vector3(90, 0, 0), true);
            }
        }

        if (team == 1) // black team deck
        {
            for (int i = 0; i < 20; i++)
            {
                blackTeamDeck.Cards().Add(SpawnSingleCard(team));
                blackTeamDeck.Cards()[i].SetPosition(new Vector3(-6.52f, 0.52f, 3.67f), true);
                blackTeamDeck.Cards()[i].SetRotation(new Vector3(90, 0, 0), true);
            }
        }

    }

    private void PositionCardsInHand(int team)
    {
        // white team, position cards depending on number
        if (team == 0)
        {
            var leftPoint = new Vector3(-1.6f, 0.5f, -6.5f);
            var rightPoint = new Vector3(1.6f, 0.5f, -6.5f);
            var delta = (rightPoint - leftPoint).magnitude;
            var howManyCardsInHand = cardsInWhiteTeamHand.Count;
            var howManyGapsBetweenCards = howManyCardsInHand - 1;
            var gapFromOneCardToNextOne = delta / howManyGapsBetweenCards;
            int theHighestIndex = howManyCardsInHand;

            //float totalTwist = 30f;
            //float twistPerCard = totalTwist / howManyCardsInHand;
            //float startTwist = -1f * (totalTwist / 2f);

            for (int i = 0; i < theHighestIndex; i++)
            {
                cardsInWhiteTeamHand[i].SetPosition(leftPoint, true);
                cardsInWhiteTeamHand[i].SetPosition(cardsInWhiteTeamHand[i].transform.position += new Vector3(i * gapFromOneCardToNextOne, 0, 0), true);
                cardsInWhiteTeamHand[i].SetRotation(new Vector3(45, 0, 0), true);
                cardsInWhiteTeamHand[i].isInHand = true;

                //float twistforThisCard = startTwist + (i * twistPerCard);
                //cardsInWhiteTeamHand[i].SetRotation(new Vector3(45f, 0f, twistforThisCard),true);

            }
        }

        // blak team
        if (team == 1)
        {
            var leftPoint = new Vector3(-1.8f, 0.5f, 6.5f);
            var rightPoint = new Vector3(1.8f, 0.5f, 6.5f);
            var delta = (rightPoint - leftPoint).magnitude;
            var howManyCardsInHand = cardsInBlackTeamHand.Count;
            var howManyGapsBetweenCards = howManyCardsInHand - 1;
            var gapFromOneCardToNextOne = delta / howManyGapsBetweenCards;
            int theHighestIndex = howManyCardsInHand;
            for (int i = 0; i < theHighestIndex; i++)
            {
                cardsInBlackTeamHand[i].SetPosition(leftPoint, true);
                cardsInBlackTeamHand[i].SetPosition(cardsInBlackTeamHand[i].transform.position += new Vector3(i * gapFromOneCardToNextOne, 0, 0), true);
                cardsInBlackTeamHand[i].SetRotation(new Vector3(-45, 0, 0), true);
                cardsInBlackTeamHand[i].isInHand = true;
            }
        }
    }

    private void PlayCard(int team)
    {
        if (team == 0) //white team
        {
            for (int i = 0; i < cardsInWhiteTeamHand.Count; i++)
            {
                if (countingSmndCards == 0) // if this is the first card that is summond in this turn
                {
                    if (cardsInWhiteTeamHand[i].isClicked)
                    {
                        cardsInWhiteTeamHand[i].Effect(ref board.whiteUnits); // testing playing card effect
                        cardsInWhiteTeamHand[i].SetPosition(new Vector3(0, 1, -3), false); // if card is summoned move card to the board 
                        //cardsInWhiteTeamHand[i].SetRotation(new Vector3(-90, 0, 0), false);
                        StartCoroutine(DelayDestroyingObject(cardsInWhiteTeamHand[i].gameObject, 3)); // remove card from board after "" seconds
                        cardsInWhiteTeamHand.Remove(cardsInWhiteTeamHand[i]); // remove card from list
                        PositionCardsInHand(0); // position all cards
                        countingSmndCards++; // count how many cards are summoned in one turn 
                    }
                }
            }
        }
        if (team == 1) //black team
        {
            for (int i = 0; i < cardsInBlackTeamHand.Count; i++)
            {
                if (countingSmndCards == 0) // if this is the first card that is summond in this turn
                {
                    if (cardsInBlackTeamHand[i].isClicked)
                    {
                        cardsInBlackTeamHand[i].Effect(ref board.blackUnits); //testing playing card effect
                        cardsInBlackTeamHand[i].SetPosition(new Vector3(0, 1, 3), false); // if card is summoned move card to the board 
                        StartCoroutine(DelayDestroyingObject(cardsInBlackTeamHand[i].gameObject, 3)); // remove card from board after "" seconds
                        cardsInBlackTeamHand.Remove(cardsInBlackTeamHand[i]);// remove card from list
                        PositionCardsInHand(1);// position all cards
                        countingSmndCards++;// count how many cards are summoned in one turn 
                    }
                }
            }
        }
    }


    IEnumerator DelayDestroyingObject(GameObject obj, float duration)
    {
        // lock mouse and temp disable raycasting on board, after duration destroy object and enable everything again

        Collider collTileOne = board.tiles[4, 4].GetComponent<Collider>();
        Collider collTileTwo = board.tiles[5, 4].GetComponent<Collider>();
        collTileOne.enabled = false;
        collTileTwo.enabled = false;

        Cursor.lockState = CursorLockMode.Locked; // temp lock mouse input

        yield return new WaitForSeconds(duration);

        Destroy(obj);
        Cursor.lockState = CursorLockMode.None; // unlock mouse input

        collTileOne.enabled = true;
        collTileTwo.enabled = true;
    }
}
