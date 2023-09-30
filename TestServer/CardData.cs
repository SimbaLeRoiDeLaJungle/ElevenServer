using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public class CardData
    {
        public int card_id;
        public int serie_id;
        public int count;
        public CardData(int _card_id,int _serie_id, int _count) 
        {
            card_id= _card_id;
            serie_id= _serie_id;
            count= _count;
        }
    }
}
