# OrderSystem

Duas aplicações independentes que se comunicam via FIX 4.4 (QuickFIX/n):

- **OrderGenerator**: API em C# com frontend React. Envia ordens (`NewOrderSingle`).
- **OrderAccumulator**: serviço em C#. Calcula a exposição financeira por símbolo e aceita ou rejeita cada ordem (`ExecutionReport`).

As aplicações não têm dependência de código entre si. A única integração é a sessão FIX.

## Arquitetura

```mermaid
flowchart LR
    UI["React"] -- "HTTP" --> API["OrderGenerator<br/>Minimal API · FIX Initiator"]
    API -- "NewOrderSingle" --> ACC["OrderAccumulator<br/>Worker Service · FIX Acceptor"]
    ACC -- "ExecutionReport" --> API
```

```mermaid
sequenceDiagram
    participant UI as React
    participant API as OrderGenerator (Initiator)
    participant ACC as OrderAccumulator (Acceptor)

    UI->>API: POST /api/orders
    API->>API: valida ordem
    API->>ACC: NewOrderSingle (ClOrdID)
    ACC->>ACC: calcula exposição projetada

    alt |exposição| ≤ R$ 100.000.000
        ACC->>ACC: grava exposição
        ACC-->>API: ExecutionReport (ExecType = New)
    else ultrapassa o limite
        ACC-->>API: ExecutionReport (ExecType = Rejected)
    end

    API-->>UI: resultado
```

### Decisões

**Geral**

- .NET 10 e QuickFIXn 1.14.1 (`QuickFIXn.Core`, `QuickFIXn.FIX44`).
- Regras de negócio em projetos `.Core`, sem dependência do QuickFIX nem do ASP.NET.
- Valores monetários em `decimal`.
- Estado em memória.
- Sessão FIX com `ResetOnLogon=Y` (estado em memória) e `UseDataDictionary=N` (validação dos campos feita na origem).

**OrderAccumulator**

- Cálculo, verificação do limite e gravação executados de forma atômica (`lock`).
- Rejeição por limite com `OrdRejReason = 3` (Order exceeds limit).
- Valida apenas o necessário para o cálculo (lado Compra ou Venda, quantidade e preço positivos). Ordens inválidas são respondidas com `ExecType = Rejected` e `OrdRejReason = 99` (Other), sem afetar a exposição. As regras do formulário são responsabilidade do OrderGenerator.
- Erros inesperados no processamento de mensagens são registrados em log e repassados ao QuickFIX/n para o tratamento padrão do protocolo.
- Acceptor iniciado e encerrado por um `IHostedService`; `ExposureCalculator` registrado como singleton.

**OrderGenerator**

- ASP.NET Core Minimal API, com o endpoint em arquivo próprio e respostas tipadas (`TypedResults`).
- Validação das regras do formulário em `OrderGenerator.Core`, retornando todos os erros por campo.
- A requisição HTTP aguarda o `ExecutionReport` correspondente, correlacionado pelo `ClOrdID` (`TaskCompletionSource` em um `ConcurrentDictionary`), com timeout de 5 segundos.
- A ordem é registrada antes do envio, evitando perder respostas que cheguem antes do registro.
- Enums trafegam como texto no JSON (`"side": "Buy"`, `"status": "Rejected"`).
- CORS liberado para o frontend em `http://localhost:5173`.

| Situação | HTTP |
|---|---|
| Ordem aceita ou rejeitada pelo OrderAccumulator | `200` |
| Campos inválidos | `400` (erros por campo) |
| Sessão FIX desconectada | `503` |
| Sem resposta em 5 segundos | `504` |

Uma ordem rejeitada pelo limite retorna `200`: a requisição foi processada e o resultado é `Rejected`.

### Premissas

- Quantidade executada = quantidade da ordem aceita.
- Exposição igual a R$ 100.000.000 é aceita; somente valores acima são rejeitados.
- O limite se aplica em valor absoluto (compra e venda).

