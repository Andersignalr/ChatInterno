using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace ChatInterno.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private static ConcurrentDictionary<string, string> UserSala =
        new();

    public override async Task OnConnectedAsync()
    {
        var nome = Context.User.Identity!.Name!;
        await Clients.Caller.SendAsync("Sistema", $"Conectado como {nome}");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();

        if (UserSala.TryRemove(userId, out var sala))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, sala);
            await Clients.Group(sala)
                .SendAsync("UsuarioSaiuDaSala", Context.User.Identity!.Name);

            await EnviarUsuariosDaSala(sala);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task EntrarNaSala(string sala)
    {
        var userId = GetUserId();
        var nome = Context.User.Identity!.Name!;

        if (UserSala.TryGetValue(userId, out var salaAnterior))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, salaAnterior);
            await Clients.Group(salaAnterior)
                .SendAsync("UsuarioSaiuDaSala", nome);
        }

        UserSala[userId] = sala;

        await Groups.AddToGroupAsync(Context.ConnectionId, sala);

        await Clients.Group(sala)
            .SendAsync("UsuarioEntrouNaSala", nome);

        await EnviarUsuariosDaSala(sala);
    }

    public async Task EnviarMensagem(string mensagem)
    {
        var userId = GetUserId();

        if (!UserSala.TryGetValue(userId, out var sala))
            return;

        await Clients.Group(sala)
            .SendAsync("ReceberMensagem",
                Context.User.Identity!.Name,
                mensagem);
    }

    public async Task EnviarMensagemPrivada(string userIdDestino, string mensagem)
    {
        var remetente = Context.User.Identity!.Name;

        await Clients.User(userIdDestino)
            .SendAsync("ReceberMensagemPrivada", remetente, mensagem);
    }

    private async Task EnviarUsuariosDaSala(string sala)
    {
        var usuarios = UserSala
            .Where(x => x.Value == sala)
            .Select(x => x.Key)
            .ToList();

        await Clients.Group(sala)
            .SendAsync("ListaUsuariosSala", usuarios);
    }

    private string GetUserId()
    {
        return Context.User!
            .FindFirst(ClaimTypes.NameIdentifier)!
            .Value;
    }
}
