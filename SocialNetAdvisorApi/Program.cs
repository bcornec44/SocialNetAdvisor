using System.Net.Sockets;
using System.Net;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}
app.UseSwagger();
app.UseSwaggerUI();

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

string localIP = LocalIPAddress();

app.Urls.Add("http://" + localIP + ":5000");

app.Run();


static string LocalIPAddress()
{
    using Socket socket = new(AddressFamily.InterNetwork, SocketType.Dgram, 0);
    socket.Connect("8.8.8.8", 65530);
    return socket.LocalEndPoint is IPEndPoint endPoint ? endPoint.Address.ToString() : "127.0.0.1";
}