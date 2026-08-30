document.addEventListener("DOMContentLoaded", () => {
  const auth = getAuth();
  if (auth) {
    window.location.href = paginaInicialPara(auth.papel);
    return;
  }

  const form = document.getElementById("form-login");
  const campoEmail = document.getElementById("login-email");
  const campoSenha = document.getElementById("login-senha");
  const feedback = document.getElementById("feedback");

  form.addEventListener("submit", async (evento) => {
    evento.preventDefault();

    const botaoEntrar = form.querySelector("button[type=submit]");
    botaoEntrar.disabled = true;

    try {
      const resultado = await api.auth.login({
        email: campoEmail.value.trim(),
        senha: campoSenha.value,
      });

      salvarAuth(resultado);
      window.location.href = paginaInicialPara(resultado.papel);
    } catch (erro) {
      mostrarFeedback(feedback, erro.message || "Não foi possível entrar. Verifique suas credenciais.", "erro");
    } finally {
      botaoEntrar.disabled = false;
    }
  });
});
