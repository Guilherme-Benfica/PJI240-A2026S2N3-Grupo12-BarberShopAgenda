// URL base da API. Em produção (GitHub Pages) aponta pro serviço no Render;
// em desenvolvimento local (localhost/127.0.0.1) mantém o backend local.
(function () {
  const ehLocal = ["localhost", "127.0.0.1"].includes(window.location.hostname);
  window.BARBERSHOP_API_URL = ehLocal
    ? "https://localhost:7001/api"
    : "https://pji240-a2026s2n3-grupo12-barbershopagenda.onrender.com/api";
})();
