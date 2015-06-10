using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Windows.Forms;
using MultiplayerGameLibrary;


namespace MultiplayerGameLibrary
{
    public interface IUserCallback
    {
        [OperationContract(IsOneWay = true)]
        void SendAllMessages(string[] messages);
        [OperationContract(IsOneWay = true)]
        void UpdateAllUsersOnline(int usersOnline);
        [OperationContract(IsOneWay = true)]
        void AllUsersOnlineNames(string[] namesOfUsers);
        [OperationContract(IsOneWay = true)]
        void UpdateUsersThatAreReady(bool ready);
        [OperationContract(IsOneWay = true)]
        void UpdateUsersWithCards(string [] usersAndCards);
        
    }

    #region Cards
    public interface ICallback
    {
        [OperationContract(IsOneWay = true)]
        void UpdateGui(CallbackInfo info);
    }

    // Shoe's service contract which is now linked to the callback contract
    [ServiceContract(CallbackContract = typeof(ICallback))]
    public interface IShoe
    {
        [OperationContract]
        Guid RegisterForCallbacks();
        [OperationContract(IsOneWay = true)]
        void UnregisterForCallbacks(Guid key);
        [OperationContract]
        Card Draw(string name);
        [OperationContract(IsOneWay = true)]
        void Shuffle();
        int NumCards { [OperationContract] get; }
        int NumDecks { [OperationContract] get; [OperationContract(IsOneWay = true)] set; } 
        [OperationContract]
        string getWinner();
        [OperationContract]
        void resetWinner();
    }
    #endregion

    #region Chat
    public interface IModeratorCallback : IUserCallback
    {
        [OperationContract(IsOneWay = true)]
        void UserJoined(string name);
        [OperationContract(IsOneWay = true)]
        void UserLeft(string name);
    }

