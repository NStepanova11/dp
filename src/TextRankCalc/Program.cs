using System;
using System.Collections.Generic;
using StackExchange.Redis;
namespace TextRankCalc
{
    class Program
    {
        const int LOCATION_CODE = 4;
        const string COUNTER_QUEUE_NAME = "vowel_cons_counter_jobs";
        const string COUNTER_HINTS_CHANNEL = "vowel_cons_counter_hints";
        static void Main(string[] args)
        {
            Console.WriteLine("TextRancCalc started");
           
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
            //IDatabase db = redis.GetDatabase();
            ISubscriber sub = redis.GetSubscriber();
            //Подписывается на сообщения вида id из шины events которые публикует backend
            sub.Subscribe ("events", (channel, message)=>
            {
                string id = message.ToString().Split(':')[1];
                int locationCode = GetLocation(redis, id);
                string text = GetIdText(id, redis, locationCode);
                string queueMessage = $"{id}:{text}:{locationCode}";
                IDatabase db = redis.GetDatabase(LOCATION_CODE);
                AddMessageToQueue(db, queueMessage);
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

        private static void AddMessageToQueue(IDatabase db, string message)
        {
           //добавляет сообщение id:text  в очередь vowel_cons_counter_jobs
           db.ListLeftPush(COUNTER_QUEUE_NAME, message, flags: CommandFlags.FireAndForget); 
           //публикует пустое сообщение в канале vowel_cons_counter_hints для уведомления подписчиков
           db.Multiplexer.GetSubscriber().Publish( COUNTER_HINTS_CHANNEL, "" );
        }
    }
}
