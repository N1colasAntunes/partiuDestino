-- ============================================================
-- PARTIU DESTINO — BANCO DE DADOS COMPLETO E ORGANIZADO
-- Objetivo: manter 1 hospedagem por pacote + quartos completos
-- Observação: script organizado para execução limpa, sem blocos duplicados
-- Última organização: junho/2026
-- ============================================================

CREATE DATABASE IF NOT EXISTS bdpartiudestino;
USE bdpartiudestino;

SET SQL_SAFE_UPDATES = 0;
SET FOREIGN_KEY_CHECKS = 0;

-- ============================================================
-- 1. LIMPEZA OPCIONAL PARA REEXECUÇÃO DO SCRIPT
-- ============================================================
-- Use somente se quiser recriar os dados do zero.
-- As tabelas são apagadas na ordem correta por causa das chaves estrangeiras.

DROP TABLE IF EXISTS quartos;
DROP TABLE IF EXISTS hospedagens;
DROP TABLE IF EXISTS pedidos;
DROP TABLE IF EXISTS carrinho;
DROP TABLE IF EXISTS viagem_personalizada;
DROP TABLE IF EXISTS pacotes;
DROP TABLE IF EXISTS destinos;
DROP TABLE IF EXISTS usuarios;

SET FOREIGN_KEY_CHECKS = 1;

-- ============================================================
-- 2. CRIAÇÃO DAS TABELAS
-- ============================================================

-- ------------------------------------------------------------
-- 2.1 Tabela: usuarios
-- ------------------------------------------------------------
CREATE TABLE usuarios (
    id INT PRIMARY KEY AUTO_INCREMENT,
    nome VARCHAR(255) NOT NULL,
    email VARCHAR(255) NOT NULL UNIQUE,
    senha VARCHAR(255) NOT NULL,
    tipo VARCHAR(20) NOT NULL DEFAULT 'usuario', -- usuario | admin
    ativo TINYINT(1) NOT NULL DEFAULT 1,
    foto_perfil VARCHAR(500) NULL DEFAULT NULL COMMENT 'Caminho relativo da foto: /uploads/perfis/nome.jpg',
    telefone VARCHAR(20) NULL DEFAULT NULL,
    documento VARCHAR(20) NULL DEFAULT NULL,
    data_nascimento DATE NULL DEFAULT NULL
);

describe usuarios;
-- ------------------------------------------------------------
-- 2.2 Tabela: destinos
-- ------------------------------------------------------------
CREATE TABLE destinos (
    id INT PRIMARY KEY AUTO_INCREMENT,
    origem_pais VARCHAR(100) NOT NULL,
    origem_estado VARCHAR(100) NOT NULL,
    pais VARCHAR(100) NOT NULL,
    estado VARCHAR(100) NOT NULL,
    imagem_url VARCHAR(500),
    preco_por_pessoa DECIMAL(10,2) NOT NULL DEFAULT 0.00
);

-- ------------------------------------------------------------
-- 2.3 Tabela: pacotes
-- ------------------------------------------------------------
CREATE TABLE pacotes (
    id INT PRIMARY KEY AUTO_INCREMENT,
    destino_id INT NOT NULL,
    nome VARCHAR(200) NOT NULL,
    descricao TEXT,
    tipo_viagem VARCHAR(50),
    duracao_dias INT,
    data_partida DATE,
    data_retorno DATE,
    preco_por_pessoa DECIMAL(10,2) NOT NULL,
    vagas_disponiveis INT,
    imagem_url VARCHAR(500),
    FOREIGN KEY (destino_id) REFERENCES destinos(id) ON DELETE CASCADE
);

ALTER TABLE pacotes
    ADD COLUMN voo_companhia_aerea VARCHAR(100) NULL,
    ADD COLUMN voo_titulo VARCHAR(150) NULL,
    ADD COLUMN voo_descricao TEXT NULL,
    ADD COLUMN voo_aeroporto_origem VARCHAR(100) NULL,
    ADD COLUMN voo_aeroporto_destino VARCHAR(100) NULL,
    ADD COLUMN voo_horario_ida VARCHAR(10) NULL,
    ADD COLUMN voo_horario_volta VARCHAR(10) NULL,
    ADD COLUMN voo_duracao_media VARCHAR(30) NULL,
    ADD COLUMN voo_bagagem_inclusa VARCHAR(150) NULL,
    ADD COLUMN voo_tipo_tarifa VARCHAR(100) NULL,
    ADD COLUMN voo_escala VARCHAR(100) NULL,
    ADD COLUMN voo_preco_adicional_por_pessoa DECIMAL(10,2) NOT NULL DEFAULT 0.00;
    
    SHOW COLUMNS FROM pacotes LIKE 'voo_companhia_aerea';

-- ------------------------------------------------------------
-- 2.4 Tabela: viagem_personalizada
-- ------------------------------------------------------------
CREATE TABLE viagem_personalizada (
    id INT PRIMARY KEY AUTO_INCREMENT,
    usuario_id INT NOT NULL,

    nome_completo VARCHAR(150) NOT NULL,
    cpf VARCHAR(14) NOT NULL,
    email VARCHAR(150) NOT NULL,
    whatsapp VARCHAR(20) NOT NULL,

    origem VARCHAR(100),
    destino VARCHAR(100),
    regiao_interesse VARCHAR(50),
    data_partida DATE,
    duracao_dias INT,
    transporte VARCHAR(50),

    tipo_hospedagem VARCHAR(50),
    categoria_hospedagem VARCHAR(50),
    preferencias_hospedagem TEXT,

    adultos INT DEFAULT 1,
    criancas INT DEFAULT 0,
    idosos INT DEFAULT 0,
    tipo_grupo VARCHAR(50),

    objetivo_viagem VARCHAR(100),
    ritmo_viagem VARCHAR(50),
    clima_viagem VARCHAR(50),

    faixa_orcamento VARCHAR(50),
    desejos_especiais TEXT,
    data_criacao TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (usuario_id) REFERENCES usuarios(id) ON DELETE CASCADE
);

