using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public class Trade
    {
        public Trade(int id)
        {
            trade_id = id;
        }

        public void Set(int _price, List<CardAndCount> _cards, int _user_db_id)
        {
            price = _price;
            cards = _cards;
            user_db_id = _user_db_id;
        }
        public const string directory_path = "C:\\Users\\mathi\\source\\repos\\ElevenServer\\Trade";

        public int user_db_id;

        public List<CardAndCount> cards = new List<CardAndCount>();

        public int trade_id;

        public int price;
        public string filePath { get { return directory_path + $"\\{trade_id}.trade";} }

        public DateTime postDate;

        public void Save()
        {
            using (StreamWriter outputFile = new StreamWriter(filePath))
            {
                outputFile.WriteLine($"{trade_id}");
                outputFile.WriteLine($"{user_db_id}");
                outputFile.WriteLine($"{price}");
                foreach (CardAndCount card in cards)
                    outputFile.WriteLine($"{card.serie_id},{card.card_id},{card.count}");
            }
        }

        public bool Load()
        {
            try
            {
                var linesRead = File.ReadLines(filePath);
                int i = 0;
                cards.Clear();
                foreach (var lineRead in linesRead)
                {

                    if (i == 0)
                    {
                        if (!int.TryParse(lineRead, out trade_id))
                        {
                            return false;
                        }
                        i++;
                        
                    }
                    else if (i == 1)
                    {
                        if (!int.TryParse(lineRead, out user_db_id))
                        {
                            return false;
                        }
                        i++;
                    }
                    else if (i == 2)
                    {
                        if (!int.TryParse(lineRead, out price))
                        {
                            return false;
                        }
                        i++;
                    }
                    else
                    {
                        int part = 0;
                        string temp = "";
                        int cardId = 0;
                        int serieId = 0;
                        int count = 0;
                        foreach (char c in lineRead)
                        {
                            if (c == ',')
                            {
                                if (part == 0)
                                {
                                    int.TryParse(temp, out serieId);
                                    temp = "";
                                }
                                else if (part == 1)
                                {
                                    int.TryParse(temp, out cardId);
                                    temp = "";
                                }
                                part++;
                                temp = "";
                            }
                            else
                            {
                                temp += c;
                            }
                        }
                        if (part == 2)
                        {
                            int.TryParse(temp, out count);
                            cards.Add(new CardAndCount(serieId, cardId, count));
                        }

                    }
                }
            }
            catch(Exception ex) 
            {
                return false;
            }
            return true;
        }

        public string ToString()
        {
            string re = "-------------\n";
            re += $"trade_id : {trade_id}\n";
            re += $"price : {price}\n";
            for (int i = 0; i < cards.Count; i++)
            {
                re += $"card{i} : {cards[i].serie_id}, {cards[i].card_id}, {cards[i].count}\n";
            }
            re += "-------------";
            return re;
        }
    }


    public class CardAndCount
    {
        public int count = 0;
        public int serie_id = 0;
        public int card_id = 0;
        public int in_trade = 0;
        public CardAndCount(int _serie_id, int _card_id, int _count, int _in_trade=0)
        {
            serie_id = _serie_id;
            card_id = _card_id;
            count = _count;
            in_trade = _in_trade;
        }
    }
}
