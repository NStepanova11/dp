using System;
using StackExchange.Redis;

namespace TextListener
{
    class Program
    {
        const int LOCATION_CODE = 4;
        static void Main(string[] args)
        {
            Console.WriteLine("TextListener started");
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
            //IDatabase db = redis.GetDatabase();

            //Подписывается на сообщения из шины events которые публикует backend и выводит их в консоль
            ISubscriber sub = redis.GetSubscriber();
            sub.Subscribe("events", (channel, message) => {
                string id = message.ToString().Split(':')[1];
                int locationCode = GetLocation(redis, id);
                string text = GetIdText(id, redis, locationCode);
                Console.WriteLine($"Id = [{id}] Text = [{text}] Location = [{locationCode}]");
            });
            Console.ReadKey();
        }

        private static int GetLocation(ConnectionMultiplexer redis, string id)
        {
            //ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
            IDatabase locationDb = redis.GetDatabase(LOCATION_CODE);
            string location = locationDb.StringGet(id);
            int dbCode = GetDatabaseCode(location);
            return dbCode;
        }
        private static String GetIdText(string id, ConnectionMultiplexer redis, int locationCode)
        {
            IDatabase db = redis.GetDatabase(locationCode);
            return db.StringGet(id);
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
    }
}
