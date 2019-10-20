using System;
using StackExchange.Redis;
using System.Collections.Generic;

namespace VowelConsCounter
{
    class Program
    {
        const int LOCATION_CODE = 4;
        const string VOWELS_COUNT = "vowels";
        const string CONSONANTS_COUNT = "consonants";
        const string COUNTER_QUEUE_NAME = "vowel_cons_counter_jobs";
        const string COUNTER_HINTS_CHANNEL = "vowel_cons_counter_hints";
        const string RATE_QUEUE_NAME = "vowel_cons_rater_jobs";
        const string RATE_HINTS_CHANNEL = "vowel_cons_rater_hints";

        private static HashSet<Char> vowelsSet = new HashSet<Char>{'a', 'e', 'i', 'o', 'u'};
        private static HashSet<Char> consonantsSet =  new HashSet<Char>{'b','c','d','f','g','h','j','k','l','m','n','p','q','r','s','t','v','w','x','y','z'};

        static void Main(string[] args)
        {
            Console.WriteLine("VowelConsCounter started");
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
            ISubscriber sub = redis.GetSubscriber();
            IDatabase locationDb = redis.GetDatabase(LOCATION_CODE);
            //подписывается на канал vowel_cons_counter_hints
            sub.Subscribe(COUNTER_HINTS_CHANNEL, delegate
            {
                string message = locationDb.ListRightPop(COUNTER_QUEUE_NAME);
                while (message != null && message != "")
                {
                    //берет из очереди сообщения вида id:text
                    string id =  message.Split(':')[0];
                    string text = message.Split(':')[1];
                    string locationCode = message.Split(':')[2];
                
                    Dictionary<String, int> result = CalculateVowelsAndConsonants(text);
                    int vowels = result[VOWELS_COUNT];
                    int consonants = result[CONSONANTS_COUNT];

                    //IDatabase db = redis.GetDatabase(Int32.Parse(locationCode));

                    SendMessage($"{id}:{vowels}:{consonants}:{locationCode}", locationDb);
                    Console.WriteLine("Message: " + id + " : " + vowels + " : " + consonants);
                    //берет из очереди следующее сообщение (для проверки условия цикла)
                    message = locationDb.ListRightPop(COUNTER_QUEUE_NAME);
                }
            });
            Console.ReadKey();
        }

        private static Dictionary<String, int> CalculateVowelsAndConsonants(string text) {
            int vowels = 0;
			int consonants = 0;
            foreach (char ch in text.ToLower()) {
                if (vowelsSet.Contains(ch)) 
                {
                    vowels++;
                } else if (consonantsSet.Contains(ch)) 
                {
                    consonants++;
                } 
            }
            Dictionary<String, int> result = new Dictionary<string, int>();
            result.Add(VOWELS_COUNT, vowels);
            result.Add(CONSONANTS_COUNT, consonants);
            return result;
        }

        private static void SendMessage(string message, IDatabase db)
        {
            //добавляет в очередь сообщение вида [id:гласные:согласные] В очередь vowel_cons_rater_jobs
            db.ListLeftPush(RATE_QUEUE_NAME, message, flags: CommandFlags.FireAndForget);
            //пустое сообщение уведомляет подписчика, что в очереди новое сообщение vowel_cons_rater_hints
            db.Multiplexer.GetSubscriber().Publish(RATE_HINTS_CHANNEL, "");
        }
    }
}