## Estrutura

```
backend/
├── OrderSystem.slnx
├── OrderAccumulator/
│   ├── src/
│   │   ├── OrderAccumulator.Core/
│   │   │   ├── Orders/        Order, OrderSide
│   │   │   └── Exposure/      ExposureCalculator, ExposureDecision
│   │   └── OrderAccumulator.Worker/
│   │       ├── Fix/           AccumulatorFixApp, FixOrderTranslator
│   │       ├── Services/      FixAcceptorService
│   │       ├── SessionConfig/ fix-acceptor.cfg
│   │       └── Program.cs
│   └── tests/
│       └── OrderAccumulator.Tests/
│           ├── CoreExposure/  ExposureCalculatorTests
│           └── WorkerFix/     FixOrderTranslatorTests
└── OrderGenerator/
    ├── src/
    │   ├── OrderGenerator.Core/
    │   │   ├── Orders/        Order, OrderSide
    │   │   └── Validation/    OrderValidator
    │   └── OrderGenerator.Api/
    │       ├── Contracts/     OrderRequest, OrderResponse, OrderStatus
    │       ├── Endpoints/     OrderEndpoints
    │       ├── Fix/           GeneratorFixApp, FixOrderSender, FixOrderTranslator,
    │       │                  PendingOrderRegistry, IOrderSender, FixSessionUnavailableException
    │       ├── Services/      FixInitiatorService
    │       ├── SessionConfig/ fix-initiator.cfg
    │       ├── OrderGenerator.Api.http
    │       └── Program.cs
    └── tests/
        └── OrderGenerator.Tests/
            ├── CoreValidation/ OrderValidatorTests
            └── ApiFix/         FixOrderTranslatorTests, PendingOrderRegistryTests
```

## Testes

xUnit, sem dependência de rede. 56 testes no total.

### OrderAccumulator

**ExposureCalculatorTests**

| Teste | Regra |
|---|---|
| `Simbolo_sem_ordens_tem_exposicao_zero` | Exposição inicial é zero |
| `Compra_aceita_aumenta_a_exposicao` | Compra soma preço × quantidade |
| `Venda_aceita_diminui_a_exposicao` | Venda subtrai preço × quantidade |
| `Compra_e_venda_se_compensam` | Compras e vendas se compensam |
| `Exposicao_e_calculada_por_simbolo` | Exposição independente por símbolo |
| `Ordem_que_atinge_exatamente_o_limite_e_aceita` | Limite exato é aceito |
| `Compra_que_ultrapassa_o_limite_e_rejeitada` | Acima de +100M é rejeitada |
| `Venda_que_ultrapassa_o_limite_negativo_e_rejeitada` | Abaixo de −100M é rejeitada |
| `Ordem_rejeitada_nao_altera_a_exposicao` | Rejeitada não entra no cálculo |
| `Ordem_que_reduz_a_exposicao_e_aceita_mesmo_no_limite` | Ordem que reduz a exposição é aceita |
| `Ordens_concorrentes_nunca_ultrapassam_o_limite` | 1.000 ordens paralelas de R$ 50M: exatamente 2 aceitas |

**FixOrderTranslatorTests**

| Teste | Regra |
|---|---|
| `TryToOrder_converte_os_campos_da_mensagem` | `NewOrderSingle` → `Order` (compra e venda) |
| `TryToOrder_rejeita_lado_nao_suportado` | Lado diferente de Compra/Venda é recusado com motivo |
| `TryToOrder_rejeita_quantidade_ou_preco_nao_positivo` | Quantidade ou preço ≤ 0 é recusado com motivo |
| `Ordem_aceita_gera_ExecutionReport_New` | `ExecType`/`OrdStatus = New`, mesmo `ClOrdID`, campos obrigatórios da 4.4 |
| `Ordem_rejeitada_gera_ExecutionReport_Rejected` | `ExecType`/`OrdStatus = Rejected`, `OrdRejReason = 3`, `Text`, `LeavesQty = 0` |
| `Ordem_invalida_gera_ExecutionReport_Rejected` | `ExecType`/`OrdStatus = Rejected`, `OrdRejReason = 99`, `Text`, `LeavesQty = 0` |