    [ServiceContract(CallbackContract = typeof(IUserCallback))]
    public interface IUser
    {
        [OperationContract]
        bool Join(string name);
        [OperationContract(IsOneWay = true)]
        void Leave(string name);
        [OperationContract(IsOneWay = true)]
        void PostMessage(string message, string name);
        [OperationContract]
        int UsersOnline();
        [OperationContract]
        string[] GetAllMessages();
        [OperationContract(IsOneWay = true)]
        void isReady(string name, bool ready);
    }    
    #endregion


    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class MessageBoard : IUser , IShoe
    {

        //Chat Stuff
        private Dictionary<string, IUserCallback> userCallbacks
            = new Dictionary<string, IUserCallback>();

        private Dictionary<string, bool> areUsersReady
            = new Dictionary<string, bool>();

        private Dictionary<string, string> usersWithCards
            = new Dictionary<string, string>();

        private IModeratorCallback modCallback = null;
        private List<string> messages = new List<string>();


        #region Cards
        //Card Stuff
        // member variables
        private List<Card> cards = new List<Card>();
        private int numDecks = 1;
        private int cardIdx;
        private string _winner = "";

        private Dictionary<Guid, ICallback> clientCallbacks
           = new Dictionary<Guid, ICallback>();


        public string Winner
        {
            get { return _winner; }
            set { _winner = value; }
        }

        public void resetWinner()
        {
            foreach (KeyValuePair<string, bool> callb in areUsersReady)
            {
                usersWithCards[callb.Key] = "";
            }
            foreach (KeyValuePair<string, string> callb in usersWithCards)
            {
                areUsersReady[callb.Key] = false;
            }
            Winner = "";
        }

        public string getWinner()
        {
            return Winner;
        }

        // C'tors
        public void Shoe()
        {
            Console.WriteLine("Creating a Shoe object.");
            cardIdx = 0;
            repopulate();
        }

        public void Shoe(int numOfDecks)
        {
            Console.WriteLine("Creating a Shoe object with {0} decks.", numOfDecks);
            cardIdx = 0;
            numDecks = numOfDecks;
            repopulate();
        }

        public Guid RegisterForCallbacks()
        {
            ICallback cb
                = OperationContext.Current.GetCallbackChannel<ICallback>();

            Guid key = Guid.NewGuid();
            clientCallbacks.Add(key, cb);
            return key;
        }

        public void UnregisterForCallbacks(Guid key)
        {
            if (clientCallbacks.Keys.Contains<Guid>(key))
                clientCallbacks.Remove(key);
        }


        public Card Draw( string name )
        {
            if (clientCallbacks.Count <= 1)
            {
                MessageBox.Show("Need more players!");
            }
            Random rnd = new Random();
            if (cardIdx == cards.Count)
            {
                repopulate();                
                cardIdx = rnd.Next(52);
            }
            else
            {
                cardIdx = rnd.Next(52);
            }
            
                //throw new System.IndexOutOfRangeException("The shoe is empty. Please reset.");

            Card card = cards[cardIdx];

            usersWithCards[name.ToUpper()] = card.Name;
            List<string> cardsWithNames = new List<string>();            
            
            foreach (KeyValuePair<string, string> callb in usersWithCards)
            {
                cardsWithNames.Add(callb.Key + "," + callb.Value);                
            }
            String[] cardsWithNames1 = cardsWithNames.ToArray();

            foreach (IUserCallback callb in userCallbacks.Values)
            {
                callb.UpdateUsersWithCards(cardsWithNames1);
            }
            int count = 0; 
            foreach (KeyValuePair<string, string> callb in usersWithCards)
            {
                if (callb.Value == "")
                    count++;
            }


            
            
            int Ace = 0;
            int Two = 0;
            int Three = 0;
            int Four = 0;
            int Five = 0;
            int Six = 0;
            int Seven = 0;
            int Eight = 0;
            int Nine = 0;
            int Ten = 0;
            int Jack = 0;
            int Queen = 0;
            int King = 0;


            string winnerString = "";
            if (count == 0)
            {
                //The winner logic here
                bool containsAce = false;
                bool containsTwo = false;
                bool containsThree = false;
                bool containsFour = false;
                bool containsFive = false;
                bool containsSix = false;
                bool containsSeven = false;
                bool containsEight = false;
                bool containsNine = false;
                bool containsTen = false;
                bool containsJack = false;
                bool containsQueen = false;
                bool containsKing = false;

                foreach (String s in cardsWithNames1)
                {
                    String[] test = s.Split(',');
                    containsAce = test[1].IndexOf("ace", StringComparison.OrdinalIgnoreCase) >= 0;
                    if (containsAce)
                    {
                        Ace++;
                    }
                    containsTwo = test[1].IndexOf("two", StringComparison.OrdinalIgnoreCase) >= 0;
                    if (containsTwo)
                    {
                        Two++;
                    }
                    containsThree = test[1].IndexOf("three", StringComparison.OrdinalIgnoreCase) >= 0;
                    if (containsTwo)
                    {
                        Three++;
                    }
                    containsFour = test[1].IndexOf("four", StringComparison.OrdinalIgnoreCase) >= 0;
                    if (containsFour)
                    {
                        Four++;
                    }
                    containsFive = test[1].IndexOf("five", StringComparison.OrdinalIgnoreCase) >= 0;
                    if (containsTwo)
                    {
                        Five++;
                    }
                    containsSix = test[1].IndexOf("six", StringComparison.OrdinalIgnoreCase) >= 0;
                    if (containsSix)
                    {
                        Six++;
                    }
                    containsSeven = test[1].IndexOf("seven", StringComparison.OrdinalIgnoreCase) >= 0;
                    if (containsSeven)
                    {
                        Seven++;
                    }
                    containsEight = test[1].IndexOf("eight", StringComparison.OrdinalIgnoreCase) >= 0;
                    if (containsEight)
                    {
                        Eight++;
                    }
                    containsNine = test[1].IndexOf("nine", StringComparison.OrdinalIgnoreCase) >= 0;
                    if (containsNine)
                    {
                        Nine++;
                    }
                    containsTen = test[1].IndexOf("ten", StringComparison.OrdinalIgnoreCase) >= 0;
                    if (containsTen)
                    {
                        Ten++;
                    }
                    containsJack = test[1].IndexOf("jack", StringComparison.OrdinalIgnoreCase) >= 0;
                    if (containsSeven)
                    {
                        Jack++;
                    }
                    containsQueen = test[1].IndexOf("queen", StringComparison.OrdinalIgnoreCase) >= 0;
                    if (containsQueen)
                    {
                        Queen++;
                    }
                    containsKing = test[1].IndexOf("king", StringComparison.OrdinalIgnoreCase) >= 0;
                    if (containsKing)
                    {
                        King++;
                    }
                }

               // Winner = "THE WINNER";
            }
            if (count == 0)
            {
                bool containsAce = false;
                bool containsTwo = false;
                bool containsThree = false;
                bool containsFour = false;
                bool containsFive = false;
                bool containsSix = false;
                bool containsSeven = false;
                bool containsEight = false;
                bool containsNine = false;
                bool containsTen = false;
                bool containsJack = false;
                bool containsQueen = false;
                bool containsKing = false;

                foreach (String s in cardsWithNames1)
                {
                    String[] test = s.Split(',');
                    containsAce = test[1].IndexOf("ace", StringComparison.OrdinalIgnoreCase) >= 0;
                    if (Ace >= 1 && containsAce)
                    {
                        if (Ace == 2)
                        {
                            Winner = "TIE";
                        }
                        else
                        {
                            Winner = test[0] + " wins with an ace";
                        }
                    }
                    containsTwo = test[1].IndexOf("two", StringComparison.OrdinalIgnoreCase) >= 0;
                    if (Two >= 1 && containsTwo)
                    {
                        if (Two == 2)
                        {
                            Winner = "TIE";
                        }
                    }
                    containsThree = test[1].IndexOf("three", StringComparison.OrdinalIgnoreCase) >= 0;
                    if (Three >= 1 && containsThree)
                    {
                        if (Three == 2)
                        {
                            Winner = "TIE";
                        }
                        else if( Three == 1 && Two == 1 )
                        {
                            Winner = test[0] + " wins with a three";
                        }
                    }
                    containsFour = test[1].IndexOf("four", StringComparison.OrdinalIgnoreCase) >= 0;
                    if (Four >= 1 && containsFour)
                    {
                        if (Four == 2)
                        {
                            Winner = "TIE";
                        }
                        else if(Four == 1 && Three <= 1 && Two <=1)
                        {
                            Winner = test[0] + " wins with a four";
                        }
                    }
                    containsFive = test[1].IndexOf("five", StringComparison.OrdinalIgnoreCase) >= 0;
                    if (Five >= 1 && containsFive)
                    {
                        if (Five == 2)
                        {
                            Winner = "TIE";
                        }
                        else if (Five == 1 && Four <= 1 && Three <= 1 && Two <= 1)
                        {
                            Winner = test[0] + " wins with a five";
                        }
                    }
                    containsSix = test[1].IndexOf("six", StringComparison.OrdinalIgnoreCase) >= 0;
                    if (Six >= 1 && containsSix)
                    {
                        if (Six == 2)
                        {
                            Winner = "TIE";
                        }
                        else if (Six == 1 && Five <= 1 && Four <= 1 && Three <= 1 && Two <= 1)
                        {
                            Winner = test[0] + " wins with a six";
                        }
                    }
                    containsSeven = test[1].IndexOf("seven", StringComparison.OrdinalIgnoreCase) >= 0;
                    if (Seven >= 1 && containsSeven)
                    {
                        if (Seven == 2)
                        {
                            Winner = "TIE";
                        }

                        else if (Seven == 1 && Six <= 1 && Five <= 1 && Four <= 1 && Three <= 1 && Two <= 1)
                        {
                            Winner = test[0] + " wins with a seven";
                        }

                    }
                    containsEight = test[1].IndexOf("eight", StringComparison.OrdinalIgnoreCase) >= 0;
                    if (Eight >= 1 && containsEight)
                    {
                        if (Eight == 2)
                        {
                            Winner = "TIE";
                        }

                        else if (Eight == 1 && Seven <= 1 && Six <= 1 && Five <= 1 && Four <= 1 && Three <= 1 && Two <= 1)
                        {
                            Winner = test[0] + " wins with a eight";
                        }
                    }
                    containsNine = test[1].IndexOf("nine", StringComparison.OrdinalIgnoreCase) >= 0;
                    if (Nine >= 1 && containsNine)
                    {
                        if (Nine == 2)
                        {
                            Winner = "TIE";
                        }
                        else if (Nine == 1 && Eight <= 1 && Seven <= 1 && Six <= 1 && Five <= 1 && Four <= 1 && Three <= 1 && Two <= 1)
                        {
                            Winner = test[0] + " wins with a nine";
                        }
                    }
                    containsTen = test[1].IndexOf("ten", StringComparison.OrdinalIgnoreCase) >= 0;
                    if (Ten >= 1 && containsTen)
                    {
                        if (Ten == 2)
                        {
                            Winner = "TIE";
                        }

                        else if (Ten == 1 && Nine <= 1 && Eight <= 1 && Seven <= 1 && Six <= 1 && Five <= 1 && Four <= 1 && Three <= 1 && Two <= 1)
                        {
                            Winner = test[0] + " wins with a ten";
                        }
                    }
                    containsJack = test[1].IndexOf("jack", StringComparison.OrdinalIgnoreCase) >= 0;
                    if (Jack >= 1 && containsJack)
                    {
                        if (Jack == 2)
                        {
                            Winner = "TIE";
                        }
                        else if (Jack == 1 && Ten <= 1 && Nine <= 1 && Eight <= 1 && Seven <= 1 && Six <= 1 && Five <= 1 && Four <= 1 && Three <= 1 && Two <= 1)
                        {
                            Winner = test[0] + " wins with a jack";
                        }
                    }
                    containsQueen = test[1].IndexOf("queen", StringComparison.OrdinalIgnoreCase) >= 0;
                    if (Queen >= 1 && containsQueen)
                    {
                        if (Queen == 2)
                        {
                            Winner = "TIE";
                        }
                        else if (Queen == 1 && Jack <= 1 && Ten <= 1 && Nine <= 1 && Eight <= 1 && Seven <= 1 && Six <= 1 && Five <= 1 && Four <= 1 && Three <= 1 && Two <= 1)
                        {
                            Winner = test[0] + " wins with a queen";
                        }
                    }
                    containsKing = test[1].IndexOf("king", StringComparison.OrdinalIgnoreCase) >= 0;
                    if (King >= 1 && containsKing)
                    {
                        if (King == 2)
                        {
                            Winner = "TIE";
                        }
                        else if (King == 1 && Queen <= 1 && Jack <= 1 && Ten <= 1 && Nine <= 1 && Eight <= 1 && Seven <= 1 && Six <= 1 && Five <= 1 && Four <= 1 && Three <= 1 && Two <= 1)
                        {
                            Winner = test[0] + " wins with a king";
                        }
                    }


                }

            }
            
            Console.WriteLine("Dealing the " + card.Name + ".");

            cardIdx++;

            updateAllClients(true);


            return card;
        }

        public void Shuffle()
        {
            if (Winner != "")
            {
                Winner = "";
            }
            Console.WriteLine("Shuffling the Shoe.");
            randomizeCards();

            updateAllClients(true);
        }

        public int NumCards
        {
            get { return cards.Count - cardIdx; }
        }

        public int NumDecks
        {
            get { return numDecks; }
            set
            {
                if (numDecks != value)
                {
                    Console.WriteLine("Now using {0} decks.", value);
                    numDecks = value;
                    repopulate();

                    updateAllClients(true);
                }
            }
        }

        private void updateAllClients(bool reset)
        {
            CallbackInfo info = new CallbackInfo(cards.Count - cardIdx,
                numDecks, reset);

            foreach (ICallback cb in clientCallbacks.Values)
                cb.UpdateGui(info);
        }

        private void repopulate()
        {
            // remove "old" cards
            cards.Clear();

            // populate with new cards
            // for each deck
            for (int d = 0; d < numDecks; d++)
            {
                // for each suit in this deck
                foreach (Card.SuitID s in Enum.GetValues(typeof(Card.SuitID)))
                {
                    // foreach rank in this suit
                    foreach (Card.RankID r in Enum.GetValues(typeof(Card.RankID)))
                    {
                        // add card
                        cards.Add(new Card(s, r));
                    }
                }
            }
            // shuffle cards
            randomizeCards();
        }

        private void randomizeCards()
        {
            Random rand = new Random();
            Card hold;
            int randIndex;

            for (int i = 0; i < cards.Count; i++)
            {
                // choose a random index
                randIndex = rand.Next(cards.Count);

                if (randIndex != i)
                {
                    // swap elements at indexes i and randIndex
                    hold = cards[i];
                    cards[i] = cards[randIndex];
                    cards[randIndex] = hold;
                }
            }
            // start dealing off the top of the deck
            cardIdx = 0;
        }
#endregion

        #region Chat

        public bool Join(string name)
        {
            if (userCallbacks.ContainsKey(name.ToUpper()))
                // User alias must be unique
                return false;
            else
            {
                IUserCallback cb = null;

                if (modCallback != null)
                    modCallback.UserJoined(name);

                // Retrieve client's callback proxy
                cb = OperationContext.Current.GetCallbackChannel<IUserCallback>();
                Console.WriteLine(name + " Joined");


                // Save alias and callback proxy
                userCallbacks.Add(name.ToUpper(), cb);
                areUsersReady.Add(name.ToUpper(), false);
                usersWithCards.Add(name.ToUpper(), "");

                int users = userCallbacks.Count();
                String[] userNames = userCallbacks.Keys.ToArray();
                foreach (IUserCallback callb in userCallbacks.Values)
                {
                    callb.AllUsersOnlineNames(userNames);
                }
                foreach (IUserCallback callb in userCallbacks.Values)
                {
                    callb.UpdateAllUsersOnline(users);
                }


                return true;
            }
        }

        public void isReady(string name, bool ready)
        {
            foreach (KeyValuePair<string, IUserCallback> callb in userCallbacks)
            {
                if (callb.Key.ToUpper() == name.ToUpper())
                {
                    areUsersReady[name.ToUpper()] = ready;
                }

            }
            int count = 0;
            foreach (KeyValuePair<string, bool> k in areUsersReady)
            {
                if (k.Value == false)
                    count++;
            }
            foreach (IUserCallback callb in userCallbacks.Values)
            {
                if (count == 0)
                    callb.UpdateUsersThatAreReady(true);
                else
                    callb.UpdateUsersThatAreReady(false);
            }



        }

        public void Leave(string name)
        {
            if (userCallbacks.ContainsKey(name.ToUpper()))
            {
                userCallbacks.Remove(name.ToUpper());
                Console.WriteLine(name + " left.");
                if (modCallback != null)
                {
                    modCallback.UserLeft(name);

                }

                int users = userCallbacks.Count();
                foreach (IUserCallback callb in userCallbacks.Values)
                {
                    callb.UpdateAllUsersOnline(users);
                }
            }
        }

        public int UsersOnline()
        {
            return userCallbacks.Count();
        }

        public void PostMessage(string message, string name)
        {
            if (userCallbacks.Count <= 1)
            {
                MessageBox.Show("More users are needed to chat");
            }
            else
            {
                messages.Insert(0, message);
                updateAllUsers();
                Console.WriteLine("Posted Message: " + message);
            }

        }

        public string[] GetAllMessages()
        {
            return messages.ToArray<string>();
        }

        // Helper methods
        private void updateAllUsers()
        {
            String[] msgs = messages.ToArray<string>();

            foreach (IUserCallback cb in userCallbacks.Values)
            {
                cb.SendAllMessages(msgs);
            }
        }
        #endregion              

    }
    


}
