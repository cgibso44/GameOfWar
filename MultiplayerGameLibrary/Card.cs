using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;

namespace MultiplayerGameLibrary
{
    public interface ICard
    {
        Card.SuitID Suit { get; }
        Card.RankID Rank { get; }
        string Name { get; }
    }
    
    [DataContract]
    public class Card : ICard
    {
        // enum of possible values for the card's suit
        public enum SuitID
        {
            Clubs, Diamonds, Hearts, Spades
        };

        // enum of possible values for the card's rank
        public enum RankID
        {
            Ace, King, Queen, Jack, Ten, Nine, Eight, Seven, Six,
            Five, Four, Three, Two
        };

        // member variables and accessor methods
        [DataMember]
        public SuitID Suit { get; private set; }
        [DataMember]
        public RankID Rank { get; private set; }
        [DataMember]
        public string Name { get; private set; }

        // c'tor which identifies which card this is
        public Card(SuitID s, RankID r)
        {
            Suit = s;
            Rank = r;

            // Moved this logic to the constructor since a data transfer 
            // object can't contain logic in any other method or property
            Name = Rank.ToString() + " of " + Suit.ToString();
        }


    }
}
