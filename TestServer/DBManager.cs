using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;

namespace GameServer
{
    class DBManager
    {
        public static MySqlConnection GetConnection()
        {
            string connstring = string.Format("Server={0}; database={1}; UID={2}; password={3}", Constants.SERVER, Constants.DATABASE_NAME, Constants.DATABASE_USERNAME, Constants.DATABASE_PASSWORD);
            try
            {
                MySqlConnection conn = new MySqlConnection(connstring);
                conn.Open();
                return conn;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while connecting to the database : {ex.ToString()}");
                return null;
            }
            
            
        }

        public static bool CreateNewUser(string username, string password, string email)
        {
            string sql = "SELECT username FROM users;";
            MySqlConnection conn = GetConnection();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();
            try
            {
                while (rdr.Read())
                {
                    if ((string)rdr[0] == username)
                    {
                        rdr.Close();
                        return false;
                    }
                }
                rdr.Close();

                MySqlCommand createCmd = conn.CreateCommand();
                createCmd.CommandText = string.Format("INSERT INTO users(username, password, email) VALUES('{0}',SHA1('{1}'),'{2}')", username, password, email);
                createCmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            rdr.Close();
            return false;

        }

        public static int CheckPassword(string username, string password)
        {
            string sql = string.Format("SELECT id FROM users WHERE username = '{0}' ",username,password);
            MySqlConnection conn = GetConnection();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();
            try
            {
                int id = -1;
                if (rdr.Read())
                {
                    id = (int)rdr[0];
                }
                rdr.Close();
                return id;
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex);
            }
            rdr.Close();
            return -1;
        }

        public static void AddUserCard(int db_id, int card_id, int serie_id)
        {
            
            string sql = string.Format("SELECT count FROM cards WHERE user_id = {0} AND card_id = {1} AND serie_id = {2}", db_id, card_id,serie_id);
            MySqlConnection conn = GetConnection();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            int count = 1;
            try
            {
                bool allreadyHaveField = false;
                MySqlDataReader rdr = cmd.ExecuteReader();
                if (rdr.Read())
                {
                    count = rdr.GetInt32(0);
                    count += 1;
                    allreadyHaveField = true;
                }
                rdr.Close();
                if(allreadyHaveField)
                {
                    sql = string.Format("UPDATE cards SET count = {0} WHERE user_id = {1} AND card_id = {2} AND serie_id = {3}", count, db_id, card_id, serie_id);
                    MySqlCommand createCmd = conn.CreateCommand();
                    createCmd.CommandText = sql;
                    createCmd.ExecuteNonQuery();
                }
                else
                {
                    sql = string.Format("INSERT INTO cards(user_id, count, card_id, serie_id) VALUES({0},{1},{2},{3})",db_id, count,card_id,serie_id);
                    MySqlCommand createCmd = conn.CreateCommand();
                    createCmd.CommandText = sql;
                    createCmd.ExecuteNonQuery();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }
        public static List<CardData> GetCollection(int db_id)
        {
            string sql = string.Format("SELECT card_id,serie_id,count FROM cards WHERE user_id = '{0}' ", db_id);
            MySqlConnection conn = GetConnection();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();
            try
            {
                List<CardData> result = new List<CardData>();
                while (rdr.Read())
                {
                    result.Add(new CardData(rdr.GetInt32(0), rdr.GetInt32(1), rdr.GetInt32(2)));
                }
                rdr.Close();
                return result;
            }
            catch(Exception ex)
            {
                rdr.Close();
                Console.WriteLine(ex);
                return new List<CardData>();
            }
        }

        public static bool SetupDeckLoader(DeckLoader _deckLoader)
        {
            if(_deckLoader.ById)
            {
                string sql = string.Format("SELECT deck_id,user_id,deck_name FROM cards WHERE user_id = {0}, deck_id = {1} ", _deckLoader.DbID, _deckLoader.DeckID);
                MySqlConnection conn = GetConnection();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                try
                {
                    if (rdr.Read())
                    {
                        _deckLoader.DeckName = rdr.GetString(2);
                        return true;
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                { 
                    rdr.Close(); 
                }
            }
            else if(_deckLoader.ByName)
            {
                string sql = string.Format("SELECT deck_id,user_id,deck_name FROM cards WHERE user_id = {0}, deck_name = '{1}' ", _deckLoader.DbID, _deckLoader.DeckName);
                MySqlConnection conn = GetConnection();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                try
                {
                    if (rdr.Read())
                    {
                        _deckLoader.DeckID = rdr.GetInt32(0);
                        return true;
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    rdr.Close();
                }
            }
            return false;

        }

        public static void UserOpenABooster(int db_id, int serie_id)
        {
            string sql = string.Format("SELECT booster_count FROM user_serie_data WHERE user_id = {0} AND serie_id = '{1}' ", db_id, serie_id);
            MySqlConnection conn = GetConnection();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();
            bool lineExist = false;
            int booster_count = 0;
            try
            {
                if (rdr.Read())
                {
                    booster_count = rdr.GetInt32(0);
                    booster_count++;
                    lineExist = true;
                }
                rdr.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                rdr.Close();
            }

            if (lineExist)
            {
                sql = string.Format("UPDATE user_serie_data SET booster_count = {0} WHERE user_id = {1} AND serie_id = {2}", booster_count, db_id, serie_id);

            }
            else
            {
                sql = string.Format("INSERT INTO user_serie_data(user_id, serie_id, booster_count) VALUES({0},{1},{2})", db_id, serie_id, 1);
            }

            try
            {
                MySqlCommand createCmd = conn.CreateCommand();
                createCmd.CommandText = sql;
                createCmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }


}
