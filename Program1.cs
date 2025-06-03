using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using PersonJSON_TCPChat;

ConcurrentDictionary<TcpClient,Person> people = new ConcurrentDictionary<TcpClient, Person>();
Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;
TcpListener tcpListener = new TcpListener(System.Net.IPAddress.Any, 1945);
tcpListener.Start();
Console.WriteLine("Server started. Waiting for a connection...");
while (true)
{
    TcpClient клієнт = await tcpListener.AcceptTcpClientAsync();
    Console.WriteLine("Client connected.");
    _ = ОбробкаКлієнту(клієнт);

}
async Task ОбробкаКлієнту(TcpClient клієнт)
{
    using NetworkStream потік = клієнт.GetStream();
    byte[] буфер_байтів = new byte[1024];
    int байтів_прочитано = await потік.ReadAsync(буфер_байтів, 0, буфер_байтів.Length);
    string джейсон = Encoding.UTF8.GetString(буфер_байтів, 0, байтів_прочитано);

    Person персона = Person.FromJson(джейсон);
    people.TryAdd(клієнт, персона);

    Console.WriteLine($"Received from {персона.Name}: {джейсон}");
    string повідомлення;
    while (true)
    {
        байтів_прочитано = await потік.ReadAsync(буфер_байтів, 0, буфер_байтів.Length);
        if (байтів_прочитано == 0) break; // Клієнт закрив з'єднання
        повідомлення = Encoding.UTF8.GetString(буфер_байтів, 0, байтів_прочитано);
        Console.WriteLine($"Received from {персона.Name}: {повідомлення}");
        if (повідомлення.Trim().ToLower() == "exit")
        {
            Console.WriteLine($"{персона.Name} disconnected.");
            people.TryRemove(клієнт, out _);
            foreach (var іншийКлієнт in people.Keys)
            {
                
                    byte[] повідомленняБайти = Encoding.UTF8.GetBytes($"{персона.Name}: вийшов з чату");
                    await іншийКлієнт.GetStream().WriteAsync(повідомленняБайти, 0, повідомленняБайти.Length);
                
            }
            потік.Close(); // Закриваємо потік після завершення читання
            клієнт.Close(); // Закриваємо клієнтське з'єднання
            break; // Виходимо з циклу, якщо клієнт відключився
        }
        // Відправляємо повідомлення всім клієнтам
        foreach (var іншийКлієнт in people.Keys)
        {
            if (іншийКлієнт != клієнт)
            {
                byte[] повідомленняБайти = Encoding.UTF8.GetBytes($"{персона.Name}[https://myp22.itstep.click/images/{персона.Image}]: {повідомлення}");
                await іншийКлієнт.GetStream().WriteAsync(повідомленняБайти, 0, повідомленняБайти.Length);
            }
        }
    }
}