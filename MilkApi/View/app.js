// auth.js
function logout() {
  localStorage.removeItem('authenticated');
  localStorage.removeItem('userId');
  alert("Você saiu da conta.");
  window.location.href = "login.html";
}

// Protege páginas internas
window.onload = function () {
  const autenticado = localStorage.getItem('authenticated');
  if (!autenticado && !window.location.href.includes("login.html")) {
    window.location.href = "login.html";
  }
}

function toggleDropdown() {
  document.getElementById("dropdownMenu").classList.toggle("show");
}

window.onclick = function (e) {
  if (!e.target.closest('.user-menu')) {
    document.getElementById("dropdownMenu").classList.remove("show");
  }
}

function toggleMenu() {
  const menu = document.getElementById("menu")
  const aberto = menu.classList.toggle("show")
  document.body.classList.toggle("escurecido", aberto)
  document.body.style.overflow = aberto ? "hidden" : "auto"
}

document.addEventListener("click", e => {
  const menu = document.getElementById("menu")
  if (menu.classList.contains("show") && !e.target.closest(".hamburger") && !e.target.closest("#menu")) {
    menu.classList.remove("show")
    document.body.classList.remove("escurecido")
    document.body.style.overflow = "auto"
  }
})

window.addEventListener("DOMContentLoaded", () => {
  if (window.innerWidth >= 1000) return

  const userMenu = document.querySelector(".user-menu")
  if (!userMenu) return

  const dropdown = document.getElementById("dropdownMenu")
  if (dropdown) dropdown.remove()

  const botoes = document.createElement("div")
  botoes.className = "perfil-btns"
  botoes.innerHTML = `
    <button onclick="window.location.href='perfil.html'" class="perfil-btn">
      <i class="ph-bold ph-user"></i> Editar Perfil
    </button>
    <button onclick="logout()" class="perfil-btn sair">
      <i class="ph-bold ph-sign-out"></i> Sair da Conta
    </button>
  `
  userMenu.appendChild(botoes)
})

