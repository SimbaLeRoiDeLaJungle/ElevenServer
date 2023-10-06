using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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
            conn.Close();
            return false;

        }

        public static int CheckPassword(string username, string password)
        {
            string sql = string.Format("SELECT id FROM users WHERE username = '{0}' AND password = SHA({1}) ",username, password);
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
                conn.Close();
                return id;
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex);
            }
            rdr.Close();
            conn.Close();
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
            finally
            {
                conn.Close();
            }

        }
        public static List<CardAndCount> GetCollection(int db_id)
        {
            string sql = string.Format("SELECT serie_id,card_id,count,in_trade FROM cards WHERE user_id = '{0}' ", db_id);
            MySqlConnection conn = GetConnection();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();
            try
            {
                List<CardAndCount> result = new List<CardAndCount>();
                while (rdr.Read())
                {
                    result.Add(new CardAndCount(rdr.GetInt32(0), rdr.GetInt32(1), rdr.GetInt32(2), rdr.GetInt32(3)));
                }
                rdr.Close();
                conn.Close();
                return result;
            }
            catch(Exception ex)
            {
                rdr.Close();
                conn.Close();
                Console.WriteLine(ex);
                return new List<CardAndCount>();
            }
        }

        public static UserData GetUserData(int db_id)
        {
            string sql = string.Format("SELECT username,email,cash FROM users WHERE id = '{0}' ", db_id);
            MySqlConnection conn = GetConnection();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();
            try
            {
                if (!rdr.Read())
                {
                    return null;
                }

                string username = rdr.GetString(0);
                string email = rdr.GetString(1);
                int cash = rdr.GetInt32(2);
                
                rdr.Close();
                conn.Close();
                return new UserData(username, email, db_id, cash);
            }
            catch (Exception ex)
            {
                rdr.Close();
                conn.Close();
                Console.WriteLine(ex);
                return null;
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
                        conn.Close();
                        rdr.Close();
                        return true;
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    conn.Close();
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

            conn.Close();
            rdr.Close();
        }

        public static Trade CreateNewTrade(int user_db_id)
        {
            string sql = string.Format("INSERT INTO trade(user_id) VALUES({0})",user_db_id);
            MySqlConnection conn = GetConnection();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();
            long id = cmd.LastInsertedId;
            Trade trade = new Trade((int)id);
            conn.Close();
            rdr.Close();
            return trade;
        }

        public static List<Trade> GetTradeFrom(DateTime beginTime, DateTime endTime)
        {
            List<Trade> result = new List<Trade>();
            string sql = string.Format("SELECT trade_id,post_date FROM trade WHERE post_date > '{0}' OR post_date < '{1}' ORDER BY post_date DESC", beginTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), endTime.ToString("yyyy-MM-dd HH:mm:ss.fff") );
            Console.WriteLine(sql);
            MySqlConnection conn = GetConnection();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();
            int maxSize = 20;
            while(rdr.Read())
            {
                int trade_id = rdr.GetInt32(0);
                Trade trade = new Trade(trade_id);
                trade.postDate = rdr.GetDateTime(1);
                if (trade.Load())
                {
                    result.Add(trade);
                    if (result.Count >= maxSize)
                    {
                        break;
                    }
                }

            }
            conn.Close();
            rdr.Close();
            return result;
        }

        public static bool LockUserCardForTrade(int user_db_id, List<CardAndCount> cards)
        {
            foreach(CardAndCount c in cards) 
            {
                string sql = string.Format("SELECT in_trade, count FROM cards WHERE user_id = {0} AND card_id = {1} AND serie_id = {2}", user_db_id, c.card_id, c.serie_id);
                MySqlConnection conn = GetConnection();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                
                if (!rdr.Read())
                    return false;


                int in_trade = rdr.GetInt32(0);
                int count = rdr.GetInt32(1);

                if(count-in_trade-c.count < 0)
                {
                    return false;
                }
                in_trade += c.count;
                
                rdr.Close();
                

                MySqlCommand createCmd = conn.CreateCommand();
                createCmd.CommandText = string.Format("UPDATE cards SET in_trade={3} WHERE user_id = {0} AND card_id = {1} AND serie_id = {2}", user_db_id, c.card_id, c.serie_id, in_trade);
                createCmd.ExecuteNonQuery();

                conn.Close();
            }

            return true;

        }
    }


}
