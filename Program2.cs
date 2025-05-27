using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Net;
Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;
Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
socket.Bind(new IPEndPoint(IPAddress.Any, 1945));
socket.Listen(10);
Console.WriteLine("Server started. Waiting for connections...");
byte[] buffer = new byte[1024];

while (true)
{
    Socket clientSocket = socket.Accept();
    Console.WriteLine("Client connected.");
    _ = HandleClientAsync(clientSocket);
    //bytesRead = clientSocket.Receive(buffer);
    //request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
    //try
    //{

    //    Console.WriteLine($"О {DateTime.Now} від [{(clientSocket.RemoteEndPoint as IPEndPoint).Address}] отримано рядок: {request}");
    //    // Process the request and send a response
    //    response = "";
    //    if (request.Contains("Привіт"))
    //    {
    //        response += "привіт, клієнте!";
    //    }
    //    if (request.Contains("видай дату"))
    //    {
    //        response += $" {DateTime.Today}";
    //        byte[] responseBytes = Encoding.UTF8.GetBytes(response);
    //        clientSocket.Send(responseBytes);
    //    }
    //    else if (request.Contains("видай час"))
    //    {
    //        response += $" {DateTime.Now}";
    //        byte[] responseBytes = Encoding.UTF8.GetBytes(response);
    //        clientSocket.Send(responseBytes);
    //    }
    //    else if (request.Contains("видай день тижня"))
    //    {
    //        response += $" {DateTime.Now.DayOfWeek}";
    //        byte[] responseBytes = Encoding.UTF8.GetBytes(response);
    //        clientSocket.Send(responseBytes);
    //    }
    //    else
    //    {
    //        response += " Невідома команда";
    //        byte[] responseBytes = Encoding.UTF8.GetBytes(response);
    //        clientSocket.Send(responseBytes);
    //    }

    //}
    //catch (Exception ex)
    //{
    //    Console.WriteLine($"Error: {ex.Message}");
    //}


}
async Task HandleClientAsync(Socket clientSocket)
{
    var buffer = new byte[1024];
    try
    {
        int bytesRead = await clientSocket.ReceiveAsync(buffer, SocketFlags.None);
        if (bytesRead > 0)
        {
            string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"О {DateTime.Now} від [{(clientSocket.RemoteEndPoint as IPEndPoint).Address}] отримано рядок: {request}");

            string response = "";
            if (request.Contains("Привіт"))
            {
                response += "привіт, клієнте!";
            }
            if (request.Contains("видай дату"))
            {
                response += $" {DateTime.Today}";
            }
            else if (request.Contains("видай час"))
            {
                response += $" {DateTime.Now}";
            }
            else if (request.Contains("видай день тижня"))
            {
                response += $" {DateTime.Now.DayOfWeek}";
            }
            else
            {
                response += " Невідома команда";
            }

            byte[] responseBytes = Encoding.UTF8.GetBytes(response);
            await clientSocket.SendAsync(responseBytes, SocketFlags.None);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
    finally
    {
        clientSocket.Shutdown(SocketShutdown.Both);
        clientSocket.Close();
    }
}