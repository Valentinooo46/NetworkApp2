using System.Net;
using System.Net.Sockets;
using System.Text;

public class Program1
{
    static void Main()
    {
        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = Encoding.UTF8;

        Console.WriteLine("Вкажіть IP сервер:");
        var ip = IPAddress.Parse(Console.ReadLine());
        Console.WriteLine("Вкажіть порт:");
        var port = int.Parse(Console.ReadLine());

        try
        {
            var serverEndPoint = new IPEndPoint(ip, port);
            // створюємо сокет, який буде на сервер відправляти повідомлення
            Socket sender = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
            sender.Connect(serverEndPoint); // конектимо клієнта до сервера
            Console.WriteLine("Вкажіть повідомлення, яке хочете надіслати");
            string text = Console.ReadLine();
            var data = Encoding.UTF8.GetBytes(text); // повідомлення перетворили у байти
            sender.Send(data); // Відправляємо байти на сервер
            data = new byte[1024]; // Чистимо буфер
            int bytes = 0; // кількість байт
            do
            {
                bytes = sender.Receive(data); // Отримали повідомлення від сервера
                Console.WriteLine($"Сервер відповів {Encoding.UTF8.GetString(data)}");
            } while (sender.Available > 0); // читаємо відповідь від сервера
        }
        catch (Exception ex)
        {
            Console.WriteLine("Щось пішло не так " + ex.Message);
        }
    }
}
