using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace GameServer
{
    class ServerHandle
    {
        public static void WelcomeReceived(int _fromClient, Packet _packet)
        {
            int _clientIdCheck = _packet.ReadInt();
            string _username = _packet.ReadString();

            Console.WriteLine($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {_fromClient}.");
            if (_fromClient != _clientIdCheck)
            {
                Console.WriteLine($"Player \"{_username}\" (ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
            }
        }

        public static void CreateUserReceive(int _fromClient, Packet _packet) 
        {
            int clientId = _packet.ReadInt();
            string username = _packet.ReadString();
            string password = _packet.ReadString();
            string email = _packet.ReadString();

            Console.WriteLine($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} try to create a account...");
            Console.WriteLine($"username{username}");

            if (_fromClient != clientId)
            {
                Console.WriteLine($"Player \"{username}\" (ID: {_fromClient}) has assumed the wrong client ID ({clientId})!");
            }
            bool accountIsCreated = DBManager.CreateNewUser(username, password, email);
            if (!accountIsCreated)
            {
                Console.WriteLine("Le nom d'utilisateur existe déjà ...");
            }
            else
            {
                Console.WriteLine("Le compte à été créer ! ");
            }
            using(Packet sendPacket = new Packet((int)ServerPackets.createUserResponse))
            {
                sendPacket.Write(clientId);
                sendPacket.Write(accountIsCreated);
                ServerSend.SendTCPData(_fromClient, sendPacket);
            }
        }

        public static void ConnectUser(int _fromClient, Packet _packet)
        {
            int clientId = _packet.ReadInt();
            string username = _packet.ReadString();
            string password = _packet.ReadString();

            int dbId = DBManager.CheckPassword(username, password);
            Server.clients[_fromClient].db_id = dbId;
            using (Packet sendPacket = new Packet((int)ServerPackets.loginResponse))
            {
                sendPacket.Write(clientId);
                sendPacket.Write(dbId);
                sendPacket.Write(username);
                ServerSend.SendTCPData(_fromClient, sendPacket);
            }
        }

        public static void AddCard(int _fromClient, Packet _packet)
        {
            bool isTheLastCard = false;
            int serie_id=0;
            int db_id = Server.clients[_fromClient].db_id;
            while (!isTheLastCard)
            {
                int card_id = _packet.ReadInt();
                
                serie_id = _packet.ReadInt();
                isTheLastCard = _packet.ReadBool();

                DBManager.AddUserCard(db_id, card_id, serie_id);
            }

            bool inBooster = _packet.ReadBool();
            if(inBooster)
            {
                DBManager.UserOpenABooster(db_id, serie_id);
            }

        }

        public static void UpdateCollectionRequest(int _fromClient, Packet _packet)
        {
            int db_id = Server.clients[_fromClient].db_id;

            List<CardAndCount> cardsData = DBManager.GetCollection(db_id);
            UserData userData = DBManager.GetUserData(db_id);
            using(Packet sendPacket = new Packet((int)ServerPackets.updateCollection))
            {
                sendPacket.Write(userData.Cash);
                sendPacket.Write(cardsData.Count);
                foreach (CardAndCount data in cardsData)
                {
                    sendPacket.Write(data.card_id);
                    sendPacket.Write(data.serie_id);
                    sendPacket.Write(data.count);
                    sendPacket.Write(data.in_trade);
                }
                ServerSend.SendTCPData(_fromClient, sendPacket);
            }

        }
    
        public static void CreateTradeRequest(int _fromClient, Packet _packet) 
        {
            int db_id = Server.clients[_fromClient].db_id;

            List<CardAndCount> result = new List<CardAndCount>();

            int receive_id = _packet.ReadInt();
            Console.WriteLine($"{_fromClient} veut créer un échange ... ");
            if(Server.clients[_fromClient].id != receive_id)
            {
                Console.WriteLine("Werid");
                return;
            }
            else
            {
                int price = _packet.ReadInt();
                bool isLast = false;
                while(!isLast)
                {
                    int serie_id = _packet.ReadInt();
                    int card_id = _packet.ReadInt();
                    int count = _packet.ReadInt();
                    result.Add(new CardAndCount(serie_id, card_id, count));
                    isLast = _packet.ReadBool();
                    Console.WriteLine(isLast);
                }

                Trade trade = DBManager.CreateNewTrade(db_id);

                DBManager.LockUserCardForTrade(db_id, result);
                
                trade.Set(price, result, db_id);
                
                trade.Save();
                
                Console.WriteLine("La proposition d'échange a été sauvegarder.");
            }


        }

        public static void UpdateTradeList(int _fromClient, Packet _packet)
        {

            int receive_id = _packet.ReadInt();

            if (Server.clients[_fromClient].id != receive_id)
            {
                Console.WriteLine("Werid");
                return;
            }
            int beginYear = _packet.ReadInt();
            int beginMonth =    _packet.ReadInt();
            int beginDay = _packet.ReadInt();
            int beginHour = _packet.ReadInt();
            int beginMinute = _packet.ReadInt();
            int beginSecond = _packet.ReadInt();
            DateTime beginDateTime = new DateTime(beginYear, beginMonth, beginDay, beginHour, beginMinute, beginSecond);
            int endYear = _packet.ReadInt();
            int endMonth = _packet.ReadInt();
            int endDay = _packet.ReadInt();
            int endHour = _packet.ReadInt();
            int endMinute = _packet.ReadInt();
            int endSecond = _packet.ReadInt();
            DateTime endDateTime = new DateTime(endYear, endMonth,endDay, endHour, endMinute, endSecond);

            List<Trade> trades = DBManager.GetTradeFrom(beginDateTime, endDateTime);
            Console.WriteLine($"Send {trades.Count} trade data to {_fromClient}");
            using (Packet packet = new Packet((int)ServerPackets.updateTradeData))
            {
                packet.Write(trades.Count > 0);
                for (int i = 0; i < trades.Count; i++)
                {
                    Trade trade = trades[i];

                    UserData userData = DBManager.GetUserData(trade.user_db_id);

                    packet.Write(trade.trade_id);
                    packet.Write(userData.Username);
                    packet.Write(trade.price);
                    packet.Write(trade.postDate.Year);
                    packet.Write(trade.postDate.Month);
                    packet.Write(trade.postDate.Day);
                    packet.Write(trade.postDate.Hour);
                    packet.Write(trade.postDate.Minute);
                    packet.Write(trade.postDate.Second);
                    for(int j = 0; j < trade.cards.Count; j++) 
                    {
                        packet.Write(trade.cards[j].serie_id);
                        packet.Write(trade.cards[j].card_id);
                        packet.Write(trade.cards[j].count);

                        packet.Write(j==trade.cards.Count-1);// isLast ? 
                    }
                    packet.Write(i==trades.Count-1); // isLast ?
                }
                ServerSend.SendTCPData(_fromClient, packet);
            }

        }
    }
}