-- ------------------------------------------------------------
-- 2.5 Tabela: carrinho
-- ------------------------------------------------------------
CREATE TABLE carrinho (
    id INT PRIMARY KEY AUTO_INCREMENT,
    usuario_id INT NOT NULL,
    tipo_item VARCHAR(30) NOT NULL, -- pacote | destino | viagem_personalizada
    item_id INT NOT NULL,
    nome_item VARCHAR(1000) NOT NULL,
    preco_unitario DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    quantidade INT NOT NULL DEFAULT 1,
    data_adicionado TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
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
describe pedidos;
use bdpartiudestino;
ALTER TABLE pedidos
ADD COLUMN codigo_reserva VARCHAR(30) NULL AFTER id,
ADD COLUMN forma_pagamento VARCHAR(30) NULL AFTER quantidade,
ADD COLUMN status_pagamento VARCHAR(40) NULL DEFAULT 'Confirmado' AFTER forma_pagamento,
ADD COLUMN valor_total_pedido DECIMAL(10,2) NULL DEFAULT 0.00 AFTER status_pagamento,
ADD COLUMN parcelas INT NULL DEFAULT 1 AFTER valor_total_pedido,
ADD COLUMN comprovante VARCHAR(80) NULL AFTER parcelas,
ADD COLUMN data_pagamento DATETIME NULL AFTER comprovante;

-- ------------------------------------------------------------
-- 2.7 Tabela: hospedagens
-- Regra atual dos pacotes: 1 hospedagem por pacote.
-- destino_id fica disponível para o fluxo futuro de passagens/destinos.
-- ------------------------------------------------------------
CREATE TABLE hospedagens (
    id INT PRIMARY KEY AUTO_INCREMENT,
    pacote_id INT NULL,
    destino_id INT NULL,
    nome VARCHAR(150) NOT NULL,
    categoria VARCHAR(50),
    descricao TEXT,
    endereco VARCHAR(255),
    imagem_url VARCHAR(500),

    checkin VARCHAR(10) NULL DEFAULT '14:00',
    checkout VARCHAR(10) NULL DEFAULT '12:00',
    cafe_incluso TINYINT(1) NOT NULL DEFAULT 1,
    wifi_incluso TINYINT(1) NOT NULL DEFAULT 1,
    estacionamento TINYINT(1) NOT NULL DEFAULT 0,
    politica_cancelamento TEXT NULL,
    regras_hospedagem TEXT NULL,
    avaliacao DECIMAL(3,1) NULL DEFAULT 8.5,
    comodidades TEXT NULL,

    FOREIGN KEY (pacote_id) REFERENCES pacotes(id) ON DELETE CASCADE,
    FOREIGN KEY (destino_id) REFERENCES destinos(id) ON DELETE CASCADE,

    CONSTRAINT chk_hospedagem_dono CHECK (
        (pacote_id IS NOT NULL AND destino_id IS NULL) OR
        (pacote_id IS NULL AND destino_id IS NOT NULL)
    )
);

-- ------------------------------------------------------------
-- 2.8 Tabela: quartos
-- ------------------------------------------------------------
CREATE TABLE quartos (
    id INT PRIMARY KEY AUTO_INCREMENT,
    hospedagem_id INT NOT NULL,
    tipo_quarto VARCHAR(100) NOT NULL,
    capacidade_adultos INT NOT NULL DEFAULT 2,
    capacidade_criancas INT NOT NULL DEFAULT 0,
    preco_adicional DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    quantidade_disponivel INT NOT NULL DEFAULT 1,
    comodidades VARCHAR(255),
    imagem_url VARCHAR(500),

    numero_camas INT NULL DEFAULT 1,
    tipo_camas VARCHAR(150) NULL,
    cafe_incluso TINYINT(1) NOT NULL DEFAULT 1,
    area_m2 DECIMAL(5,2) NULL,
    descricao TEXT NULL,
    politica_cancelamento TEXT NULL,

    FOREIGN KEY (hospedagem_id) REFERENCES hospedagens(id) ON DELETE CASCADE
);

-- ============================================================
-- 3. ÍNDICES
-- ============================================================
CREATE INDEX idx_carrinho_usuario ON carrinho(usuario_id);
CREATE INDEX idx_pacotes_destino ON pacotes(destino_id);
CREATE INDEX idx_viagem_usuario ON viagem_personalizada(usuario_id);
CREATE INDEX idx_hospedagens_pacote ON hospedagens(pacote_id);
CREATE INDEX idx_hospedagens_destino ON hospedagens(destino_id);
CREATE INDEX idx_quartos_hospedagem ON quartos(hospedagem_id);

-- ============================================================
-- 4. INSERTS BASE
-- ============================================================

-- ------------------------------------------------------------
-- 4.1 Usuário administrador padrão
-- ------------------------------------------------------------
INSERT INTO usuarios (nome, email, senha, tipo, ativo) VALUES
('Julia Costa', 'julia@gmail.com', '$2a$11$KG8AxkIziG2A6C9aOIzWkeD82eW96KTcXrDiM2JMYZGlSmLVoU2am', 'admin', 1);

-- ------------------------------------------------------------
-- 4.2 Destinos
-- ------------------------------------------------------------
INSERT INTO destinos (origem_pais, origem_estado, pais, estado, imagem_url, preco_por_pessoa) VALUES
('Brasil', 'São Paulo', 'Brasil', 'Rio de Janeiro', 'https://images.unsplash.com/photo-1483729558449-99ef09a8c325?w=800&q=80', 1200.00),
('Brasil', 'São Paulo', 'Brasil', 'Bahia', 'https://images.unsplash.com/photo-1591233055842-a984961b71af?w=800&q=80', 980.00),
('Brasil', 'São Paulo', 'Brasil', 'Ceará', 'https://images.unsplash.com/photo-1661692612848-37801f680815?w=800&q=80', 850.00),
('Brasil', 'São Paulo', 'Estados Unidos', 'Califórnia', 'https://images.unsplash.com/photo-1501594907352-04cda38ebc29?w=800&q=80', 8500.00),
('Brasil', 'São Paulo', 'Estados Unidos', 'Flórida', 'https://images.unsplash.com/photo-1754269675202-6fb0016d9f21?w=800&q=80', 7900.00),
('Brasil', 'São Paulo', 'França', 'Provença-Alpes-Costa Azul', 'https://images.unsplash.com/photo-1502602898657-3e91760cbb34?w=800&q=80', 11500.00),
('Brasil', 'São Paulo', 'Itália', 'Toscana', 'https://images.unsplash.com/photo-1759062012196-ab43aef31a6f?w=800&q=80', 10800.00),
('Brasil', 'São Paulo', 'Japão', 'Tóquio', 'https://images.unsplash.com/photo-1540959733332-eab4deabeeaf?w=800&q=80', 13200.00),
('Brasil', 'Rio de Janeiro', 'Portugal', 'Lisboa', 'https://images.unsplash.com/photo-1585208798174-6cedd86e019a?w=800&q=80', 9400.00),
('Brasil', 'Rio de Janeiro', 'Argentina', 'Buenos Aires', 'https://images.unsplash.com/photo-1612294037637-ec328d0e075e?w=800&q=80', 4200.00),
('Brasil', 'Minas Gerais', 'Chile', 'Região Metropolitana de Santiago', 'https://images.unsplash.com/photo-1689850543263-01a52ccc6943?w=800&q=80', 5600.00),
('Brasil', 'Paraná', 'México', 'Quintana Roo', 'https://images.unsplash.com/photo-1552074284-5e88ef1aef18?w=800&q=80', 6300.00);

-- ------------------------------------------------------------
-- 4.3 Pacotes
-- ------------------------------------------------------------
INSERT INTO pacotes
(destino_id, nome, descricao, tipo_viagem, duracao_dias, data_partida, data_retorno, preco_por_pessoa, vagas_disponiveis, imagem_url)
VALUES
(1, 'Rio Premium Experience', 'Pacote completo para conhecer as praias e pontos turísticos do Rio de Janeiro.', 'Praia', 7, '2026-07-10', '2026-07-17', 4599.90, 20, 'https://images.unsplash.com/photo-1483729558449-99ef09a8c325?w=800&q=80'),
(2, 'Bahia All Inclusive', 'Experiência incrível em resort all inclusive na Bahia.', 'Relaxamento', 6, '2026-08-05', '2026-08-11', 3899.50, 15, 'https://images.unsplash.com/photo-1591233055842-a984961b71af?w=800&q=80'),
(4, 'Califórnia Dreams', 'Conheça Los Angeles, praias e parques famosos da Califórnia.', 'Internacional', 10, '2026-09-12', '2026-09-22', 12999.99, 12, 'https://images.unsplash.com/photo-1501594907352-04cda38ebc29?w=800&q=80'),
(4, 'Nova York Experience', 'Pacote completo para explorar Nova York e seus principais pontos turísticos.', 'Internacional', 8, '2026-11-03', '2026-11-11', 14500.00, 10, 'https://images.unsplash.com/photo-1534430480872-3498386e7856?w=800&q=80'),
(6, 'Paris Romântica', 'Uma viagem inesquecível para casais na cidade luz.', 'Romance', 7, '2026-06-15', '2026-06-22', 16990.90, 8, 'https://images.unsplash.com/photo-1502602898657-3e91760cbb34?w=800&q=80'),
(7, 'Toscana Gourmet', 'Experiência gastronômica e cultural na Toscana.', 'Gastronomia', 9, '2026-10-02', '2026-10-11', 15200.00, 10, 'https://images.unsplash.com/photo-1759062012196-ab43aef31a6f?w=800&q=80'),
(8, 'Tóquio Tech Tour', 'Conheça o Japão moderno e tradicional em uma experiência única.', 'Cultura', 12, '2026-09-05', '2026-09-17', 18990.00, 9, 'https://media.cntraveller.com/photos/6343df288d5d266e2e66f082/16:9/w_2560%2Cc_limit/tokyoGettyImages-1031467664.jpeg'),
(12, 'Bali Paradise', 'Pacote de luxo em Bali com hospedagem premium.', 'Relaxamento', 10, '2026-12-01', '2026-12-11', 17499.99, 6, 'https://images.unsplash.com/photo-1537996194471-e657df975ab4?w=800&q=80'),
(12, 'Dubai Lux Experience', 'Explore o luxo e modernidade de Dubai.', 'Luxo', 7, '2026-08-18', '2026-08-25', 19990.00, 5, 'https://images.unsplash.com/photo-1512453979798-5ea266f8880c?w=800&q=80');




-- PACOTE 1 - RIO PREMIUM EXPERIENCE
UPDATE pacotes
SET
    voo_companhia_aerea = 'Azul Linhas Aéreas',
    voo_titulo = 'Voo nacional direto',
    voo_descricao = 'Voo direto de São Paulo para o Rio de Janeiro, com horários práticos para ida e retorno. Ideal para quem busca conforto e bom custo-benefício.',
    voo_aeroporto_origem = 'CGH - Congonhas',
    voo_aeroporto_destino = 'SDU - Santos Dumont',
    voo_horario_ida = '08:30',
    voo_horario_volta = '18:45',
    voo_duracao_media = '1h 10min',
    voo_bagagem_inclusa = 'Bagagem de mão inclusa',
    voo_tipo_tarifa = 'Tarifa econômica incluída no pacote',
    voo_escala = 'Voo direto',
    voo_preco_adicional_por_pessoa = 0.00
WHERE id = 1;

-- PACOTE 2 - BAHIA ALL INCLUSIVE
UPDATE pacotes
SET
    voo_companhia_aerea = 'Gol Linhas Aéreas',
    voo_titulo = 'Voo nacional para Salvador',
    voo_descricao = 'Voo direto para Salvador, com horário confortável de chegada e retorno. Ideal para aproveitar melhor o resort e o período da viagem.',
    voo_aeroporto_origem = 'GRU - Guarulhos',
    voo_aeroporto_destino = 'SSA - Salvador',
    voo_horario_ida = '09:20',
    voo_horario_volta = '19:10',
    voo_duracao_media = '2h 20min',
    voo_bagagem_inclusa = 'Bagagem de mão inclusa',
    voo_tipo_tarifa = 'Tarifa econômica incluída no pacote',
    voo_escala = 'Voo direto',
    voo_preco_adicional_por_pessoa = 0.00
WHERE id = 2;

-- PACOTE 3 - CALIFÓRNIA DREAMS
UPDATE pacotes
SET
    voo_companhia_aerea = 'LATAM Airlines',
    voo_titulo = 'Voo internacional com conexão',
    voo_descricao = 'Voo internacional para Los Angeles com conexão, tarifa econômica e bagagem de mão inclusa. Indicado para viagem internacional de longa duração.',
    voo_aeroporto_origem = 'GRU - Guarulhos',
    voo_aeroporto_destino = 'LAX - Los Angeles',
    voo_horario_ida = '22:30',
    voo_horario_volta = '19:40',
    voo_duracao_media = '14h 30min',
    voo_bagagem_inclusa = 'Bagagem de mão inclusa',
    voo_tipo_tarifa = 'Tarifa econômica internacional',
    voo_escala = '1 conexão',
    voo_preco_adicional_por_pessoa = 0.00
WHERE id = 3;

-- PACOTE 4 - NOVA YORK EXPERIENCE
UPDATE pacotes
SET
    voo_companhia_aerea = 'American Airlines',
    voo_titulo = 'Voo internacional para Nova York',
    voo_descricao = 'Voo internacional para Nova York com conexão otimizada e horários adequados para melhor aproveitamento da viagem.',
    voo_aeroporto_origem = 'GRU - Guarulhos',
    voo_aeroporto_destino = 'JFK - Nova York',
    voo_horario_ida = '21:45',
    voo_horario_volta = '18:10',
    voo_duracao_media = '13h 40min',
    voo_bagagem_inclusa = 'Bagagem de mão inclusa',
    voo_tipo_tarifa = 'Tarifa econômica internacional',
    voo_escala = '1 conexão',
    voo_preco_adicional_por_pessoa = 0.00
WHERE id = 4;

-- PACOTE 5 - PARIS ROMÂNTICA
UPDATE pacotes
SET
    voo_companhia_aerea = 'Air France',
    voo_titulo = 'Voo internacional para Paris',
    voo_descricao = 'Voo internacional para Paris, ideal para casais, com experiência confortável e tarifa econômica incluída.',
    voo_aeroporto_origem = 'GRU - Guarulhos',
    voo_aeroporto_destino = 'CDG - Paris',
    voo_horario_ida = '20:45',
    voo_horario_volta = '18:00',
    voo_duracao_media = '11h 30min',
    voo_bagagem_inclusa = 'Bagagem de mão inclusa',
    voo_tipo_tarifa = 'Tarifa econômica internacional',
    voo_escala = 'Voo direto',
    voo_preco_adicional_por_pessoa = 0.00
WHERE id = 5;

-- PACOTE 6 - TOSCANA GOURMET
UPDATE pacotes
SET
    voo_companhia_aerea = 'ITA Airways',
    voo_titulo = 'Voo internacional para Itália',
    voo_descricao = 'Voo internacional para a Itália, com conexão e horários adequados para chegada e deslocamento até a Toscana.',
    voo_aeroporto_origem = 'GRU - Guarulhos',
    voo_aeroporto_destino = 'FCO - Roma',
    voo_horario_ida = '20:40',
    voo_horario_volta = '18:30',
    voo_duracao_media = '12h 10min',
    voo_bagagem_inclusa = 'Bagagem de mão inclusa',
    voo_tipo_tarifa = 'Tarifa econômica internacional',
    voo_escala = '1 conexão',
    voo_preco_adicional_por_pessoa = 0.00
WHERE id = 6;

-- PACOTE 7 - TÓQUIO TECH TOUR
UPDATE pacotes
SET
    voo_companhia_aerea = 'Emirates',
    voo_titulo = 'Voo internacional para Tóquio',
    voo_descricao = 'Voo internacional com conexão para Tóquio, indicado para longa distância com boa experiência de viagem.',
    voo_aeroporto_origem = 'GRU - Guarulhos',
    voo_aeroporto_destino = 'HND - Tóquio',
    voo_horario_ida = '01:25',
    voo_horario_volta = '23:10',
    voo_duracao_media = '24h 30min',
    voo_bagagem_inclusa = 'Bagagem de mão inclusa',
    voo_tipo_tarifa = 'Tarifa econômica internacional',
    voo_escala = '1 conexão',
    voo_preco_adicional_por_pessoa = 0.00
WHERE id = 7;

-- PACOTE 8 - BALI PARADISE
UPDATE pacotes
SET
    voo_companhia_aerea = 'Qatar Airways',
    voo_titulo = 'Voo internacional para Bali',
    voo_descricao = 'Voo internacional com conexão para Bali, ideal para pacote de relaxamento, resort e longa estadia.',
    voo_aeroporto_origem = 'GRU - Guarulhos',
    voo_aeroporto_destino = 'DPS - Bali',
    voo_horario_ida = '02:20',
    voo_horario_volta = '22:40',
    voo_duracao_media = '24h 45min',
    voo_bagagem_inclusa = 'Bagagem de mão inclusa',
    voo_tipo_tarifa = 'Tarifa econômica internacional',
    voo_escala = '1 conexão',
    voo_preco_adicional_por_pessoa = 0.00
WHERE id = 8;

-- PACOTE 9 - DUBAI LUX EXPERIENCE
UPDATE pacotes
SET
    voo_companhia_aerea = 'Emirates',
    voo_titulo = 'Voo internacional para Dubai',
    voo_descricao = 'Voo internacional para Dubai com companhia premium, boa experiência de viagem e horário adequado para chegada ao destino.',
    voo_aeroporto_origem = 'GRU - Guarulhos',
    voo_aeroporto_destino = 'DXB - Dubai',
    voo_horario_ida = '01:25',
    voo_horario_volta = '20:30',
    voo_duracao_media = '14h 00min',
    voo_bagagem_inclusa = 'Bagagem de mão inclusa',
    voo_tipo_tarifa = 'Tarifa econômica internacional',
    voo_escala = 'Voo direto',
    voo_preco_adicional_por_pessoa = 0.00
WHERE id = 9;
-- ============================================================
-- 5. HOSPEDAGENS — 1 POR PACOTE
-- ============================================================
INSERT INTO hospedagens
(pacote_id, destino_id, nome, categoria, descricao, endereco, imagem_url, checkin, checkout, cafe_incluso, wifi_incluso, estacionamento, politica_cancelamento, regras_hospedagem, avaliacao, comodidades)
VALUES
(1, NULL, 'Hotel Copacabana Mar', 'Hotel 4 estrelas', 'Hotel confortável próximo à praia de Copacabana, ideal para quem deseja aproveitar o Rio de Janeiro com praticidade, boa localização e café da manhã incluso.', 'Av. Atlântica, 1500 - Copacabana, Rio de Janeiro - RJ', 'https://images.unsplash.com/photo-1566073771259-6a8506099945?w=700&q=80', '14:00', '12:00', 1, 1, 0, 'Cancelamento e alterações seguem as políticas da Partiu Destino e dos fornecedores envolvidos. Valores pagos podem estar sujeitos a taxas administrativas e regras específicas do pacote.', 'Documento obrigatório no check-in. Menores de idade devem estar acompanhados por responsável legal. Horários de entrada e saída devem ser respeitados conforme política da hospedagem.', 8.7, 'Wi-Fi, Café da manhã, Recepção 24h, Ar-condicionado, Restaurante, Serviço de quarto'),
(2, NULL, 'Resort Bahia Sol', 'Resort', 'Resort com piscina, área de lazer, restaurante e estrutura completa para descanso e diversão durante a experiência na Bahia.', 'Rodovia BA-099, km 45 - Salvador - BA', 'https://images.unsplash.com/photo-1571896349842-33c89424de2d?w=700&q=80', '15:00', '12:00', 1, 1, 1, 'Cancelamentos seguem as regras do pacote e do resort. Alterações de data ou quarto estão sujeitas à disponibilidade e podem gerar custos adicionais.', 'Documento obrigatório no check-in. Pulseiras de identificação podem ser exigidas durante a estadia. Menores devem estar acompanhados por responsável legal.', 9.1, 'Wi-Fi, Café da manhã, Piscina, Restaurante, Área de lazer, Bar, Recepção 24h, Estacionamento'),
(3, NULL, 'California Dream Hotel', 'Hotel 4 estrelas', 'Hotel moderno em Los Angeles, ideal para conhecer praias, parques e pontos turísticos da Califórnia com conforto e boa localização.', 'Sunset Boulevard, 7200 - Los Angeles - Califórnia', 'https://images.unsplash.com/photo-1551882547-ff40c63fe5fa?w=700&q=80', '15:00', '11:00', 1, 1, 1, 'Cancelamentos e alterações devem seguir as regras da Partiu Destino e dos fornecedores internacionais. Alterações podem sofrer variação cambial e taxas.', 'Documento e passaporte podem ser solicitados no check-in. O hóspede deve respeitar as normas locais e horários definidos pela hospedagem.', 8.8, 'Wi-Fi, Café da manhã, Academia, Estacionamento, Recepção 24h, Ar-condicionado, Restaurante'),
(4, NULL, 'Manhattan City Hotel', 'Hotel Boutique', 'Hotel localizado em Manhattan, próximo aos principais pontos turísticos de Nova York, ideal para quem deseja praticidade e localização central.', 'West 45th Street, 150 - Manhattan - Nova York', 'https://images.unsplash.com/photo-1445019980597-93fa8acb246c?w=700&q=80', '15:00', '11:00', 0, 1, 0, 'Cancelamentos seguem as políticas do pacote contratado. Alterações de quarto, datas ou quantidade de hóspedes podem gerar custos adicionais.', 'Documento obrigatório no check-in. Taxas locais podem ser cobradas pela hospedagem. Respeitar horários de entrada e saída.', 8.6, 'Wi-Fi, Recepção 24h, Ar-condicionado, Elevador, Serviço de quarto, Localização central'),
(5, NULL, 'Paris Lumière Hotel', 'Hotel Boutique', 'Hotel elegante e confortável para uma experiência romântica em Paris, com ótima localização e ambiente aconchegante.', 'Rue Saint-Dominique, 82 - Paris - França', 'https://images.unsplash.com/photo-1564501049412-61c2a3083791?w=700&q=80', '14:00', '12:00', 1, 1, 0, 'Cancelamentos e remarcações seguem as regras da Partiu Destino e dos fornecedores. Alterações estão sujeitas à disponibilidade.', 'Documento obrigatório no check-in. Menores devem estar acompanhados por responsável legal. Taxas locais podem ser cobradas pela hospedagem.', 9.0, 'Wi-Fi, Café da manhã, Recepção 24h, Ar-condicionado, Restaurante, Serviço de quarto'),
(6, NULL, 'Villa Toscana Hotel', 'Hotel Boutique', 'Hospedagem charmosa na Toscana, ideal para turismo gastronômico e cultural, com ambiente tranquilo e vista para paisagens típicas da região.', 'Strada del Chianti, 40 - Toscana - Itália', 'https://images.unsplash.com/photo-1518005020951-eccb494ad742?w=700&q=80', '14:00', '11:00', 1, 1, 1, 'Cancelamentos seguem a política do pacote. Alterações de datas, quarto ou quantidade de hóspedes estão sujeitas à disponibilidade.', 'Documento obrigatório no check-in. Respeitar horários da hospedagem e normas locais. Menores devem estar acompanhados por responsável legal.', 8.9, 'Wi-Fi, Café da manhã, Estacionamento, Restaurante, Jardim, Ar-condicionado, Recepção'),
(7, NULL, 'Tokyo Central Hotel', 'Hotel 4 estrelas', 'Hotel moderno em Tóquio, próximo a regiões tecnológicas e pontos culturais, ideal para uma experiência equilibrada entre tradição e modernidade.', 'Shinjuku-ku, 3-12-8 - Tóquio - Japão', 'https://images.unsplash.com/photo-1542314831-068cd1dbfeeb?w=700&q=80', '15:00', '11:00', 1, 1, 0, 'Cancelamentos e alterações seguem as regras da Partiu Destino e fornecedores internacionais. Alterações podem gerar taxas adicionais.', 'Documento e passaporte podem ser solicitados. Respeitar normas locais, horários de entrada e saída e orientações da hospedagem.', 8.8, 'Wi-Fi, Café da manhã, Recepção 24h, Ar-condicionado, Restaurante, Elevador'),
(8, NULL, 'Bali Paradise Resort', 'Resort Luxo', 'Resort em Bali com piscina, spa, área de lazer e estrutura completa para descanso em uma experiência premium.', 'Jalan Pantai, 88 - Bali - Indonésia', 'https://images.unsplash.com/photo-1535827841776-24afc1e255ac?w=700&q=80', '14:00', '12:00', 1, 1, 1, 'Cancelamentos e alterações seguem as regras do pacote e do resort. Serviços extras podem ter cobrança separada.', 'Documento obrigatório no check-in. Menores devem estar acompanhados. Uso de áreas comuns sujeito às regras da hospedagem.', 9.3, 'Wi-Fi, Café da manhã, Piscina, Spa, Restaurante, Estacionamento, Recepção 24h, Área de lazer'),
(9, NULL, 'Dubai Skyline Hotel', 'Hotel Luxo', 'Hotel de luxo em Dubai, com excelente localização, estrutura premium e vista para a cidade.', 'Sheikh Zayed Road, 1200 - Dubai', 'https://images.unsplash.com/photo-1561501878-aabd62634533?w=700&q=80', '15:00', '12:00', 1, 1, 1, 'Cancelamentos e alterações seguem as políticas da Partiu Destino e fornecedores. Serviços de luxo e extras podem possuir regras próprias.', 'Documento ou passaporte obrigatório no check-in. Respeitar normas locais, horários e regras da hospedagem. Taxas locais podem ser aplicadas.', 9.2, 'Wi-Fi, Café da manhã, Piscina, Academia, Restaurante, Estacionamento, Recepção 24h, Serviço de quarto');

-- ============================================================
-- 6. QUARTOS — 4 OPÇÕES POR HOSPEDAGEM
-- ============================================================
INSERT INTO quartos
(hospedagem_id, tipo_quarto, capacidade_adultos, capacidade_criancas, preco_adicional, quantidade_disponivel, comodidades, imagem_url, numero_camas, tipo_camas, cafe_incluso, area_m2, descricao, politica_cancelamento)
VALUES
-- Hotel Copacabana Mar — hospedagem_id 1
(1, 'Quarto Standard', 2, 1, 0.00, 10, 'Wi-Fi, Ar-condicionado, TV e café da manhã', 'https://images.unsplash.com/photo-1611892440504-42a792e24d32?w=600&q=80', 1, '1 cama de casal', 1, 22.00, 'Quarto confortável para casal ou pequena família, com ar-condicionado, banheiro privativo, Wi-Fi e boa estrutura para estadias curtas.', 'Alterações e cancelamentos seguem as regras do pacote e da hospedagem.'),
(1, 'Quarto Luxo', 2, 1, 350.00, 6, 'Wi-Fi, Ar-condicionado, Frigobar e vista para a cidade', 'https://images.unsplash.com/photo-1582719478250-c89cae4dc85b?w=600&q=80', 1, '1 cama queen size', 1, 28.00, 'Quarto mais espaçoso, indicado para quem busca mais conforto, com cama queen, frigobar, Wi-Fi e melhor vista.', 'Alterações e cancelamentos seguem as regras do pacote e da hospedagem.'),
(1, 'Suíte Vista Mar', 2, 1, 700.00, 4, 'Wi-Fi, Ar-condicionado, Varanda e vista para o mar', 'https://images.unsplash.com/photo-1590490360182-c33d57733427?w=600&q=80', 1, '1 cama king size', 1, 35.00, 'Suíte com varanda e vista para o mar, ideal para quem deseja uma experiência mais completa durante a hospedagem.', 'Alterações e cancelamentos seguem as regras do pacote e da hospedagem.'),
(1, 'Quarto Família', 4, 2, 900.00, 3, 'Wi-Fi, Ar-condicionado, 2 camas e espaço família', 'https://images.unsplash.com/photo-1566665797739-1674de7a421a?w=600&q=80', 3, '1 cama de casal e 2 camas de solteiro', 1, 42.00, 'Quarto amplo para famílias, com camas múltiplas, banheiro privativo, Wi-Fi e espaço confortável para adultos e crianças.', 'Alterações e cancelamentos seguem as regras do pacote e da hospedagem.'),
-- Resort Bahia Sol — hospedagem_id 2
(2, 'Quarto Standard', 2, 1, 0.00, 12, 'Wi-Fi, Ar-condicionado, TV e alimentação inclusa', 'https://images.unsplash.com/photo-1505693416388-ac5ce068fe85?w=600&q=80', 1, '1 cama de casal', 1, 24.00, 'Quarto confortável com estrutura básica do resort, ideal para casal ou pequena família.', 'Alterações e cancelamentos seguem as regras do pacote e do resort.'),
(2, 'Quarto Superior', 2, 2, 300.00, 8, 'Wi-Fi, Ar-condicionado, Vista para o jardim e frigobar', 'https://images.unsplash.com/photo-1566665797739-1674de7a421a?w=600&q=80', 2, '1 cama de casal e 1 cama de solteiro', 1, 30.00, 'Quarto superior com vista para o jardim, frigobar e melhor espaço interno para famílias pequenas.', 'Alterações e cancelamentos seguem as regras do pacote e do resort.'),
(2, 'Quarto Luxo', 3, 2, 550.00, 5, 'Wi-Fi, Ar-condicionado, Varanda e vista para piscina', 'https://images.unsplash.com/photo-1560448204-e02f11c3d0e2?w=600&q=80', 2, '1 cama queen size e 1 cama de solteiro', 1, 36.00, 'Quarto luxo com varanda e vista para piscina, recomendado para quem busca mais conforto durante a estadia.', 'Alterações e cancelamentos seguem as regras do pacote e do resort.'),
(2, 'Suíte Família', 4, 2, 850.00, 4, 'Wi-Fi, Ar-condicionado, sala pequena e frigobar', 'https://images.unsplash.com/photo-1598928506311-c55ded91a20c?w=600&q=80', 3, '1 cama de casal e 2 camas de solteiro', 1, 45.00, 'Suíte espaçosa para família, com sala pequena, frigobar e conforto para adultos e crianças.', 'Alterações e cancelamentos seguem as regras do pacote e do resort.'),
-- California Dream Hotel — hospedagem_id 3
(3, 'Quarto Standard', 2, 0, 0.00, 8, 'Wi-Fi, Ar-condicionado, TV e mesa de trabalho', 'https://images.unsplash.com/photo-1618773928121-c32242e63f39?w=600&q=80', 1, '1 cama de casal', 1, 23.00, 'Quarto funcional e confortável, ideal para casal ou viajantes que desejam praticidade em Los Angeles.', 'Alterações e cancelamentos seguem as regras do pacote e da hospedagem.'),
(3, 'Quarto Deluxe', 2, 1, 500.00, 6, 'Wi-Fi, Ar-condicionado, cama queen e frigobar', 'https://images.unsplash.com/photo-1595576508898-0ad5c879a061?w=600&q=80', 1, '1 cama queen size', 1, 30.00, 'Quarto deluxe com cama queen, frigobar e espaço adicional para maior conforto.', 'Alterações e cancelamentos seguem as regras do pacote e da hospedagem.'),
(3, 'Quarto Premium', 2, 1, 850.00, 4, 'Wi-Fi, Ar-condicionado, vista privilegiada e cafeteira', 'https://images.unsplash.com/photo-1566195992011-5f6b21e539aa?w=600&q=80', 1, '1 cama king size', 1, 34.00, 'Quarto premium com vista privilegiada, cafeteira e estrutura superior para estadia internacional.', 'Alterações e cancelamentos seguem as regras do pacote e da hospedagem.'),
(3, 'Quarto Família', 4, 2, 1100.00, 3, 'Wi-Fi, Ar-condicionado, 2 camas e espaço família', 'https://images.unsplash.com/photo-1560448075-bb485b067938?w=600&q=80', 3, '1 cama de casal e 2 camas de solteiro', 1, 44.00, 'Quarto família amplo para viagens internacionais em grupo ou com crianças.', 'Alterações e cancelamentos seguem as regras do pacote e da hospedagem.'),
-- Manhattan City Hotel — hospedagem_id 4
(4, 'Quarto Standard', 2, 0, 0.00, 9, 'Wi-Fi, Ar-condicionado, TV e cofre', 'https://images.unsplash.com/photo-1560448075-bb485b067938?w=600&q=80', 1, '1 cama de casal', 0, 20.00, 'Quarto compacto e funcional em Manhattan, ideal para quem pretende aproveitar a cidade durante o dia.', 'Alterações e cancelamentos seguem as regras do pacote e da hospedagem.'),
(4, 'Quarto Superior', 2, 1, 600.00, 6, 'Wi-Fi, Ar-condicionado, vista da cidade e frigobar', 'https://images.unsplash.com/photo-1560185007-cde436f6a4d0?w=600&q=80', 1, '1 cama queen size', 0, 27.00, 'Quarto superior com vista da cidade, frigobar e melhor conforto para estadia urbana.', 'Alterações e cancelamentos seguem as regras do pacote e da hospedagem.'),
(4, 'Quarto Deluxe', 2, 1, 950.00, 4, 'Wi-Fi, Ar-condicionado, cafeteira e vista urbana', 'https://images.unsplash.com/photo-1600585154340-be6161a56a0c?w=600&q=80', 1, '1 cama king size', 0, 32.00, 'Quarto deluxe com cafeteira, vista urbana e estrutura diferenciada para maior conforto.', 'Alterações e cancelamentos seguem as regras do pacote e da hospedagem.'),
(4, 'Suíte Executiva', 3, 1, 1400.00, 2, 'Wi-Fi, Ar-condicionado, sala de estar e mesa executiva', 'https://images.unsplash.com/photo-1600566753086-00f18fb6b3ea?w=600&q=80', 2, '1 cama king size e 1 sofá-cama', 0, 45.00, 'Suíte executiva com sala de estar, mesa de apoio e estrutura premium em Manhattan.', 'Alterações e cancelamentos seguem as regras do pacote e da hospedagem.'),
-- Paris Lumière Hotel — hospedagem_id 5
(5, 'Quarto Casal Standard', 2, 0, 0.00, 7, 'Wi-Fi, Ar-condicionado, TV e café da manhã', 'https://images.unsplash.com/photo-1615873968403-89e068629265?w=600&q=80', 1, '1 cama de casal', 1, 21.00, 'Quarto aconchegante para casal, ideal para uma experiência romântica em Paris.', 'Alterações e cancelamentos seguem as regras do pacote e da hospedagem.'),
(5, 'Quarto Superior', 2, 0, 550.00, 5, 'Wi-Fi, Ar-condicionado, frigobar e decoração premium', 'https://images.unsplash.com/photo-1618221195710-dd6b41faaea6?w=600&q=80', 1, '1 cama queen size', 1, 28.00, 'Quarto superior com decoração premium, frigobar e ambiente mais confortável para casal.', 'Alterações e cancelamentos seguem as regras do pacote e da hospedagem.'),
(5, 'Quarto Luxo com Varanda', 2, 0, 900.00, 3, 'Wi-Fi, Ar-condicionado, varanda e vista da cidade', 'https://images.unsplash.com/photo-1600210492486-724fe5c67fb0?w=600&q=80', 1, '1 cama king size', 1, 32.00, 'Quarto luxo com varanda e vista da cidade, ideal para casais que desejam mais charme na hospedagem.', 'Alterações e cancelamentos seguem as regras do pacote e da hospedagem.'),
(5, 'Suíte Romântica', 2, 0, 1400.00, 2, 'Wi-Fi, Hidromassagem, varanda e decoração especial', 'https://images.unsplash.com/photo-1600607687939-ce8a6c25118c?w=600&q=80', 1, '1 cama king size', 1, 40.00, 'Suíte romântica com hidromassagem, varanda e decoração especial para casais.', 'Alterações e cancelamentos seguem as regras do pacote e da hospedagem.'),
-- Villa Toscana Hotel — hospedagem_id 6
(6, 'Quarto Standard', 2, 0, 0.00, 6, 'Wi-Fi, Ar-condicionado, café da manhã e vista para jardim', 'https://images.unsplash.com/photo-1616486338812-3dadae4b4ace?w=600&q=80', 1, '1 cama de casal', 1, 24.00, 'Quarto confortável com vista para jardim, ideal para estadia tranquila na Toscana.', 'Alterações e cancelamentos seguem as regras do pacote e da hospedagem.'),
(6, 'Quarto Superior', 2, 0, 500.00, 5, 'Wi-Fi, Ar-condicionado, vista para vinhedo e frigobar', 'https://images.unsplash.com/photo-1618220179428-22790b461013?w=600&q=80', 1, '1 cama queen size', 1, 30.00, 'Quarto superior com vista para vinhedo, frigobar e ambiente típico da região.', 'Alterações e cancelamentos seguem as regras do pacote e da hospedagem.'),
(6, 'Quarto Luxo', 2, 1, 850.00, 3, 'Wi-Fi, Ar-condicionado, kit café e vista panorâmica', 'https://images.unsplash.com/photo-1615529162924-f8605388461d?w=600&q=80', 1, '1 cama king size', 1, 35.00, 'Quarto luxo com vista panorâmica, kit café e estrutura superior para experiência gastronômica e cultural.', 'Alterações e cancelamentos seguem as regras do pacote e da hospedagem.'),
(6, 'Suíte Família', 4, 2, 1200.00, 2, 'Wi-Fi, Ar-condicionado, sala de estar e vista para vinhedo', 'https://images.unsplash.com/photo-1618220179428-22790b461013?w=600&q=80', 3, '1 cama de casal e 2 camas de solteiro', 1, 46.00, 'Suíte ampla para famílias, com sala de estar e vista para vinhedos da Toscana.', 'Alterações e cancelamentos seguem as regras do pacote e da hospedagem.'),
-- Tokyo Central Hotel — hospedagem_id 7
(7, 'Quarto Standard', 2, 0, 0.00, 10, 'Wi-Fi, Ar-condicionado, TV e mesa compacta', 'https://images.unsplash.com/photo-1554995207-c18c203602cb?w=600&q=80', 1, '1 cama de casal', 1, 19.00, 'Quarto moderno e compacto, ideal para aproveitar Tóquio com praticidade.', 'Alterações e cancelamentos seguem as regras do pacote e da hospedagem.'),
(7, 'Quarto Superior', 2, 0, 600.00, 6, 'Wi-Fi, Ar-condicionado, vista urbana e frigobar', 'https://images.unsplash.com/photo-1617098474202-0d0d7f60c56b?w=600&q=80', 1, '1 cama queen size', 1, 25.00, 'Quarto superior com vista urbana, frigobar e mais conforto para estadia em Tóquio.', 'Alterações e cancelamentos seguem as regras do pacote e da hospedagem.'),
(7, 'Quarto Deluxe', 2, 1, 950.00, 4, 'Wi-Fi, Ar-condicionado, automação e vista para a cidade', 'https://images.unsplash.com/photo-1616594039964-ae9021a400a0?w=600&q=80', 1, '1 cama king size', 1, 31.00, 'Quarto deluxe com automação, vista para a cidade e ambiente moderno.', 'Alterações e cancelamentos seguem as regras do pacote e da hospedagem.'),
(7, 'Suíte Família', 4, 2, 1400.00, 2, 'Wi-Fi, Ar-condicionado, sala e espaço família', 'https://images.unsplash.com/photo-1600566753190-17f0baa2a6c3?w=600&q=80', 3, '1 cama de casal e 2 camas de solteiro', 1, 43.00, 'Suíte família com espaço ampliado, indicada para grupos e famílias em viagem internacional.', 'Alterações e cancelamentos seguem as regras do pacote e da hospedagem.'),
-- Bali Paradise Resort — hospedagem_id 8
(8, 'Quarto Standard', 2, 1, 0.00, 6, 'Wi-Fi, Ar-condicionado, varanda e vista para jardim', 'https://images.unsplash.com/photo-1598928506311-c55ded91a20c?w=600&q=80', 1, '1 cama de casal', 1, 28.00, 'Quarto confortável com varanda e vista para jardim, ideal para descanso em Bali.', 'Alterações e cancelamentos seguem as regras do pacote e do resort.'),
(8, 'Quarto Deluxe', 2, 1, 700.00, 4, 'Wi-Fi, Ar-condicionado, vista para piscina e frigobar', 'https://images.unsplash.com/photo-1560448204-603b3fc33ddc?w=600&q=80', 1, '1 cama king size', 1, 36.00, 'Quarto deluxe com vista para piscina, frigobar e estrutura premium para relaxamento.', 'Alterações e cancelamentos seguem as regras do pacote e do resort.'),
(8, 'Villa Privativa', 2, 0, 1600.00, 3, 'Wi-Fi, Ar-condicionado, piscina privativa e spa', 'https://images.unsplash.com/photo-1564013799919-ab600027ffc6?w=600&q=80', 1, '1 cama king size', 1, 55.00, 'Villa privativa com piscina e estrutura exclusiva para casal.', 'Alterações e cancelamentos seguem as regras do pacote e do resort.'),
(8, 'Villa Família', 4, 2, 2200.00, 2, 'Wi-Fi, Ar-condicionado, piscina privativa e 2 quartos', 'https://images.unsplash.com/photo-1600607688969-a5bfcd646154?w=600&q=80', 3, '1 cama de casal e 2 camas de solteiro', 1, 70.00, 'Villa família com piscina privativa, dois ambientes e conforto para adultos e crianças.', 'Alterações e cancelamentos seguem as regras do pacote e do resort.'),
-- Dubai Skyline Hotel — hospedagem_id 9
(9, 'Quarto Standard Luxo', 2, 0, 0.00, 5, 'Wi-Fi, Ar-condicionado, TV e vista urbana', 'https://images.unsplash.com/photo-1590490360182-c33d57733427?w=600&q=80', 1, '1 cama queen size', 1, 30.00, 'Quarto luxo com vista urbana, ideal para uma estadia confortável em Dubai.', 'Alterações e cancelamentos seguem as regras do pacote e da hospedagem.'),
(9, 'Quarto Deluxe', 2, 1, 900.00, 4, 'Wi-Fi, Ar-condicionado, vista skyline e frigobar', 'https://images.unsplash.com/photo-1591088398332-8a7791972843?w=600&q=80', 1, '1 cama king size', 1, 38.00, 'Quarto deluxe com vista skyline, frigobar e estrutura superior.', 'Alterações e cancelamentos seguem as regras do pacote e da hospedagem.'),
(9, 'Quarto Premium', 2, 1, 1700.00, 3, 'Wi-Fi, Ar-condicionado, vista privilegiada e cafeteira', 'https://images.unsplash.com/photo-1600607687939-ce8a6c25118c?w=600&q=80', 1, '1 cama king size', 1, 44.00, 'Quarto premium com vista privilegiada, cafeteira e acabamento superior.', 'Alterações e cancelamentos seguem as regras do pacote e da hospedagem.'),
(9, 'Suíte Família Luxo', 4, 2, 2800.00, 2, 'Wi-Fi, Ar-condicionado, sala, serviço premium e 2 quartos', 'https://images.unsplash.com/photo-1600566753086-00f18fb6b3ea?w=600&q=80', 3, '1 cama de casal e 2 camas de solteiro', 1, 72.00, 'Suíte família luxo com sala, serviço premium e estrutura ampla para família.', 'Alterações e cancelamentos seguem as regras do pacote e da hospedagem.');

-- ============================================================
-- 7. SELECTS DE VERIFICAÇÃO
-- ============================================================

-- 7.1 Confere se cada pacote tem exatamente 1 hospedagem
SELECT 
    p.id AS pacote_id,
    p.nome AS pacote,
    COUNT(h.id) AS total_hospedagens
FROM pacotes p
LEFT JOIN hospedagens h ON h.pacote_id = p.id
GROUP BY p.id, p.nome
ORDER BY p.id;

-- 7.2 Confere hospedagens e principais informações
SELECT 
    p.id AS pacote_id,
    p.nome AS pacote,
    h.id AS hospedagem_id,
    h.nome AS hospedagem,
    h.categoria,
    h.checkin,
    h.checkout,
    h.cafe_incluso,
    h.wifi_incluso,
    h.estacionamento,
    h.avaliacao
FROM pacotes p
INNER JOIN hospedagens h ON h.pacote_id = p.id
ORDER BY p.id;

-- 7.3 Confere se cada hospedagem tem 4 quartos
SELECT
    h.id AS hospedagem_id,
    h.nome AS hospedagem,
    COUNT(q.id) AS total_quartos
FROM hospedagens h
LEFT JOIN quartos q ON q.hospedagem_id = h.id
WHERE h.pacote_id IS NOT NULL
GROUP BY h.id, h.nome
ORDER BY h.id;

-- 7.4 Lista quartos completos
SELECT
    h.id AS hospedagem_id,
    h.nome AS hospedagem,
    q.id AS quarto_id,
    q.tipo_quarto,
    q.capacidade_adultos,
    q.capacidade_criancas,
    q.preco_adicional,
    q.quantidade_disponivel,
    q.numero_camas,
    q.tipo_camas,
    q.cafe_incluso,
    q.area_m2,
    q.descricao
FROM hospedagens h
INNER JOIN quartos q ON q.hospedagem_id = h.id
ORDER BY h.id, q.preco_adicional;

SET SQL_SAFE_UPDATES = 1;

DESCRIBE viagem_personalizada;
SHOW CREATE TABLE viagem_personalizada;