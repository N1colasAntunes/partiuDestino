// Inicializa o EmailJS com a Public Key
emailjs.init("HCQS01u6JER5HTprf");

function enviarEmailJS() {
    const nome = document.getElementById("contato-nome").value.trim();
    const email = document.getElementById("contato-email").value.trim();
    const mensagem = document.getElementById("contato-mensagem").value.trim();
    const btn = document.getElementById("btn-enviar-contato");

    if (!nome || !email || !mensagem) {
        alert("Por favor, preencha todos os campos.");
        return;
    }

    btn.disabled = true;
    btn.textContent = "Enviando...";

    const templateParams = {
        from_name: nome,
        from_email: email,
        message: mensagem
    };

    emailjs.send("service_tq95iti", "template_a6cz0oz", templateParams)
        .then(function () {
            document.getElementById("alerta-sucesso").style.display = "block";
            document.getElementById("alerta-erro").style.display = "none";

            document.getElementById("contato-nome").value = "";
            document.getElementById("contato-email").value = "";
            document.getElementById("contato-mensagem").value = "";

            btn.disabled = false;
            btn.textContent = "Enviar mensagem";

            setTimeout(function () {
                document.getElementById("alerta-sucesso").style.display = "none";
            }, 5000);
        })
        .catch(function (error) {
            console.error("Erro EmailJS:", error);
            document.getElementById("alerta-erro").style.display = "block";
            document.getElementById("alerta-sucesso").style.display = "none";

            btn.disabled = false;
            btn.textContent = "Enviar mensagem";
        });
}