using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static System.Net.Mime.MediaTypeNames;

namespace GameServer
{
    public class DeckLoader
    {
        public bool ById {get; private set;}
        public bool ByName { get; private set;}

        public int DbID {get; set;}
        public int DeckID { get; set;}

        public string DeckName { get; set;}

        public string filePath { get { return $"{Constants.APP_PATH} + \\decks\\{DbID}\\{DeckID}.deck"; } }
        
        public List<CardAndCount> cards = new List<CardAndCount>();

        public DeckLoader(int _db_id, int _deck_id)
        {
            ById = true;
            ByName = false;
            DbID = _db_id;
            DeckID = _deck_id;
            DeckName = "No-Name";
        }

        public DeckLoader(int _db_id, string _deck_name)
        {
            ById = false;
            ByName = true;
            DbID = _db_id;
            DeckName = _deck_name;
        }
        
        public void Load()
        {
            String line;
            
            DBManager.SetupDeckLoader(this);

            if(File.Exists(filePath)) 
            {
                try
                {

                    StreamReader sr = new StreamReader(filePath);

                    line = sr.ReadLine();

                    while (line != null)
                    {
                        //write the line to console window
                        Console.WriteLine(line);
                        //Read the next line
                        line = sr.ReadLine();
                    }
                    //close the file
                    sr.Close();
                    Console.ReadLine();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: " + e.Message);
                }
                finally
                {
                    Console.WriteLine("Executing finally block.");
                }
            }
            else
            {
                // Deck n'existe pas
            }
            
        }

        public void Save()
        {
            using (StreamWriter outputFile = new StreamWriter(filePath))
            {
                outputFile.WriteLine($"{DeckName}");
                foreach (CardAndCount card in cards)
                    outputFile.WriteLine($"{card.serieId},{card.cardId},{card.count}");
            }
        }

        public struct CardAndCount
        {
            public int cardId;
            public int serieId;
            public int count;
        }
    }
}
