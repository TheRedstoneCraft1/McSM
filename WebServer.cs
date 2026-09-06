namespace WebServer;

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

public class Server
{
    public static async Task Main()
    {
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:8080/");
        listener.Start();
        Console.WriteLine("WS: http://localhost:8080/");
        Process.Start(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? new ProcessStartInfo("http://localhost:8080") { UseShellExecute = true } : RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? new ProcessStartInfo("open", "http://localhost:8080") : new ProcessStartInfo("xdg-open", "http://localhost:8080"));


        while (true)
        {
            HttpListenerContext context = await listener.GetContextAsync();
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            Console.WriteLine($"Anfrage: {request.Url.PathAndQuery}");

            try
            {
                // HTML-Datei laden
                string filePath = "./WS/index.html";

                if (File.Exists(filePath))
                {
                    byte[] buffer = File.ReadAllBytes(filePath);
                    response.ContentType = "text/html; charset=utf-8";
                    response.ContentLength64 = buffer.Length;
                    response.StatusCode = 200;
                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                }
                else
                {
                    string errorMsg = "404 - Datei nicht gefunden";
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(errorMsg);
                    response.StatusCode = 404;
                    response.ContentLength64 = buffer.Length;
                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler: {ex.Message}");
                response.StatusCode = 500;
            }
            finally
            {
                response.OutputStream.Close();
            }
        }
    }
}
