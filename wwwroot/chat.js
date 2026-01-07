/* =========================
   VERIFICA LOGIN
========================= */
async function verificarLogin() {
    const response = await fetch("/auth/me");

    if (!response.ok) {
        window.location.href = "/login.html";
        return;
    }

    const data = await response.json();
    document.getElementById("usuario").innerText = data.user;
}

verificarLogin();

/* =========================
   SIGNALR
========================= */
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chat") // confirme se é /chat ou /chathub
    .build();

let salaAtual = null;

/* =========================
   EVENTOS DO HUB
========================= */

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
    const select = document.getElementById("destinatario");

    ul.innerHTML = "";
    select.innerHTML = "";

    usuarios.forEach(nome => {
        const li = document.createElement("li");
        li.textContent = nome;
        ul.appendChild(li);

        const opt = document.createElement("option");
        opt.value = nome;
        opt.textContent = nome;
        select.appendChild(opt);
    });
});

connection.on("ReceberMensagemPrivada", (nome, mensagem) => {
    adicionarMensagem(`🔒 <strong>${nome} (privado):</strong> ${mensagem}`);
});

/* =========================
   CONECTA
========================= */
connection.start()
    .catch(err => console.error(err));

/* =========================
   AÇÕES
========================= */

function entrarSala() {
    const sala = document.getElementById("sala").value;
    if (!sala) return;

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

function enviarPrivado() {
    const destinatario = document.getElementById("destinatario").value;
    const mensagem = document.getElementById("mensagemPrivada").value;

    if (!destinatario || !mensagem) return;

    connection.invoke("EnviarMensagemPrivada", destinatario, mensagem);
    document.getElementById("mensagemPrivada").value = "";
}

/* =========================
   UI
========================= */

function adicionarMensagem(html) {
    const div = document.createElement("div");
    div.innerHTML = html;

    const mensagens = document.getElementById("mensagens");
    mensagens.appendChild(div);
    mensagens.scrollTop = mensagens.scrollHeight;
}
