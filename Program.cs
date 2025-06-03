using PersonJSON_TCPChat;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace ChatTCPClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            int port = 1945;
            string ip = "127.0.0.1";
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            //Створюємо об'єкт для серілазації та десеріалізації
            Console.WriteLine("Enter your name:");
            string? name = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Name cannot be empty.");
                return;
            }

            Person person = new Person { Name = name };
            //завантажуємо фото на сторонній сервер а потім отримуємо поилання на фото та зберігаємо його в об'єкті Person
            Console.WriteLine("Enter your image path:");
            string? imagePath = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(imagePath) && File.Exists(imagePath))
            {
                using HttpClient httpClient = new HttpClient();
                byte[] data = await File.ReadAllBytesAsync(imagePath);
                string imageBase64 = Convert.ToBase64String(data);
                var json = JsonSerializer.Serialize(new { Photo = imageBase64 });
                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync("https://myp22.itstep.click/api/Galleries/upload", content);
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Image uploaded successfully.");
                }
                else
                {
                    Console.WriteLine($"Failed to upload image: {result}");
                }

                using JsonDocument doc = JsonDocument.Parse(result);
                if (doc.RootElement.TryGetProperty("image", out JsonElement photoElement))
                {
                    person.Image = photoElement.GetString();
                }
            }
            //створюємо TCP-з'єднання з сервером
            using TcpClient client = new TcpClient();
            try
            {
                await client.ConnectAsync(ip, port);
                Console.WriteLine("Connected to the server.");
                // Отримуємо мережевий потік для читання та запису даних
                using NetworkStream stream = client.GetStream();

                // Надсилаємо ім'я та фото
                byte[] nameBytes = Encoding.UTF8.GetBytes(person.ToJson());
                await stream.WriteAsync(nameBytes, 0, nameBytes.Length);

                // Запускаємо прийом повідомлень у окремому завданні
                var receiveTask = ReceiveMessagesAsync(stream);
                string? message;
                byte[] messageBytes;
                // Відправляємо повідомлення нескінченно, поки не буде введено "exit"
                while (true)
                {
                     message= Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(message)) continue;

                    
                    messageBytes = Encoding.UTF8.GetBytes(message);
                    await stream.WriteAsync(messageBytes, 0, messageBytes.Length);

                    if (message.Trim().ToLower() == "exit")
                        break;
                }


                // Дочекаємось завершення прийому повідомлень
                await receiveTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("Disconnected from the server.");
            }
        }

        static async Task ReceiveMessagesAsync(NetworkStream stream)
        {
            byte[] buffer = new byte[1024];
            try
            {
                while (true)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break; // Connection closed
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine(message);
                }
                stream.Close(); // Закриваємо потік після завершення читання
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving message: {ex.Message}");
            }
        }
    }
}
