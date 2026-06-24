-- ============================================================
--  PARTIU DESTINO — BANCO DE DADOS COMPLETO
--  Organizado por: Autores do TCC
--  Última atualização: junho de 2026

-- ============================================================
-- 1. CRIAÇÃO DO BANCO DE DADOS
-- ============================================================
CREATE DATABASE bdpartiudestino;
USE bdpartiudestino;

-- ============================================================
-- 2. CRIAÇÃO DAS TABELAS
-- ============================================================
-- ------------------------------------------------------------
-- 2.1 Tabela: usuarios
-- ------------------------------------------------------------
CREATE TABLE usuarios (
    id    INT          PRIMARY KEY AUTO_INCREMENT,
    nome  VARCHAR(255) NOT NULL,
    email VARCHAR(255) NOT NULL UNIQUE,
    senha VARCHAR(255) NOT NULL,
    tipo  VARCHAR(20)  NOT NULL DEFAULT 'usuario'   -- 'usuario' | 'admin'
);

-- 2.2 Tabela: destinos
--     preco_por_pessoa: preço base de referência do destino

CREATE TABLE destinos (
    id               INT            PRIMARY KEY AUTO_INCREMENT,
    origem_pais      VARCHAR(100)   NOT NULL,
    origem_estado    VARCHAR(100)   NOT NULL,
    pais             VARCHAR(100)   NOT NULL,
    estado           VARCHAR(100)   NOT NULL,
    imagem_url       VARCHAR(500),
    preco_por_pessoa DECIMAL(10,2)  NOT NULL DEFAULT 0.00
);

-- 2.3 Tabela: pacotes
--     imagem_url: imagem própria de cada pacote

CREATE TABLE pacotes (
    id                INT            PRIMARY KEY AUTO_INCREMENT,
    destino_id        INT            NOT NULL,
    nome              VARCHAR(200)   NOT NULL,
    descricao         TEXT,
    tipo_viagem       VARCHAR(50),
    duracao_dias      INT,
    data_partida      DATE,
    data_retorno      DATE,
    preco_por_pessoa  DECIMAL(10,2)  NOT NULL,
    vagas_disponiveis INT,
    imagem_url        VARCHAR(500)
);


-- ------------------------------------------------------------
-- 2.4 Tabela: viagem_personalizada
-- ------------------------------------------------------------
CREATE TABLE viagem_personalizada (
    id                INT          PRIMARY KEY AUTO_INCREMENT,
    usuario_id        INT          NOT NULL,
    nome_completo     VARCHAR(150),
    cpf               VARCHAR(14),
    email             VARCHAR(150),
    whatsapp          VARCHAR(20),
    destino           VARCHAR(100),
    hospedagem        VARCHAR(100),
    data_partida      DATE,
    duracao_dias      INT,
    clima_viagem      VARCHAR(100),
    orcamento         VARCHAR(100),
    adultos           INT,
    criancas          INT,
    desejos_especiais TEXT,
    data_criacao      TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (usuario_id) REFERENCES usuarios(id) ON DELETE CASCADE
);

-- ------------------------------------------------------------
-- 2.5 Tabela: carrinho
-- ------------------------------------------------------------
CREATE TABLE carrinho (
    id              INT           PRIMARY KEY AUTO_INCREMENT,
    usuario_id      INT           NOT NULL,
    tipo_item       VARCHAR(30)   NOT NULL,    -- 'pacote' | 'destino' | 'viagem_personalizada'
    item_id         INT           NOT NULL,
    nome_item       VARCHAR(255)  NOT NULL,
    preco_unitario  DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    quantidade      INT           NOT NULL DEFAULT 1,
    data_adicionado TIMESTAMP     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (usuario_id) REFERENCES usuarios(id) ON DELETE CASCADE
);

