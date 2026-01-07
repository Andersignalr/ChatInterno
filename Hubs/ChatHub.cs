using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace ChatInterno.Hubs;

public class ChatHub : Hub
{
    private static ConcurrentDictionary<string, string> Usuarios =
        new ConcurrentDictionary<string, string>();

    private static ConcurrentDictionary<string, string> SalasPorConexao =
        new ConcurrentDictionary<string, string>();

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Usuarios.TryRemove(Context.ConnectionId, out var nome))
        {
            if (SalasPorConexao.TryRemove(Context.ConnectionId, out var sala))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, sala);
                await Clients.Group(sala).SendAsync("UsuarioSaiuDaSala", nome);
                await EnviarUsuariosDaSala(sala);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task RegistrarUsuario(string nome)
    {
        Usuarios[Context.ConnectionId] = nome;
    }

    public async Task EntrarNaSala(string sala)
    {
        var connectionId = Context.ConnectionId;

        // Sai da sala anterior
        if (SalasPorConexao.TryGetValue(connectionId, out var salaAnterior))
        {
            await Groups.RemoveFromGroupAsync(connectionId, salaAnterior);
            await Clients.Group(salaAnterior)
                .SendAsync("UsuarioSaiuDaSala", Usuarios[connectionId]);
        }

        SalasPorConexao[connectionId] = sala;

        await Groups.AddToGroupAsync(connectionId, sala);

        await Clients.Group(sala)
            .SendAsync("UsuarioEntrouNaSala", Usuarios[connectionId]);

        await EnviarUsuariosDaSala(sala);
    }

    public async Task EnviarMensagem(string mensagem)
    {
        var connectionId = Context.ConnectionId;

        if (!Usuarios.TryGetValue(connectionId, out var nome))
            return;

        if (!SalasPorConexao.TryGetValue(connectionId, out var sala))
            return;

        await Clients.Group(sala)
            .SendAsync("ReceberMensagem", nome, mensagem);
    }

    private async Task EnviarUsuariosDaSala(string sala)
    {
        var usuariosDaSala = SalasPorConexao
            .Where(x => x.Value == sala)
            .Select(x => Usuarios[x.Key])
            .OrderBy(n => n)
            .ToList();

        await Clients.Group(sala)
            .SendAsync("ListaUsuariosSala", usuariosDaSala);
    }
}
