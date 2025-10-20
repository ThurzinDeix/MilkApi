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
  menu.classList.toggle("show")
}

