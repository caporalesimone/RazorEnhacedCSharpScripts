using System;

using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Text;
using System.Collections.Generic;


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
                while (true)
                {
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

                }
            }
            catch (Exception ex)
            {
                Misc.SendMessage("Error: " + nameof(Move) + "() - " + ex.Message);
            }
        }


        public void Run()
        {
            var request = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:3000/api/");

            int startX = Player.Position.X;
            int startY = Player.Position.Y;
            int startZ = Player.Position.Z;
            int destX = 1418;
            int destY = 1577; 
            int destZ = 30;

            //string json = "{ \"TracePath\":{ \"world\":1,\"sx\":3485,\"sy\":2581,\"sz\":15,\"dx\":3452,\"dy\":2677,\"dz\":24,\"options\":{ } } }";

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
                    ""options"": {{}}    
                }}
            }}
            ";

            var data = Encoding.ASCII.GetBytes(json);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            JSONReply tmp = JsonConvert.DeserializeObject<JSONReply>(responseString);

            Misc.SendMessage("Points found: " + tmp.TraceReply.points.Count);

            foreach (var item in tmp.TraceReply.points)
            {
                Move(item.x, item.y, true);
            }

            Player.HeadMessage(2, "Done");


        }
    }
}
