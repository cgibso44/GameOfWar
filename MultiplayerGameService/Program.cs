using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ServiceModel;
using MultiplayerGameLibrary;

namespace MultiplayerGameService
{
    public class Program
    {
        static void Main(string[] args)
        {
            ServiceHost servHost = null;
            try
            {
                // Create the service host 
                servHost = new ServiceHost(typeof(MessageBoard));

                // Start the service
                servHost.Open();
                Console.WriteLine("Service started. Press a key to quit.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Console.ReadKey();
                if (servHost != null)
                    servHost.Close();
            }

        }
    }
}
