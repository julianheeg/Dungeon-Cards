using Assets.Scripts.Game.Cards.CardTypes;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Type = Assets.Scripts.Game.Cards.CardTypes.Type;

/// <summary>
/// a script that handles card movement and information that would be available if the card was face-down
/// </summary>
public class CardMeta : MonoBehaviour {

    public enum Location { Deck, Hand, Field, Graveyard }

    public int instanceID { get; private set; }
    public int owner;
    public Location location;
    public CardFace face;

    //game object fields
    public Text title, description;
    public RawImage image;
    public CanvasGroup canvasGroup;

    /// <summary>
    /// initialises the card for reference by the server (the face is not yet visualised)
    /// </summary>
    /// <param name="instanceID">the instance id of this card for synchronization with the server</param>
    /// <param name="owner">the player index of the owner</param>
    /// <param name="location">the location of this card</param>
    public void Init(int instanceID, int owner, Location location)
    {
        this.instanceID = instanceID;
        this.owner = owner;
        this.location = location;
        
        //TODO: get render position
    }

    public override string ToString()
    {
        return "[instanceID: " + instanceID + ", owner: " + owner + ", location: " + location + ", card face: " + face != null ? face.ToString() : "" + "]";
    }


    public class PositionAndRotation
    {
        public readonly Vector3 position;
        public readonly Quaternion rotation;

        public PositionAndRotation(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }
    }
}