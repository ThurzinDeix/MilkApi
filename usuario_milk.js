const API_URL = 'http://localhost:7092/api/usuario'; // Altere a porta se necessário

function carregarUsuarios() {
    fetch(API_URL)
        .then(res => res.json())
        .then(data => {
            const tbody = document.getElementById("usuarioTableBody");
            tbody.innerHTML = "";
            data.forEach(usuario => {
                const row = `<tr>
                    <td>${usuario.id}</td>
                    <td>${usuario.id_Telefone}</td>
                    <td>${usuario.data_Nasc}</td>
                    <td>${usuario.id_Fazenda}</td>
                    <td>
                        <button onclick="deletarUsuario(${usuario.id})">Excluir</button>
                    </td>
                </tr>`;
                tbody.innerHTML += row;
            });
        });
}

function deletarUsuario(id) {
    fetch(API_URL + '/' + id, {
        method: 'DELETE'
    }).then(() => carregarUsuarios());
}

document.getElementById("usuarioForm").addEventListener("submit", function (e) {
    e.preventDefault();
    const novoUsuario = {
        id_Telefone: parseInt(document.getElementById("idTelefone").value),
        data_Nasc: document.getElementById("dataNasc").value,
        id_Fazenda: parseInt(document.getElementById("idFazenda").value)
    };

    fetch(API_URL, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(novoUsuario)
    }).then(() => {
        this.reset();
        carregarUsuarios();
    });
});

function checkAuthentication() {
    // Verifique se o usuário está autenticado comparando com o valor no localStorage
    const isAuthenticated = localStorage.getItem('authenticated');

    // Se isAuthenticated for verdadeiro, o usuário está autenticado
    return isAuthenticated === 'true';
}

// Verificar a autenticação antes de permitir o acesso à dashboard
window.addEventListener('DOMContentLoaded', () => {
    if (!checkAuthentication()) {
        // Se o usuário não estiver autenticado, redirecione para a página de login
        window.location.href = 'login.html';
    }
});

// Inicializar
carregarUsuarios();
