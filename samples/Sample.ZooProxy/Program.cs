using Keep.ZooProxy;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sample.ZooProxy
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string connStr = "192.168.117.52:2181/test";
            var zkClient = new ZooKeeperClient(connStr);
            await zkClient.OpenAsync();

            var n1 = await zkClient.ProxyNodeAsync("n1");
            await n1.CreateAsync(Permission.All, ignoreExists: true);

            var book = new Book { Name = "yellow", Price = 7.5 };
            var nbook = await n1.ProxyJsonNodeAsync<Book>("book", true);
            await nbook.CreateAsync(book, Permission.All, ignoreExists: true);
            nbook.DataChanged += (_, e) =>
            {
                var bk = e.Data;
                Console.WriteLine($"book: [name: {bk.Name}, price: {bk.Price}]");
            };

            var props = new Dictionary<string, string>
            {
                { "zkclient", "good" },
                { "zooproxy", "better" }
            };
            var n2 = await zkClient.ProxyPropertyNodeAsync("n2");
            await n2.CreateAsync(props, Permission.All, Mode.Ephemeral, true);

            Console.WriteLine("done!");
            Console.ReadLine();
        }

        class Book
        {
            public string Name { get; set; }
            public double Price { get; set; }
        }
    }
}
