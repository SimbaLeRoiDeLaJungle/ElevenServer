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

            List<CardData> cardsData = DBManager.GetCollection(db_id);
            using(Packet sendPacket = new Packet((int)ServerPackets.updateCollection))
            {
                sendPacket.Write(cardsData.Count);
                foreach (CardData data in cardsData)
                {
                    sendPacket.Write(data.card_id);
                    sendPacket.Write(data.serie_id);
                    sendPacket.Write(data.count);
                }
                ServerSend.SendTCPData(_fromClient, sendPacket);
            }

        }
    }
}