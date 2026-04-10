# CloudFlare-DUC-Tool

Cliente de DNS Dinâmico (DDNS) leve e multi-tenant para Cloudflare. Sincroniza automaticamente IPs dinâmicos com seus registros DNS (suporte a IPv4 e IPv6).

## 1. Credenciais da Cloudflare
Você precisa de dois dados do seu painel Cloudflare:
1. **Zone ID:** Localizado na barra lateral direita da página "Visão Geral" do seu domínio.
2. **Token de API:** Acesse `Meu Perfil > Tokens de API > Criar Token`. Utilize o modelo **Editar zona DNS** e restrinja o acesso estritamente ao seu domínio. Não utilize sua Chave Global.

## 2. Configuração
Crie um arquivo chamado `config.ini` no mesmo diretório do executável. O sistema suporta múltiplos perfis simultâneos; basta adicionar novos blocos `[NomeDoPerfil]`.

```ini
[General]
; Intervalo de verificação em minutos. Mínimo recomendado: 5 (evita Rate Limit).
IntervalMinutes=5

; Exemplo IPv4
[ServidorPrincipal]
ApiToken=SEU_TOKEN_AQUI
ZoneId=SEU_ZONE_ID_AQUI
RecordName=vpn.seudominio.com.br
RecordType=A
Proxied=false
TTL=1

; Exemplo IPv6
[SistemaWeb]
ApiToken=SEU_TOKEN_AQUI
ZoneId=SEU_ZONE_ID_AQUI
RecordName=app.seudominio.com.br
RecordType=AAAA
Proxied=true
TTL=120