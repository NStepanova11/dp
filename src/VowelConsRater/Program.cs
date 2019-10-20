using System;
using StackExchange.Redis;


namespace VowelConsRater
{
    class Program
    {
        const int LOCATION_CODE = 4;
        const string RATER_QUEUE_NAME = "vowel_cons_rater_jobs";
        const string RATER_HINTS_CHANNEL = "vowel_cons_rater_hints";
        static void Main(string[] args)
        {
            Console.WriteLine("VowelConsRater started");
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
            IDatabase locationDb = redis.GetDatabase(LOCATION_CODE);
            ISubscriber sub = redis.GetSubscriber();
            sub.Subscribe(RATER_HINTS_CHANNEL, delegate
            {
                //берет из очереди сообщение вида [id:гласные:согласные]
                string message = locationDb.ListRightPop(RATER_QUEUE_NAME);
                while (message != null && message != "")
                {
                    string id = message.Split(':')[0];
                    string vowels = message.Split(':')[1];
                    string consonants = message.Split(':')[2];
                    int  locationCode = Int32.Parse(message.Split(':')[3]);

                    string result = vowels + "\\" + consonants;

                    string rankId = "RANK_" + id;
                    //сохраняет в базу запись вида RANK_id:гласные\согласные
                    IDatabase db = redis.GetDatabase(locationCode);
                    db.StringSet(rankId, result);
                    Console.WriteLine($"{rankId} : {result} saved to db = {locationCode}");
                    message = locationDb.ListRightPop(RATER_QUEUE_NAME);
                }
            }
            );
            Console.ReadKey();
        }
    }
}