-- ------------------------------------------------------------
-- 2.6 Tabela: pedidos
-- ------------------------------------------------------------
CREATE TABLE pedidos (
    id INT AUTO_INCREMENT PRIMARY KEY,
    usuario_id INT NOT NULL,
    tipo_item VARCHAR(50) NOT NULL,
    item_id INT NOT NULL,
    nome_item VARCHAR(255) NOT NULL,
    preco_unitario DECIMAL(10,2) DEFAULT 0,
    quantidade INT DEFAULT 1,
    data_pedido DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- ------------------------------------------------------------
-- 2.7 Tabela: hospedagens
--     Cada pacote pode ter VÁRIAS opções de hospedagem
-- ------------------------------------------------------------
CREATE TABLE hospedagens (
    id          INT           PRIMARY KEY AUTO_INCREMENT,
    pacote_id   INT           NOT NULL,
    nome        VARCHAR(150)  NOT NULL,
    categoria   VARCHAR(50),      -- ex: '3 estrelas', 'Resort', 'Pousada'
    descricao   TEXT,
    endereco    VARCHAR(255),
    imagem_url  VARCHAR(500),
    FOREIGN KEY (pacote_id) REFERENCES pacotes(id) ON DELETE CASCADE
);


-- =====================================================
-- CAMPOS EXTRAS PARA HOSPEDAGENS
-- =====================================================
ALTER TABLE hospedagens
    ADD COLUMN checkin VARCHAR(10) NULL DEFAULT '14:00',
    ADD COLUMN checkout VARCHAR(10) NULL DEFAULT '12:00',
    ADD COLUMN cafe_incluso TINYINT(1) NOT NULL DEFAULT 1,
    ADD COLUMN wifi_incluso TINYINT(1) NOT NULL DEFAULT 1,
    ADD COLUMN estacionamento TINYINT(1) NOT NULL DEFAULT 0,
    ADD COLUMN politica_cancelamento TEXT NULL,
    ADD COLUMN regras_hospedagem TEXT NULL,
    ADD COLUMN avaliacao DECIMAL(3,1) NULL DEFAULT 8.5,
    ADD COLUMN comodidades TEXT NULL;
    
    
USE bdpartiudestino;

SET SQL_SAFE_UPDATES = 0;

-- =====================================================
-- ATUALIZAÇÃO DAS HOSPEDAGENS COM AS NOVAS COLUNAS
-- =====================================================

UPDATE hospedagens
SET
    checkin = '14:00',
    checkout = '12:00',
    cafe_incluso = 1,
    wifi_incluso = 1,
    estacionamento = 0,
    politica_cancelamento = 'Cancelamento e alterações seguem as políticas da Partiu Destino e dos fornecedores envolvidos. Valores pagos podem estar sujeitos a taxas administrativas e regras específicas do pacote.',
    regras_hospedagem = 'Documento obrigatório no check-in. Menores de idade devem estar acompanhados por responsável legal. Horários de entrada e saída devem ser respeitados conforme política da hospedagem.',
    avaliacao = 8.7,
    comodidades = 'Wi-Fi, Café da manhã, Recepção 24h, Ar-condicionado, Restaurante, Serviço de quarto'
WHERE nome = 'Hotel Copacabana Mar';


UPDATE hospedagens
SET
    checkin = '15:00',
    checkout = '12:00',
    cafe_incluso = 1,
    wifi_incluso = 1,
    estacionamento = 1,
    politica_cancelamento = 'Cancelamentos seguem as regras do pacote e do resort. Alterações de data ou quarto estão sujeitas à disponibilidade e podem gerar custos adicionais.',
    regras_hospedagem = 'Documento obrigatório no check-in. Pulseiras de identificação podem ser exigidas durante a estadia. Menores devem estar acompanhados por responsável legal.',
    avaliacao = 9.1,
    comodidades = 'Wi-Fi, Café da manhã, Piscina, Restaurante, Área de lazer, Bar, Recepção 24h, Estacionamento'
WHERE nome = 'Resort Bahia Sol';


UPDATE hospedagens
SET
    checkin = '15:00',
    checkout = '11:00',
    cafe_incluso = 1,
    wifi_incluso = 1,
    estacionamento = 1,
    politica_cancelamento = 'Cancelamentos e alterações devem seguir as regras da Partiu Destino e dos fornecedores internacionais. Alterações podem sofrer variação cambial e taxas.',
    regras_hospedagem = 'Documento e passaporte podem ser solicitados no check-in. O hóspede deve respeitar as normas locais e horários definidos pela hospedagem.',
    avaliacao = 8.8,
    comodidades = 'Wi-Fi, Café da manhã, Academia, Estacionamento, Recepção 24h, Ar-condicionado, Restaurante'
WHERE nome = 'California Dream Hotel';


UPDATE hospedagens
SET
    checkin = '15:00',
    checkout = '11:00',
    cafe_incluso = 0,
    wifi_incluso = 1,
    estacionamento = 0,
    politica_cancelamento = 'Cancelamentos seguem as políticas do pacote contratado. Alterações de quarto, datas ou quantidade de hóspedes podem gerar custos adicionais.',
    regras_hospedagem = 'Documento obrigatório no check-in. Taxas locais podem ser cobradas pela hospedagem. Respeitar horários de entrada e saída.',
    avaliacao = 8.6,
    comodidades = 'Wi-Fi, Recepção 24h, Ar-condicionado, Elevador, Serviço de quarto, Localização central'
WHERE nome = 'Manhattan City Hotel';


UPDATE hospedagens
SET
    checkin = '14:00',
    checkout = '12:00',
    cafe_incluso = 1,
    wifi_incluso = 1,
    estacionamento = 0,
    politica_cancelamento = 'Cancelamentos e remarcações seguem as regras da Partiu Destino e dos fornecedores. Alterações estão sujeitas à disponibilidade.',
    regras_hospedagem = 'Documento obrigatório no check-in. Menores devem estar acompanhados por responsável legal. Taxas locais podem ser cobradas pela hospedagem.',
    avaliacao = 9.0,
    comodidades = 'Wi-Fi, Café da manhã, Recepção 24h, Ar-condicionado, Restaurante, Serviço de quarto'
WHERE nome = 'Paris Lumière Hotel';


UPDATE hospedagens
SET
    checkin = '14:00',
    checkout = '11:00',
    cafe_incluso = 1,
    wifi_incluso = 1,
    estacionamento = 1,
    politica_cancelamento = 'Cancelamentos seguem a política do pacote. Alterações de datas, quarto ou quantidade de hóspedes estão sujeitas à disponibilidade.',
    regras_hospedagem = 'Documento obrigatório no check-in. Respeitar horários da hospedagem e normas locais. Menores devem estar acompanhados por responsável legal.',
    avaliacao = 8.9,
    comodidades = 'Wi-Fi, Café da manhã, Estacionamento, Restaurante, Jardim, Ar-condicionado, Recepção'
WHERE nome = 'Villa Toscana Hotel';


UPDATE hospedagens
SET
    checkin = '15:00',
    checkout = '11:00',
    cafe_incluso = 1,
    wifi_incluso = 1,
    estacionamento = 0,
    politica_cancelamento = 'Cancelamentos e alterações seguem as regras da Partiu Destino e fornecedores internacionais. Alterações podem gerar taxas adicionais.',
    regras_hospedagem = 'Documento e passaporte podem ser solicitados. Respeitar normas locais, horários de entrada e saída e orientações da hospedagem.',
    avaliacao = 8.8,
    comodidades = 'Wi-Fi, Café da manhã, Recepção 24h, Ar-condicionado, Restaurante, Elevador'
WHERE nome = 'Tokyo Central Hotel';


UPDATE hospedagens
SET
    checkin = '14:00',
    checkout = '12:00',
    cafe_incluso = 1,
    wifi_incluso = 1,
    estacionamento = 1,
    politica_cancelamento = 'Cancelamentos e alterações seguem as regras do pacote e do resort. Serviços extras podem ter cobrança separada.',
    regras_hospedagem = 'Documento obrigatório no check-in. Menores devem estar acompanhados. Uso de áreas comuns sujeito às regras da hospedagem.',
    avaliacao = 9.3,
    comodidades = 'Wi-Fi, Café da manhã, Piscina, Spa, Restaurante, Estacionamento, Recepção 24h, Área de lazer'
WHERE nome = 'Bali Paradise Resort';


UPDATE hospedagens
SET
    checkin = '15:00',
    checkout = '12:00',
    cafe_incluso = 1,
    wifi_incluso = 1,
    estacionamento = 1,
    politica_cancelamento = 'Cancelamentos e alterações seguem as políticas da Partiu Destino e fornecedores. Serviços de luxo e extras podem possuir regras próprias.',
    regras_hospedagem = 'Documento ou passaporte obrigatório no check-in. Respeitar normas locais, horários e regras da hospedagem. Taxas locais podem ser aplicadas.',
    avaliacao = 9.2,
    comodidades = 'Wi-Fi, Café da manhã, Piscina, Academia, Restaurante, Estacionamento, Recepção 24h, Serviço de quarto'
WHERE nome = 'Dubai Skyline Hotel';    



-- ------------------------------------------------------------
-- 2.8 Tabela: quartos
--     Cada hospedagem pode ter VÁRIOS tipos de quarto
--     preco_adicional é somado ao preco_por_pessoa do pacote
-- ------------------------------------------------------------
CREATE TABLE quartos (
    id                    INT           PRIMARY KEY AUTO_INCREMENT,
    hospedagem_id         INT           NOT NULL,
    tipo_quarto           VARCHAR(100)  NOT NULL,   -- ex: Standard, Luxo, Suíte
    capacidade_adultos    INT           NOT NULL DEFAULT 2,
    capacidade_criancas   INT           NOT NULL DEFAULT 0,
    preco_adicional       DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    quantidade_disponivel INT           NOT NULL DEFAULT 1,
    comodidades           VARCHAR(255),             -- ex: 'Wi-Fi, Ar-condicionado, Vista mar'
    imagem_url            VARCHAR(500),
    FOREIGN KEY (hospedagem_id) REFERENCES hospedagens(id) ON DELETE CASCADE
);

-- =====================================================
-- CAMPOS EXTRAS PARA QUARTOS
-- =====================================================

ALTER TABLE quartos
    ADD COLUMN numero_camas INT NULL DEFAULT 1,
    ADD COLUMN tipo_camas VARCHAR(150) NULL,
    ADD COLUMN cafe_incluso TINYINT(1) NOT NULL DEFAULT 1,
    ADD COLUMN area_m2 DECIMAL(5,2) NULL,
    ADD COLUMN descricao TEXT NULL,
    ADD COLUMN politica_cancelamento TEXT NULL;
    
    
-- =====================================================
-- ATUALIZAÇÃO DOS QUARTOS COM AS NOVAS COLUNAS
-- =====================================================

-- =========================
-- RIO - Hotel Copacabana Mar
-- =========================

UPDATE quartos q
INNER JOIN hospedagens h ON h.id = q.hospedagem_id
SET
    q.numero_camas = 1,
    q.tipo_camas = '1 cama de casal',
    q.cafe_incluso = 1,
    q.area_m2 = 22.00,
    q.descricao = 'Quarto confortável para casal ou pequena família, com ar-condicionado, banheiro privativo, Wi-Fi e boa estrutura para estadias curtas.',
    q.politica_cancelamento = 'Alterações e cancelamentos seguem as regras do pacote e da hospedagem.'
WHERE h.nome = 'Hotel Copacabana Mar'
  AND q.tipo_quarto = 'Quarto Standard';

UPDATE quartos q
INNER JOIN hospedagens h ON h.id = q.hospedagem_id
SET
    q.numero_camas = 1,
    q.tipo_camas = '1 cama queen size',
    q.cafe_incluso = 1,
    q.area_m2 = 28.00,
    q.descricao = 'Quarto mais espaçoso, indicado para quem busca mais conforto, com cama queen, ar-condicionado, Wi-Fi e frigobar.',
    q.politica_cancelamento = 'Alterações e cancelamentos seguem as regras do pacote e da hospedagem.'
WHERE h.nome = 'Hotel Copacabana Mar'
  AND q.tipo_quarto = 'Quarto Luxo';

UPDATE quartos q
INNER JOIN hospedagens h ON h.id = q.hospedagem_id
SET
    q.numero_camas = 1,
    q.tipo_camas = '1 cama king size',
    q.cafe_incluso = 1,
    q.area_m2 = 34.00,
    q.descricao = 'Suíte com vista para o mar, cama king size, ambiente amplo, frigobar, Wi-Fi e estrutura ideal para uma experiência mais confortável.',
    q.politica_cancelamento = 'Alterações e cancelamentos seguem as regras do pacote e da hospedagem.'
WHERE h.nome = 'Hotel Copacabana Mar'
  AND q.tipo_quarto = 'Suíte Vista Mar';

UPDATE quartos q
INNER JOIN hospedagens h ON h.id = q.hospedagem_id
SET
    q.numero_camas = 3,
    q.tipo_camas = '1 cama de casal e 2 camas de solteiro',
    q.cafe_incluso = 1,
    q.area_m2 = 38.00,
    q.descricao = 'Quarto amplo para famílias, com camas múltiplas, ar-condicionado, Wi-Fi, banheiro privativo e espaço confortável para adultos e crianças.',
    q.politica_cancelamento = 'Alterações e cancelamentos seguem as regras do pacote e da hospedagem.'
WHERE h.nome = 'Hotel Copacabana Mar'
  AND q.tipo_quarto = 'Quarto Família';


-- =========================
-- BAHIA - Resort Bahia Sol
-- =========================

UPDATE quartos q
INNER JOIN hospedagens h ON h.id = q.hospedagem_id
SET
    q.numero_camas = 1,
    q.tipo_camas = '1 cama de casal',
    q.cafe_incluso = 1,
    q.area_m2 = 24.00,
    q.descricao = 'Quarto confortável em resort, com café da manhã, Wi-Fi, ar-condicionado e acesso às áreas comuns da hospedagem.',
    q.politica_cancelamento = 'Alterações e cancelamentos seguem as regras do resort e do pacote contratado.'
WHERE h.nome = 'Resort Bahia Sol'
  AND q.tipo_quarto = 'Quarto Standard';

UPDATE quartos q
INNER JOIN hospedagens h ON h.id = q.hospedagem_id
SET
    q.numero_camas = 2,
    q.tipo_camas = '1 cama de casal e 1 cama de solteiro',
    q.cafe_incluso = 1,
    q.area_m2 = 30.00,
    q.descricao = 'Quarto superior com mais espaço, indicado para família pequena, com ar-condicionado, Wi-Fi, frigobar e café da manhã incluso.',
    q.politica_cancelamento = 'Alterações e cancelamentos seguem as regras do resort e do pacote contratado.'
WHERE h.nome = 'Resort Bahia Sol'
  AND q.tipo_quarto = 'Quarto Superior';

UPDATE quartos q
INNER JOIN hospedagens h ON h.id = q.hospedagem_id
SET
    q.numero_camas = 2,
    q.tipo_camas = '1 cama queen size e 1 cama auxiliar',
    q.cafe_incluso = 1,
    q.area_m2 = 34.00,
    q.descricao = 'Quarto luxo com melhor localização dentro do resort, cama queen, cama auxiliar, Wi-Fi, frigobar e café incluso.',
    q.politica_cancelamento = 'Alterações e cancelamentos seguem as regras do resort e do pacote contratado.'
WHERE h.nome = 'Resort Bahia Sol'
  AND q.tipo_quarto = 'Quarto Luxo';

UPDATE quartos q
INNER JOIN hospedagens h ON h.id = q.hospedagem_id
SET
    q.numero_camas = 3,
    q.tipo_camas = '2 camas de casal e 1 cama de solteiro',
    q.cafe_incluso = 1,
    q.area_m2 = 42.00,
    q.descricao = 'Suíte família espaçosa, ideal para grupos com adultos e crianças, com camas múltiplas, Wi-Fi, ar-condicionado e café incluso.',
    q.politica_cancelamento = 'Alterações e cancelamentos seguem as regras do resort e do pacote contratado.'
WHERE h.nome = 'Resort Bahia Sol'
  AND q.tipo_quarto = 'Suíte Família';


-- =========================
-- CALIFÓRNIA - California Dream Hotel
-- =========================

UPDATE quartos q
INNER JOIN hospedagens h ON h.id = q.hospedagem_id
SET
    q.numero_camas = 1,
    q.tipo_camas = '1 cama de casal',
    q.cafe_incluso = 1,
    q.area_m2 = 23.00,
    q.descricao = 'Quarto funcional e confortável para viagem internacional, com Wi-Fi, ar-condicionado e banheiro privativo.',
    q.politica_cancelamento = 'Alterações podem gerar custos adicionais conforme regras internacionais do fornecedor.'
WHERE h.nome = 'California Dream Hotel'
  AND q.tipo_quarto = 'Quarto Standard';

UPDATE quartos q
INNER JOIN hospedagens h ON h.id = q.hospedagem_id
SET
    q.numero_camas = 1,
    q.tipo_camas = '1 cama queen size',
    q.cafe_incluso = 1,
    q.area_m2 = 29.00,
    q.descricao = 'Quarto deluxe com cama queen, ambiente moderno, Wi-Fi, frigobar e estrutura confortável para estadia em Los Angeles.',
    q.politica_cancelamento = 'Alterações podem gerar custos adicionais conforme regras internacionais do fornecedor.'
WHERE h.nome = 'California Dream Hotel'
  AND q.tipo_quarto = 'Quarto Deluxe';

UPDATE quartos q
INNER JOIN hospedagens h ON h.id = q.hospedagem_id
SET
    q.numero_camas = 1,
    q.tipo_camas = '1 cama king size',
    q.cafe_incluso = 1,
    q.area_m2 = 36.00,
    q.descricao = 'Quarto premium com cama king size, maior conforto, Wi-Fi, ar-condicionado, frigobar e melhor localização na hospedagem.',
    q.politica_cancelamento = 'Alterações podem gerar custos adicionais conforme regras internacionais do fornecedor.'
WHERE h.nome = 'California Dream Hotel'
  AND q.tipo_quarto = 'Quarto Premium';

UPDATE quartos q
INNER JOIN hospedagens h ON h.id = q.hospedagem_id
SET
    q.numero_camas = 3,
    q.tipo_camas = '1 cama de casal e 2 camas de solteiro',
    q.cafe_incluso = 1,
    q.area_m2 = 40.00,
    q.descricao = 'Quarto família com boa capacidade, ideal para grupos, com camas múltiplas, Wi-Fi, ar-condicionado e banheiro privativo.',
    q.politica_cancelamento = 'Alterações podem gerar custos adicionais conforme regras internacionais do fornecedor.'
WHERE h.nome = 'California Dream Hotel'
  AND q.tipo_quarto = 'Quarto Família';


-- =========================
-- NOVA YORK - Manhattan City Hotel
-- =========================

UPDATE quartos q
INNER JOIN hospedagens h ON h.id = q.hospedagem_id
SET
    q.numero_camas = 1,
    q.tipo_camas = '1 cama de casal',
    q.cafe_incluso = 0,
    q.area_m2 = 18.00,
    q.descricao = 'Quarto compacto e funcional em Manhattan, ideal para quem busca localização central e praticidade durante a viagem.',
    q.politica_cancelamento = 'Cancelamentos seguem as regras do hotel e do pacote contratado.'
WHERE h.nome = 'Manhattan City Hotel'
  AND q.tipo_quarto = 'Quarto Standard';

UPDATE quartos q
INNER JOIN hospedagens h ON h.id = q.hospedagem_id
SET
    q.numero_camas = 1,
    q.tipo_camas = '1 cama queen size',
    q.cafe_incluso = 0,
    q.area_m2 = 23.00,
    q.descricao = 'Quarto superior com melhor espaço interno, cama queen, Wi-Fi, ar-condicionado e boa localização na hospedagem.',
    q.politica_cancelamento = 'Cancelamentos seguem as regras do hotel e do pacote contratado.'
WHERE h.nome = 'Manhattan City Hotel'
  AND q.tipo_quarto = 'Quarto Superior';

UPDATE quartos q
INNER JOIN hospedagens h ON h.id = q.hospedagem_id
SET
    q.numero_camas = 2,
    q.tipo_camas = '1 cama queen size e 1 cama auxiliar',
    q.cafe_incluso = 0,
    q.area_m2 = 28.00,
    q.descricao = 'Quarto deluxe com mais conforto, Wi-Fi, ar-condicionado, frigobar e estrutura adequada para estadia em Nova York.',
    q.politica_cancelamento = 'Cancelamentos seguem as regras do hotel e do pacote contratado.'
WHERE h.nome = 'Manhattan City Hotel'
  AND q.tipo_quarto = 'Quarto Deluxe';

UPDATE quartos q
INNER JOIN hospedagens h ON h.id = q.hospedagem_id
SET
    q.numero_camas = 2,
    q.tipo_camas = '1 cama king size e 1 sofá-cama',
    q.cafe_incluso = 0,
    q.area_m2 = 35.00,
    q.descricao = 'Suíte executiva com cama king, sofá-cama, Wi-Fi, ambiente amplo e estrutura ideal para maior conforto na viagem.',
    q.politica_cancelamento = 'Cancelamentos seguem as regras do hotel e do pacote contratado.'
WHERE h.nome = 'Manhattan City Hotel'
  AND q.tipo_quarto = 'Suíte Executiva';    
  
-- =========================
-- PARIS - Paris Lumière Hotel
-- =========================

UPDATE quartos q
INNER JOIN hospedagens h ON h.id = q.hospedagem_id
SET
    q.numero_camas = 1,
    q.tipo_camas = '1 cama de casal',
    q.cafe_incluso = 1,
    q.area_m2 = 21.00,
    q.descricao = 'Quarto casal confortável, com decoração elegante, Wi-Fi, ar-condicionado e café da manhã incluso.',
    q.politica_cancelamento = 'Alterações e cancelamentos seguem as regras da hospedagem e do pacote.'
WHERE h.nome = 'Paris Lumière Hotel'
  AND q.tipo_quarto = 'Quarto Casal Standard';

UPDATE quartos q
INNER JOIN hospedagens h ON h.id = q.hospedagem_id
SET
    q.numero_camas = 1,
    q.tipo_camas = '1 cama queen size',
    q.cafe_incluso = 1,
    q.area_m2 = 27.00,
    q.descricao = 'Quarto superior com cama queen, ambiente confortável, Wi-Fi, frigobar e café incluso.',
    q.politica_cancelamento = 'Alterações e cancelamentos seguem as regras da hospedagem e do pacote.'
WHERE h.nome = 'Paris Lumière Hotel'
  AND q.tipo_quarto = 'Quarto Superior';

UPDATE quartos q
INNER JOIN hospedagens h ON h.id = q.hospedagem_id
SET
    q.numero_camas = 1,
    q.tipo_camas = '1 cama queen size',
    q.cafe_incluso = 1,
    q.area_m2 = 31.00,
    q.descricao = 'Quarto luxo com varanda, ideal para casal, com Wi-Fi, ar-condicionado, frigobar e ambiente mais reservado.',
    q.politica_cancelamento = 'Alterações e cancelamentos seguem as regras da hospedagem e do pacote.'
WHERE h.nome = 'Paris Lumière Hotel'
  AND q.tipo_quarto = 'Quarto Luxo com Varanda';

UPDATE quartos q
INNER JOIN hospedagens h ON h.id = q.hospedagem_id
SET
    q.numero_camas = 1,
    q.tipo_camas = '1 cama king size',
    q.cafe_incluso = 1,
    q.area_m2 = 38.00,
    q.descricao = 'Suíte romântica com cama king size, ambiente elegante, Wi-Fi, café incluso e estrutura ideal para viagem a dois.',
    q.politica_cancelamento = 'Alterações e cancelamentos seguem as regras da hospedagem e do pacote.'
WHERE h.nome = 'Paris Lumière Hotel'
  AND q.tipo_quarto = 'Suíte Romântica';


-- =========================
-- TOSCANA - Villa Toscana Hotel
-- =========================

UPDATE quartos q
INNER JOIN hospedagens h ON h.id = q.hospedagem_id
SET
    q.numero_camas = 1,
    q.tipo_camas = '1 cama de casal',
    q.cafe_incluso = 1,
    q.area_m2 = 23.00,
    q.descricao = 'Quarto aconchegante, com decoração regional, Wi-Fi, café da manhã e estrutura confortável para estadia na Toscana.',
    q.politica_cancelamento = 'Alterações e cancelamentos seguem as regras do pacote e da hospedagem.'
WHERE h.nome = 'Villa Toscana Hotel'
  AND q.tipo_quarto = 'Quarto Standard';

UPDATE quartos q
INNER JOIN hospedagens h ON h.id = q.hospedagem_id
SET
    q.numero_camas = 1,
    q.tipo_camas = '1 cama queen size',
    q.cafe_incluso = 1,
    q.area_m2 = 29.00,
    q.descricao = 'Quarto superior com cama queen, mais espaço, Wi-Fi, café incluso e vista agradável da região.',
    q.politica_cancelamento = 'Alterações e cancelamentos seguem as regras do pacote e da hospedagem.'
WHERE h.nome = 'Villa Toscana Hotel'
  AND q.tipo_quarto = 'Quarto Superior';

UPDATE quartos q
INNER JOIN hospedagens h ON h.id = q.hospedagem_id
SET
    q.numero_camas = 1,
    q.tipo_camas = '1 cama king size',
    q.cafe_incluso = 1,
    q.area_m2 = 35.00,
    q.descricao = 'Quarto luxo com cama king, ambiente amplo, Wi-Fi, café incluso, frigobar e maior conforto para a estadia.',
    q.politica_cancelamento = 'Alterações e cancelamentos seguem as regras do pacote e da hospedagem.'
WHERE h.nome = 'Villa Toscana Hotel'
  AND q.tipo_quarto = 'Quarto Luxo';

UPDATE quartos q
INNER JOIN hospedagens h ON h.id = q.hospedagem_id
SET
    q.numero_camas = 3,
    q.tipo_camas = '1 cama de casal e 2 camas de solteiro',
    q.cafe_incluso = 1,
    q.area_m2 = 41.00,
    q.descricao = 'Suíte família espaçosa, ideal para adultos e crianças, com camas múltiplas, Wi-Fi, café incluso e banheiro privativo.',
    q.politica_cancelamento = 'Alterações e cancelamentos seguem as regras do pacote e da hospedagem.'
WHERE h.nome = 'Villa Toscana Hotel'
  AND q.tipo_quarto = 'Suíte Família';


-- =========================
-- TÓQUIO - Tokyo Central Hotel
-- =========================

UPDATE quartos q
INNER JOIN hospedagens h ON h.id = q.hospedagem_id
SET
    q.numero_camas = 1,
    q.tipo_camas = '1 cama de casal',
    q.cafe_incluso = 1,
    q.area_m2 = 19.00,
    q.descricao = 'Quarto funcional em Tóquio, com Wi-Fi, ar-condicionado, banheiro privativo e café da manhã incluso.',
    q.politica_cancelamento = 'Alterações seguem as regras internacionais da hospedagem e do pacote contratado.'
WHERE h.nome = 'Tokyo Central Hotel'
  AND q.tipo_quarto = 'Quarto Standard';

UPDATE quartos q
INNER JOIN hospedagens h ON h.id = q.hospedagem_id
SET
    q.numero_camas = 1,
    q.tipo_camas = '1 cama queen size',
    q.cafe_incluso = 1,
    q.area_m2 = 24.00,
    q.descricao = 'Quarto superior com cama queen, Wi-Fi, café incluso e estrutura confortável para estadia em Tóquio.',
    q.politica_cancelamento = 'Alterações seguem as regras internacionais da hospedagem e do pacote contratado.'
WHERE h.nome = 'Tokyo Central Hotel'
  AND q.tipo_quarto = 'Quarto Superior';

UPDATE quartos q
INNER JOIN hospedagens h ON h.id = q.hospedagem_id
SET
    q.numero_camas = 2,
    q.tipo_camas = '1 cama queen size e 1 cama auxiliar',
    q.cafe_incluso = 1,
    q.area_m2 = 30.00,
    q.descricao = 'Quarto deluxe com espaço adicional, Wi-Fi, ar-condicionado, frigobar e café incluso.',
    q.politica_cancelamento = 'Alterações seguem as regras internacionais da hospedagem e do pacote contratado.'
WHERE h.nome = 'Tokyo Central Hotel'
  AND q.tipo_quarto = 'Quarto Deluxe';

UPDATE quartos q
INNER JOIN hospedagens h ON h.id = q.hospedagem_id
SET
    q.numero_camas = 3,
    q.tipo_camas = '1 cama de casal e 2 camas de solteiro',
    q.cafe_incluso = 1,
    q.area_m2 = 36.00,
    q.descricao = 'Suíte família com camas múltiplas, indicada para grupos, com Wi-Fi, café incluso e banheiro privativo.',
    q.politica_cancelamento = 'Alterações seguem as regras internacionais da hospedagem e do pacote contratado.'
WHERE h.nome = 'Tokyo Central Hotel'
  AND q.tipo_quarto = 'Suíte Família';


-- =========================
-- BALI - Bali Paradise Resort
-- =========================

UPDATE quartos q
INNER JOIN hospedagens h ON h.id = q.hospedagem_id
SET
    q.numero_camas = 1,
    q.tipo_camas = '1 cama de casal',
    q.cafe_incluso = 1,
    q.area_m2 = 25.00,
    q.descricao = 'Quarto confortável em resort, com Wi-Fi, café incluso, ar-condicionado e acesso às áreas comuns da hospedagem.',
    q.politica_cancelamento = 'Cancelamentos seguem as regras do resort e do pacote contratado.'
WHERE h.nome = 'Bali Paradise Resort'
  AND q.tipo_quarto = 'Quarto Standard';

UPDATE quartos q
INNER JOIN hospedagens h ON h.id = q.hospedagem_id
SET
    q.numero_camas = 1,
    q.tipo_camas = '1 cama queen size',
    q.cafe_incluso = 1,
    q.area_m2 = 32.00,
    q.descricao = 'Quarto deluxe com cama queen, ambiente amplo, Wi-Fi, café incluso, frigobar e acesso à estrutura do resort.',
    q.politica_cancelamento = 'Cancelamentos seguem as regras do resort e do pacote contratado.'
WHERE h.nome = 'Bali Paradise Resort'
  AND q.tipo_quarto = 'Quarto Deluxe';

UPDATE quartos q
INNER JOIN hospedagens h ON h.id = q.hospedagem_id
SET
    q.numero_camas = 1,
    q.tipo_camas = '1 cama king size',
    q.cafe_incluso = 1,
    q.area_m2 = 48.00,
    q.descricao = 'Villa privativa com cama king, ambiente reservado, Wi-Fi, café incluso, frigobar e maior conforto para descanso.',
    q.politica_cancelamento = 'Cancelamentos seguem as regras do resort e do pacote contratado.'
WHERE h.nome = 'Bali Paradise Resort'
  AND q.tipo_quarto = 'Villa Privativa';

UPDATE quartos q
INNER JOIN hospedagens h ON h.id = q.hospedagem_id
SET
    q.numero_camas = 3,
    q.tipo_camas = '2 camas de casal e 1 cama de solteiro',
    q.cafe_incluso = 1,
    q.area_m2 = 58.00,
    q.descricao = 'Villa família espaçosa, ideal para grupos com adultos e crianças, com camas múltiplas, Wi-Fi, café incluso e área privativa.',
    q.politica_cancelamento = 'Cancelamentos seguem as regras do resort e do pacote contratado.'
WHERE h.nome = 'Bali Paradise Resort'
  AND q.tipo_quarto = 'Villa Família';


-- =========================
-- DUBAI - Dubai Skyline Hotel
-- =========================

UPDATE quartos q
INNER JOIN hospedagens h ON h.id = q.hospedagem_id
SET
    q.numero_camas = 1,
    q.tipo_camas = '1 cama queen size',
    q.cafe_incluso = 1,
    q.area_m2 = 28.00,
    q.descricao = 'Quarto standard luxo com cama queen, Wi-Fi, café incluso, ar-condicionado e estrutura de hotel de alto padrão.',
    q.politica_cancelamento = 'Cancelamentos e alterações seguem regras do hotel e fornecedores internacionais.'
WHERE h.nome = 'Dubai Skyline Hotel'
  AND q.tipo_quarto = 'Quarto Standard Luxo';

UPDATE quartos q
INNER JOIN hospedagens h ON h.id = q.hospedagem_id
SET
    q.numero_camas = 1,
    q.tipo_camas = '1 cama king size',
    q.cafe_incluso = 1,
    q.area_m2 = 36.00,
    q.descricao = 'Quarto deluxe com cama king, ambiente sofisticado, Wi-Fi, café incluso, frigobar e excelente estrutura.',
    q.politica_cancelamento = 'Cancelamentos e alterações seguem regras do hotel e fornecedores internacionais.'
WHERE h.nome = 'Dubai Skyline Hotel'
  AND q.tipo_quarto = 'Quarto Deluxe';

UPDATE quartos q
INNER JOIN hospedagens h ON h.id = q.hospedagem_id
SET
    q.numero_camas = 1,
    q.tipo_camas = '1 cama king size',
    q.cafe_incluso = 1,
    q.area_m2 = 42.00,
    q.descricao = 'Quarto premium com cama king, vista diferenciada, Wi-Fi, café incluso, frigobar e ambiente de alto conforto.',
    q.politica_cancelamento = 'Cancelamentos e alterações seguem regras do hotel e fornecedores internacionais.'
WHERE h.nome = 'Dubai Skyline Hotel'
  AND q.tipo_quarto = 'Quarto Premium';

UPDATE quartos q
INNER JOIN hospedagens h ON h.id = q.hospedagem_id
SET
    q.numero_camas = 3,
    q.tipo_camas = '2 camas de casal e 1 sofá-cama',
    q.cafe_incluso = 1,
    q.area_m2 = 60.00,
    q.descricao = 'Suíte família luxo, ampla e sofisticada, com camas múltiplas, Wi-Fi, café incluso e estrutura ideal para famílias.',
    q.politica_cancelamento = 'Cancelamentos e alterações seguem regras do hotel e fornecedores internacionais.'
WHERE h.nome = 'Dubai Skyline Hotel'
  AND q.tipo_quarto = 'Suíte Família Luxo';

SET SQL_SAFE_UPDATES = 1;  

CREATE INDEX idx_hospedagens_pacote ON hospedagens(pacote_id);
CREATE INDEX idx_quartos_hospedagem ON quartos(hospedagem_id);
-- ============================================================
-- 3. INSERÇÃO DE DADOS
-- ============================================================

-- ------------------------------------------------------------
-- 3.1 Usuário administrador padrão
-- ------------------------------------------------------------
INSERT INTO usuarios (nome, email, senha, tipo) VALUES
    ('Julia Costa', 'julia@gmail.com', '$2a$11$KG8AxkIziG2A6C9aOIzWkeD82eW96KTcXrDiM2JMYZGlSmLVoU2am', 'admin');


-- ------------------------------------------------------------
-- 3.2 Destinos (com imagem_url e preco_por_pessoa)
-- ------------------------------------------------------------
INSERT INTO destinos (origem_pais, origem_estado, pais, estado, imagem_url, preco_por_pessoa) VALUES

    -- Nacionais (saindo de São Paulo)
    ('Brasil', 'São Paulo', 'Brasil', 'Rio de Janeiro',
        'https://images.unsplash.com/photo-1483729558449-99ef09a8c325?w=700&q=80',
        1200.00),

    ('Brasil', 'São Paulo', 'Brasil', 'Bahia',
        'https://images.unsplash.com/photo-1591233055842-a984961b71af?w=800&q=80',
        980.00),

    ('Brasil', 'São Paulo', 'Brasil', 'Ceará',
        'https://images.unsplash.com/photo-1538565756327-7e5b9dc67c3f?w=700&q=80',
        850.00),

    -- Internacionais (saindo de São Paulo)
    ('Brasil', 'São Paulo', 'Estados Unidos', 'Califórnia',
        'https://images.unsplash.com/photo-1501594907352-04cda38ebc29?w=700&q=80',
        8500.00),

    ('Brasil', 'São Paulo', 'Estados Unidos', 'Flórida',
        'https://images.unsplash.com/photo-1533106418989-88406c7cc8ca?w=700&q=80',
        7900.00),

    ('Brasil', 'São Paulo', 'França', 'Provença-Alpes-Costa Azul',
        'https://images.unsplash.com/photo-1502602898657-3e91760cbb34?w=700&q=80',
        11500.00),

    ('Brasil', 'São Paulo', 'Itália', 'Toscana',
        'https://images.unsplash.com/photo-1543429776-2782fc8e3e56?w=700&q=80',
        10800.00),

    ('Brasil', 'São Paulo', 'Japão', 'Tóquio',
        'https://images.unsplash.com/photo-1540959733332-eab4deabeeaf?w=700&q=80',
        13200.00),

    -- Internacionais (saindo do Rio de Janeiro)
    ('Brasil', 'Rio de Janeiro', 'Portugal', 'Lisboa',
        'https://images.unsplash.com/photo-1555881400-74d7acaacd8b?w=700&q=80',
        9400.00),

    ('Brasil', 'Rio de Janeiro', 'Argentina', 'Buenos Aires',
        'https://images.unsplash.com/photo-1583285233058-4a9e6a5e34d8?w=700&q=80',
        4200.00),

    -- Internacionais (saindo de Minas Gerais / Paraná)
    ('Brasil', 'Minas Gerais', 'Chile', 'Região Metropolitana de Santiago',
        'https://images.unsplash.com/photo-1554254648-2d58a1bc3fd5?w=700&q=80',
        5600.00),

    ('Brasil', 'Paraná', 'México', 'Quintana Roo',
        'https://images.unsplash.com/photo-1552074284-5e88ef1aef18?w=700&q=80',
        6300.00);

-- ------------------------------------------------------------
-- 3.3 Pacotes (com imagem_url própria de cada pacote)
-- ------------------------------------------------------------
INSERT INTO pacotes
    (destino_id, nome, descricao, tipo_viagem, duracao_dias,
     data_partida, data_retorno, preco_por_pessoa, vagas_disponiveis, imagem_url)
VALUES

    (1, 'Rio Premium Experience',
        'Pacote completo para conhecer as praias e pontos turísticos do Rio de Janeiro.',
        'Praia', 7, '2026-07-10', '2026-07-17', 4599.90, 20,
        'https://images.unsplash.com/photo-1483729558449-99ef09a8c325?w=700&q=80'),

    (2, 'Bahia All Inclusive',
        'Experiência incrível em resort all inclusive na Bahia.',
        'Relaxamento', 6, '2026-08-05', '2026-08-11', 3899.50, 15,
        'https://images.unsplash.com/photo-1590060879041-cfab4e95d716?w=700&q=80'),

    (4, 'Califórnia Dreams',
        'Conheça Los Angeles, praias e parques famosos da Califórnia.',
        'Internacional', 10, '2026-09-12', '2026-09-22', 12999.99, 12,
        'https://images.unsplash.com/photo-1501594907352-04cda38ebc29?w=700&q=80'),

    (4, 'Nova York Experience',
        'Pacote completo para explorar Nova York e seus principais pontos turísticos.',
        'Internacional', 8, '2026-11-03', '2026-11-11', 14500.00, 10,
        'https://images.unsplash.com/photo-1490644658840-3f2e3f8c5625?w=700&q=80'),

    (6, 'Paris Romântica',
        'Uma viagem inesquecível para casais na cidade luz.',
        'Romance', 7, '2026-06-15', '2026-06-22', 16990.90, 8,
        'https://images.unsplash.com/photo-1502602898657-3e91760cbb34?w=700&q=80'),

    (7, 'Toscana Gourmet',
        'Experiência gastronômica e cultural na Toscana.',
        'Gastronomia', 9, '2026-10-02', '2026-10-11', 15200.00, 10,
        'https://images.unsplash.com/photo-1543429776-2782fc8e3e56?w=700&q=80'),


    (8, 'Tóquio Tech Tour',
        'Conheça o Japão moderno e tradicional em uma experiência única.',
        'Cultura', 12, '2026-09-05', '2026-09-17', 18990.00, 9,
        'https://images.unsplash.com/photo-1540959733332-eab4deabeeaf?w=700&q=80'),

    (12, 'Bali Paradise',
        'Pacote de luxo em Bali com hospedagem premium.',
        'Relaxamento', 10, '2026-12-01', '2026-12-11', 17499.99, 6,
        'https://images.unsplash.com/photo-1537996194471-e657df975ab4?w=700&q=80'),

    (12, 'Dubai Lux Experience',
        'Explore o luxo e modernidade de Dubai.',
        'Luxo', 7, '2026-08-18', '2026-08-25', 19990.00, 5,
        'https://images.unsplash.com/photo-1512453979798-5ea266f8880c?w=700&q=80');

-- Hospedagens + Quartos de exemplo para o pacote "Rio Premium Experience" (id 1)
INSERT INTO hospedagens (pacote_id, nome, categoria, descricao, endereco, imagem_url) VALUES
(1, 'Copacabana Palace Inn', '4 estrelas', 'Hotel a 200m da praia de Copacabana, café da manhã incluso.', 'Av. Atlântica, 1500 - Copacabana, RJ',
 'https://images.unsplash.com/photo-1566073771259-6a8506099945?w=700&q=80'),
(1, 'Pousada Vista Mar RJ', 'Pousada', 'Pousada charmosa e econômica, a 5 minutos a pé da praia.', 'Rua Barata Ribeiro, 300 - Copacabana, RJ',
 'https://images.unsplash.com/photo-1520250497591-112f2f40a3f4?w=700&q=80');

INSERT INTO quartos (hospedagem_id, tipo_quarto, capacidade_adultos, capacidade_criancas, preco_adicional, quantidade_disponivel, comodidades) VALUES
(1, 'Standard', 2, 1, 0.00, 10, 'Wi-Fi, Ar-condicionado, TV a cabo'),
(1, 'Luxo Vista Mar', 2, 1, 450.00, 5, 'Wi-Fi, Ar-condicionado, Varanda, Vista mar'),
(1, 'Suíte Master', 4, 2, 890.00, 3, 'Wi-Fi, Ar-condicionado, Jacuzzi, Vista mar, Frigobar'),
(2, 'Quarto Simples', 2, 0, 0.00, 8, 'Wi-Fi, Ventilador'),
(2, 'Quarto Família', 4, 2, 180.00, 4, 'Wi-Fi, Ar-condicionado, 2 camas');

SET SQL_SAFE_UPDATES = 0;

-- IMAGENS DOS QUARTOS DO PACOTE RIO
UPDATE quartos SET imagem_url = 'https://images.unsplash.com/photo-1611892440504-42a792e24d32?w=600&q=80' WHERE tipo_quarto = 'Standard';
UPDATE quartos SET imagem_url = 'https://images.unsplash.com/photo-1582719478250-c89cae4dc85b?w=600&q=80' WHERE tipo_quarto = 'Luxo Vista Mar';
UPDATE quartos SET imagem_url = 'https://images.unsplash.com/photo-1582719508461-905c673771fd?w=600&q=80' WHERE tipo_quarto = 'Suíte Master';
UPDATE quartos SET imagem_url = 'https://images.unsplash.com/photo-1505693416388-ac5ce068fe85?w=600&q=80' WHERE tipo_quarto = 'Quarto Simples';
UPDATE quartos SET imagem_url = 'https://images.unsplash.com/photo-1566665797739-1674de7a421a?w=600&q=80' WHERE tipo_quarto = 'Quarto Família';

-- ------------------------------------------------------------
-- 4.1 Promover usuário para administrador
-- ------------------------------------------------------------
UPDATE usuarios
SET tipo = 'admin'
WHERE email = 'julia@gmail.com';

-- ============================================================
-- PACOTES
-- ============================================================
-- Mapa de IDs:
--   1  → Rio Premium Experience        (destino 1 – RJ)
--   2  → Bahia All Inclusive           (destino 2 – BA)
--   3  → Califórnia Dreams             (destino 4 – CA)
--   4  → Nova York Experience          (destino 4 – NY)
--   5  → Paris Romântica               (destino 6 – FR)
--   6  → Toscana Gourmet               (destino 7 – IT)
--   7  → Patagônia Argentina           (destino 10 – AR)
--   8  → Tóquio Tech Tour              (destino 8 – JP)
--   9  → Bali Paradise                 (destino 12 – ID)
--  10  → Dubai Lux Experience          (destino 12 – AE)
-- ============================================================

-- 1 · Rio Premium Experience — praia de Copacabana / Ipanema
UPDATE pacotes
SET imagem_url = 'https://images.unsplash.com/photo-1483729558449-99ef09a8c325?w=800&q=80'
WHERE id = 1;

-- 2 · Bahia All Inclusive — praia de Morro de São Paulo / Trancoso
UPDATE pacotes
SET imagem_url = 'https://images.unsplash.com/photo-1483729558449-99ef09a8c325?w=800&q=80'
WHERE id = 2;

-- 3 · Califórnia Dreams — rodovia 1 / costa da Califórnia
UPDATE pacotes
SET imagem_url = 'https://images.unsplash.com/photo-1501594907352-04cda38ebc29?w=800&q=80'
WHERE id = 3;

-- 4 · Nova York Experience — Times Square / Manhattan skyline
UPDATE pacotes
SET imagem_url = 'https://images.unsplash.com/photo-1534430480872-3498386e7856?w=800&q=80'
WHERE id = 4;

-- 5 · Paris Romântica — Torre Eiffel ao entardecer
UPDATE pacotes
SET imagem_url = 'https://images.unsplash.com/photo-1502602898657-3e91760cbb34?w=800&q=80'
WHERE id = 5;

-- 6 · Toscana Gourmet — vinhedos e villa toscana
UPDATE pacotes
SET imagem_url = 'https://americachip.com/wp-content/uploads/2023/10/wikipedia-toscana.jpg'
WHERE id = 6;

-- 7 · Patagônia Argentina — Torres del Paine / Perito Moreno
UPDATE pacotes
SET imagem_url = 'https://images.unsplash.com/photo-1501854248509-c7e427ccd5ae?w=800&q=80'
WHERE id = 7;

-- 8 · Tóquio Tech Tour — Shibuya crossing / templos
UPDATE pacotes
SET imagem_url = 'https://media.cntraveller.com/photos/6343df288d5d266e2e66f082/16:9/w_2560%2Cc_limit/tokyoGettyImages-1031467664.jpeg'
WHERE id = 8;

-- 9 · Bali Paradise — terraços de arroz / templo Uluwatu
UPDATE pacotes
SET imagem_url = 'https://images.unsplash.com/photo-1537996194471-e657df975ab4?w=800&q=80'
WHERE id = 9;

-- 10 · Dubai Lux Experience — Burj Khalifa / skyline Dubai
UPDATE pacotes
SET imagem_url = 'https://images.unsplash.com/photo-1512453979798-5ea266f8880c?w=800&q=80'
WHERE id = 10;

-- ------------------------------------------------------------
-- 4.2 Atualizar imagem_url dos destinos (por id — chave primária)
-- ------------------------------------------------------------
-- ============================================================
-- DESTINOS
-- ============================================================
-- Mapa de IDs:
--   1  → Brasil / Rio de Janeiro        (SP → RJ)
--   2  → Brasil / Bahia                 (SP → BA)
--   3  → Brasil / Ceará                 (SP → CE)
--   4  → Estados Unidos / Califórnia    (SP → CA)
--   5  → Estados Unidos / Flórida       (SP → FL)
--   6  → França / Provença-Alpes-Costa Azul (SP → FR)
--   7  → Itália / Toscana              (SP → IT)
--   8  → Japão / Tóquio               (SP → JP)
--   9  → Portugal / Lisboa             (RJ → PT)
--  10  → Argentina / Buenos Aires      (RJ → AR)
--  11  → Chile / Região Metropolitana  (MG → CL)
--  12  → México / Quintana Roo         (PR → MX)
-- ============================================================

-- 1 · Rio de Janeiro — Cristo Redentor e Pão de Açúcar
UPDATE destinos
SET imagem_url = 'https://images.unsplash.com/photo-1483729558449-99ef09a8c325?w=800&q=80'
WHERE id = 1;

-- 2 · Bahia — Pelourinho / Salvador colorido
UPDATE destinos
SET imagem_url = 'https://images.unsplash.com/photo-1591233055842-a984961b71af?w=800&q=80'
WHERE id = 2;

-- 3 · Ceará — Jericoacoara / dunas e lagoa
UPDATE destinos
SET imagem_url = 'https://images.unsplash.com/photo-1661692612848-37801f680815?w=800&q=80'
WHERE id = 3;

-- 4 · Califórnia — Golden Gate / São Francisco
UPDATE destinos
SET imagem_url = 'https://images.unsplash.com/photo-1501594907352-04cda38ebc29?w=800&q=80'
WHERE id = 4;

-- 5 · Flórida — Miami Beach / South Beach
UPDATE destinos
SET imagem_url = 'https://images.unsplash.com/photo-1754269675202-6fb0016d9f21?w=800&q=80'
WHERE id = 5;

-- 6 · França / Provença — Torre Eiffel Paris
UPDATE destinos
SET imagem_url = 'https://images.unsplash.com/photo-1502602898657-3e91760cbb34?w=800&q=80'
WHERE id = 6;

-- 7 · Itália / Toscana — vinhedos e colinas típicas
UPDATE destinos
SET imagem_url = 'https://images.unsplash.com/photo-1759062012196-ab43aef31a6f?w=800&q=80'
WHERE id = 7;

-- 8 · Japão / Tóquio — skyline noturno de Tóquio
UPDATE destinos
SET imagem_url = 'https://images.unsplash.com/photo-1540959733332-eab4deabeeaf?w=800&q=80'
WHERE id = 8;

-- 9 · Portugal / Lisboa — Belém / Torre de Belém
UPDATE destinos
SET imagem_url = 'https://images.unsplash.com/photo-1585208798174-6cedd86e019a?w=800&q=80'
WHERE id = 9;

-- 10 · Argentina / Buenos Aires — obelisco / avenida 9 de Julho
UPDATE destinos
SET imagem_url = 'https://images.unsplash.com/photo-1612294037637-ec328d0e075e?w=800&q=80'
WHERE id = 10;

-- 11 · Chile / Santiago — vista panorâmica cidade + Andes
UPDATE destinos
SET imagem_url = 'https://images.unsplash.com/photo-1689850543263-01a52ccc6943?w=800&q=80'
WHERE id = 11;

-- 12 · México / Quintana Roo — Cancún / Riviera Maya praia turquesa
UPDATE destinos
SET imagem_url = 'https://images.unsplash.com/photo-1552074284-5e88ef1aef18?w=800&q=80'
WHERE id = 12;


-- ------------------------------------------------------------
-- 4.3 Atualizar preco_por_pessoa dos destinos (por id)
-- ------------------------------------------------------------
UPDATE destinos SET preco_por_pessoa = 1200.00  WHERE id = 1;   -- Rio de Janeiro
UPDATE destinos SET preco_por_pessoa = 980.00   WHERE id = 2;   -- Bahia
UPDATE destinos SET preco_por_pessoa = 850.00   WHERE id = 3;   -- Ceará
UPDATE destinos SET preco_por_pessoa = 8500.00  WHERE id = 4;   -- Califórnia
UPDATE destinos SET preco_por_pessoa = 7900.00  WHERE id = 5;   -- Flórida
UPDATE destinos SET preco_por_pessoa = 11500.00 WHERE id = 6;   -- França
UPDATE destinos SET preco_por_pessoa = 10800.00 WHERE id = 7;   -- Itália
UPDATE destinos SET preco_por_pessoa = 13200.00 WHERE id = 8;   -- Japão
UPDATE destinos SET preco_por_pessoa = 9400.00  WHERE id = 9;   -- Portugal
UPDATE destinos SET preco_por_pessoa = 4200.00  WHERE id = 10;  -- Argentina
UPDATE destinos SET preco_por_pessoa = 5600.00  WHERE id = 11;  -- Chile
UPDATE destinos SET preco_por_pessoa = 6300.00  WHERE id = 12;  -- México

-- ------------------------------------------------------------
-- 4.4 Atualizar imagem_url dos pacotes (por id)
-- ------------------------------------------------------------
-- ============================================================
-- PACOTES
-- ============================================================
-- Mapa de IDs:
--   1  → Rio Premium Experience        (destino 1 – RJ)
--   2  → Bahia All Inclusive           (destino 2 – BA)
--   3  → Califórnia Dreams             (destino 4 – CA)
--   4  → Nova York Experience          (destino 4 – NY)
--   5  → Paris Romântica               (destino 6 – FR)
--   6  → Toscana Gourmet               (destino 7 – IT)
--   7  → Patagônia Argentina           (destino 10 – AR)
--   8  → Tóquio Tech Tour              (destino 8 – JP)
--   9  → Bali Paradise                 (destino 12 – ID)
--  10  → Dubai Lux Experience          (destino 12 – AE)
-- ============================================================

-- 1 · Rio Premium Experience — praia de Copacabana / Ipanema
UPDATE pacotes
SET imagem_url = 'https://images.unsplash.com/photo-1483729558449-99ef09a8c325?w=800&q=80'
WHERE id = 1;

-- 2 · Bahia All Inclusive — praia de Morro de São Paulo / Trancoso
UPDATE pacotes
SET imagem_url = 'https://images.unsplash.com/photo-1591233055842-a984961b71af?w=800&q=80'
WHERE id = 2;

-- 3 · Califórnia Dreams — rodovia 1 / costa da Califórnia
UPDATE pacotes
SET imagem_url = 'https://images.unsplash.com/photo-1501594907352-04cda38ebc29?w=800&q=80'
WHERE id = 3;

-- 4 · Nova York Experience — Times Square / Manhattan skyline
UPDATE pacotes
SET imagem_url = 'https://images.unsplash.com/photo-1534430480872-3498386e7856?w=800&q=80'
WHERE id = 4;

-- 5 · Paris Romântica — Torre Eiffel ao entardecer
UPDATE pacotes
SET imagem_url = 'https://images.unsplash.com/photo-1502602898657-3e91760cbb34?w=800&q=80'
WHERE id = 5;

-- 6 · Toscana Gourmet — vinhedos e villa toscana
UPDATE pacotes
SET imagem_url = 'https://images.unsplash.com/photo-1759062012196-ab43aef31a6f?w=800&q=80'
WHERE id = 6;

-- 7 · Patagônia Argentina — Torres del Paine / Perito Moreno
UPDATE pacotes
SET imagem_url = 'https://images.unsplash.com/photo-1741684650296-19f452c8814f?w=800&q=80'
WHERE id = 7;

-- 8 · Tóquio Tech Tour — Shibuya crossing / templos
UPDATE pacotes
SET imagem_url = 'https://images.unsplash.com/photo-1537996194471-e657df975ab4?w=800&q=80'
WHERE id = 8;

-- 9 · Bali Paradise — terraços de arroz / templo Uluwatu
UPDATE pacotes
SET imagem_url = 'https://images.unsplash.com/photo-1512453979798-5ea266f8880c?w=800&q=80'
WHERE id = 9;

-- 10 · Dubai Lux Experience — Burj Khalifa / skyline Dubai
UPDATE pacotes
SET imagem_url = 'https://images.unsplash.com/photo-1512453979798-5ea266f8880c?w=800&q=80'
WHERE id = 10;


-- ============================================================
-- HOSPEDAGENS E QUARTOS POR PACOTE
-- ============================================================

-- ==========================================
-- INSERINDO 1 HOSPEDAGEM PARA CADA PACOTE
-- ==========================================

INSERT INTO hospedagens 
(pacote_id, nome, categoria, descricao, endereco, imagem_url) 
VALUES
(1, 'Hotel Copacabana Mar', 'Hotel 4 estrelas', 
'Hotel confortável próximo à praia de Copacabana, com café da manhã incluso.',
'Av. Atlântica, 1500 - Copacabana, Rio de Janeiro - RJ',
'https://images.unsplash.com/photo-1566073771259-6a8506099945?w=700&q=80'),

(2, 'Resort Bahia Sol', 'Resort', 
'Resort com área de lazer, piscina e alimentação inclusa para aproveitar a Bahia.',
'Rodovia BA-099, km 45 - Salvador - BA',
'https://images.unsplash.com/photo-1571896349842-33c89424de2d?w=700&q=80'),

(3, 'California Dream Hotel', 'Hotel 4 estrelas', 
'Hotel moderno em Los Angeles, ideal para conhecer praias, parques e pontos turísticos.',
'Sunset Boulevard, 7200 - Los Angeles - Califórnia',
'https://images.unsplash.com/photo-1551882547-ff40c63fe5fa?w=700&q=80'),

(4, 'Manhattan City Hotel', 'Hotel Boutique', 
'Hotel localizado em Manhattan, próximo aos principais pontos turísticos de Nova York.',
'West 45th Street, 150 - Manhattan - Nova York',
'https://images.unsplash.com/photo-1445019980597-93fa8acb246c?w=700&q=80'),

(5, 'Paris Lumière Hotel', 'Hotel Boutique', 
'Hotel elegante e confortável para uma experiência romântica em Paris.',
'Rue Saint-Dominique, 82 - Paris - França',
'https://images.unsplash.com/photo-1564501049412-61c2a3083791?w=700&q=80'),

(6, 'Villa Toscana Hotel', 'Hotel Boutique', 
'Hospedagem charmosa na Toscana, ideal para turismo gastronômico e cultural.',
'Strada del Chianti, 40 - Toscana - Itália',
'https://images.unsplash.com/photo-1518005020951-eccb494ad742?w=700&q=80'),

(7, 'Tokyo Central Hotel', 'Hotel 4 estrelas', 
'Hotel moderno em Tóquio, próximo a regiões tecnológicas e pontos culturais.',
'Shinjuku-ku, 3-12-8 - Tóquio - Japão',
'https://images.unsplash.com/photo-1542314831-068cd1dbfeeb?w=700&q=80'),

(8, 'Bali Paradise Resort', 'Resort Luxo', 
'Resort em Bali com piscina, spa e estrutura para descanso.',
'Jalan Pantai, 88 - Bali - Indonésia',
'https://images.unsplash.com/photo-1535827841776-24afc1e255ac?w=700&q=80'),

(9, 'Dubai Skyline Hotel', 'Hotel Luxo', 
'Hotel de luxo em Dubai com ótima localização e vista para a cidade.',
'Sheikh Zayed Road, 1200 - Dubai',
'https://images.unsplash.com/photo-1561501878-aabd62634533?w=700&q=80');

-- ==========================================
-- QUARTOS DO PACOTE 1 - RIO DE JANEIRO
-- ==========================================

INSERT INTO quartos
(hospedagem_id, tipo_quarto, capacidade_adultos, capacidade_criancas, preco_adicional, quantidade_disponivel, comodidades, imagem_url)
VALUES
(12, 'Quarto Standard', 2, 1, 0.00, 10, 
'Wi-Fi, Ar-condicionado, TV e café da manhã',
'https://images.unsplash.com/photo-1611892440504-42a792e24d32?w=600&q=80'),

(12, 'Quarto Luxo', 2, 1, 350.00, 6, 
'Wi-Fi, Ar-condicionado, Frigobar e vista para a cidade',
'https://images.unsplash.com/photo-1582719478250-c89cae4dc85b?w=600&q=80'),

(12, 'Suíte Vista Mar', 2, 1, 700.00, 4, 
'Wi-Fi, Ar-condicionado, Varanda e vista para o mar',
'https://images.unsplash.com/photo-1590490360182-c33d57733427?w=600&q=80'),

(12, 'Quarto Família', 4, 2, 900.00, 3, 
'Wi-Fi, Ar-condicionado, 2 camas e espaço família',
'https://images.unsplash.com/photo-1566665797739-1674de7a421a?w=600&q=80');


-- ==========================================
-- QUARTOS DO PACOTE 2 - BAHIA
-- ==========================================

INSERT INTO quartos
(hospedagem_id, tipo_quarto, capacidade_adultos, capacidade_criancas, preco_adicional, quantidade_disponivel, comodidades, imagem_url)
VALUES
(4, 'Quarto Standard', 2, 1, 0.00, 12, 
'Wi-Fi, Ar-condicionado, TV e alimentação inclusa',
'https://images.unsplash.com/photo-1505693416388-ac5ce068fe85?w=600&q=80'),

(4, 'Quarto Superior', 2, 2, 300.00, 8, 
'Wi-Fi, Ar-condicionado, Vista para o jardim e frigobar',
'https://images.unsplash.com/photo-1566665797739-1674de7a421a?w=600&q=80'),

(4, 'Quarto Luxo', 3, 2, 550.00, 5, 
'Wi-Fi, Ar-condicionado, Varanda e vista para piscina',
'https://images.unsplash.com/photo-1560448204-e02f11c3d0e2?w=600&q=80'),

(4, 'Suíte Família', 4, 2, 850.00, 4, 
'Wi-Fi, Ar-condicionado, sala pequena e frigobar',
'https://images.unsplash.com/photo-1598928506311-c55ded91a20c?w=600&q=80');


-- ==========================================
-- QUARTOS DO PACOTE 3 - CALIFÓRNIA
-- ==========================================

INSERT INTO quartos
(hospedagem_id, tipo_quarto, capacidade_adultos, capacidade_criancas, preco_adicional, quantidade_disponivel, comodidades, imagem_url)
VALUES
(5, 'Quarto Standard', 2, 0, 0.00, 8, 
'Wi-Fi, Ar-condicionado, TV e mesa de trabalho',
'https://images.unsplash.com/photo-1618773928121-c32242e63f39?w=600&q=80'),

(5, 'Quarto Deluxe', 2, 1, 500.00, 6, 
'Wi-Fi, Ar-condicionado, cama queen e frigobar',
'https://images.unsplash.com/photo-1595576508898-0ad5c879a061?w=600&q=80'),

(5, 'Quarto Premium', 2, 1, 850.00, 4, 
'Wi-Fi, Ar-condicionado, vista privilegiada e cafeteira',
'https://images.unsplash.com/photo-1566195992011-5f6b21e539aa?w=600&q=80'),

(5, 'Quarto Família', 4, 2, 1100.00, 3, 
'Wi-Fi, Ar-condicionado, 2 camas e espaço família',
'https://images.unsplash.com/photo-1560448075-bb485b067938?w=600&q=80');


-- ==========================================
-- QUARTOS DO PACOTE 4 - NOVA YORK
-- ==========================================

INSERT INTO quartos
(hospedagem_id, tipo_quarto, capacidade_adultos, capacidade_criancas, preco_adicional, quantidade_disponivel, comodidades, imagem_url)
VALUES
(6, 'Quarto Standard', 2, 0, 0.00, 9, 
'Wi-Fi, Ar-condicionado, TV e cofre',
'https://images.unsplash.com/photo-1560448075-bb485b067938?w=600&q=80'),

(6, 'Quarto Superior', 2, 1, 600.00, 6, 
'Wi-Fi, Ar-condicionado, vista da cidade e frigobar',
'https://images.unsplash.com/photo-1560185007-cde436f6a4d0?w=600&q=80'),

(6, 'Quarto Deluxe', 2, 1, 950.00, 4, 
'Wi-Fi, Ar-condicionado, cafeteira e vista urbana',
'https://images.unsplash.com/photo-1600585154340-be6161a56a0c?w=600&q=80'),

(6, 'Suíte Executiva', 3, 1, 1400.00, 2, 
'Wi-Fi, Ar-condicionado, sala de estar e mesa executiva',
'https://images.unsplash.com/photo-1600566753086-00f18fb6b3ea?w=600&q=80');


-- ==========================================
-- QUARTOS DO PACOTE 5 - PARIS
-- ==========================================

INSERT INTO quartos
(hospedagem_id, tipo_quarto, capacidade_adultos, capacidade_criancas, preco_adicional, quantidade_disponivel, comodidades, imagem_url)
VALUES
(7, 'Quarto Casal Standard', 2, 0, 0.00, 7, 
'Wi-Fi, Ar-condicionado, TV e café da manhã',
'https://images.unsplash.com/photo-1615873968403-89e068629265?w=600&q=80'),

(7, 'Quarto Superior', 2, 0, 550.00, 5, 
'Wi-Fi, Ar-condicionado, frigobar e decoração premium',
'https://images.unsplash.com/photo-1618221195710-dd6b41faaea6?w=600&q=80'),

(7, 'Quarto Luxo com Varanda', 2, 0, 900.00, 3, 
'Wi-Fi, Ar-condicionado, varanda e vista da cidade',
'https://images.unsplash.com/photo-1600210492486-724fe5c67fb0?w=600&q=80'),

(7, 'Suíte Romântica', 2, 0, 1400.00, 2, 
'Wi-Fi, Hidromassagem, varanda e decoração especial',
'https://images.unsplash.com/photo-1600607687939-ce8a6c25118c?w=600&q=80');


-- ==========================================
-- QUARTOS DO PACOTE 6 - TOSCANA
-- ==========================================

INSERT INTO quartos
(hospedagem_id, tipo_quarto, capacidade_adultos, capacidade_criancas, preco_adicional, quantidade_disponivel, comodidades, imagem_url)
VALUES
(8, 'Quarto Standard', 2, 0, 0.00, 6, 
'Wi-Fi, Ar-condicionado, café da manhã e vista para jardim',
'https://images.unsplash.com/photo-1616486338812-3dadae4b4ace?w=600&q=80'),

(8, 'Quarto Superior', 2, 0, 500.00, 5, 
'Wi-Fi, Ar-condicionado, vista para vinhedo e frigobar',
'https://images.unsplash.com/photo-1618220179428-22790b461013?w=600&q=80'),

(8, 'Quarto Luxo', 2, 1, 850.00, 3, 
'Wi-Fi, Ar-condicionado, kit café e vista panorâmica',
'https://images.unsplash.com/photo-1615529162924-f8605388461d?w=600&q=80'),

(8, 'Suíte Família', 4, 2, 1200.00, 2, 
'Wi-Fi, Ar-condicionado, sala de estar e vista para vinhedo',
'https://images.unsplash.com/photo-1618220179428-22790b461013?w=600&q=80');


-- ==========================================
-- QUARTOS DO PACOTE 7 - TÓQUIO
-- ==========================================

INSERT INTO quartos
(hospedagem_id, tipo_quarto, capacidade_adultos, capacidade_criancas, preco_adicional, quantidade_disponivel, comodidades, imagem_url)
VALUES
(9, 'Quarto Standard', 2, 0, 0.00, 10, 
'Wi-Fi, Ar-condicionado, TV e mesa compacta',
'https://images.unsplash.com/photo-1554995207-c18c203602cb?w=600&q=80'),

(9, 'Quarto Superior', 2, 0, 600.00, 6, 
'Wi-Fi, Ar-condicionado, vista urbana e frigobar',
'https://images.unsplash.com/photo-1617098474202-0d0d7f60c56b?w=600&q=80'),

(9, 'Quarto Deluxe', 2, 1, 950.00, 4, 
'Wi-Fi, Ar-condicionado, automação e vista para a cidade',
'https://images.unsplash.com/photo-1616594039964-ae9021a400a0?w=600&q=80'),

(9, 'Suíte Família', 4, 2, 1400.00, 2, 
'Wi-Fi, Ar-condicionado, sala e espaço família',
'https://images.unsplash.com/photo-1600566753190-17f0baa2a6c3?w=600&q=80');


-- ==========================================
-- QUARTOS DO PACOTE 8 - BALI
-- ==========================================

INSERT INTO quartos
(hospedagem_id, tipo_quarto, capacidade_adultos, capacidade_criancas, preco_adicional, quantidade_disponivel, comodidades, imagem_url)
VALUES
(10, 'Quarto Standard', 2, 1, 0.00, 6, 
'Wi-Fi, Ar-condicionado, varanda e vista para jardim',
'https://images.unsplash.com/photo-1598928506311-c55ded91a20c?w=600&q=80'),

(10, 'Quarto Deluxe', 2, 1, 700.00, 4, 
'Wi-Fi, Ar-condicionado, vista para piscina e frigobar',
'https://images.unsplash.com/photo-1560448204-603b3fc33ddc?w=600&q=80'),

(10, 'Villa Privativa', 2, 0, 1600.00, 3, 
'Wi-Fi, Ar-condicionado, piscina privativa e spa',
'https://images.unsplash.com/photo-1564013799919-ab600027ffc6?w=600&q=80'),

(10, 'Villa Família', 4, 2, 2200.00, 2, 
'Wi-Fi, Ar-condicionado, piscina privativa e 2 quartos',
'https://images.unsplash.com/photo-1600607688969-a5bfcd646154?w=600&q=80');


-- ==========================================
-- QUARTOS DO PACOTE 9 - DUBAI
-- ==========================================
INSERT INTO quartos
(hospedagem_id, tipo_quarto, capacidade_adultos, capacidade_criancas, preco_adicional, quantidade_disponivel, comodidades, imagem_url)
VALUES
(11, 'Quarto Standard Luxo', 2, 0, 0.00, 5, 
'Wi-Fi, Ar-condicionado, TV e vista urbana',
'https://images.unsplash.com/photo-1590490360182-c33d57733427?w=600&q=80'),

(11, 'Quarto Deluxe', 2, 1, 900.00, 4, 
'Wi-Fi, Ar-condicionado, vista skyline e frigobar',
'https://images.unsplash.com/photo-1591088398332-8a7791972843?w=600&q=80'),

(11, 'Quarto Premium', 2, 1, 1700.00, 3, 
'Wi-Fi, Ar-condicionado, vista privilegiada e cafeteira',
'https://images.unsplash.com/photo-1600607687939-ce8a6c25118c?w=600&q=80'),

(11, 'Suíte Família Luxo', 4, 2, 2800.00, 2, 
'Wi-Fi, Ar-condicionado, sala, serviço premium e 2 quartos',
'https://images.unsplash.com/photo-1600566753086-00f18fb6b3ea?w=600&q=80');
-- ============================================================
-- 5. ÍNDICES
-- ============================================================

CREATE INDEX idx_carrinho_usuario ON carrinho(usuario_id);
CREATE INDEX idx_pacotes_destino  ON pacotes(destino_id);
CREATE INDEX idx_viagem_usuario   ON viagem_personalizada(usuario_id);
    
    SELECT * FROM pacotes;
    SELECT * FROM usuarios;
    SELECT * FROM destinos;
    SELECT * FROM pedidos;
    
    
    