using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;

using MultiplayerGameLibrary;
using System.ServiceModel;
using System.Text.RegularExpressions;

namespace MultiplayerGameClient
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
    public partial class MainWindow : Window, ICallback,IModeratorCallback
    {
        //chat stuff
        private IUser msgBrd = null;        
        private string prefix = "";
        int count = 0;

        //Cards Stuff
        private IShoe shoe;
        private Guid myCallbackKey;

        public MainWindow()
        {
            InitializeComponent();

            try
            {

                //// Create the channel factory
                DuplexChannelFactory<IShoe> channelShoe
                    = new DuplexChannelFactory<IShoe>(this, "Shoe");

                // Activate a Shoe object
                shoe = channelShoe.CreateChannel();

                // Register this client for callbacks
                myCallbackKey = shoe.RegisterForCallbacks();
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (textPost.Text != "")
            {
                try
                {
                    msgBrd.PostMessage(prefix + textPost.Text, textAlias.Name);
                    textPost.Clear();
                    // ** Now handled by callback **
                    //listMessages.ItemsSource = msgBrd.GetAllMessages();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void buttonSet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (textAlias.Text != "")
                {
                    prefix = "[" + textAlias.Text + "] ";
                    buttonSet.IsEnabled = textAlias.IsEnabled = false;
                    buttonSubmit.IsEnabled = textPost.IsEnabled = listMessages.IsEnabled = true;

                    //Can remove this moderator crap
                    if (textAlias.Text.ToUpper() == "MODERATOR")
                    {
                        connectToMessageBoard(true);
                    }
                    else
                    {
                        connectToMessageBoard(false);
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                listMessages.ItemsSource = msgBrd.GetAllMessages();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (msgBrd != null)
                msgBrd.Leave(textAlias.Text);
        }

        // Helper methods

        private void connectToMessageBoard(bool isModerator)
        {
            try
            {               
               
                    // Configure the ABCs of using the MessageBoard service
                    DuplexChannelFactory<IUser> channel
                        = new DuplexChannelFactory<IUser>(this, "User");

                    // Activate a MessageBoard object
                    msgBrd = channel.CreateChannel();
           
               
               

                if (msgBrd.Join(textAlias.Text))
                {
                    // Alias accepted by the service so update GUI
                    listMessages.ItemsSource = msgBrd.GetAllMessages();
                    textAlias.IsEnabled = buttonSet.IsEnabled = false;
                    StartGameBtn.IsEnabled = true;
                    textUsersOnline.Text = msgBrd.UsersOnline().ToString();
                    myCardsText.Visibility = Visibility.Visible;
                    myCardsText.Text = textAlias.Text + "'s Cards";
                    WaitingForPlayersText.Visibility = Visibility.Visible;
                    


                    //Get names of users online and place it on the screen.
                }
                else
                {
                    // Alias rejected by the service so nullify service proxies
                    msgBrd = null;
                    textAlias.IsEnabled = buttonSet.IsEnabled = true;
                    buttonSubmit.IsEnabled = textPost.IsEnabled = false;
                    MessageBox.Show("ERROR: Alias in use. Please try again.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private delegate void GuiUpdateDelegate(string[] messages);
        private delegate void GuiUpdateDelegate1(int usersOnline);
        private delegate void ClientUpdateDelegate(CallbackInfo info);
        private delegate void ClientUpdateDelegate1(string[] userNames);
        private delegate void ClientUpdateDelegate2(bool ready);
        private delegate void ClientUpdateDelegate3(string[] usersAndCards);


        public void UpdateUsersWithCards(string[] usersAndCards)
        {
            if (this.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                bool contains = false;
                bool contains1 = false;
                String g = "";
                String f = "";
                
                try
                {
                    foreach (String s in usersAndCards)
                    {
                        contains = myCardsText.Text.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0;
                        if (contains)
                        {
                            continue;
                        }
                        String[] test = s.Split(',');
                        contains1 = User2CardsText.Text.IndexOf(test[0], StringComparison.OrdinalIgnoreCase) >= 0;
                        if (contains1)
                        {

                            User2cardsDelt.Text = test[1];
                            User2cardsDelt.Visibility = Visibility.Visible;
                            continue;
                        }
                        //else if (User3CardsText.Text == "" && User2CardsText.Text != "" && !contains)
                        //{
                        //    User3CardsText.Text = s.ToUpperInvariant() + "'s Cards";
                        //    User3CardsText.Visibility = Visibility.Visible;
                        //    continue;
                        //}
                        //else if (User4CardsText.Text == "" && User3CardsText.Text != "" && !contains)
                        //{
                        //    User3CardsText.Text = s.ToUpperInvariant() + "'s Cards";
                        //    User3CardsText.Visibility = Visibility.Visible;
                        //    continue;
                        //}
                    }
                    if (shoe.getWinner() != "")
                    {
                        MessageBox.Show(shoe.getWinner());
                        DrawBtn.IsEnabled = false;
                        StartGameBtn.IsEnabled = true;                       
                        WaitingForPlayersText.Visibility = Visibility.Visible;
                        cardDealtText.Visibility = Visibility.Hidden;
                        User2cardsDelt.Visibility = Visibility.Hidden;
                        //msgBrd.isReady(textAlias.Text, false);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
                this.Dispatcher.BeginInvoke(new ClientUpdateDelegate3(UpdateUsersWithCards), new object[] { usersAndCards });
        }

        public void UpdateUsersThatAreReady(bool ready)
        {
            if (this.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                try
                {
                    if (ready)
                    {
                         DrawBtn.IsEnabled = true;
                         WaitingForPlayersText.Visibility = Visibility.Hidden;
                         StartGameBtn.IsEnabled = false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
                this.Dispatcher.BeginInvoke(new ClientUpdateDelegate2(UpdateUsersThatAreReady), new object[] { ready });
        }

        public void AllUsersOnlineNames(string[] userNames)
        {
            
            bool contains = false;
            String g = "";
            if (this.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                try
                {
                    foreach (String s in userNames)
                    {
                        
                        contains = myCardsText.Text.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0;
                        if (contains)
                        {
                            g = s;
                            continue;
                        }
                            
                        else if (User2CardsText.Text == "" && !contains)
                        {
                            User2CardsText.Text = s.ToUpperInvariant() + "'s Cards";
                            User2CardsText.Visibility = Visibility.Visible;
                            continue;
                        }
                        else if (User3CardsText.Text == "" && User2CardsText.Text != "" && !contains)
                        {
                            User3CardsText.Text = s.ToUpperInvariant() + "'s Cards";
                            User3CardsText.Visibility = Visibility.Visible;
                            continue;
                        }
                        else if (User4CardsText.Text == "" && User3CardsText.Text != "" && !contains)
                        {
                            User3CardsText.Text = s.ToUpperInvariant() + "'s Cards";
                            User3CardsText.Visibility = Visibility.Visible;
                            continue;
                        }
                        
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
                this.Dispatcher.BeginInvoke(new GuiUpdateDelegate(AllUsersOnlineNames), new object[] { userNames });
        }

        public void SendAllMessages(string[] messages)
        {
            if (this.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                try
                {
                    listMessages.ItemsSource = messages;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
                this.Dispatcher.BeginInvoke(new GuiUpdateDelegate(SendAllMessages), new object[] { messages });
        }

        public void UpdateAllUsersOnline(int usersOnline)
        {
            if (this.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                try
                {
                    textUsersOnline.Text = usersOnline.ToString();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
                this.Dispatcher.BeginInvoke(new GuiUpdateDelegate1(UpdateAllUsersOnline), new object[] { usersOnline });
        }

        public void UpdateGui(CallbackInfo info)
        {
            if (this.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                try
                {
                    // Update the user interface
                    //txtShoeCount.Text = info.NumCards.ToString();
                    //sliderDecks.Value = info.NumDecks;
                    //if (shoe.NumDecks == 1)
                    //    txtDeckCount.Text = "1 Deck";
                    //else
                    //    txtDeckCount.Text = shoe.NumDecks + " Decks";
                    //if (info.Reset)
                    //{
                    //    lstCards.Items.Clear();
                    //    txtHandCount.Text = "0";
                    //}
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
                // Send the method call to the GUI's main thread
                this.Dispatcher.BeginInvoke(new ClientUpdateDelegate(
                    UpdateGui), new object[] { info });


        }

        public void UserJoined(string name)
        {
            MessageBox.Show("User '" + name + "' joined the message board.");
        }

        public void UserLeft(string name)
        {
            MessageBox.Show("User '" + name + "' left the message board.");
        }

        private void StartGameBtn_Click(object sender, RoutedEventArgs e)
        {
            if (msgBrd.UsersOnline() <= 1)
            {
                MessageBox.Show("Need more players");
            }            
            else
            {
                msgBrd.isReady(textAlias.Text, true);
                StartGameBtn.IsEnabled = false;               
            }
        }

        private void DrawBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // get a card from the shoe and add it to the hand
                if (shoe.getWinner() != "")
                    shoe.resetWinner();
                shoe.NumDecks = 1;                
                Card card = shoe.Draw(textAlias.Text);
                cardDealtText.Text = card.Name;
                cardDealtText.Visibility = Visibility.Visible;
              
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
                

    }
}
