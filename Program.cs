
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using StackExchange.Redis;
using System;

namespace Redis.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            IDatabase database = GetDatabase();

            //Simple Ping command to check if Database is Up, if database is up response will be pong
            var commandresponse = database.Execute("PING");
            Console.WriteLine(commandresponse.ToString());

            //Set a Value in Cache
            database.StringSet("TestConsole", "Hello from Console App, how are you doing");

            //Read recently setted Value From Cache
            var cachedresponse = database.StringGet("TestConsole");
            Console.WriteLine(cachedresponse.ToString());

            //Add an Object to Redis
            Employee e1 = new Employee() { Id = 1, Name = "Console Employee" };
            var jsonString = JsonConvert.SerializeObject(e1);
            database.StringSet("e1", jsonString);

            //Read An Object From redis
            cachedresponse = database.StringGet("e1");
            var employee = JsonConvert.DeserializeObject<Employee>(cachedresponse.ToString());
            Console.WriteLine($"Employee Id from Cache: {employee.Id}");
            Console.WriteLine($"Employee Name from Cache: {employee.Name}");

            Console.WriteLine("Press Any Key to exit....");
            Console.ReadLine();
        }

        public class Employee
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        #region Connection
        //Connection
        //This approach to sharing a ConnectionMultiplexer instance
        //in your application uses a static property that returns a connected instance.
        //The code provides a thread-safe way to initialize only a single connected
        //ConnectionMultiplexer instance. abortConnect is set to false, which means that the
        //call succeeds even if a connection to the Azure Cache for Redis is not established.
        //One key feature of ConnectionMultiplexer is that it automatically restores connectivity
        //to the cache once the network issue or other causes are resolved.
        private static Lazy<ConnectionMultiplexer> lazyConnection = CreateConnection();

        public static ConnectionMultiplexer Connection
        {
            get { return lazyConnection.Value; }
        }

        private static Lazy<ConnectionMultiplexer> CreateConnection()
        {
            return new Lazy<ConnectionMultiplexer>(() =>
            {
                return ConnectionMultiplexer.Connect("localhost:5001");//Provide your connecting string
            });
        }
        #endregion

        #region Retry Policy
        // Built using Polly
        private static RetryPolicy retryPolicy = Policy
                                                    .Handle<Exception>()
                                                    .WaitAndRetry(5, p =>
                                                    {
                                                        var timeToWait = TimeSpan.FromSeconds(90);
                                                        Console.WriteLine($"Waiting for reconnection {timeToWait}");
                                                        return timeToWait;
                                                    });

        #endregion

        //Get Redis Database 
        public static IDatabase GetDatabase()
        {
            return retryPolicy.Execute(() => Connection.GetDatabase());
        }

        //Get Redis Endpoint
        public static System.Net.EndPoint[] GetEndPoints()
        {
            return retryPolicy.Execute(() => Connection.GetEndPoints());
        }

        //Get Redis Server
        public static IServer GetServer(string host, int port)
        {
            return retryPolicy.Execute(() => Connection.GetServer(host, port));
        }
    }
}