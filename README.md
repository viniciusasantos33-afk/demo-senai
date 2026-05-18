# Careers - Command Injection Simulation (Equifax Case Study)

Este projeto é uma aplicação web desenvolvida em ASP.NET Core MVC que simula uma plataforma de envio de currículos inspirada no design minimalista da Apple. O principal objetivo deste repositório é fins **didáticos e acadêmicos**, demonstrando de forma prática e controlada uma vulnerabilidade de **Execução Remota de Código (RCE)** via **Injeção de Comando (Command Injection)**, simulando a mecânica do famoso ataque histórico sofrido pela **Equifax** em 2017 (CVE-2017-5638).

## 🛠️ Pré-requisitos

Antes de começar, você vai precisar ter instalado em sua máquina as seguintes ferramentas:
* [Git](https://git-scm.com) (Para clonar o projeto)
* [.NET SDK 8.0 ou 9.0](https://dotnet.microsoft.com/download)
* Uma IDE como [Visual Studio](https://visualstudio.microsoft.com/) ou [VS Code](https://code.visualstudio.com/)
* Uma ferramenta de testes de API como **HTTPie**, **cURL** ou **Postman**

## 📦 Como baixar e rodar o projeto

### 1. Clonar o repositório
```bash
git clone [https://github.com/seu-usuario/nome-do-seu-repositorio.git](https://github.com/seu-usuario/nome-do-seu-repositorio.git)

```

### 2. Entrar no diretório do projeto

```bash
cd nome-do-seu-repositorio

```

### 3. Executar a aplicação

```bash
dotnet run

```

> 💡 **Nota:** O terminal exibirá as URLs locais (geralmente `http://localhost:5000` ou `https://localhost:5001`). Abra o endereço indicado no seu navegador.

## 📁 Como Gerar os Arquivos para o Teste

Para realizar os testes descritos abaixo, você precisará de arquivos na pasta onde está rodando o seu terminal. Você pode gerá-los rapidamente usando os comandos a seguir:

### No Windows (PowerShell)

```powershell
# Gerar um arquivo de currículo normal para testar pelo navegador
New-Item -Path . -Name "meu_curriculo.pdf" -ItemType "file" -Value "Conteudo legítimo do PDF"

# Gerar o arquivo simulando o payload malicioso em lote (para testes avançados)
New-Item -Path . -Name "curriculo & whoami & rem .pdf" -ItemType "file" -Value "Payload de teste"

```

### No Linux / macOS / Git Bash

```bash
# Gerar um arquivo de currículo normal para testar pelo navegador
echo "Conteudo legitimo do PDF" > meu_curriculo.pdf

# Gerar o arquivo simulando o payload malicioso em lote (para testes avançados)
echo "Payload de teste" > "curriculo & whoami & rem .pdf"

```

## 💻 Como Funciona a Demonstração da Falha

A aplicação possui dois fluxos distintos projetados especificamente para apresentações e defesas de trabalhos:

### 1. O Fluxo Legítimo (Navegador)

* Acesse a interface visual estilo Apple pelo navegador.
* Anexe o arquivo `meu_curriculo.pdf` normal que você gerou e clique em **Enviar Candidatura**.
* O formulário processará o arquivo com sucesso, a interface mudará dinamicamente para o estado **Verde** indicando o recebimento, e um log técnico de depuração do servidor será impresso discretamente no `console.log` do desenvolvedor (F12).

### 2. O Fluxo do Ataque (Baseado no caso Equifax)

Alinhado com a brecha real da Equifax, onde o framework processava metadados de requisições de upload de forma insegura, o backend deste projeto foi configurado para inspecionar parâmetros textuais da requisição HTTP e executá-los no terminal do Sistema Operacional.

Para testar a injeção de comandos de forma isolada, limpa e profissional, você pode utilizar o **HTTPie** ou o **cURL** para enviar o payload diretamente nos metadados da rede sem poluir a tela com código HTML.

#### Executando via cURL (Windows Prompt):

```bash
curl -i -s -X POST http://localhost:5000/Curriculum/Enviar -F "arquivo=@meu_curriculo.pdf;filename='curriculo & whoami & rem .pdf'" | findstr /I "HTTP/1.1 X-Terminal-Output Server"
```

> 📊 **Resultado do Exploit:** O servidor processará o comando injetado no cabeçalho `X-Exploit-Command` e devolverá a resposta do terminal do Windows/Linux de forma cirúrgica diretamente no cabeçalho de resposta da rede sob a propriedade **`X-Terminal-Output`**, provando a quebra do isolamento do protocolo HTTP.

## 🛡️ O que este projeto ensina sobre Defesa?

A existência desta falha no código demonstra a importância de:

1. **Nunca confiar na entrada do usuário:** Metadados, cabeçalhos HTTP e nomes de arquivos devem passar por severa sanitização (*allowlists*) antes de qualquer manipulação.
2. **Evitar a concatenação de comandos no OS:** Sempre utilizar APIs nativas da linguagem de programação que tratem argumentos como dados literais e não como instruções executáveis.
3. **Princípio do Menor Privilégio:** O processo que roda o servidor Kestrel/.NET nunca deve rodar como Administrador/Root do sistema.

## 🛠️ Tecnologias Utilizadas

* **C# / .NET Core (MVC)**
* **Razor Engines** (Com condicionais dinâmicos para renderização de estados)
* **Tailwind CSS** (Para a fidelidade visual da interface)

Feito com ❤️ para fins de conscientização em Segurança da Informação. 😉
