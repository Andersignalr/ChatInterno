const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chat")
    .build();

let usuarioRegistrado = false;
let salaAtual = null;

// Eventos
connection.on("UsuarioEntrouNaSala", (nome) => {
    adicionarMensagem(`🟢 ${nome} entrou na sala`);
});

connection.on("UsuarioSaiuDaSala", (nome) => {
    adicionarMensagem(`🔴 ${nome} saiu da sala`);
});

connection.on("ReceberMensagem", (nome, mensagem) => {
    adicionarMensagem(`<strong>${nome}:</strong> ${mensagem}`);
});

connection.on("ListaUsuariosSala", (usuarios) => {
    const ul = document.getElementById("listaUsuarios");
    ul.innerHTML = "";

    usuarios.forEach(nome => {
        const li = document.createElement("li");
        li.textContent = nome;
        ul.appendChild(li);
    });
});

connection.start().catch(err => console.error(err));

// Ações
function registrar() {
    const nome = document.getElementById("nome").value;
    if (!nome) return;

    connection.invoke("RegistrarUsuario", nome)
        .then(() => {
            usuarioRegistrado = true;
            document.getElementById("login").style.display = "none";
            document.getElementById("chat").style.display = "block";
        });
}

function entrarSala() {
    const sala = document.getElementById("sala").value;
    if (!sala || !usuarioRegistrado) return;

    salaAtual = sala;
    document.getElementById("salaAtual").innerText = `(${sala})`;
    document.getElementById("mensagens").innerHTML = "";

    connection.invoke("EntrarNaSala", sala);
}

function enviar() {
    const mensagem = document.getElementById("mensagem").value;
    if (!mensagem || !salaAtual) return;

    connection.invoke("EnviarMensagem", mensagem);
    document.getElementById("mensagem").value = "";
}

function adicionarMensagem(html) {
    const div = document.createElement("div");
    div.innerHTML = html;
    document.getElementById("mensagens").appendChild(div);
}
