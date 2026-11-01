-- ============================================================
-- Productos API - Script de base de datos (MySQL 8 / MariaDB)
-- Ejecutar completo en MySQL Workbench o con:
--   mysql -u root -p < database/script.sql
-- ============================================================

CREATE DATABASE IF NOT EXISTS tienda_db
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE tienda_db;
SET NAMES utf8mb4;  -- para que los acentos se guarden bien al correr el script desde consola

DROP TABLE IF EXISTS productos;

CREATE TABLE productos (
    id              INT            NOT NULL AUTO_INCREMENT,
    nombre          VARCHAR(100)   NOT NULL,
    descripcion     VARCHAR(255)   NULL,
    categoria       VARCHAR(50)    NOT NULL,
    precio          DECIMAL(10,2)  NOT NULL DEFAULT 0.00,
    stock           INT            NOT NULL DEFAULT 0,
    activo          TINYINT(1)     NOT NULL DEFAULT 1,
    fecha_registro  DATETIME       NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT pk_productos PRIMARY KEY (id),
    CONSTRAINT chk_productos_precio CHECK (precio >= 0),
    CONSTRAINT chk_productos_stock  CHECK (stock >= 0)
);

CREATE INDEX ix_productos_nombre ON productos (nombre);
CREATE INDEX ix_productos_categoria ON productos (categoria);

-- Datos de ejemplo
INSERT INTO productos (nombre, descripcion, categoria, precio, stock, activo) VALUES
('Teclado mecánico',     'Switches rojos, retroiluminación RGB',  'Electrónica', 899.50, 25, 1),
('Mouse inalámbrico',    'Sensor óptico de 1600 DPI',             'Electrónica', 349.00, 40, 1),
('Cuaderno profesional', '100 hojas, cuadro chico',               'Papelería',    45.90, 120, 1),
('Memoria USB 64 GB',    'USB 3.2, carcasa metálica',             'Electrónica', 189.00, 60, 1),
('Plumas gel (paquete)', 'Paquete de 10 colores',                 'Papelería',    79.00,  0, 0);

SELECT * FROM productos;

-- ============================================================
-- Usuario exclusivo para la API (no se usa root desde la app)
-- Solo tiene permisos CRUD sobre tienda_db.
-- ============================================================
CREATE USER IF NOT EXISTS 'productos_api'@'localhost' IDENTIFIED BY 'ProductosApi2026';
GRANT SELECT, INSERT, UPDATE, DELETE ON tienda_db.* TO 'productos_api'@'localhost';
FLUSH PRIVILEGES;
