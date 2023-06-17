using System;

using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Text;
using System.Collections.Generic;
using System.Security.Policy;
using Assistant;


//#forcedebug
//#assembly <Newtonsoft.Json.dll>

namespace RazorEnhanced
{


    /*
"allow_diagonal_move": true, 
"heuristic_distance": "Manhattan",
"heuristic_straight": 5,
"heuristic_diagonal": 5,
"cost_turn": 1,
"cost_move_straight": 1,
"cost_move_diagonal": 1,
"left": 0,
"top": 0,
"right": 6144,
"bottom": 4096,
"all_points": false
*/

    public class Options
    {
        public bool allow_diagonal_move { get; set; }
        public string heuristic_distance { get; set; }
        public int heuristic_straight { get; set; }
        public int heuristic_diagonal { get; set; }
        public int cost_turn { get; set; }
        public int cost_move_straight { get; set; }
        public int cost_move_diagonal { get; set; }
        public int left { get; set; }
        public int top { get; set; }
        public int right { get; set; }
        public int bottom { get; set; }
        public bool all_points { get; set; }
    }


    public class point
    {
        [JsonProperty("x")]
        public int x { get; set; }
        [JsonProperty("y")]
        public int y { get; set; }
        [JsonProperty("z")]
        public int z { get; set; }
        [JsonProperty("w")]
        public int w { get; set; }
    }
    
    public class TraceReply 
    {
        [JsonProperty("points")]
        public List<point> points;
    }

    public class JSONReply
    {
        [JsonProperty("TraceReply")]
        public TraceReply TraceReply;
    }


    public class items
    {
        public byte world { get; set; }
        public int serial { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; }  
        public int graphic { get; set; }
    }

    public class ItemsAdd
    {
        public List<items> items { get; set; }
    }

    public class JsonAddItem
    {
        [JsonProperty("ItemsAdd")]
        public ItemsAdd ItemsAdd;
    }

/*
Send json {
"ItemsAdd": {
"items": [{
"world": u8, serial": u32, "x": isize, "y": isize, "z": i8, "graphic": u16}, ...]}}
*/


    public class PathFindingRust
    {
        private void Move(int nextPosX, int nextPosY, bool run)
        {
            try
            {
                var pPosX = Player.Position.X;
                var pPosY = Player.Position.Y;

                if (Math.Abs(pPosX - nextPosX) > 1 || Math.Abs(pPosY - pPosY) > 1)
                {
                    Misc.SendMessage("Distance to great");
                    return;
                }

                var nextTileX = nextPosX - pPosX;
                var nextTileY = nextPosY - pPosY;

                var direction = "";
                if (nextTileX == 0 && nextTileY == -1) { direction = "North"; }
                else if (nextTileX == 1 && nextTileY == -1) { direction = "Right"; }
                else if (nextTileX == 1 && nextTileY == 0) { direction = "East"; }
                else if (nextTileX == 1 && nextTileY == 1) { direction = "Down"; }
                else if (nextTileX == 0 && nextTileY == 1) { direction = "South"; }
                else if (nextTileX == -1 && nextTileY == 1) { direction = "Left"; }
                else if (nextTileX == -1 && nextTileY == 0) { direction = "West"; }
                else if (nextTileX == -1 && nextTileY == -1) { direction = "Up"; }

                var wasMovementCalled = false;

                int cnt = 1000;
                while (true)
                {
                    if (cnt-- <= 0) { break; }

                    if (Player.Position.X == nextPosX && Player.Position.Y == nextPosY)
                    {
                        return;
                    }

                    if (wasMovementCalled == false)
                    {
                        wasMovementCalled = true;

                        if (direction != Player.Direction)
                        {
                            Player.Run(direction);
                        }

                        if (run == true)
                        {
                            Player.Run(direction);
                        }
                        else
                        {
                            Player.Walk(direction);
                        }
                    }
                    Misc.Pause(1);
                }
            }
            catch (Exception ex)
            {
                Misc.SendMessage("Error: " + nameof(Move) + "() - " + ex.Message);
            }
        }


        string CreateJSONtoDestintion(int x, int y, int z)
        {
            Options opt = new Options
            {
                allow_diagonal_move = true,
                heuristic_distance = "Manhattan",
                heuristic_straight = 5,
                heuristic_diagonal = 5,
                cost_turn = 1,
                cost_move_straight = 1,
                cost_move_diagonal = 1,
                left = 0,
                top = 0,
                right = 6144,
                bottom = 4096,
                all_points = false
            };

            int startX = Player.Position.X;
            int startY = Player.Position.Y;
            int startZ = Player.Position.Z;
            int destX = x;
            int destY = y;
            int destZ = z;

            string json = $@"
            {{
                ""TracePath"":
                {{
                    ""world"":{Player.Map},
                    ""sx"":{startX},
                    ""sy"":{startY},
                    ""sz"":{startZ},
                    ""dx"":{destX},
                    ""dy"":{destY},
                    ""dz"":{destZ},
                    ""options"": {JsonConvert.SerializeObject(opt)}
                }}
            }}
            ";
            return json;
        }


        JSONReply GetPathFromServer(string json)
        {
            var request = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:3000/api/");

            byte[] data = Encoding.ASCII.GetBytes(json);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            return JsonConvert.DeserializeObject<JSONReply>(responseString);
        }


        void sendItems(string json)
        {
            var request = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:3000/api/");

            byte[] data = Encoding.ASCII.GetBytes(json);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
        }

        string CreateJsonSendItems()
        {
            ItemsAdd itm = new ItemsAdd();

            itm.items = new List<items>();


            Items.Filter findFilter = new Items.Filter();
            List<Item> allitems = Items.ApplyFilter(findFilter);

            foreach (var item in allitems)
            {

                if (Misc.Distance(item.Position.X, item.Position.Y, Player.Position.X, Player.Position.Y) > 10) continue;

                if (Math.Abs(item.Position.Z - Player.Position.Z) < 5)
                {
                    items it1 = new items()
                    {
                        x = item.Position.X,
                        y = item.Position.Y,
                        z = item.Position.Z,
                        graphic = item.ItemID,
                        serial = item.Serial,
                        world = (byte)Player.Map
                    };

                    itm.items.Add(it1);
                }
            }

            JsonAddItem additem = new JsonAddItem()
            {
                ItemsAdd = itm
            };

            Player.HeadMessage(5, $"Sent {itm.items.Count} items");
            string str = JsonConvert.SerializeObject(additem);

            return str;
        }


        public void Run()
        {
            /*
            int destx = 1418;
            int desty = 1577;
            int destz = 30;
             */

            int destx = 1216;
            int desty = 1219;
            int destz = -90;

            while (true)
            {
                sendItems(CreateJsonSendItems());

                string json = CreateJSONtoDestintion(destx, desty, destz);
                var tmp = GetPathFromServer(json);

                int count = tmp.TraceReply.points.Count;
                int breakcnt = 10;
                foreach (var item in tmp.TraceReply.points)
                {
                    //Player.HeadMessage(10, $"{count-- * 100 / tmp.TraceReply.points.Count}");
                    Move(item.x, item.y, true);
                    if (breakcnt-- <= 0) break;
                }

                var dist = Misc.Distance(Player.Position.X, Player.Position.Y, destx, desty);
                if (dist < 2) 
                { 
                    break; 
                }

                
            }

            Player.HeadMessage(2, "Done");


        }
    }
}
