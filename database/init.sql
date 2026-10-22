CREATE TABLE IF NOT EXISTS users (
    id uuid PRIMARY KEY,
    email varchar(200) NOT NULL UNIQUE,
    password_hash text NOT NULL,
    role varchar(40) NOT NULL DEFAULT 'Administrator',
    is_active boolean NOT NULL DEFAULT true,
    created_at timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS products (
    id uuid PRIMARY KEY,
    sku varchar(40) NOT NULL UNIQUE,
    name varchar(140) NOT NULL,
    description varchar(500),
    unit_price numeric(12, 2) NOT NULL CHECK (unit_price >= 0),
    minimum_stock integer NOT NULL CHECK (minimum_stock >= 0),
    is_active boolean NOT NULL DEFAULT true,
    created_at timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS inventory_movements (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    product_id uuid NOT NULL REFERENCES products(id),
    user_id uuid NOT NULL REFERENCES users(id),
    movement_type varchar(3) NOT NULL CHECK (movement_type IN ('IN', 'OUT')),
    quantity integer NOT NULL CHECK (quantity > 0),
    note varchar(240),
    created_at timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS ix_inventory_movements_created_at ON inventory_movements(created_at DESC);
CREATE INDEX IF NOT EXISTS ix_inventory_movements_product_id ON inventory_movements(product_id);

CREATE OR REPLACE PROCEDURE register_stock_entry(
    p_product_id uuid,
    p_quantity integer,
    p_note varchar,
    p_user_id uuid
)
LANGUAGE plpgsql
AS $$
BEGIN
    IF p_quantity <= 0 THEN
        RAISE EXCEPTION USING ERRCODE = '22023', MESSAGE = 'INVALID_QUANTITY';
    END IF;

    PERFORM 1 FROM products WHERE id = p_product_id AND is_active FOR UPDATE;
    IF NOT FOUND THEN
        RAISE EXCEPTION USING ERRCODE = '23503', MESSAGE = 'PRODUCT_NOT_FOUND';
    END IF;

    INSERT INTO inventory_movements(product_id, user_id, movement_type, quantity, note)
    VALUES (p_product_id, p_user_id, 'IN', p_quantity, NULLIF(trim(p_note), ''));
END;
$$;

CREATE OR REPLACE PROCEDURE register_stock_exit(
    p_product_id uuid,
    p_quantity integer,
    p_note varchar,
    p_user_id uuid
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_current_stock integer;
BEGIN
    IF p_quantity <= 0 THEN
        RAISE EXCEPTION USING ERRCODE = '22023', MESSAGE = 'INVALID_QUANTITY';
    END IF;

    -- The product row lock serializes concurrent movements for the same SKU.
    PERFORM 1 FROM products WHERE id = p_product_id AND is_active FOR UPDATE;
    IF NOT FOUND THEN
        RAISE EXCEPTION USING ERRCODE = '23503', MESSAGE = 'PRODUCT_NOT_FOUND';
    END IF;

    SELECT COALESCE(SUM(CASE WHEN movement_type = 'IN' THEN quantity ELSE -quantity END), 0)::integer
      INTO v_current_stock
      FROM inventory_movements
     WHERE product_id = p_product_id;

    IF p_quantity > v_current_stock THEN
        RAISE EXCEPTION USING ERRCODE = 'P0001', MESSAGE = 'INSUFFICIENT_STOCK';
    END IF;

    INSERT INTO inventory_movements(product_id, user_id, movement_type, quantity, note)
    VALUES (p_product_id, p_user_id, 'OUT', p_quantity, NULLIF(trim(p_note), ''));
END;
$$;

CREATE OR REPLACE PROCEDURE get_current_inventory(INOUT p_cursor refcursor DEFAULT 'inventory_cursor')
LANGUAGE plpgsql
AS $$
BEGIN
    OPEN p_cursor FOR
    SELECT p.id AS product_id,
           p.sku,
           p.name,
           p.unit_price,
           p.minimum_stock,
           COALESCE(SUM(CASE WHEN m.movement_type = 'IN' THEN m.quantity ELSE -m.quantity END), 0)::integer AS current_stock,
           CASE
               WHEN COALESCE(SUM(CASE WHEN m.movement_type = 'IN' THEN m.quantity ELSE -m.quantity END), 0) = 0 THEN 'Sin existencias'
               WHEN COALESCE(SUM(CASE WHEN m.movement_type = 'IN' THEN m.quantity ELSE -m.quantity END), 0) <= p.minimum_stock THEN 'Stock bajo'
               ELSE 'Disponible'
           END AS status
      FROM products p
      LEFT JOIN inventory_movements m ON m.product_id = p.id
     WHERE p.is_active
     GROUP BY p.id, p.sku, p.name, p.unit_price, p.minimum_stock
     ORDER BY p.name;
END;
$$;

INSERT INTO products(id, sku, name, description, unit_price, minimum_stock)
VALUES
    ('c0e9f56f-cb20-4da3-9ccb-902411003001', 'TEC-001', 'Audífonos Studio Pro', 'Audio inalámbrico con cancelación de ruido', 489900, 8),
    ('c0e9f56f-cb20-4da3-9ccb-902411003002', 'TEC-002', 'Teclado mecánico K2', 'Teclado compacto con switches táctiles', 329900, 10),
    ('c0e9f56f-cb20-4da3-9ccb-902411003003', 'TEC-003', 'Mouse ergonómico MX', 'Mouse inalámbrico para oficina', 219900, 8),
    ('c0e9f56f-cb20-4da3-9ccb-902411003004', 'ACC-001', 'Soporte aluminio', 'Base ajustable para portátil', 149900, 6),
    ('c0e9f56f-cb20-4da3-9ccb-902411003005', 'ACC-002', 'Hub USB-C 7 en 1', 'Adaptador multipuerto para equipos USB-C', 189900, 5),
    ('c0e9f56f-cb20-4da3-9ccb-902411003006', 'TEC-004', 'Monitor View 27', 'Monitor IPS de 27 pulgadas', 1149900, 4)
ON CONFLICT (sku) DO NOTHING;
