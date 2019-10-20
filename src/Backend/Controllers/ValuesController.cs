using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using StackExchange.Redis;
using System.Threading;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        const int LOCATION_CODE = 4;
        // GET api/values/<id>
        [HttpGet("{id}")]
        public ActionResult Get(string id)
        {
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
            
            /*
            IServer server = redis.GetServer("localhost", 6379);
            foreach(var key in server.Keys()) {
                string rank = db.StringGet(key); 
                if (key.ToString() == "RANK_" + id)
                    Console.Write("true--> ");
                Console.WriteLine(key.ToString()+":"+rank);
            }
            */
           
            //Console.WriteLine("i need id: \n"+"RANK_" + id);
            for (int i = 0; i < 5; ++i)
			{
                int locationDb = GetLocation(redis, id);
                IDatabase db = redis.GetDatabase(locationDb);
				string rank = db.StringGet("RANK_" + id);
                //Console.WriteLine("i need id: "+rank);
				if (rank == null)
				{
					Thread.Sleep(200);
				}
				else
				{
					return Ok(rank);
				}
			}
			return new NotFoundResult();
        }
        
        // POST api/values
        [HttpPost]
        public string Post([FromBody]string data)
        {
            var id = Guid.NewGuid().ToString();
            string text = data.Split(':')[0];
            string location = data.Split(':')[1];

            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
            this.saveData(id, text, location);
            this.publishEvent(redis, id, text);
        
            return id;
        }

        private void saveData(String id, String text, String location)
        {
            int locationDbCode = 4;
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
            //Сохраняет id И text в бд указанной страны
            IDatabase db = redis.GetDatabase(GetDatabaseCode(location));
            db.StringSet(id, text);

            //сохраняет id и название страны, в чью базу сохранен текст, в базу №4 
            IDatabase location_db = redis.GetDatabase(locationDbCode);
            location_db.StringSet(id, location);
            Console.WriteLine("[ValuesController "+id+" : "+text+" ] saved to database in "+location);
        }
        private static int GetDatabaseCode(string location)
        {
             switch (location.ToLower())
            {
                case "rus":
                    return 1;
                case "eu":
                    return 2;
                case "usa":
                    return 3;
            }
            return 1;
        }
        private void publishEvent(ConnectionMultiplexer redis, String id, String data)
        {
            //публикует сообщение в шину сообщений events
            ISubscriber sub = redis.GetSubscriber();
            sub.Publish("events", "Text Created:"+id);
        }

         private static int GetLocation(ConnectionMultiplexer redis, string id)
        {
            //ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
            IDatabase locationDb = redis.GetDatabase(LOCATION_CODE);
            string location = locationDb.StringGet(id);
            int dbCode = GetDatabaseCode(location);
            return dbCode;
        }
    }
}