### OrderGenerator

**OrderValidatorTests**

| Teste | Regra |
|---|---|
| `Ordem_valida_nao_tem_erros` | Ordem dentro de todas as regras é aceita |
| `Simbolos_permitidos_sao_aceitos` | PETR4, VALE3 e VIIA4 |
| `Simbolo_fora_da_lista_e_rejeitado` | Outro símbolo, minúsculas, vazio ou nulo |
| `Lado_invalido_e_rejeitado` | Lado fora de Compra/Venda |
| `Quantidade_dentro_do_limite_e_aceita` | 1 e 99.999 |
| `Quantidade_fora_do_limite_e_rejeitada` | 0, negativa e 100.000 |
| `Preco_valido_e_aceito` | 0,01, 10,50 e 999,99 |
| `Preco_invalido_e_rejeitado` | 0, negativo, 1.000, acima de 1.000 e não múltiplo de 0,01 |
| `Varios_erros_sao_retornados_juntos` | Todos os erros retornados em uma única resposta |

**FixOrderTranslatorTests**

| Teste | Regra |
|---|---|
| `ToNewOrderSingle_preenche_os_campos_da_mensagem` | `ClOrdID`, símbolo, lado, quantidade, preço, `OrdType = Limit` e `TransactTime` |
| `ExecutionReport_New_vira_resposta_New` | Status `New`, `ClOrdID`, `OrderID` e dados da ordem |
| `ExecutionReport_Rejected_vira_resposta_Rejected_com_motivo` | Status `Rejected` com o texto do motivo |

**PendingOrderRegistryTests**

| Teste | Regra |
|---|---|
| `Ordem_registrada_fica_aguardando_resposta` | Registro devolve uma tarefa pendente |
| `Resposta_completa_a_ordem_aguardando` | A resposta é entregue a quem aguarda |
| `Resposta_para_ClOrdID_desconhecido_e_ignorada` | Resposta sem ordem registrada é descartada |
| `Resposta_duplicada_e_ignorada` | Apenas a primeira resposta é entregue |
| `Ordem_removida_nao_recebe_resposta` | Resposta após timeout é descartada |
| `ClOrdID_duplicado_nao_pode_ser_registrado` | Dois registros com o mesmo `ClOrdID` são recusados |
| `Respostas_sao_entregues_a_ordem_correta` | Correlação correta com várias ordens em andamento |

## Execução

Pré-requisito: .NET 10 SDK. Comandos executados a partir de `backend/`.

### Testes

```powershell
dotnet test
```

### OrderAccumulator

```powershell
dotnet run --project OrderAccumulator/src/OrderAccumulator.Worker
```

### OrderGenerator

Em outro terminal:

```powershell
dotnet run --project OrderGenerator/src/OrderGenerator.Api
```

A API fica disponível em `http://localhost:5298`. O arquivo `OrderGenerator.Api.http` contém requisições de exemplo (ordem válida, ordem inválida e rejeição por limite) que podem ser executadas pelo Visual Studio, VS Code ou Rider.

O Initiator tenta reconectar a cada 5 segundos, portanto as aplicações podem ser iniciadas em qualquer ordem.

### Sessão FIX

| Parâmetro | OrderAccumulator (Acceptor) | OrderGenerator (Initiator) |
|---|---|---|
| BeginString | `FIX.4.4` | `FIX.4.4` |
| SenderCompID | `ACCUMULATOR` | `GENERATOR` |
| TargetCompID | `GENERATOR` | `ACCUMULATOR` |
| Porta | `5001` (escuta) | `5001` (conecta) |

Os diretórios `store/` e `log/` são gerados pelo QuickFIX/n em tempo de execução e não são versionados.
