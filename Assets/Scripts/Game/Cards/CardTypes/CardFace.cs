using Assets.Scripts.Game.Cards.CardTypes;
using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Type = Assets.Scripts.Game.Cards.CardTypes.Type;

/// <summary>
/// a script that handles the information on the face of a card
/// </summary>
public abstract class CardFace : MonoBehaviour, IDragHandler
{
    public int instanceID;
    public string CardName { get; private set; }
    public int CardID { get; private set; }
    public int cost;
    public Type Type { get; private set; }

    //TODO image

    Vector3 beforeDragPosition;
    
    //game object fields
    Text title, description;
    RawImage image;
    CanvasGroup canvasGroup;

    /// <summary>
    /// attaches this card face to a card object
    /// </summary>
    /// <param name="meta">the card object to attach to</param>
    /// <param name="cardID">the card ID of that card</param>
    public static void Attach(CardMeta meta, int cardID)
    {
        CardFace cardFace;

        CardTemplate data = CardDatabase.GetCardData(cardID);
        switch (data.type)
        {
            case Type.Monster:
                cardFace = meta.gameObject.AddComponent<MonsterCard>();
                break;
            default:
                throw new NotImplementedException();
        }

        meta.face = cardFace;

        //copy fields
        cardFace.title = meta.title;
        cardFace.description = meta.description;
        cardFace.image = meta.image;
        cardFace.canvasGroup = meta.canvasGroup;

        cardFace.Init(meta.instanceID, data);
    }

    /// <summary>
    /// initializes the face of the card
    /// </summary>
    /// <param name="instanceID">the instance ID of the card prefab that this card is attached to</param>
    /// <param name="cardID">the card ID of this card</param>
    private void Init(int instanceID, CardTemplate data)
    {
        this.instanceID = instanceID;

        CardID = data.id;
        CardName = data.cardName;
        title.text = CardName;
        description.text = data.cardDescription;
        cost = data.cost;
        image.texture = CardDatabase.LoadTexture(CardID);

        InitCardTypeSpecific(data);

        //TODO load and activate effects
    }

    /// <summary>
    /// continues initialization of this card's face depending on the card type. Subclasses override this.
    /// </summary>
    /// <param name="data"></param>
    protected abstract void InitCardTypeSpecific(CardTemplate data);

    #region Drag and Drop

    /// <summary>
    /// begins a drag
    /// sets a previous position in case that card is dropped somewhere and needs to return to its previous position
    /// </summary>
    public void OnMouseDown()
    {
        beforeDragPosition = gameObject.transform.position;
        PossibleActionsHighlighter.HighlightValidDropTiles(this);
    }

    /// <summary>
    /// highlights some possible actions with this card
    /// </summary>
    /// <param name="eventData"></param>
    public void OnDrag(PointerEventData eventData)
    {
        //adjust position
        gameObject.transform.position = PlayerCamera.GetRaycastIntersection(eventData.position);

        //cast ray onto map
        RaycastHit hit;
        if (PlayerCamera.RaycastOntoMap(eventData.position, out hit, LayerMask.GetMask("Map")))
        {
            //cast to mesh collider and get submesh
            if (hit.collider is MeshCollider)
            {
                MeshCollider meshCollider = (MeshCollider)hit.collider;
                MeshSuperClass mesh = meshCollider.GetComponent<MeshSuperClass>();

                Assert.IsNotNull(mesh);

                //get cell
                Cell? maybeCell = mesh.TriangleIndexToCell(hit.triangleIndex);
                if (maybeCell != null)
                {
                    Cell cell = (Cell)maybeCell;
                    Debug.Log("Card.OnDrag: Card is being dragged over cell " + cell.ToString());
                    //TODO check if something should happen
                    //TODO visual effect
                }
            }
            else
            {
                Debug.LogError("Card.OnDrag(): raycast hit an object that is not a mesh collider. hit object: " + hit.collider.ToString() + "at " + hit.point.ToString());
            }
        }
    }

    /// <summary>
    /// checks if the drop is a valid action to perform, else puts the card back. If the action is valid, it is executed. Removes the valid location Highlighting afterwards
    /// </summary>
    public void OnMouseUp()
    {
        Cell? cell = null;

        //cast ray onto map
        RaycastHit hit;
        if (PlayerCamera.RaycastOntoMap(Input.mousePosition, out hit, LayerMask.GetMask("Map")))
        {
            //cast to mesh collider and get mesh
            if (hit.collider is MeshCollider)
            {
                MeshCollider meshCollider = (MeshCollider)hit.collider;
                Debug.Log("Card.OnEndDrag(): Hit a mesh collider of type " + meshCollider.GetComponent<MeshSuperClass>().GetType().ToString());
                MeshSuperClass mesh = meshCollider.GetComponent<MeshSuperClass>();
                Assert.IsNotNull(mesh);

                //get cell
                cell = mesh.TriangleIndexToCell(hit.triangleIndex);
                Assert.IsTrue(cell != null);
                Debug.Log("Card.OnEndDrag(): Card is being dropped on cell " + cell.ToString());

                //compare cell to valid locations
                if (cell != null && PossibleActionsHighlighter.IsValidLocation((Cell)cell))
                {
                    Debug.Log("Card.OnEndDrag(): Drop valid");

                    //TODO check for other activation conditions
                    //TODO visual effect
                    Messages.SendCardActivation(instanceID, cell);
                }
                else
                {
                    Debug.Log("Card.OnEndDrag(): Drop not valid");
                    AnimationController.QueueAnimation(new AnimationController.EventAnimation(gameObject, beforeDragPosition, null));
                }
            }
            else
            {
                Debug.LogError("Card.OnEndDrag(): raycast hit an object that is not a mesh collider. hit object: " + hit.collider.ToString() + "at " + hit.point.ToString());
            }
        }

        PossibleActionsHighlighter.RemoveHighlighting();
    }

    public override string ToString()
    {
        return "[cardID: " + CardID + ", type: " + Type + ", cost: " + cost + AdditionalToStringAttributes() + "]";
    }

    protected abstract string AdditionalToStringAttributes();

    #endregion
}