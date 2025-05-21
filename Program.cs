using System.Net.Sockets;
using System.Threading;

string host = "rozetka.com.ua";
int min_port = Int32.Parse(Console.ReadLine());
int max_port = Int32.Parse(Console.ReadLine());

if (min_port > max_port)
{
    (min_port, max_port) = (max_port, min_port);
}
if (min_port < 1 || max_port > 65535)
{
    Console.WriteLine("Вказано некоректний діапазон портів (допустимо 1-65535)");
    return;
}

for (int i = min_port; i <= max_port; i++)
{
    using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    try
    {
        // Встановлюємо таймаут на підключення
        IAsyncResult result = socket.BeginConnect(host, i, null, null);
        bool success = result.AsyncWaitHandle.WaitOne(1000, true); // 1 секунда таймаут

        if (!success)
        {
            Console.WriteLine($"Порт {i} закритий (таймаут)");
            socket.Close();
            continue;
        }

        socket.EndConnect(result);
        Console.WriteLine($"Порт {i} відкритий");
    }
    catch (SocketException)
    {
        Console.WriteLine($"Порт {i} закритий");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Виникла помилка: {ex.Message}");
    }
}